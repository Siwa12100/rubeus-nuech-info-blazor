namespace NuitInfo.Rubeus.RadioOccitania.Models;

/// <summary>
/// Représente l'état d'avancement d'un traitement IA (transcription ou synthèse).
/// </summary>
public enum StatutTraitementIA
{
    /// <summary>
    /// Le traitement n'a pas encore été demandé ou lancé.
    /// </summary>
    NonDemarre = 0,

    /// <summary>
    /// Le traitement est actuellement en cours d'exécution.
    /// </summary>
    EnCours = 1,

    /// <summary>
    /// Le traitement s'est terminé avec succès.
    /// </summary>
    Termine = 2,

    /// <summary>
    /// Le traitement a échoué ou rencontré une erreur.
    /// </summary>
    Erreur = 3
}

/// <summary>
/// Extensions utilitaires pour l'enum StatutTraitementIA.
/// </summary>
public static class StatutTraitementIAExtensions
{
    /// <summary>
    /// Retourne une représentation textuelle du statut pour l'affichage UI.
    /// </summary>
    public static string VersTexte(this StatutTraitementIA statut)
    {
        return statut switch
        {
            StatutTraitementIA.NonDemarre => "Non démarré",
            StatutTraitementIA.EnCours => "En cours...",
            StatutTraitementIA.Termine => "Terminé",
            StatutTraitementIA.Erreur => "Erreur",
            _ => "Inconnu"
        };
    }

    /// <summary>
    /// Retourne une classe CSS suggérée pour l'affichage du badge de statut.
    /// </summary>
    public static string VersCssClass(this StatutTraitementIA statut)
    {
        return statut switch
        {
            StatutTraitementIA.NonDemarre => "badge-secondary",
            StatutTraitementIA.EnCours => "badge-info",
            StatutTraitementIA.Termine => "badge-success",
            StatutTraitementIA.Erreur => "badge-danger",
            _ => "badge-secondary"
        };
    }

    /// <summary>
    /// Retourne une icône suggérée (nom MudBlazor ou classe FontAwesome).
    /// </summary>
    public static string VersIcone(this StatutTraitementIA statut)
    {
        return statut switch
        {
            StatutTraitementIA.NonDemarre => "far fa-circle",
            StatutTraitementIA.EnCours => "fas fa-spinner fa-spin",
            StatutTraitementIA.Termine => "fas fa-check-circle",
            StatutTraitementIA.Erreur => "fas fa-exclamation-circle",
            _ => "far fa-question-circle"
        };
    }
}
