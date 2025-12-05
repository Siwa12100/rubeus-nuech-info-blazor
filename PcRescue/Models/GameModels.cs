namespace NuitInfo.Rubeus.PcRescue.Models;

/// <summary>
/// Les différentes phases du jeu PC Rescue
/// </summary>
public enum GamePhase
{
    Opening,      // Ouverture du boîtier
    Extraction,   // Extraction des composants
    Shop,         // Achat de nouveaux composants
    Montage,      // Installation des nouveaux composants
    Scoreboard    // Affichage du score final
}

/// <summary>
/// Les emplacements possibles pour les composants
/// </summary>
public enum ComponentSlot
{
    Cpu,
    Gpu,
    Ram,
    Storage,
    Psu
}

/// <summary>
/// L'état actuel d'un composant dans le workflow du jeu
/// </summary>
public enum ComponentState
{
    InstalledOld,  // Composant ancien installé dans le PC
    Removed,       // Composant extrait du PC
    Sorted,        // Composant trié dans une poubelle
    BoughtNew,     // Nouveau composant acheté
    InstalledNew   // Nouveau composant installé
}

/// <summary>
/// La condition physique d'un composant
/// </summary>
public enum ComponentCondition
{
    Intact,   // Composant en bon état
    Damaged,  // Composant légèrement endommagé
    Broken    // Composant cassé
}

/// <summary>
/// Le résultat d'un mini-jeu QTE (Quick Time Event)
/// </summary>
public enum QteResult
{
    Perfect,  // Réussi parfaitement
    Medium,   // Réussi moyennement
    Fail      // Raté
}

/// <summary>
/// Représente un composant PC dans le jeu
/// </summary>
public class PcComponent
{
    /// <summary>
    /// Identifiant unique du composant
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Emplacement du composant (CPU, GPU, RAM, Storage)
    /// </summary>
    public ComponentSlot Slot { get; set; }

    /// <summary>
    /// État actuel dans le workflow
    /// </summary>
    public ComponentState State { get; set; }

    /// <summary>
    /// Condition physique du composant
    /// </summary>
    public ComponentCondition Condition { get; set; }

    /// <summary>
    /// Performance de base du composant
    /// </summary>
    public int BasePerf { get; set; }

    /// <summary>
    /// Score écologique de base
    /// </summary>
    public int BaseEco { get; set; }

    /// <summary>
    /// Valeur marchande de base
    /// </summary>
    public int BaseValue { get; set; }

    /// <summary>
    /// Chemin vers l'image du composant dans wwwroot
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;
}

/// <summary>
/// État global du jeu PC Rescue
/// </summary>
public class GameState
{
    /// <summary>
    /// Budget disponible pour acheter de nouveaux composants
    /// </summary>
    public int Budget { get; set; }

    /// <summary>
    /// Score écologique cumulé
    /// </summary>
    public int EcoScore { get; set; }

    /// <summary>
    /// Score de performance cumulé
    /// </summary>
    public int PerfScore { get; set; }

    /// <summary>
    /// Phase actuelle du jeu
    /// </summary>
    public GamePhase Phase { get; set; }

    /// <summary>
    /// Liste de tous les composants du jeu
    /// </summary>
    public List<PcComponent> Components { get; set; } = new();
}
