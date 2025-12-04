using System.Diagnostics;
using NAudio.Wave;
using Microsoft.Extensions.Logging;
using NuitInfo.Rubeus.RadioOccitania.Modeles;
using NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;

namespace NuitInfo.Rubeus.RadioOccitania.Services.Implementations;

/// <summary>
/// Service de capture audio utilisant NAudio.
/// Gère l'enregistrement, la pause/reprise et la rotation des fichiers.
/// </summary>
public class EnregistreurAudioService : IEnregistreurAudioService, IDisposable
{
    private readonly ILogger<EnregistreurAudioService> _logger;
    private readonly IConfigurateurEnregistrementService _configurateur;
    private readonly IStockageEnregistrementsService _stockage;
    
    private WaveInEvent? _captureAudio;
    private WaveFileWriter? _fichierSortie;
    private EnregistrementAudio? _enregistrementActuel;
    private DateTime _dateDebutEnregistrement;
    private bool _estEnPause;
    private readonly object _verrou = new();
    
    // Statistiques audio
    private double _dernierNiveauDb;
    private DateTime _debutSilenceActuel;
    private const double SeuilSilenceDb = -40.0; // Seuil de détection de silence

    public EnregistreurAudioService(
        ILogger<EnregistreurAudioService> logger,
        IConfigurateurEnregistrementService configurateur,
        IStockageEnregistrementsService stockage)
    {
        _logger = logger;
        _configurateur = configurateur;
        _stockage = stockage;
    }

    #region Propriétés publiques

    public bool EstEnCours
    {
        get
        {
            lock (_verrou)
            {
                return _captureAudio != null && _fichierSortie != null && !_estEnPause;
            }
        }
    }

    public EnregistrementAudio? EnregistrementActuel
    {
        get
        {
            lock (_verrou)
            {
                return _enregistrementActuel;
            }
        }
    }

    public TimeSpan DureeActuelle
    {
        get
        {
            lock (_verrou)
            {
                if (_enregistrementActuel == null)
                    return TimeSpan.Zero;
                
                return DateTime.Now - _dateDebutEnregistrement;
            }
        }
    }

    #endregion

    #region Événements

    public event EventHandler<EnregistrementAudio>? EnregistrementDemarre;
    public event EventHandler<EnregistrementAudio>? EnregistrementArrete;
    public event EventHandler<Exception>? ErreurEnregistrement;

    #endregion

    #region Démarrage/Arrêt

    public async Task<EnregistrementAudio> DemarrerEnregistrementAsync()
    {
        lock (_verrou)
        {
            if (EstEnCours)
            {
                throw new InvalidOperationException("Un enregistrement est déjà en cours.");
            }
        }

        try
        {
            var config = _configurateur.ObtenirConfiguration();
            
            // Vérifier que le répertoire existe
            await _stockage.VerifierEtCreerRepertoireStockageAsync();

            // Générer le chemin du nouveau fichier
            var dateDebut = DateTime.Now;
            var cheminFichier = _stockage.GenererCheminNouveauFichier(dateDebut);
            
            _logger.LogInformation("Démarrage de l'enregistrement vers : {Chemin}", cheminFichier);

            // Créer les métadonnées
            var dateExpiration = dateDebut.AddDays(config.DureeConservationJours);
            var enregistrement = new EnregistrementAudio
            {
                CheminFichier = cheminFichier,
                NomFichier = Path.GetFileName(cheminFichier),
                DateDebut = dateDebut,
                DateExpiration = dateExpiration,
                DateFin = null // Toujours en cours
            };

            // Initialiser la capture audio
            lock (_verrou)
            {
                _captureAudio = new WaveInEvent
                {
                    DeviceNumber = config.IndexPeripheriqueAudio == -1 
                        ? 0  // Utiliser le premier périphérique si -1
                        : config.IndexPeripheriqueAudio,
                    WaveFormat = new WaveFormat(
                        rate: config.FrequenceEchantillonnage,
                        bits: config.ProfondeurBits,
                        channels: config.NombreCanaux
                    ),
                    BufferMilliseconds = 100
                };

                // Créer le fichier de sortie
                _fichierSortie = new WaveFileWriter(cheminFichier, _captureAudio.WaveFormat);

                // Connecter les événements
                _captureAudio.DataAvailable += OnDataAvailable;
                _captureAudio.RecordingStopped += OnRecordingStopped;

                // Démarrer la capture
                _captureAudio.StartRecording();

                _enregistrementActuel = enregistrement;
                _dateDebutEnregistrement = dateDebut;
                _estEnPause = false;
            }

            // Sauvegarder les métadonnées
            await _stockage.SauvegarderMetadonneesAsync(enregistrement);

            _logger.LogInformation(
                "Enregistrement démarré : {Id} - {NomFichier}", 
                enregistrement.Id, 
                enregistrement.NomFichier
            );

            // Déclencher l'événement
            EnregistrementDemarre?.Invoke(this, enregistrement);

            return enregistrement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du démarrage de l'enregistrement");
            ErreurEnregistrement?.Invoke(this, ex);
            throw;
        }
    }

    public async Task<EnregistrementAudio> ArreterEnregistrementAsync()
    {
        EnregistrementAudio? enregistrement;
        
        lock (_verrou)
        {
            if (!EstEnCours && _enregistrementActuel == null)
            {
                throw new InvalidOperationException("Aucun enregistrement en cours.");
            }

            enregistrement = _enregistrementActuel;
            
            // Arrêter la capture
            _captureAudio?.StopRecording();
        }

        // Attendre un court instant pour que tous les buffers soient vidés
        await Task.Delay(200);

        lock (_verrou)
        {
            // Fermer les ressources
            _fichierSortie?.Dispose();
            _fichierSortie = null;

            if (_captureAudio != null)
            {
                _captureAudio.DataAvailable -= OnDataAvailable;
                _captureAudio.RecordingStopped -= OnRecordingStopped;
                _captureAudio.Dispose();
                _captureAudio = null;
            }

            // Finaliser les métadonnées
            if (enregistrement != null)
            {
                enregistrement.DateFin = DateTime.Now;
                
                // Obtenir la taille du fichier
                if (File.Exists(enregistrement.CheminFichier))
                {
                    var infoFichier = new FileInfo(enregistrement.CheminFichier);
                    enregistrement.TailleOctets = infoFichier.Length;
                }
            }

            _enregistrementActuel = null;
            _estEnPause = false;
        }

        // Sauvegarder les métadonnées finales
        if (enregistrement != null)
        {
            await _stockage.SauvegarderMetadonneesAsync(enregistrement);

            _logger.LogInformation(
                "Enregistrement arrêté : {Id} - Durée: {Duree:hh\\:mm\\:ss} - Taille: {Taille}",
                enregistrement.Id,
                enregistrement.Duree,
                enregistrement.TailleFormatee
            );

            // Déclencher l'événement
            EnregistrementArrete?.Invoke(this, enregistrement);
        }

        return enregistrement!;
    }

    public async Task<EnregistrementAudio> RotationnerFichierAsync()
    {
        _logger.LogInformation("Rotation du fichier d'enregistrement demandée");

        // Arrêter l'enregistrement actuel
        var ancienEnregistrement = await ArreterEnregistrementAsync();

        // Démarrer un nouveau segment
        var nouveauEnregistrement = await DemarrerEnregistrementAsync();

        _logger.LogInformation(
            "Rotation effectuée : {AncienId} -> {NouveauId}",
            ancienEnregistrement.Id,
            nouveauEnregistrement.Id
        );

        return nouveauEnregistrement;
    }

    #endregion

    #region Pause/Reprise

    public Task PauseAsync()
    {
        lock (_verrou)
        {
            if (!EstEnCours)
            {
                throw new InvalidOperationException("Aucun enregistrement en cours.");
            }

            if (_estEnPause)
            {
                throw new InvalidOperationException("L'enregistrement est déjà en pause.");
            }

            _captureAudio?.StopRecording();
            _estEnPause = true;

            _logger.LogInformation("Enregistrement mis en pause : {Id}", _enregistrementActuel?.Id);
        }

        return Task.CompletedTask;
    }

    public Task ReprendreAsync()
    {
        lock (_verrou)
        {
            if (_captureAudio == null || !_estEnPause)
            {
                throw new InvalidOperationException("Aucun enregistrement en pause.");
            }

            _captureAudio.StartRecording();
            _estEnPause = false;

            _logger.LogInformation("Enregistrement repris : {Id}", _enregistrementActuel?.Id);
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Statistiques

    public StatistiquesAudio? ObtenirStatistiquesActuelles()
    {
        lock (_verrou)
        {
            if (!EstEnCours || _enregistrementActuel == null)
                return null;

            var estSilence = _dernierNiveauDb < SeuilSilenceDb;
            var dureeSilence = estSilence 
                ? DateTime.Now - _debutSilenceActuel 
                : TimeSpan.Zero;

            long tailleFichier = 0;
            if (File.Exists(_enregistrementActuel.CheminFichier))
            {
                tailleFichier = new FileInfo(_enregistrementActuel.CheminFichier).Length;
            }

            return new StatistiquesAudio
            {
                NiveauDb = _dernierNiveauDb,
                NiveauNormalise = ConvertirDbEnNormalise(_dernierNiveauDb),
                EstSilence = estSilence,
                DureeSilenceActuel = dureeSilence,
                TailleFichier = tailleFichier
            };
        }
    }

    private double ConvertirDbEnNormalise(double db)
    {
        // Convertir de dB vers échelle 0-1
        // On considère -60dB = 0.0 et 0dB = 1.0
        const double dbMin = -60.0;
        const double dbMax = 0.0;
        
        var normalise = (db - dbMin) / (dbMax - dbMin);
        return Math.Clamp(normalise, 0.0, 1.0);
    }

    #endregion

    #region Gestion des événements NAudio

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        try
        {
            lock (_verrou)
            {
                if (_fichierSortie != null && !_estEnPause)
                {
                    // Écrire les données audio
                    _fichierSortie.Write(e.Buffer, 0, e.BytesRecorded);

                    // Calculer le niveau audio pour les statistiques
                    _dernierNiveauDb = CalculerNiveauDb(e.Buffer, e.BytesRecorded);

                    // Détecter le début/fin de silence
                    var estSilence = _dernierNiveauDb < SeuilSilenceDb;
                    if (estSilence && _debutSilenceActuel == DateTime.MinValue)
                    {
                        _debutSilenceActuel = DateTime.Now;
                    }
                    else if (!estSilence)
                    {
                        _debutSilenceActuel = DateTime.MinValue;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'écriture des données audio");
            ErreurEnregistrement?.Invoke(this, ex);
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            _logger.LogError(e.Exception, "Enregistrement arrêté avec une erreur");
            ErreurEnregistrement?.Invoke(this, e.Exception);
        }
    }

    private double CalculerNiveauDb(byte[] buffer, int bytesRecorded)
    {
        // Calculer le niveau RMS (Root Mean Square)
        double sommeCarres = 0;
        int nombreEchantillons = bytesRecorded / 2; // 16-bit audio = 2 bytes par échantillon

        for (int i = 0; i < bytesRecorded - 1; i += 2)
        {
            short echantillon = BitConverter.ToInt16(buffer, i);
            double normalise = echantillon / 32768.0; // Normaliser vers -1.0 à 1.0
            sommeCarres += normalise * normalise;
        }

        double rms = Math.Sqrt(sommeCarres / nombreEchantillons);
        
        // Convertir en dB
        if (rms > 0)
        {
            return 20 * Math.Log10(rms);
        }
        
        return -60.0; // Silence
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        lock (_verrou)
        {
            _captureAudio?.Dispose();
            _fichierSortie?.Dispose();
        }
    }

    #endregion
}
