using NuitInfo.Rubeus.RadioOccitania.Modeles;

namespace NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;

/// <summary>
/// Service de synthèse et résumé de texte par IA.
/// Génère des résumés à partir de transcriptions d'enregistrements.
/// </summary>
public interface ISyntheseService
{
    /// <summary>
    /// Génère un résumé à partir d'une transcription.
    /// </summary>
    /// <param name="transcription">Texte transcrit à résumer.</param>
    /// <returns>Résumé généré.</returns>
    Task<string> GenererSyntheseAsync(string transcription);

    /// <summary>
    /// Génère un résumé complet pour un enregistrement (avec transcription préalable si nécessaire).
    /// Met à jour les métadonnées de l'enregistrement.
    /// </summary>
    /// <param name="enregistrement">Enregistrement à résumer.</param>
    /// <returns>Résumé généré.</returns>
    Task<string> GenererSyntheseEnregistrementAsync(EnregistrementAudio enregistrement);

    /// <summary>
    /// Génère une synthèse structurée avec différentes sections.
    /// </summary>
    /// <param name="transcription">Texte transcrit.</param>
    /// <returns>Synthèse structurée.</returns>
    Task<SyntheseStructuree> GenererSyntheseStructureeAsync(string transcription);

    /// <summary>
    /// Extrait les points clés d'une transcription.
    /// </summary>
    /// <param name="transcription">Texte transcrit.</param>
    /// <returns>Liste des points clés identifiés.</returns>
    Task<List<string>> ExtrairePointsClesAsync(string transcription);

    /// <summary>
    /// Détecte les sujets principaux abordés dans la transcription.
    /// </summary>
    /// <param name="transcription">Texte transcrit.</param>
    /// <returns>Liste des sujets détectés avec leur importance.</returns>
    Task<List<(string sujet, double importance)>> DetecterSujetsAsync(string transcription);

    /// <summary>
    /// Événement déclenché lorsqu'une synthèse démarre.
    /// </summary>
    event EventHandler<EnregistrementAudio>? SyntheseDemarree;

    /// <summary>
    /// Événement déclenché lorsqu'une synthèse se termine.
    /// </summary>
    event EventHandler<(EnregistrementAudio enregistrement, string synthese)>? SyntheseTerminee;

    /// <summary>
    /// Événement déclenché lors d'une erreur de synthèse.
    /// </summary>
    event EventHandler<(EnregistrementAudio enregistrement, Exception erreur)>? ErreurSynthese;
}

/// <summary>
/// Représente une synthèse structurée avec différentes sections.
/// </summary>
public class SyntheseStructuree
{
    /// <summary>
    /// Résumé global en quelques phrases.
    /// </summary>
    public string ResumeGlobal { get; set; } = string.Empty;

    /// <summary>
    /// Points clés extraits.
    /// </summary>
    public List<string> PointsCles { get; set; } = new();

    /// <summary>
    /// Sujets principaux abordés.
    /// </summary>
    public List<string> SujetsPrincipaux { get; set; } = new();

    /// <summary>
    /// Décisions ou actions identifiées (si pertinent).
    /// </summary>
    public List<string> Decisions { get; set; } = new();

    /// <summary>
    /// Horodatage des moments importants.
    /// </summary>
    public List<(TimeSpan timestamp, string description)> MomentsImportants { get; set; } = new();

    /// <summary>
    /// Sentiment général détecté (positif, négatif, neutre).
    /// </summary>
    public string? SentimentGeneral { get; set; }
}
