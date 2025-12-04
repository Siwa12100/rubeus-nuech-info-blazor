using NuitInfo.Rubeus.RadioOccitania.Modeles;

namespace NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;

/// <summary>
/// Service de transcription audio vers texte (Speech-to-Text).
/// Encapsule l'appel à un moteur STT et la gestion de la transcription.
/// </summary>
public interface ITranscriptionService
{
    /// <summary>
    /// Transcrit un enregistrement audio complet en texte.
    /// </summary>
    /// <param name="enregistrement">Enregistrement à transcrire.</param>
    /// <returns>Texte transcrit.</returns>
    /// <exception cref="InvalidOperationException">Si l'enregistrement n'est pas finalisé.</exception>
    Task<string> TranscrireAsync(EnregistrementAudio enregistrement);

    /// <summary>
    /// Transcrit un segment spécifique d'un enregistrement.
    /// </summary>
    /// <param name="enregistrement">Enregistrement source.</param>
    /// <param name="debut">Position de début du segment.</param>
    /// <param name="duree">Durée du segment à transcrire.</param>
    /// <returns>Texte transcrit du segment.</returns>
    Task<string> TranscrireSegmentAsync(EnregistrementAudio enregistrement, TimeSpan debut, TimeSpan duree);

    /// <summary>
    /// Transcrit et sauvegarde la transcription dans un fichier associé.
    /// Met à jour les métadonnées de l'enregistrement.
    /// </summary>
    /// <param name="enregistrement">Enregistrement à transcrire.</param>
    /// <returns>Chemin du fichier de transcription créé.</returns>
    Task<string> TranscrireEtSauvegarderAsync(EnregistrementAudio enregistrement);

    /// <summary>
    /// Obtient le statut actuel d'une transcription en cours.
    /// </summary>
    /// <param name="enregistrement">Enregistrement dont on veut le statut.</param>
    /// <returns>Progression de la transcription (0.0 à 1.0), ou null si pas en cours.</returns>
    Task<double?> ObtenirProgressionAsync(EnregistrementAudio enregistrement);

    /// <summary>
    /// Annule une transcription en cours.
    /// </summary>
    /// <param name="enregistrement">Enregistrement dont on veut annuler la transcription.</param>
    Task AnnulerTranscriptionAsync(EnregistrementAudio enregistrement);

    /// <summary>
    /// Événement déclenché lorsqu'une transcription démarre.
    /// </summary>
    event EventHandler<EnregistrementAudio>? TranscriptionDemarree;

    /// <summary>
    /// Événement déclenché lorsqu'une transcription se termine.
    /// </summary>
    event EventHandler<(EnregistrementAudio enregistrement, string transcription)>? TranscriptionTerminee;

    /// <summary>
    /// Événement déclenché lors d'une erreur de transcription.
    /// </summary>
    event EventHandler<(EnregistrementAudio enregistrement, Exception erreur)>? ErreurTranscription;
}
