using System.Text.Json;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NuitInfo.Rubeus.RadioOccitania.Modeles;
using NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;

namespace NuitInfo.Rubeus.RadioOccitania.Services.Implementations;

/// <summary>
/// Service de gestion de la configuration de l'enregistrement audio.
/// Permet de charger, sauvegarder et valider la configuration depuis un fichier JSON.
/// </summary>
public class ConfigurateurEnregistrementService : IConfigurateurEnregistrementService
{
    private readonly ILogger<ConfigurateurEnregistrementService> _logger;
    private readonly string _cheminFichierConfig;
    private ConfigurationEnregistrementAudio? _configurationActuelle;
    private readonly object _verrou = new();

    public event EventHandler<ConfigurationEnregistrementAudio>? ConfigurationModifiee;

    public ConfigurateurEnregistrementService(
        ILogger<ConfigurateurEnregistrementService> logger,
        string cheminFichierConfig = "appsettings.enregistrement.json")
    {
        _logger = logger;
        _cheminFichierConfig = cheminFichierConfig;
    }

    public ConfigurationEnregistrementAudio ObtenirConfiguration()
    {
        lock (_verrou)
        {
            if (_configurationActuelle != null)
            {
                return _configurationActuelle;
            }

            _configurationActuelle = ChargerDepuisFichier() ?? CreerConfigurationParDefaut();
            return _configurationActuelle;
        }
    }

    public async Task MettreAJourConfigurationAsync(ConfigurationEnregistrementAudio configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var erreurs = configuration.Valider();
        if (erreurs.Count > 0)
        {
            var messageErreur = string.Join(", ", erreurs);
            throw new InvalidOperationException($"Configuration invalide: {messageErreur}");
        }

        try
        {
            var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(_cheminFichierConfig, json);

            lock (_verrou)
            {
                _configurationActuelle = configuration;
            }

            _logger.LogInformation(
                "Configuration mise à jour : {Projet} -> {Chemin}",
                configuration.NomProjet,
                _cheminFichierConfig
            );

            OnConfigurationModifiee(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de la configuration");
            throw;
        }
    }

    public async Task<ConfigurationEnregistrementAudio> RechargerConfigurationAsync()
    {
        _logger.LogInformation("Rechargement de la configuration depuis le fichier");

        var config = ChargerDepuisFichier();

        if (config == null)
        {
            _logger.LogWarning(
                "Fichier de configuration introuvable : {Chemin}. Création d'une configuration par défaut.",
                _cheminFichierConfig
            );

            config = CreerConfigurationParDefaut();
            await MettreAJourConfigurationAsync(config);
        }

        lock (_verrou)
        {
            _configurationActuelle = config;
        }

        OnConfigurationModifiee(config);
        
        return config;
    }

    public List<string> ValiderConfiguration()
    {
        var config = ObtenirConfiguration();
        return config.Valider();
    }

    public List<string> ValiderConfiguration(ConfigurationEnregistrementAudio configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return configuration.Valider();
    }

    public async Task ReinitialiserConfigurationAsync()
    {
        _logger.LogWarning("Réinitialisation de la configuration aux valeurs par défaut");

        var configParDefaut = CreerConfigurationParDefaut();
        await MettreAJourConfigurationAsync(configParDefaut);

        _logger.LogInformation("Configuration réinitialisée avec succès");
    }

    public List<PeripheriqueAudio> ObtenirPeripheriquesDisponibles()
    {
        var peripheriques = new List<PeripheriqueAudio>();

        try
        {
            int nombrePeripheriques = WaveInEvent.DeviceCount;

            _logger.LogInformation(
                "Détection de {Count} périphérique(s) audio d'entrée",
                nombrePeripheriques
            );

            for (int i = 0; i < nombrePeripheriques; i++)
            {
                try
                {
                    var capabilities = WaveInEvent.GetCapabilities(i);

                    var peripherique = new PeripheriqueAudio
                    {
                        Index = i,
                        Nom = capabilities.ProductName,
                        NombreCanaux = capabilities.Channels,
                        EstParDefaut = (i == 0)
                    };

                    peripheriques.Add(peripherique);

                    _logger.LogDebug(
                        "Périphérique détecté : [{Index}] {Nom} ({Canaux} canaux)",
                        i,
                        capabilities.ProductName,
                        capabilities.Channels
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Erreur lors de la lecture des capacités du périphérique {Index}",
                        i
                    );
                }
            }

            if (peripheriques.Count == 0)
            {
                _logger.LogWarning(
                    "Aucun périphérique audio d'entrée détecté. " +
                    "Vérifiez que votre microphone est branché et activé."
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'énumération des périphériques audio");
        }

        return peripheriques;
    }

    // === MÉTHODES PRIVÉES ===

    private ConfigurationEnregistrementAudio? ChargerDepuisFichier()
    {
        try
        {
            if (!File.Exists(_cheminFichierConfig))
            {
                return null;
            }

            var json = File.ReadAllText(_cheminFichierConfig);
            var config = JsonSerializer.Deserialize<ConfigurationEnregistrementAudio>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            if (config != null)
            {
                _logger.LogInformation(
                    "Configuration chargée depuis : {Chemin}",
                    _cheminFichierConfig
                );
            }

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erreur lors du chargement de la configuration depuis {Chemin}",
                _cheminFichierConfig
            );
            return null;
        }
    }

    private ConfigurationEnregistrementAudio CreerConfigurationParDefaut()
    {
        var cheminParDefaut = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "EnregistrementsRadio"
        );

        return new ConfigurationEnregistrementAudio
        {
            NomProjet = "Radio Occitania",
            CheminBaseStockage = cheminParDefaut,
            PrefixeNomFichier = "antenne",
            FormatSortie = "wav",
            FrequenceEchantillonnage = 44_100,
            ProfondeurBits = 16,
            NombreCanaux = 2,
            IndexPeripheriqueAudio = -1,
            DureeSegmentMinutes = 60,
            DureeConservationJours = 30,
            LancerAutomatiquementAuDemarrage = false,
            SeuilSilenceDb = -40.0,
            DureeMinSilenceSecondes = 30,
            PatronNommage = new ModeleNomFichier
            {
                Patron = "%prefix%_%date%_%heure%h%minute%"
            }
        };
    }

    private void OnConfigurationModifiee(ConfigurationEnregistrementAudio configuration)
    {
        try
        {
            ConfigurationModifiee?.Invoke(this, configuration);
            _logger.LogDebug("Événement ConfigurationModifiee déclenché");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du déclenchement de l'événement ConfigurationModifiee");
        }
    }
}
