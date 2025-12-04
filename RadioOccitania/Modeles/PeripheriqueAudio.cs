namespace NuitInfo.Rubeus.RadioOccitania.Modeles;

/// <summary>
/// Représente un périphérique audio disponible pour l'enregistrement.
/// </summary>
public class PeripheriqueAudio
{
    /// <summary>
    /// Index du périphérique (pour NAudio).
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Nom du périphérique (ex: "Microphone (Realtek HD Audio)").
    /// </summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de canaux supportés (1 = mono, 2 = stéréo, etc.).
    /// </summary>
    public int NombreCanaux { get; set; }

    /// <summary>
    /// Indique si c'est le périphérique par défaut du système.
    /// </summary>
    public bool EstParDefaut { get; set; }

    public override string ToString()
    {
        var defaut = EstParDefaut ? " [PAR DÉFAUT]" : "";
        return $"[{Index}] {Nom} ({NombreCanaux} canaux){defaut}";
    }
}
