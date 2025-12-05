using Microsoft.AspNetCore.Components;
using NuitInfo.Rubeus.RadioOccitania.Modeles;

namespace NuitInfo.Rubeus.RadioOccitania.Components.ListeEnregistrementsPanel;

/// <summary>
/// Composant d'affichage de la liste des enregistrements audio avec contrôles.
/// </summary>
public partial class ListeEnregistrementsPanel
{
    // ==================== PARAMÈTRES ====================

    /// <summary>
    /// Liste des enregistrements à afficher.
    /// </summary>
    [Parameter]
    public IEnumerable<EnregistrementAudio>? Enregistrements { get; set; }

    /// <summary>
    /// Indique si un enregistrement est actuellement en cours.
    /// </summary>
    [Parameter]
    public bool EstEnregistrementEnCours { get; set; }

    /// <summary>
    /// Callback appelé lors du démarrage d'un enregistrement.
    /// </summary>
    [Parameter]
    public EventCallback OnDemarrerEnregistrement { get; set; }

    /// <summary>
    /// Callback appelé lors de l'arrêt d'un enregistrement.
    /// </summary>
    [Parameter]
    public EventCallback OnArreterEnregistrement { get; set; }

    /// <summary>
    /// Callback appelé lors du rafraîchissement de la liste.
    /// </summary>
    [Parameter]
    public EventCallback OnRafraichir { get; set; }

    /// <summary>
    /// Callback appelé pour demander la transcription d'un enregistrement.
    /// </summary>
    [Parameter]
    public EventCallback<EnregistrementAudio> OnDemanderTranscription { get; set; }

    /// <summary>
    /// Callback appelé pour demander la synthèse d'un enregistrement.
    /// </summary>
    [Parameter]
    public EventCallback<EnregistrementAudio> OnDemanderSynthese { get; set; }

    // ==================== MÉTHODES PONTS (CALLBACKS) ====================

    /// <summary>
    /// Démarre un nouvel enregistrement.
    /// </summary>
    private async Task DemarrerAsync()
    {
        // TODO: Implémenter la logique métier
        await OnDemarrerEnregistrement.InvokeAsync();
    }

    /// <summary>
    /// Arrête l'enregistrement en cours.
    /// </summary>
    private async Task ArreterAsync()
    {
        // TODO: Implémenter la logique métier
        await OnArreterEnregistrement.InvokeAsync();
    }

    /// <summary>
    /// Rafraîchit la liste des enregistrements.
    /// </summary>
    private async Task RafraichirAsync()
    {
        // TODO: Implémenter la logique métier
        await OnRafraichir.InvokeAsync();
    }

    /// <summary>
    /// Demande la transcription d'un enregistrement.
    /// </summary>
    private async Task DemanderTranscriptionAsync(EnregistrementAudio enregistrement)
    {
        // TODO: Implémenter la logique métier
        await OnDemanderTranscription.InvokeAsync(enregistrement);
    }

    /// <summary>
    /// Demande la synthèse d'un enregistrement.
    /// </summary>
    private async Task DemanderSyntheseAsync(EnregistrementAudio enregistrement)
    {
        // TODO: Implémenter la logique métier
        await OnDemanderSynthese.InvokeAsync(enregistrement);
    }

    // ==================== MÉTHODES UTILITAIRES POUR L'AFFICHAGE ====================

    /// <summary>
    /// Obtient le statut global d'un enregistrement basé sur ses statuts de transcription et synthèse.
    /// </summary>
    /// <param name="enregistrement">L'enregistrement à analyser.</param>
    /// <returns>Le statut global calculé.</returns>
    private StatutTraitementIA ObtenirStatutGlobal(EnregistrementAudio enregistrement)
    {
        // Priorité : synthèse terminée = traitement complet
        if (enregistrement.StatutSynthese == StatutTraitementIA.Termine)
            return StatutTraitementIA.Termine;

        // Transcription terminée mais pas de synthèse
        if (enregistrement.StatutTranscription == StatutTraitementIA.Termine)
            return StatutTraitementIA.Termine;

        // Au moins un traitement en cours
        if (enregistrement.StatutSynthese == StatutTraitementIA.EnCours ||
            enregistrement.StatutTranscription == StatutTraitementIA.EnCours)
            return StatutTraitementIA.EnCours;

        // Au moins une erreur
        if (enregistrement.StatutSynthese == StatutTraitementIA.Erreur ||
            enregistrement.StatutTranscription == StatutTraitementIA.Erreur)
            return StatutTraitementIA.Erreur;

        // Aucun traitement demandé
        return StatutTraitementIA.NonDemarre;
    }

    /// <summary>
    /// Obtient l'icône emoji correspondant au statut d'un enregistrement.
    /// </summary>
    /// <param name="enregistrement">L'enregistrement à analyser.</param>
    /// <returns>L'emoji représentant le statut.</returns>
    private string ObtenirIconeStatut(EnregistrementAudio enregistrement)
    {
        return ObtenirStatutGlobal(enregistrement) switch
        {
            StatutTraitementIA.NonDemarre => "⚪",
            StatutTraitementIA.EnCours => "🔵",
            StatutTraitementIA.Termine => "✅",
            StatutTraitementIA.Erreur => "❌",
            _ => "⚪"
        };
    }

    /// <summary>
    /// Obtient le libellé en occitan correspondant au statut d'un enregistrement.
    /// </summary>
    /// <param name="enregistrement">L'enregistrement à analyser.</param>
    /// <returns>Le texte en occitan décrivant le statut.</returns>
    private string ObtenirLibelleStatut(EnregistrementAudio enregistrement)
    {
        var statut = ObtenirStatutGlobal(enregistrement);

        // Cas spécial : si terminé, on affine selon le type de traitement
        if (statut == StatutTraitementIA.Termine)
        {
            if (enregistrement.StatutSynthese == StatutTraitementIA.Termine)
                return "Sintetizat";
            
            if (enregistrement.StatutTranscription == StatutTraitementIA.Termine)
                return "Transcrit";
        }

        // Cas généraux
        return statut switch
        {
            StatutTraitementIA.NonDemarre => "Non tractat",
            StatutTraitementIA.EnCours => "Tractament...",
            StatutTraitementIA.Termine => "Acabat",
            StatutTraitementIA.Erreur => "Error",
            _ => "Non tractat"
        };
    }

    /// <summary>
    /// Formate une durée TimeSpan en format lisible HH:mm:ss ou mm:ss.
    /// </summary>
    /// <param name="duree">La durée à formater.</param>
    /// <returns>La chaîne formatée.</returns>
    private string FormaterseDuree(TimeSpan duree)
    {
        // Si la durée est supérieure ou égale à 1 heure, afficher les heures
        if (duree.TotalHours >= 1)
            return duree.ToString(@"hh\:mm\:ss");
        
        // Sinon, afficher uniquement minutes:secondes
        return duree.ToString(@"mm\:ss");
    }
}
