using NuitInfo.Rubeus.RadioOccitania.Modeles;

namespace NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;

/// <summary>
/// Service de détection et analyse des silences dans les enregistrements audio.
/// Permet de détecter les "blancs" anormaux (panne technique, perte de signal).
/// </summary>
public interface IAnalyseSilencesService
{
    /// <summary>
    /// Vérifie si un enregistrement contient un blanc suspect (silence anormalement long).
    /// </summary>
    /// <param name="enregistrement">Enregistrement à analyser.</param>
    /// <returns>True si un blanc suspect est détecté.</returns>
    Task<bool> ContientBlancSuspectAsync(EnregistrementAudio enregistrement);

    /// <summary>
    /// Détecte tous les segments de silence dans un enregistrement.
    /// Version avancée qui retourne les timestamps et caractéristiques de chaque silence.
    /// </summary>
    /// <param name="enregistrement">Enregistrement à analyser.</param>
    /// <returns>Liste des segments de silence détectés.</returns>
    Task<IReadOnlyList<SegmentSilence>> DetecterSilencesAsync(EnregistrementAudio enregistrement);

    /// <summary>
    /// Analyse un segment audio en temps réel pendant l'enregistrement.
    /// </summary>
    /// <param name="buffer">Buffer audio à analyser.</param>
    /// <param name="bytesEnregistres">Nombre d'octets dans le buffer.</param>
    /// <returns>Niveau audio en dB et indicateur de silence.</returns>
    (double niveauDb, bool estSilence) AnalyserSegmentTempsReel(byte[] buffer, int bytesEnregistres);

    /// <summary>
    /// Calcule les statistiques globales de silence pour un enregistrement.
    /// </summary>
    /// <param name="enregistrement">Enregistrement à analyser.</param>
    /// <returns>Statistiques de silence.</returns>
    Task<StatistiquesSilence> CalculerStatistiquesSilenceAsync(EnregistrementAudio enregistrement);
}

/// <summary>
/// Représente un segment de silence détecté dans un enregistrement.
/// </summary>
public class SegmentSilence
{
    /// <summary>
    /// Position de début du silence (offset depuis le début du fichier).
    /// </summary>
    public TimeSpan Debut { get; set; }

    /// <summary>
    /// Position de fin du silence.
    /// </summary>
    public TimeSpan Fin { get; set; }

    /// <summary>
    /// Durée totale du silence.
    /// </summary>
    public TimeSpan Duree => Fin - Debut;

    /// <summary>
    /// Niveau audio moyen pendant ce segment (en dB).
    /// </summary>
    public double NiveauMoyenDb { get; set; }

    /// <summary>
    /// Classification du silence (naturel, suspect, critique).
    /// </summary>
    public TypeSilence Type { get; set; }

    /// <summary>
    /// Confiance de la détection (0.0 à 1.0).
    /// </summary>
    public double Confiance { get; set; }
}

/// <summary>
/// Type de silence détecté.
/// </summary>
public enum TypeSilence
{
    /// <summary>
    /// Silence naturel (pause normale, respiration, etc.).
    /// </summary>
    Naturel,

    /// <summary>
    /// Silence suspect (durée anormale mais pas critique).
    /// </summary>
    Suspect,

    /// <summary>
    /// Silence critique (très long, probablement une panne).
    /// </summary>
    Critique
}

/// <summary>
/// Statistiques globales de silence pour un enregistrement.
/// </summary>
public class StatistiquesSilence
{
    /// <summary>
    /// Nombre total de segments de silence détectés.
    /// </summary>
    public int NombreSegments { get; set; }

    /// <summary>
    /// Durée totale de silence.
    /// </summary>
    public TimeSpan DureeTotale { get; set; }

    /// <summary>
    /// Pourcentage de silence par rapport à la durée totale.
    /// </summary>
    public double PourcentageSilence { get; set; }

    /// <summary>
    /// Plus long segment de silence.
    /// </summary>
    public SegmentSilence? PlusLongSegment { get; set; }

    /// <summary>
    /// Nombre de silences suspects.
    /// </summary>
    public int NombreSilencesSuspects { get; set; }

    /// <summary>
    /// Nombre de silences critiques.
    /// </summary>
    public int NombreSilencesCritiques { get; set; }
}
