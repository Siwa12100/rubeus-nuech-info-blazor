namespace NuitInfo.Rubeus.RadioOccitania.Modeles;

/// <summary>
/// Configuration du patron de nommage des fichiers d'enregistrement.
/// </summary>
public class PatronNommage
{
    /// <summary>
    /// Utiliser la date et l'heure dans le nom du fichier.
    /// </summary>
    public bool UtiliserDateHeure { get; set; } = true;

    /// <summary>
    /// Format de la date/heure (ex: "yyyy-MM-dd_HH-mm-ss").
    /// </summary>
    public string FormatDateHeure { get; set; } = "yyyy-MM-dd_HH-mm-ss";

    /// <summary>
    /// Utiliser un compteur incrémental dans le nom.
    /// </summary>
    public bool UtiliserCompteur { get; set; } = false;

    /// <summary>
    /// Valeur du compteur (auto-incrémenté).
    /// </summary>
    public int ValeurCompteur { get; set; } = 1;

    /// <summary>
    /// Séparateur entre les éléments du nom (ex: "_", "-").
    /// </summary>
    public string SeparateurElements { get; set; } = "_";

    /// <summary>
    /// Inclure un identifiant unique (GUID court).
    /// </summary>
    public bool InclureIdentifiantUnique { get; set; } = false;

    /// <summary>
    /// Génère un nom de fichier selon le patron configuré.
    /// </summary>
    public string GenererNomFichier(string prefixe, string extension)
    {
        var elements = new List<string> { prefixe };

        if (UtiliserDateHeure)
        {
            elements.Add(DateTime.Now.ToString(FormatDateHeure));
        }

        if (UtiliserCompteur)
        {
            elements.Add($"{ValeurCompteur:D4}");
            ValeurCompteur++;
        }

        if (InclureIdentifiantUnique)
        {
            elements.Add(Guid.NewGuid().ToString()[..8]);
        }

        return string.Join(SeparateurElements, elements) + extension;
    }
}
