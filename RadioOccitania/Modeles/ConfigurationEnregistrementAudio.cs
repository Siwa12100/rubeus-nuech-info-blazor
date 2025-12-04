namespace NuitInfo.Rubeus.RadioOccitania.Modeles;

/// <summary>
/// Représente la configuration complète du module d'enregistrement audio.
/// Cette classe est bindée directement sur l'UI de configuration et persistée en JSON ou en base.
/// </summary>
public class ConfigurationEnregistrementAudio
{
    /// <summary>
    /// Nom du projet d'enregistrement (ex: "Antenne Principale", "Émission Spéciale").
    /// </summary>
    public string NomProjet { get; set; } = "Mon Enregistrement";

    /// <summary>
    /// Dossier racine du projet (optionnel, peut servir pour organiser plusieurs projets).
    /// </summary>
    public string? DossierProjet { get; set; }

    /// <summary>
    /// Chemin de base où seront stockés tous les enregistrements.
    /// Exemple: C:\EnregistrementsRadio
    /// </summary>
    public string CheminBaseStockage { get; set; } = string.Empty;

    /// <summary>
    /// Préfixe ajouté au nom de chaque fichier audio (ex: "antenne", "reunion", "emission").
    /// </summary>
    public string PrefixeNomFichier { get; set; } = "enregistrement";

    /// <summary>
    /// Format de sortie du fichier audio (wav, mp3, etc.).
    /// </summary>
    public string FormatSortie { get; set; } = "wav";

    /// <summary>
    /// Fréquence d'échantillonnage en Hz (ex: 44100, 48000).
    /// </summary>
    public int FrequenceEchantillonnage { get; set; } = 44_100;

    /// <summary>
    /// Durée maximale d'un segment d'enregistrement en minutes avant découpe automatique.
    /// 0 = pas de découpe automatique.
    /// </summary>
    public int DureeSegmentMinutes { get; set; } = 60;

    /// <summary>
    /// Durée de conservation des enregistrements en jours (TTL - Time To Live).
    /// Au-delà, les fichiers sont automatiquement supprimés.
    /// </summary>
    public int DureeConservationJours { get; set; } = 30;

    /// <summary>
    /// Si true, l'enregistrement démarre automatiquement au lancement de l'application.
    /// </summary>
    public bool LancerAutomatiquementAuDemarrage { get; set; } = false;

    /// <summary>
    /// Adresse email pour recevoir des alertes en cas de détection de blanc prolongé (optionnel).
    /// </summary>
    public string? AdresseMailAlerteBlanc { get; set; }

    /// <summary>
    /// Seuil de détection du silence en décibels (dB).
    /// Valeurs négatives typiques: -40 dB, -50 dB.
    /// </summary>
    public double SeuilSilenceDb { get; set; } = -40.0;

    /// <summary>
    /// Durée minimale de silence en secondes avant de considérer qu'il y a un "blanc" problématique.
    /// </summary>
    public int DureeMinSilenceSecondes { get; set; } = 10;

    /// <summary>
    /// Valide la configuration et retourne les erreurs éventuelles.
    /// </summary>
    public List<string> Valider()
    {
        var erreurs = new List<string>();

        if (string.IsNullOrWhiteSpace(NomProjet))
            erreurs.Add("Le nom du projet est obligatoire.");

        if (string.IsNullOrWhiteSpace(CheminBaseStockage))
            erreurs.Add("Le chemin de base de stockage est obligatoire.");

        if (string.IsNullOrWhiteSpace(PrefixeNomFichier))
            erreurs.Add("Le préfixe du nom de fichier est obligatoire.");

        if (FrequenceEchantillonnage <= 0)
            erreurs.Add("La fréquence d'échantillonnage doit être positive.");

        if (DureeSegmentMinutes < 0)
            erreurs.Add("La durée de segment ne peut pas être négative.");

        if (DureeConservationJours <= 0)
            erreurs.Add("La durée de conservation doit être au moins de 1 jour.");

        if (DureeMinSilenceSecondes < 0)
            erreurs.Add("La durée minimale de silence ne peut pas être négative.");

        return erreurs;
    }
}
