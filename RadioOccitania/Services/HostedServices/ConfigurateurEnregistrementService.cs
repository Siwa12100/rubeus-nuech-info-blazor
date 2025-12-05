using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NuitInfo.Rubeus.RadioOccitania.Modeles;
using NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;

namespace NuitInfo.Rubeus.RadioOccitania.Services
{
    /// <summary>
    /// Implémentation du service de configuration de l'enregistrement audio.
    /// Gère la persistance, le cache et la validation de la configuration.
    /// </summary>
    public class ConfigurateurEnregistrementService : IConfigurateurEnregistrementService
    {
        private readonly ILogger<ConfigurateurEnregistrementService> _logger;
        private readonly string _cheminConfiguration;
        private ConfigurationEnregistrementAudio? _configurationCache;
        private readonly SemaphoreSlim _verrou = new(1, 1);

        public event EventHandler<ConfigurationEnregistrementAudio>? ConfigurationModifiee;

        public ConfigurateurEnregistrementService(ILogger<ConfigurateurEnregistrementService> logger)
        {
            _logger = logger;

            // Chemin du fichier de configuration
            var dossierAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dossierApp = Path.Combine(dossierAppData, "RadioOccitania");
            Directory.CreateDirectory(dossierApp);
            _cheminConfiguration = Path.Combine(dossierApp, "configuration-enregistrement.json");

            _logger.LogInformation("Configuration stockée dans : {Chemin}", _cheminConfiguration);
        }

        /// <summary>
        /// Obtient la configuration actuelle (depuis le cache ou le fichier).
        /// </summary>
        public ConfigurationEnregistrementAudio ObtenirConfiguration()
        {
            if (_configurationCache != null)
            {
                return _configurationCache;
            }

            try
            {
                if (File.Exists(_cheminConfiguration))
                {
                    var json = File.ReadAllText(_cheminConfiguration);
                    _configurationCache = JsonSerializer.Deserialize<ConfigurationEnregistrementAudio>(json);

                    if (_configurationCache != null)
                    {
                        _logger.LogInformation("Configuration chargée depuis le fichier");
                        return _configurationCache;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la configuration");
            }

            // Configuration par défaut
            _configurationCache = CreerConfigurationParDefaut();
            _logger.LogInformation("Configuration par défaut créée");
            return _configurationCache;
        }

        /// <summary>
        /// Met à jour la configuration et la sauvegarde.
        /// </summary>
        public async Task MettreAJourConfigurationAsync(ConfigurationEnregistrementAudio configuration)
        {
            await _verrou.WaitAsync();
            try
            {
                // Valider avant de sauvegarder
                var erreurs = configuration.Valider();
                if (erreurs.Any())
                {
                    var message = string.Join(", ", erreurs);
                    _logger.LogWarning("Configuration invalide : {Erreurs}", message);
                    throw new InvalidOperationException($"Configuration invalide : {message}");
                }

                // Sauvegarder dans le fichier
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };

                var json = JsonSerializer.Serialize(configuration, options);
                await File.WriteAllTextAsync(_cheminConfiguration, json);

                // Mettre à jour le cache
                _configurationCache = configuration;

                _logger.LogInformation("Configuration mise à jour : {Resume}", configuration.ObtenirResume());

                // Déclencher l'événement
                ConfigurationModifiee?.Invoke(this, configuration);
            }
            finally
            {
                _verrou.Release();
            }
        }

        /// <summary>
        /// Recharge la configuration depuis le fichier.
        /// </summary>
        public async Task<ConfigurationEnregistrementAudio> RechargerConfigurationAsync()
        {
            await _verrou.WaitAsync();
            try
            {
                _configurationCache = null;
                return ObtenirConfiguration();
            }
            finally
            {
                _verrou.Release();
            }
        }

        /// <summary>
        /// Valide la configuration actuelle.
        /// </summary>
        public List<string> ValiderConfiguration()
        {
            var config = ObtenirConfiguration();
            return config.Valider();
        }

        /// <summary>
        /// Valide une configuration spécifique.
        /// </summary>
        public List<string> ValiderConfiguration(ConfigurationEnregistrementAudio configuration)
        {
            return configuration.Valider();
        }

        /// <summary>
        /// Réinitialise la configuration aux valeurs par défaut.
        /// </summary>
        public async Task ReinitialiserConfigurationAsync()
        {
            _logger.LogInformation("Réinitialisation de la configuration");
            var configDefaut = CreerConfigurationParDefaut();
            await MettreAJourConfigurationAsync(configDefaut);
        }

        /// <summary>
        /// Obtient la liste des périphériques audio disponibles.
        /// </summary>
        public List<PeripheriqueAudio> ObtenirPeripheriquesDisponibles()
        {
            var peripheriques = new List<PeripheriqueAudio>();

            try
            {
                // Périphérique par défaut
                peripheriques.Add(new PeripheriqueAudio
                {
                    Index = -1,
                    Nom = "Périphérique par défaut du système",
                    NombreCanaux = 2,
                    EstParDefaut = true
                });

                // Lister tous les périphériques disponibles
                for (int i = 0; i < WaveInEvent.DeviceCount; i++)
                {
                    var capacites = WaveInEvent.GetCapabilities(i);
                    peripheriques.Add(new PeripheriqueAudio
                    {
                        Index = i,
                        Nom = capacites.ProductName,
                        NombreCanaux = capacites.Channels,
                        EstParDefaut = false
                    });
                }

                _logger.LogInformation("Détectés {Count} périphériques audio", peripheriques.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des périphériques audio");
            }

            return peripheriques;
        }

        /// <summary>
        /// Crée une configuration par défaut.
        /// </summary>
        private ConfigurationEnregistrementAudio CreerConfigurationParDefaut()
        {
            var dossierDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dossierEnregistrements = Path.Combine(dossierDocuments, "RadioOccitania", "Enregistrements");

            return new ConfigurationEnregistrementAudio
            {
                NomProjet = "Radio Occitania",
                CheminBaseStockage = dossierEnregistrements,
                PrefixeNomFichier = "antenne",
                FormatSortie = "wav",
                FrequenceEchantillonnage = 44_100,
                ProfondeurBits = 16,
                NombreCanaux = 2,
                IndexPeripheriqueAudio = -1,
                DureeSegmentMinutes = 60,
                DureeConservationJours = 30,
                ActiverDecoupageAutomatique = true,

                // Silence
                ActiverDetectionSilence = false,
                SeuilSilenceDb = -40.0,
                DureeMinSilenceSecondes = 5,

                LancerAutomatiquementAuDemarrage = false,

                // Modèle de nommage
                PatronNommage = new ModeleNomFichier
                {
                    Patron = "%prefix%_%date%_%heure%h%minute%"
                }
            };
        }
    }
}
