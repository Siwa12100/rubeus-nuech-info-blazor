using System.Text.Json;
using NuitInfo.Rubeus.RadioOccitania.Modeles;
using NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace NuitInfo.Rubeus.RadioOccitania.Services.Implementations;

/// <summary>
/// Implémentation du service de gestion du stockage physique des enregistrements audio.
/// Gère les fichiers audio et un index JSON pour les métadonnées.
/// </summary>
public class StockageEnregistrementsService : IStockageEnregistrementsService
{
    private const string NomFichierIndex = "index-enregistrements.json";
    
    private readonly ILogger<StockageEnregistrementsService> _logger;
    private readonly IConfigurateurEnregistrementService _configurateur;
    private readonly JsonSerializerOptions _optionsJson;
    private readonly SemaphoreSlim _verrou = new(1, 1);

    public StockageEnregistrementsService(
        ILogger<StockageEnregistrementsService> logger,
        IConfigurateurEnregistrementService configurateur)
    {
        _logger = logger;
        _configurateur = configurateur;

        _optionsJson = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc />
    public string GenererCheminNouveauFichier(DateTime dateDebut)
    {
        var config = _configurateur.ObtenirConfiguration();
        var nomFichier = GenererNomFichier(dateDebut);
        return Path.Combine(config.CheminBaseStockage, nomFichier);
    }

    /// <inheritdoc />
    public string GenererNomFichier(DateTime dateDebut)
    {
        var config = _configurateur.ObtenirConfiguration();
        var nomSansExtension = config.PatronNommage.Generer(
            config.PrefixeNomFichier,
            dateDebut
        );
        return $"{nomSansExtension}.{config.FormatSortie}";
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EnregistrementAudio>> ListerEnregistrementsAsync()
    {
        await _verrou.WaitAsync();
        try
        {
            var config = _configurateur.ObtenirConfiguration();
            
            if (!Directory.Exists(config.CheminBaseStockage))
            {
                _logger.LogWarning("Répertoire de stockage inexistant: {Chemin}", config.CheminBaseStockage);
                return Enumerable.Empty<EnregistrementAudio>();
            }

            // Charger l'index des métadonnées
            var index = await ChargerIndexAsync();

            // Lister tous les fichiers audio
            var patterns = new[] { "*.wav", "*.mp3", "*.flac", "*.ogg" };
            var fichiers = patterns
                .SelectMany(pattern => Directory.GetFiles(config.CheminBaseStockage, pattern, SearchOption.TopDirectoryOnly))
                .ToList();

            var enregistrements = new List<EnregistrementAudio>();

            foreach (var cheminFichier in fichiers)
            {
                try
                {
                    var fichierInfo = new FileInfo(cheminFichier);
                    var nomFichier = fichierInfo.Name;

                    // Chercher dans l'index
                    var metadonnees = index.FirstOrDefault(e => e.NomFichier == nomFichier);

                    if (metadonnees != null)
                    {
                        // Mettre à jour avec les infos du fichier réel
                        metadonnees.CheminComplet = cheminFichier;
                        metadonnees.TailleFichierOctets = fichierInfo.Length;
                        enregistrements.Add(metadonnees);
                    }
                    else
                    {
                        // Créer une entrée basique si pas dans l'index
                        var enregistrement = CreerEnregistrementDepuisFichier(fichierInfo, config);
                        enregistrements.Add(enregistrement);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erreur lors du traitement du fichier {Fichier}", cheminFichier);
                }
            }

            _logger.LogInformation("Listage terminé: {Nombre} enregistrements trouvés", enregistrements.Count);
            return enregistrements;
        }
        finally
        {
            _verrou.Release();
        }
    }

    /// <summary>
    /// Récupère tous les enregistrements disponibles.
    /// </summary>
    public async Task<List<EnregistrementAudio>> ObtenirEnregistrementsAsync()
    {
        try
        {
            return await _context.Enregistrements
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de tous les enregistrements");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<EnregistrementAudio?> ObtenirEnregistrementAsync(Guid id)
    {
        var enregistrements = await ListerEnregistrementsAsync();
        return enregistrements.FirstOrDefault(e => e.Id == id);
    }

    /// <inheritdoc />
    public async Task<int> SupprimerEnregistrementsExpiresAsync()
    {
        await _verrou.WaitAsync();
        try
        {
            var enregistrements = await ListerEnregistrementsAsync();
            var maintenant = DateTime.Now;
            var expiresCount = 0;

            foreach (var enregistrement in enregistrements)
            {
                if (enregistrement.DateExpiration <= maintenant)
                {
                    try
                    {
                        if (File.Exists(enregistrement.CheminComplet))
                        {
                            File.Delete(enregistrement.CheminComplet);
                            _logger.LogInformation("Enregistrement expiré supprimé: {Nom}", enregistrement.NomFichier);
                            expiresCount++;
                        }

                        // Supprimer les fichiers associés (transcription, synthèse)
                        SupprimerFichiersAssocies(enregistrement);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors de la suppression de {Fichier}", enregistrement.NomFichier);
                    }
                }
            }

            if (expiresCount > 0)
            {
                // Mettre à jour l'index
                await MettreAJourIndexAsync();
                _logger.LogInformation("{Nombre} enregistrements expirés supprimés", expiresCount);
            }

            return expiresCount;
        }
        finally
        {
            _verrou.Release();
        }
    }

    /// <inheritdoc />
    public async Task<bool> SupprimerEnregistrementAsync(Guid id)
    {
        await _verrou.WaitAsync();
        try
        {
            var enregistrement = await ObtenirEnregistrementAsync(id);
            
            if (enregistrement == null)
            {
                _logger.LogWarning("Tentative de suppression d'un enregistrement inexistant: {Id}", id);
                return false;
            }

            try
            {
                // Supprimer le fichier audio
                if (File.Exists(enregistrement.CheminComplet))
                {
                    File.Delete(enregistrement.CheminComplet);
                }

                // Supprimer les fichiers associés
                SupprimerFichiersAssocies(enregistrement);

                // Mettre à jour l'index
                await MettreAJourIndexAsync();

                _logger.LogInformation("Enregistrement supprimé: {Nom} (ID: {Id})", enregistrement.NomFichier, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'enregistrement {Id}", id);
                throw;
            }
        }
        finally
        {
            _verrou.Release();
        }
    }

    /// <inheritdoc />
    public async Task SauvegarderMetadonneesAsync(EnregistrementAudio enregistrement)
    {
        await _verrou.WaitAsync();
        try
        {
            var index = await ChargerIndexAsync();

            // Retirer l'ancienne version si elle existe
            index.RemoveAll(e => e.Id == enregistrement.Id);

            // Ajouter la nouvelle version
            index.Add(enregistrement);

            // Sauvegarder
            await SauvegarderIndexAsync(index);

            _logger.LogDebug("Métadonnées sauvegardées pour {Nom}", enregistrement.NomFichier);
        }
        finally
        {
            _verrou.Release();
        }
    }

    /// <inheritdoc />
    public async Task<long> CalculerEspaceTotalUtiliseAsync()
    {
        var enregistrements = await ListerEnregistrementsAsync();
        return enregistrements.Sum(e => e.TailleFichierOctets);
    }

    /// <inheritdoc />
    public async Task VerifierEtCreerRepertoireStockageAsync()
    {
        var config = _configurateur.ObtenirConfiguration();
        var chemin = config.CheminBaseStockage;

        if (string.IsNullOrWhiteSpace(chemin))
        {
            throw new InvalidOperationException("Le chemin de stockage n'est pas configuré.");
        }

        try
        {
            if (!Directory.Exists(chemin))
            {
                Directory.CreateDirectory(chemin);
                _logger.LogInformation("Répertoire de stockage créé: {Chemin}", chemin);
            }

            // Vérifier les droits en écriture
            var fichierTest = Path.Combine(chemin, $".test_write_{Guid.NewGuid()}.tmp");
            await File.WriteAllTextAsync(fichierTest, "test");
            File.Delete(fichierTest);

            _logger.LogDebug("Répertoire de stockage accessible en écriture: {Chemin}", chemin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Impossible d'accéder au répertoire de stockage: {Chemin}", chemin);
            throw new IOException($"Le répertoire de stockage '{chemin}' n'est pas accessible en écriture.", ex);
        }
    }

    /// <summary>
    /// Charge l'index des métadonnées depuis le fichier JSON.
    /// </summary>
    private async Task<List<EnregistrementAudio>> ChargerIndexAsync()
    {
        var config = _configurateur.ObtenirConfiguration();
        var cheminIndex = Path.Combine(config.CheminBaseStockage, NomFichierIndex);

        if (!File.Exists(cheminIndex))
        {
            return new List<EnregistrementAudio>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(cheminIndex);
            var index = JsonSerializer.Deserialize<List<EnregistrementAudio>>(json, _optionsJson);
            return index ?? new List<EnregistrementAudio>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors du chargement de l'index, création d'un nouvel index");
            return new List<EnregistrementAudio>();
        }
    }

    /// <summary>
    /// Sauvegarde l'index des métadonnées sur disque.
    /// </summary>
    private async Task SauvegarderIndexAsync(List<EnregistrementAudio> index)
    {
        var config = _configurateur.ObtenirConfiguration();
        var cheminIndex = Path.Combine(config.CheminBaseStockage, NomFichierIndex);

        try
        {
            var json = JsonSerializer.Serialize(index, _optionsJson);
            await File.WriteAllTextAsync(cheminIndex, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la sauvegarde de l'index");
            throw;
        }
    }

    /// <summary>
    /// Met à jour l'index en supprimant les entrées sans fichier correspondant.
    /// </summary>
    private async Task MettreAJourIndexAsync()
    {
        var index = await ChargerIndexAsync();
        var indexNettoye = index.Where(e => File.Exists(e.CheminComplet)).ToList();
        
        if (indexNettoye.Count != index.Count)
        {
            await SauvegarderIndexAsync(indexNettoye);
            _logger.LogInformation("Index nettoyé: {Supprime} entrées obsolètes supprimées", 
                index.Count - indexNettoye.Count);
        }
    }

    /// <summary>
    /// Crée un objet EnregistrementAudio à partir d'un FileInfo.
    /// </summary>
    private EnregistrementAudio CreerEnregistrementDepuisFichier(FileInfo fichierInfo, ConfigurationEnregistrementAudio config)
    {
        var dateCreation = fichierInfo.CreationTime;
        var dureeConservation = TimeSpan.FromDays(config.DureeConservationJours);

        return new EnregistrementAudio
        {
            Id = Guid.NewGuid(),
            NomFichier = fichierInfo.Name,
            CheminComplet = fichierInfo.FullName,
            DateDebut = dateCreation,
            DateFin = dateCreation, // On ne connaît pas la vraie date de fin
            DateExpiration = dateCreation.Add(dureeConservation),
            TailleFichierOctets = fichierInfo.Length,
            StatutTranscription = StatutTraitementIA.NonDemarre,
            StatutSynthese = StatutTraitementIA.NonDemarre
        };
    }

    /// <summary>
    /// Supprime les fichiers associés à un enregistrement (transcription, synthèse).
    /// </summary>
    private void SupprimerFichiersAssocies(EnregistrementAudio enregistrement)
    {
        try
        {
            // Supprimer le fichier de transcription
            if (!string.IsNullOrEmpty(enregistrement.CheminTranscription) && 
                File.Exists(enregistrement.CheminTranscription))
            {
                File.Delete(enregistrement.CheminTranscription);
                _logger.LogDebug("Transcription supprimée: {Chemin}", enregistrement.CheminTranscription);
            }

            // Supprimer le fichier de synthèse
            if (!string.IsNullOrEmpty(enregistrement.CheminSynthese) && 
                File.Exists(enregistrement.CheminSynthese))
            {
                File.Delete(enregistrement.CheminSynthese);
                _logger.LogDebug("Synthèse supprimée: {Chemin}", enregistrement.CheminSynthese);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors de la suppression des fichiers associés");
        }
    }

    /// <summary>
    /// Obtient la liste de tous les enregistrements.
    /// </summary>
    public async Task<List<Enregistrement>> ObtenirEnregistrementsAsync()
    {
        try
        {
            return await _context.Enregistrements
                .OrderByDescending(e => e.DateCreation)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des enregistrements");
            return new List<Enregistrement>();
        }
    }

}
