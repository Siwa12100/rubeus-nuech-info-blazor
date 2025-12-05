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
    public GameState State { get; private set; }

    public PcRescueGameService()
    {
        State = new GameState();
    }

    /// <summary>
    /// Initialise une nouvelle partie avec les composants de départ
    /// </summary>
    public void Initialize()
    {
        // Réinitialisation de l'état du jeu
        State = new GameState
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
            Slot = ComponentSlot.Gpu,
            State = ComponentState.InstalledOld,
            Condition = ComponentCondition.Intact,
            BasePerf = 30,
            BaseEco = 10,
            BaseValue = 40,
            ImagePath = "/pc-rescue-game/gtx950.png"
        });

        // Création de la RAM de départ
        State.Components.Add(new PcComponent
        {
            Id = "ram_old",
            Slot = ComponentSlot.Ram,
            State = ComponentState.InstalledOld,
            Condition = ComponentCondition.Intact,
            BasePerf = 20,
            BaseEco = 15,
            BaseValue = 30,
            ImagePath = "/pc-rescue-game/DDR38Go.png"
        });

        // Création du Storage de départ
        State.Components.Add(new PcComponent
        {
            Id = "storage_old",
            Slot = ComponentSlot.Storage,
            State = ComponentState.InstalledOld,
            Condition = ComponentCondition.Intact,
            BasePerf = 25,
            BaseEco = 20,
            BaseValue = 35,
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

        // Modification de la condition selon le résultat du QTE
        switch (result)
        {
            case QteResult.Perfect:
                // Aucun dommage
                break;

            case QteResult.Medium:
                // Risque d'endommagement si intact
                if (component.Condition == ComponentCondition.Intact)
                {
                    component.Condition = ComponentCondition.Damaged;
                }
                break;

            case QteResult.Fail:
                // Dommage assuré
                component.Condition = component.Condition == ComponentCondition.Intact
                    ? ComponentCondition.Damaged
                    : ComponentCondition.Broken;
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
            QteResult.Perfect => 1.0,   // 100% de performance
            QteResult.Medium => 0.75,   // 75% de performance
            QteResult.Fail => 0.5,      // 50% de performance
            _ => 0.5
        };
    }
}
