using NuitInfo.Rubeus.RadioOccitania.Modeles;

namespace NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;

/// <summary>
/// Service principal de capture audio.
/// Responsable du démarrage/arrêt de l'enregistrement et de la gestion du flux audio.
/// </summary>
public interface IEnregistreurAudioService
{
    /// <summary>
    /// Indique si un enregistrement est actuellement en cours.
    /// </summary>
    bool EstEnCours { get; }

    /// <summary>
    /// Métadonnées de l'enregistrement actuellement en cours (null si aucun).
    /// </summary>
    EnregistrementAudio? EnregistrementActuel { get; }

    /// <summary>
    /// Durée écoulée depuis le début de l'enregistrement actuel.
    /// </summary>
    TimeSpan DureeActuelle { get; }

    /// <summary>
    /// Démarre un nouvel enregistrement audio.
    /// </summary>
    /// <exception cref="InvalidOperationException">Si un enregistrement est déjà en cours.</exception>
    /// <returns>Métadonnées de l'enregistrement démarré.</returns>
    Task<EnregistrementAudio> DemarrerEnregistrementAsync();

    /// <summary>
    /// Arrête l'enregistrement en cours et finalise le fichier.
    /// </summary>
    /// <exception cref="InvalidOperationException">Si aucun enregistrement n'est en cours.</exception>
    /// <returns>Métadonnées de l'enregistrement finalisé.</returns>
    Task<EnregistrementAudio> ArreterEnregistrementAsync();

    /// <summary>
    /// Met en pause l'enregistrement en cours (si supporté).
    /// </summary>
    Task PauseAsync();

    /// <summary>
    /// Reprend l'enregistrement après une pause.
    /// </summary>
    Task ReprendreAsync();

    /// <summary>
    /// Force la rotation du fichier actuel (crée un nouveau segment).
    /// Utile pour découper automatiquement selon la durée configurée.
    /// </summary>
    /// <returns>Nouveau segment créé.</returns>
    Task<EnregistrementAudio> RotationnerFichierAsync();

    /// <summary>
    /// Obtient les statistiques en temps réel de l'enregistrement (niveau audio, etc.).
    /// </summary>
    /// <returns>Statistiques audio actuelles, ou null si pas d'enregistrement en cours.</returns>
    StatistiquesAudio? ObtenirStatistiquesActuelles();

    /// <summary>
    /// Événement déclenché lorsqu'un enregistrement démarre.
    /// </summary>
    event EventHandler<EnregistrementAudio>? EnregistrementDemarre;

    /// <summary>
    /// Événement déclenché lorsqu'un enregistrement s'arrête.
    /// </summary>
    event EventHandler<EnregistrementAudio>? EnregistrementArrete;

    /// <summary>
    /// Événement déclenché lors d'une erreur d'enregistrement.
    /// </summary>
    event EventHandler<Exception>? ErreurEnregistrement;
}

/// <summary>
/// Statistiques en temps réel d'un enregistrement audio.
/// </summary>
public class StatistiquesAudio
{
    /// <summary>
    /// Niveau audio actuel en décibels (dB).
    /// </summary>
    public double NiveauDb { get; set; }

    /// <summary>
    /// Niveau audio normalisé (0.0 à 1.0).
    /// </summary>
    public double NiveauNormalise { get; set; }

    /// <summary>
    /// Indique si le niveau actuel est considéré comme un silence.
    /// </summary>
    public bool EstSilence { get; set; }

    /// <summary>
    /// Durée du silence actuel (si EstSilence = true).
    /// </summary>
    public TimeSpan DureeSilenceActuel { get; set; }

    /// <summary>
    /// Taille actuelle du fichier en octets.
    /// </summary>
    public long TailleFichier { get; set; }
}
