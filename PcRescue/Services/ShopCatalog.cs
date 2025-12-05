using NuitInfo.Rubeus.PcRescue.Models;

namespace NuitInfo.Rubeus.PcRescue.Services;

/// <summary>
/// Catalogue statique contenant tous les articles disponibles dans le shop
/// </summary>
public static class ShopCatalog
{
    /// <summary>
    /// Retourne tous les articles disponibles du shop
    /// </summary>
    public static List<ShopItem> GetAllItems()
    {
        return new List<ShopItem>
        {
            // ==================== PROCESSEURS ====================
            new ShopItem
            {
                Id = "cpu_i5_new",
                Name = "Intel i5 (Neuf)",
                Type = ComponentType.Cpu,
                Quality = ComponentQuality.New,
                Cost = 45,
                PerformanceBoost = 35,
                EcoScore = -5,
                Description = "Processeur Intel i5 neuf avec garantie constructeur",
                ImagePath = "/pc-rescue-game/intel_i5.png"
            },
            new ShopItem
            {
                Id = "cpu_ryzen7_used",
                Name = "Ryzen 7 (Occasion)",
                Type = ComponentType.Cpu,
                Quality = ComponentQuality.Used,
                Cost = 35,
                PerformanceBoost = 40,
                EcoScore = 8,
                Description = "Processeur Ryzen 7 d'occasion en bon état",
                ImagePath = "/pc-rescue-game/Ryzen7.png"
            },
            new ShopItem
            {
                Id = "cpu_i3_refurb",
                Name = "Intel i3 (Récup)",
                Type = ComponentType.Cpu,
                Quality = ComponentQuality.Refurb,
                Cost = 15,
                PerformanceBoost = 15,
                EcoScore = 15,
                Description = "Processeur Intel i3 reconditionné et fonctionnel",
                ImagePath = "/pc-rescue-game/intel_i3.png"
            },

            // ==================== CARTES GRAPHIQUES ====================
            new ShopItem
            {
                Id = "gpu_gtx1080_refurb",
                Name = "GTX 1080 (Récup)",
                Type = ComponentType.Gpu,
                Quality = ComponentQuality.Refurb,
                Cost = 25,
                PerformanceBoost = 50,
                EcoScore = 10,
                Description = "Carte graphique GTX 1080 recondtionnée, bonne performance",
                ImagePath = "/pc-rescue-game/gtx1080.png"
            },
            new ShopItem
            {
                Id = "gpu_rtx2060_used",
                Name = "RTX 2060 (Occasion)",
                Type = ComponentType.Gpu,
                Quality = ComponentQuality.Used,
                Cost = 40,
                PerformanceBoost = 55,
                EcoScore = 5,
                Description = "Carte graphique RTX 2060 d'occasion, ray-tracing supporté",
                ImagePath = "/pc-rescue-game/gtx950.png"
            },
            new ShopItem
            {
                Id = "gpu_rx9070_new",
                Name = "RX 9070 (Neuf)",
                Type = ComponentType.Gpu,
                Quality = ComponentQuality.New,
                Cost = 55,
                PerformanceBoost = 65,
                EcoScore = -8,
                Description = "Carte graphique RX 9070 dernière génération",
                ImagePath = "/pc-rescue-game/RX9070.png"
            },

            // ==================== MÉMOIRE RAM ====================
            new ShopItem
            {
                Id = "ram_16gb_ddr5_new",
                Name = "16Go DDR5 (Neuf)",
                Type = ComponentType.Ram,
                Quality = ComponentQuality.New,
                Cost = 50,
                PerformanceBoost = 30,
                EcoScore = -6,
                Description = "16Go de RAM DDR5 neuve à haut débit",
                ImagePath = "/pc-rescue-game/DDR516Go.png"
            },
            new ShopItem
            {
                Id = "ram_16gb_ddr4_used",
                Name = "2x8Go DDR4 (Occasion)",
                Type = ComponentType.Ram,
                Quality = ComponentQuality.Used,
                Cost = 30,
                PerformanceBoost = 25,
                EcoScore = 10,
                Description = "Kit de 2x8Go DDR4 d'occasion, compatible avec anciens systèmes",
                ImagePath = "/pc-rescue-game/2DDR48Go.png"
            },
            new ShopItem
            {
                Id = "ram_8gb_ddr4_refurb",
                Name = "8Go DDR4 (Récup)",
                Type = ComponentType.Ram,
                Quality = ComponentQuality.Refurb,
                Cost = 12,
                PerformanceBoost = 12,
                EcoScore = 14,
                Description = "8Go de RAM DDR4 recondtionnée et testée",
                ImagePath = "/pc-rescue-game/DDR48Go.png"
            },

            // ==================== ALIMENTATIONS ====================
            new ShopItem
            {
                Id = "psu_850w_new",
                Name = "Alimentation 850W (Neuf)",
                Type = ComponentType.PowerSupply,
                Quality = ComponentQuality.New,
                Cost = 40,
                PerformanceBoost = 5,
                EcoScore = -4,
                Description = "Alimentation 850W certifiée 80+ Gold",
                ImagePath = "/pc-rescue-game/850W.png"
            },
            new ShopItem
            {
                Id = "psu_550w_used",
                Name = "Alimentation 550W (Occasion)",
                Type = ComponentType.PowerSupply,
                Quality = ComponentQuality.Used,
                Cost = 20,
                PerformanceBoost = 2,
                EcoScore = 6,
                Description = "Alimentation 550W d'occasion, certifiée 80+ Bronze",
                ImagePath = "/pc-rescue-game/550W.png"
            },
            new ShopItem
            {
                Id = "psu_480w_refurb",
                Name = "Alimentation 480W (Récup)",
                Type = ComponentType.PowerSupply,
                Quality = ComponentQuality.Refurb,
                Cost = 10,
                PerformanceBoost = 1,
                EcoScore = 12,
                Description = "Alimentation 480W recondtionnée et fiable",
                ImagePath = "/pc-rescue-game/480W.png"
            },

            // ==================== STOCKAGE ====================
            new ShopItem
            {
                Id = "ssd_2tb_new",
                Name = "SSD 2To (Neuf)",
                Type = ComponentType.Storage,
                Quality = ComponentQuality.New,
                Cost = 60,
                PerformanceBoost = 45,
                EcoScore = -3,
                Description = "SSD 2To NVMe dernière génération, très rapide",
                ImagePath = "/pc-rescue-game/hdd256.png"
            },
            new ShopItem
            {
                Id = "ssd_500gb_used",
                Name = "SSD 500Go (Occasion)",
                Type = ComponentType.Storage,
                Quality = ComponentQuality.Used,
                Cost = 20,
                PerformanceBoost = 25,
                EcoScore = 8,
                Description = "SSD 500Go d'occasion, encore bon état",
                ImagePath = "/pc-rescue-game/hdd256.png"
            },
            new ShopItem
            {
                Id = "hdd_1tb_refurb",
                Name = "HDD 1To (Récup)",
                Type = ComponentType.Storage,
                Quality = ComponentQuality.Refurb,
                Cost = 8,
                PerformanceBoost = 10,
                EcoScore = 11,
                Description = "Disque dur 1To recondtionné, stockage économique",
                ImagePath = "/pc-rescue-game/hdd256.png"
            },

            // ==================== SYSTÈMES D'EXPLOITATION ====================
            new ShopItem
            {
                Id = "os_windows11",
                Name = "Windows 11 (Payant)",
                Type = ComponentType.Os,
                Quality = ComponentQuality.New,
                Cost = 30,
                PerformanceBoost = 0,
                EcoScore = -5,
                Description = "Licence Windows 11 Pro, support Microsoft",
                ImagePath = "/pc-rescue-game/Windows.png"
            },
            new ShopItem
            {
                Id = "os_linux",
                Name = "Linux (Gratuit)",
                Type = ComponentType.Os,
                Quality = ComponentQuality.New,
                Cost = 0,
                PerformanceBoost = 5,
                EcoScore = 20,
                Description = "Linux Ubuntu, open-source et écologique",
                ImagePath = "/pc-rescue-game/Linux.png"
            }
        };
    }

    /// <summary>
    /// Retourne les articles disponibles en fonction du type de composant
    /// </summary>
    /// <param name="type">Le type de composant filtré</param>
    public static List<ShopItem> GetItemsByType(ComponentType type)
    {
        return GetAllItems().Where(item => item.Type == type).ToList();
    }

    /// <summary>
    /// Retourne les articles disponibles dans le budget spécifié
    /// </summary>
    /// <param name="maxBudget">Budget maximal</param>
    public static List<ShopItem> GetAffordableItems(int maxBudget)
    {
        return GetAllItems().Where(item => item.Cost <= maxBudget).ToList();
    }
}
