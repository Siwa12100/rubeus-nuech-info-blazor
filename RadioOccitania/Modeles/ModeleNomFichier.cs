namespace NuitInfo.Rubeus.RadioOccitania.Models;

/// <summary>
/// Représente un modèle/pattern de nommage de fichier audio.
/// Exemple: "%prefix%_%date%_%heure%h%minute%"
/// Les tokens seront remplacés dynamiquement par le service de stockage.
/// </summary>
public class ModeleNomFichier
{
    /// <summary>
    /// Pattern de nommage avec tokens remplaçables.
    /// Tokens supportés:
    /// - %prefix% : préfixe défini dans la configuration
    /// - %date% : date au format yyyyMMdd
    /// - %heure% : heure au format HH
    /// - %minute% : minute au format mm
    /// - %seconde% : seconde au format ss
    /// - %timestamp% : timestamp Unix
    /// - %guid% : GUID unique
    /// </summary>
    public string Patron { get; set; } = "%prefix%_%date%_%heure%h%minute%";

    /// <summary>
    /// Indique si le pattern inclut un identifiant unique (guid ou timestamp).
    /// </summary>
    public bool InclutIdentifiantUnique => 
        Patron.Contains("%guid%") || Patron.Contains("%timestamp%");

    /// <summary>
    /// Indique si le pattern inclut des informations de date/heure.
    /// </summary>
    public bool InclutDateHeure => 
        Patron.Contains("%date%") || 
        Patron.Contains("%heure%") || 
        Patron.Contains("%minute%") || 
        Patron.Contains("%seconde%");

    /// <summary>
    /// Liste des tokens disponibles avec leur description.
    /// </summary>
    public static Dictionary<string, string> TokensDisponibles => new()
    {
        { "%prefix%", "Préfixe défini dans la configuration" },
        { "%date%", "Date au format yyyyMMdd (ex: 20240115)" },
        { "%heure%", "Heure au format HH (00-23)" },
        { "%minute%", "Minute au format mm (00-59)" },
        { "%seconde%", "Seconde au format ss (00-59)" },
        { "%timestamp%", "Timestamp Unix en secondes" },
        { "%guid%", "Identifiant unique (GUID)" }
    };

    /// <summary>
    /// Valide le pattern et retourne les erreurs éventuelles.
    /// </summary>
    public List<string> Valider()
    {
        var erreurs = new List<string>();

        if (string.IsNullOrWhiteSpace(Patron))
        {
            erreurs.Add("Le pattern ne peut pas être vide.");
            return erreurs;
        }

        // Vérifier que le pattern contient au moins un token
        if (!TokensDisponibles.Keys.Any(token => Patron.Contains(token)))
        {
            erreurs.Add("Le pattern doit contenir au moins un token valide.");
        }

        // Vérifier les caractères invalides pour un nom de fichier
        var invalidChars = Path.GetInvalidFileNameChars();
        var patronSansTokens = Patron;
        foreach (var token in TokensDisponibles.Keys)
        {
            patronSansTokens = patronSansTokens.Replace(token, "");
        }

        if (patronSansTokens.Any(c => invalidChars.Contains(c)))
        {
            erreurs.Add("Le pattern contient des caractères invalides pour un nom de fichier.");
        }

        return erreurs;
    }

    /// <summary>
    /// Retourne un exemple de nom généré avec ce pattern.
    /// </summary>
    public string GenererExemple(string prefixe = "antenne")
    {
        var maintenant = DateTime.Now;
        var exemple = Patron
            .Replace("%prefix%", prefixe)
            .Replace("%date%", maintenant.ToString("yyyyMMdd"))
            .Replace("%heure%", maintenant.ToString("HH"))
            .Replace("%minute%", maintenant.ToString("mm"))
            .Replace("%seconde%", maintenant.ToString("ss"))
            .Replace("%timestamp%", DateTimeOffset.Now.ToUnixTimeSeconds().ToString())
            .Replace("%guid%", Guid.NewGuid().ToString("N")[..8]);

        return exemple;
    }
}
