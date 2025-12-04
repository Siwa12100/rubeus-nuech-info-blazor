using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuitInfo.Rubeus.RadioOccitania.Modeles;
using NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;

namespace NuitInfo.Rubeus.RadioOccitania.HostedServices;

/// <summary>
/// Service hébergé qui nettoie périodiquement les enregistrements expirés.
/// S'exécute en arrière-plan selon un intervalle configurable.
/// </summary>
public class NettoyageEnregistrementsHostedService : BackgroundService
{
    private readonly ILogger<NettoyageEnregistrementsHostedService> _logger;
    private readonly IStockageEnregistrementsService _stockageService;
    private readonly ConfigurationEnregistrementAudio _config;
    private readonly TimeSpan _intervalle;

    public NettoyageEnregistrementsHostedService(
        ILogger<NettoyageEnregistrementsHostedService> logger,
        IStockageEnregistrementsService stockageService,
        IOptions<ConfigurationEnregistrementAudio> options)
    {
        _logger = logger;
        _stockageService = stockageService;
        _config = options.Value;

        // Intervalle de nettoyage : toutes les 6 heures par défaut
        _intervalle = TimeSpan.FromHours(6);
    }

    /// <summary>
    /// Démarre le service de nettoyage.
    /// </summary>
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🧹 Service de nettoyage automatique démarré");
        _logger.LogInformation("   📅 Conservation : {Jours} jours", _config.ConserverEnregistrementsJours);
        _logger.LogInformation("   ⏱️ Intervalle de vérification : {Intervalle}", _intervalle);
        
        return base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Boucle principale d'exécution du nettoyage.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Attendre 30 secondes avant le premier nettoyage (laisser l'app démarrer)
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EffectuerNettoyageAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors du nettoyage automatique");
            }

            // Attendre jusqu'au prochain cycle
            try
            {
                await Task.Delay(_intervalle, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Normal lors de l'arrêt de l'application
                break;
            }
        }
    }

    /// <summary>
    /// Effectue le nettoyage des enregistrements expirés.
    /// </summary>
    private async Task EffectuerNettoyageAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🧹 Début du cycle de nettoyage automatique...");

        try
        {
            // Lister tous les enregistrements
            var enregistrements = await _stockageService.ObtenirEnregistrementsAsync();
            var totalAvant = enregistrements.Count;

            if (totalAvant == 0)
            {
                _logger.LogInformation("   ℹ️ Aucun enregistrement à nettoyer");
                return;
            }

            // Date limite de conservation
            var dateLimite = DateTime.UtcNow.AddDays(-_config.ConserverEnregistrementsJours);
            
            // Filtrer les enregistrements expirés
            var enregistrementsExpires = enregistrements
                .Where(e => e.DateCreation < dateLimite)
                .ToList();

            if (enregistrementsExpires.Count == 0)
            {
                _logger.LogInformation("   ✅ Aucun enregistrement expiré (total: {Total})", totalAvant);
                return;
            }

            // Supprimer les enregistrements expirés
            var totalSupprimes = 0;
            var tailleLiberee = 0L;

            foreach (var enregistrement in enregistrementsExpires)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("⚠️ Nettoyage interrompu par arrêt de l'application");
                    break;
                }

                try
                {
                    await _stockageService.SupprimerEnregistrementAsync(enregistrement.Id);
                    totalSupprimes++;
                    tailleLiberee += enregistrement.TailleFichier;

                    _logger.LogDebug("   🗑️ Supprimé : {Nom} (créé le {Date})", 
                        enregistrement.NomFichier, 
                        enregistrement.DateCreation);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "   ⚠️ Impossible de supprimer {Nom}", enregistrement.NomFichier);
                }
            }

            // Rapport de nettoyage
            var tailleLibereeMo = tailleLiberee / (1024.0 * 1024.0);
            _logger.LogInformation("   ✅ Nettoyage terminé :");
            _logger.LogInformation("      • {Supprimes}/{Expires} enregistrements supprimés", 
                totalSupprimes, enregistrementsExpires.Count);
            _logger.LogInformation("      • {Taille:F2} Mo libérés", tailleLibereeMo);
            _logger.LogInformation("      • {Restants} enregistrements conservés", 
                totalAvant - totalSupprimes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors du cycle de nettoyage");
        }
    }

    /// <summary>
    /// Arrête proprement le service de nettoyage.
    /// </summary>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛑 Arrêt du service de nettoyage automatique");
        return base.StopAsync(cancellationToken);
    }
}
