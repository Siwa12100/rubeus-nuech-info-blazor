using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NuitInfo.Rubeus.RadioOccitania.Modeles;
using NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;

namespace NuitInfo.Rubeus.RadioOccitania.Services.Implementations;

/// <summary>
/// Implémentation stub du service d'analyse des silences.
/// Version simplifiée qui simule la détection de silences sans IA réelle.
/// </summary>
public class AnalyseSilencesServiceStub : IAnalyseSilencesService
{
    private readonly ILogger<AnalyseSilencesServiceStub> _logger;
    private readonly IConfigurateurEnregistrementService _configurateur;

    // Seuils par défaut (peuvent être overridés par la config)
    private const double SEUIL_SILENCE_DB_DEFAUT = -40.0;
    private const int DUREE_MIN_SILENCE_SECONDES_DEFAUT = 30;
    private const int DUREE_CRITIQUE_SILENCE_SECONDES = 120; // 2 minutes

    public AnalyseSilencesServiceStub(
        ILogger<AnalyseSilencesServiceStub> logger,
        IConfigurateurEnregistrementService configurateur)
    {
        _logger = logger;
        _configurateur = configurateur;
    }

    public async Task<bool> ContientBlancSuspectAsync(EnregistrementAudio enregistrement)
    {
        ArgumentNullException.ThrowIfNull(enregistrement);

        _logger.LogInformation(
            "[STUB] Analyse de blanc suspect pour : {Fichier}",
            enregistrement.NomFichier
        );

        // Simulation : on considère qu'un enregistrement a un blanc suspect si :
        // - La durée est anormalement courte (< 5 minutes)
        // - Ou la taille du fichier est très petite pour la durée

        await Task.Delay(100); // Simule un traitement

        var config = _configurateur.ObtenirConfiguration();
        var dureeMinimale = TimeSpan.FromMinutes(5);

        bool blancSuspect = enregistrement.Duree < dureeMinimale;

        if (blancSuspect)
        {
            _logger.LogWarning(
                "[STUB] ⚠️ Blanc suspect détecté : durée {Duree} < {DureeMin}",
                enregistrement.Duree,
                dureeMinimale
            );
        }
        else
        {
            _logger.LogInformation(
                "[STUB] ✅ Pas de blanc suspect détecté (durée : {Duree})",
                enregistrement.Duree
            );
        }

        return blancSuspect;
    }

    public async Task<IReadOnlyList<SegmentSilence>> DetecterSilencesAsync(EnregistrementAudio enregistrement)
    {
        ArgumentNullException.ThrowIfNull(enregistrement);

        _logger.LogInformation(
            "[STUB] Détection des silences pour : {Fichier}",
            enregistrement.NomFichier
        );

        if (!File.Exists(enregistrement.CheminComplet))
        {
            _logger.LogWarning(
                "[STUB] Fichier introuvable : {Chemin}",
                enregistrement.CheminComplet
            );
            return Array.Empty<SegmentSilence>();
        }

        await Task.Delay(200); // Simule un traitement plus long

        var config = _configurateur.ObtenirConfiguration();
        var segments = new List<SegmentSilence>();

        // Simulation : on crée quelques segments de silence factices
        // basés sur la durée de l'enregistrement

        var dureeEnregistrement = enregistrement.Duree;
        var nombreSegmentsSimules = (int)(dureeEnregistrement.TotalMinutes / 15); // 1 segment toutes les 15 min

        for (int i = 0; i < nombreSegmentsSimules; i++)
        {
            var debut = TimeSpan.FromMinutes(i * 15 + 5); // Silence à +5 min de chaque tranche
            var dureeSilence = TimeSpan.FromSeconds(Random.Shared.Next(10, 45));
            var fin = debut + dureeSilence;

            var type = dureeSilence.TotalSeconds switch
            {
                < DUREE_MIN_SILENCE_SECONDES_DEFAUT => TypeSilence.Naturel,
                < DUREE_CRITIQUE_SILENCE_SECONDES => TypeSilence.Suspect,
                _ => TypeSilence.Critique
            };

            segments.Add(new SegmentSilence
            {
                Debut = debut,
                Fin = fin,
                NiveauMoyenDb = Random.Shared.NextDouble() * -50 - 40, // Entre -40 et -90 dB
                Type = type,
                Confiance = Random.Shared.NextDouble() * 0.3 + 0.7 // Entre 0.7 et 1.0
            });
        }

        _logger.LogInformation(
            "[STUB] {Count} segment(s) de silence détecté(s)",
            segments.Count
        );

        foreach (var segment in segments)
        {
            _logger.LogDebug(
                "[STUB] Silence {Type} : {Debut} → {Fin} ({Duree:F1}s, {Niveau:F1} dB)",
                segment.Type,
                segment.Debut,
                segment.Fin,
                segment.Duree.TotalSeconds,
                segment.NiveauMoyenDb
            );
        }

        return segments.AsReadOnly();
    }

    public (double niveauDb, bool estSilence) AnalyserSegmentTempsReel(byte[] buffer, int bytesEnregistres)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (bytesEnregistres <= 0)
        {
            return (-90.0, true); // Silence absolu si pas de données
        }

        var config = _configurateur.ObtenirConfiguration();
        var seuilSilence = config.SeuilSilenceDb;

        // Calcul du niveau RMS simplifié (16-bit samples)
        double sommeCarres = 0;
        int nombreEchantillons = bytesEnregistres / 2; // 16-bit = 2 bytes par échantillon

        for (int i = 0; i < bytesEnregistres - 1; i += 2)
        {
            short echantillon = (short)((buffer[i + 1] << 8) | buffer[i]);
            double normalise = echantillon / 32768.0; // Normalisation -1.0 à 1.0
            sommeCarres += normalise * normalise;
        }

        double rms = Math.Sqrt(sommeCarres / nombreEchantillons);
        
        // Conversion en dB (avec protection contre log(0))
        double niveauDb = rms > 0.0001 
            ? 20 * Math.Log10(rms) 
            : -90.0;

        bool estSilence = niveauDb < seuilSilence;

        // Log uniquement si changement de statut (évite le spam)
        // Note : dans un vrai service, on utiliserait un état pour tracker les changements

        return (niveauDb, estSilence);
    }

    public async Task<StatistiquesSilence> CalculerStatistiquesSilenceAsync(EnregistrementAudio enregistrement)
    {
        ArgumentNullException.ThrowIfNull(enregistrement);

        _logger.LogInformation(
            "[STUB] Calcul des statistiques de silence pour : {Fichier}",
            enregistrement.NomFichier
        );

        // Réutilise la détection de silences
        var segments = await DetecterSilencesAsync(enregistrement);

        var dureeTotale = TimeSpan.FromSeconds(
            segments.Sum(s => s.Duree.TotalSeconds)
        );

        var pourcentage = enregistrement.Duree.TotalSeconds > 0
            ? (dureeTotale.TotalSeconds / enregistrement.Duree.TotalSeconds) * 100
            : 0;

        var plusLongSegment = segments
            .OrderByDescending(s => s.Duree)
            .FirstOrDefault();

        var stats = new StatistiquesSilence
        {
            NombreSegments = segments.Count,
            DureeTotale = dureeTotale,
            PourcentageSilence = pourcentage,
            PlusLongSegment = plusLongSegment,
            NombreSilencesSuspects = segments.Count(s => s.Type == TypeSilence.Suspect),
            NombreSilencesCritiques = segments.Count(s => s.Type == TypeSilence.Critique)
        };

        _logger.LogInformation(
            "[STUB] Statistiques : {Segments} segments, {Duree} total ({Pourcentage:F1}%), " +
            "{Suspects} suspects, {Critiques} critiques",
            stats.NombreSegments,
            stats.DureeTotale,
            stats.PourcentageSilence,
            stats.NombreSilencesSuspects,
            stats.NombreSilencesCritiques
        );

        if (plusLongSegment != null)
        {
            _logger.LogInformation(
                "[STUB] Plus long silence : {Duree:F1}s ({Type})",
                plusLongSegment.Duree.TotalSeconds,
                plusLongSegment.Type
            );
        }

        return stats;
    }
}
