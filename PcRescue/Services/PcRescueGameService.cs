using NuitInfo.Rubeus.PcRescue.Models;

namespace NuitInfo.Rubeus.PcRescue.Services;

/// <summary>
/// Service principal gérant la logique du jeu PC Rescue
/// </summary>
public class PcRescueGameService
{
    /// <summary>
    /// État actuel du jeu
    /// </summary>
    public PcRescueGameState State { get; private set; }

    public PcRescueGameService()
    {
        State = new PcRescueGameState();
    }

    /// <summary>
    /// Initialise une nouvelle partie avec les composants de départ
    /// </summary>
    public void Initialize()
    {
        // Réinitialisation de l'état du jeu
        State = new PcRescueGameState
        {
            Budget = 100,
            EcoScore = 0,
            PerfScore = 0,
            Phase = GamePhase.Opening,
            Components = new List<PcComponent>()
        };

        // Création du GPU de départ
        State.Components.Add(new PcComponent
        {
            Id = "gpu_old",
            Name = "Carte Graphique (Ancienne)",
            Slot = ComponentSlot.Gpu,
            State = ComponentState.InstalledOld,
            Condition = ComponentCondition.Intact,
            BasePerf = 30,
            BaseEco = 10,
            BaseValue = 40,
            ImagePath = "/pc-rescue-game/gtx950.png"
            ImagePath = "/pc-rescue-game/gtx950.png"
        });

        // Création de la RAM de départ
        State.Components.Add(new PcComponent
        {
            Id = "ram_old",
            Name = "Mémoire RAM (Ancienne)",
            Slot = ComponentSlot.Ram,
            State = ComponentState.InstalledOld,
            Condition = ComponentCondition.Intact,
            BasePerf = 20,
            BaseEco = 15,
            BaseValue = 30,
            ImagePath = "/pc-rescue-game/DDR38Go.png"
            ImagePath = "/pc-rescue-game/DDR38Go.png"
        });

        // Création du Storage de départ
        State.Components.Add(new PcComponent
        {
            Id = "storage_old",
            Name = "Stockage (Ancien)",
            Slot = ComponentSlot.Storage,
            State = ComponentState.InstalledOld,
            Condition = ComponentCondition.Intact,
            BasePerf = 25,
            BaseEco = 20,
            BaseValue = 35,
            ImagePath = "/pc-rescue-game/hdd256.png"
            ImagePath = "/pc-rescue-game/hdd256.png"
        });

        // Création du CPU de départ
        State.Components.Add(new PcComponent
        {
            Id = "cpu_old",
            Slot = ComponentSlot.Cpu,
            State = ComponentState.InstalledOld,
            Condition = ComponentCondition.Intact,
            BasePerf = 35,
            BaseEco = 12,
            BaseValue = 50,
            ImagePath = "/pc-rescue-game/intel_i3.png"
        });

        // Création de l'alimentation de départ
        State.Components.Add(new PcComponent
        {
            Id = "psu_old",
            Slot = ComponentSlot.Psu,
            State = ComponentState.InstalledOld,
            Condition = ComponentCondition.Intact,
            BasePerf = 15,
            BaseEco = 8,
            BaseValue = 25,
            ImagePath = "/pc-rescue-game/100W.png"
        });
    }

    /// <summary>
    /// Applique le résultat d'un QTE d'extraction sur un composant
    /// </summary>
    /// <param name="component">Le composant à extraire</param>
    /// <param name="result">Le résultat du QTE</param>
    public void ApplyQteExtraction(PcComponent component, QteResult result)
    {
        if (component.State != ComponentState.InstalledOld)
            return;

        // Le composant passe à l'état "Removed"
        component.State = ComponentState.Removed;

        // La condition doit refléter strictement la couleur cliquée (vert/jaune/rouge)
        // Vert -> aucun dégât supplémentaire, Jaune -> au moins Endommagé, Rouge -> Détruit
        switch (result)
        {
            case QteResult.Perfect:
                // Aucun changement d'état (pas de "soin")
                break;

            case QteResult.Medium:
                if (component.Condition == ComponentCondition.Intact)
                {
                    component.Condition = ComponentCondition.Damaged;
                }
                break;

            case QteResult.Fail:
                component.Condition = ComponentCondition.Broken;
                break;
        }
    }

    /// <summary>
    /// Applique le tri d'un composant dans une poubelle
    /// </summary>
    /// <param name="component">Le composant à trier</param>
    /// <param name="bin">Type de poubelle : "trash", "recycle", "reuse"</param>
    public void ApplySorting(PcComponent component, string bin)
    {
        if (component.State != ComponentState.Removed)
            return;

        component.State = ComponentState.Sorted;

        // Calcul des gains selon le type de poubelle et la condition
        int ecoGain = 0;
        int moneyGain = 0;

        switch (bin.ToLower())
        {
            case "trash":
                // Poubelle normale : peu écologique, pas d'argent
                ecoGain = -5;
                moneyGain = 0;
                break;

            case "recycle":
                // Recyclage : bon pour l'écologie, peu d'argent
                ecoGain = component.Condition == ComponentCondition.Broken ? 10 : 15;
                moneyGain = component.BaseValue / 4;
                break;

            case "reuse":
                // Réutilisation : meilleur choix si le composant est intact
                if (component.Condition == ComponentCondition.Intact)
                {
                    ecoGain = 25;
                    moneyGain = component.BaseValue / 2;
                }
                else if (component.Condition == ComponentCondition.Damaged)
                {
                    ecoGain = 15;
                    moneyGain = component.BaseValue / 3;
                }
                else
                {
                    // Composant cassé : mauvais choix de le réutiliser
                    ecoGain = 5;
                    moneyGain = 0;
                }
                break;
        }

        // Application des gains
        State.EcoScore += ecoGain;
        State.Budget += moneyGain;
    }

    /// <summary>
    /// Applique le résultat d'un QTE d'installation d'un nouveau composant
    /// </summary>
    /// <param name="component">Le composant à installer</param>
    /// <param name="result">Le résultat du QTE</param>
    public void ApplyQteInstall(PcComponent component, QteResult result)
    {
        if (component.State != ComponentState.BoughtNew)
            return;

        component.State = ComponentState.InstalledNew;

        // Le score de performance dépend de la qualité de l'installation
        double multiplier = GetQteMultiplier(result);
        int perfGain = (int)(component.BasePerf * multiplier);

        State.PerfScore += perfGain;
    }

    /// <summary>
    /// Retourne le multiplicateur associé au résultat d'un QTE
    /// </summary>
    /// <param name="result">Le résultat du QTE</param>
    /// <returns>Multiplicateur entre 0.5 et 1.0</returns>
    public double GetQteMultiplier(QteResult result)
    {
        return result switch
        {
            QteResult.Perfect => 1.0,   // 100% de performance - Installation parfaite !
            QteResult.Medium => 0.8,    // 80% de performance - Un peu de travers...
            QteResult.Fail => 0.5,      // 50% de performance - Les pins sont tordus !
            _ => 0.5
        };
    }

    /// <summary>
    /// Achète un composant du shop et l'ajoute à l'état du jeu
    /// </summary>
    /// <param name="shopItem">L'article du shop à acheter</param>
    /// <returns>true si l'achat est réussi, false sinon (budget insuffisant)</returns>
    public bool BuyComponent(ShopItem shopItem)
    {
        // Vérification du budget
        if (shopItem.Cost > State.Budget)
            return false;

        // Déduction du budget
        State.Budget -= shopItem.Cost;

        // Création du nouveau composant à partir de l'article du shop
        var newComponent = new PcComponent
        {
            Id = shopItem.Id + "_" + Guid.NewGuid().ToString().Substring(0, 8),
            Slot = GetSlotFromComponentType(shopItem.Type),
            State = ComponentState.BoughtNew,
            Condition = ComponentCondition.Intact,
            BasePerf = shopItem.PerformanceBoost,
            BaseEco = shopItem.EcoScore,
            BaseValue = shopItem.Cost,
            ImagePath = shopItem.ImagePath
        };

        // Ajout du composant à la liste
        State.Components.Add(newComponent);

        // Application des scores écologiques immédiatement
        State.EcoScore += shopItem.EcoScore;

        return true;
    }

    /// <summary>
    /// Convertit un type de composant en emplacement de slot
    /// </summary>
    private ComponentSlot GetSlotFromComponentType(ComponentType type)
    {
        return type switch
        {
            ComponentType.Cpu => ComponentSlot.Cpu,
            ComponentType.Gpu => ComponentSlot.Gpu,
            ComponentType.Ram => ComponentSlot.Ram,
            ComponentType.Storage => ComponentSlot.Storage,
            ComponentType.PowerSupply => ComponentSlot.PowerSupply,
            ComponentType.Os => ComponentSlot.Os,
            _ => ComponentSlot.Cpu
        };
    }
}

