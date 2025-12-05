namespace NuitInfo.Rubeus.RadioOccitania.Modeles;

/// <summary>
/// Représente un fichier audio enregistré avec ses métadonnées et son état de traitement IA.
/// </summary>
public class EnregistrementAudio
{
    /// <summary>
    /// Identifiant unique de l'enregistrement.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Chemin absolu du fichier audio sur le système de fichiers.
    /// Alias pour CheminComplet (compatibilité).
    /// </summary>
    public string CheminFichier { get; set; } = string.Empty;

    /// <summary>
    /// Chemin absolu complet du fichier audio (propriété principale).
    /// </summary>
    public string CheminComplet
    {
        get => CheminFichier;
        set => CheminFichier = value;
    }

    /// <summary>
    /// Nom du fichier (sans le chemin complet) - version "friendly" pour l'affichage.
    /// </summary>
    public string NomFichier { get; set; } = string.Empty;

    /// <summary>
    /// Date et heure de début de l'enregistrement.
    /// </summary>
    public DateTime DateDebut { get; set; }

    /// <summary>
    /// Date et heure de fin de l'enregistrement.
    /// Null si l'enregistrement est encore en cours.
    /// </summary>
    public DateTime? DateFin { get; set; }

    /// <summary>
    /// Date de création de l'enregistrement (alias pour DateDebut).
    /// Utilisé pour la compatibilité avec différentes parties du code.
    /// </summary>
    public DateTime DateCreation
    {
        get => DateDebut;
        set => DateDebut = value;
    }

    /// <summary>
    /// Alias pour TailleOctets (autre variante de compatibilité).
    /// </summary>
    public long TailleFichier
    {
        get => TailleOctets;
        set => TailleOctets = value;
    }


    /// <summary>
    /// Taille du fichier en octets.
    /// </summary>
    public long TailleOctets { get; set; }

    /// <summary>
    /// Alias pour TailleOctets (compatibilité avec différentes parties du code).
    /// </summary>
    public long TailleFichierOctets
    {
        get => TailleOctets;
        set => TailleOctets = value;
    }

    /// <summary>
    /// Date d'expiration automatique du fichier.
    /// </summary>
    public DateTime DateExpiration { get; set; }

    /// <summary>
    /// Statut du traitement de transcription (Speech-to-Text).
    /// </summary>
    public StatutTraitementIA StatutTranscription { get; set; } = StatutTraitementIA.NonDemarre;

    /// <summary>
    /// Statut du traitement de synthèse/résumé par IA.
    /// </summary>
    public StatutTraitementIA StatutSynthese { get; set; } = StatutTraitementIA.NonDemarre;

    /// <summary>
    /// Texte du résumé généré par l'IA (si disponible).
    /// </summary>
    public string? ResumeTexte { get; set; }

    /// <summary>
    /// Chemin vers le fichier de transcription complète (si stocké séparément).
    /// </summary>
    public string? CheminTranscription { get; set; }

    /// <summary>
    /// Chemin vers le fichier de synthèse/résumé (si stocké séparément).
    /// </summary>
    public string? CheminSynthese { get; set; }

    /// <summary>
    /// Message d'erreur en cas d'échec du traitement IA.
    /// </summary>
    public string? MessageErreur { get; set; }

    // Propriétés calculées pour l'UI

    /// <summary>
    /// Indique si l'enregistrement est actuellement en cours.
    /// </summary>
    public bool EstEnCours => DateFin == null;

    /// <summary>
    /// Durée de l'enregistrement.
    /// </summary>
    public TimeSpan Duree => (DateFin ?? DateTime.Now) - DateDebut;

    /// <summary>
    /// Taille formatée pour l'affichage (Ko, Mo, Go).
    /// </summary>
    public string TailleFormatee
    {
        get
        {
            if (TailleOctets < 1024)
                return $"{TailleOctets} o";
            if (TailleOctets < 1024 * 1024)
                return $"{TailleOctets / 1024.0:F2} Ko";
            if (TailleOctets < 1024 * 1024 * 1024)
                return $"{TailleOctets / (1024.0 * 1024.0):F2} Mo";
            return $"{TailleOctets / (1024.0 * 1024.0 * 1024.0):F2} Go";
        }
    }

    /// <summary>
    /// Nombre de jours restants avant expiration.
    /// </summary>
    public int JoursAvantExpiration => (DateExpiration - DateTime.Now).Days;

    /// <summary>
    /// Indique si le fichier est expiré.
    /// </summary>
    public bool EstExpire => DateTime.Now > DateExpiration;

    /// <summary>
    /// Indique si le fichier existe physiquement sur le disque.
    /// </summary>
    public bool FichierExiste => File.Exists(CheminFichier);

    /// <summary>
    /// Indique si au moins un traitement IA est terminé.
    /// </summary>
    public bool AUnTraitementTermine =>
        StatutTranscription == StatutTraitementIA.Termine ||
        StatutSynthese == StatutTraitementIA.Termine;

    /// <summary>
    /// Indique si un traitement IA est en cours.
    /// </summary>
    public bool AUnTraitementEnCours =>
        StatutTranscription == StatutTraitementIA.EnCours ||
        StatutSynthese == StatutTraitementIA.EnCours;
}
