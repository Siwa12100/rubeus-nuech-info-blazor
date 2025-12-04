using NuitInfo.Rubeus.RadioOccitania.Modeles;

namespace NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;

/// <summary>
/// Service de gestion centralisée de la configuration du module d'enregistrement audio.
/// Responsable du chargement, sauvegarde et fourniture de la configuration aux autres services.
/// </summary>
public interface IConfigurateurEnregistrementService
{
    /// <summary>
    /// Obtient la configuration actuelle du module d'enregistrement.
    /// </summary>
    /// <returns>Configuration chargée depuis le stockage persistant.</returns>
    ConfigurationEnregistrementAudio ObtenirConfiguration();

    /// <summary>
    /// Met à jour la configuration avec de nouvelles valeurs et la persiste.
    /// </summary>
    /// <param name="nouvelleConfig">Nouvelle configuration à sauvegarder.</param>
    /// <exception cref="ArgumentException">Si la configuration fournie n'est pas valide.</exception>
    Task MettreAJourConfigurationAsync(ConfigurationEnregistrementAudio nouvelleConfig);

    /// <summary>
    /// Recharge la configuration depuis le stockage (utile après modification manuelle du fichier).
    /// </summary>
    Task RechargerConfigurationAsync();

    /// <summary>
    /// Valide la configuration actuelle et retourne les erreurs éventuelles.
    /// </summary>
    /// <returns>Liste des erreurs de validation (vide si valide).</returns>
    List<string> ValiderConfiguration();

    /// <summary>
    /// Réinitialise la configuration aux valeurs par défaut.
    /// </summary>
    Task ReinitialiserConfigurationAsync();

    /// <summary>
    /// Événement déclenché lorsque la configuration est modifiée.
    /// Permet aux autres services de réagir aux changements.
    /// </summary>
    event EventHandler<ConfigurationEnregistrementAudio>? ConfigurationModifiee;
}
