using NuitInfo.Rubeus.RadioOccitania.Modeles;

namespace NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;

/// <summary>
/// Service de configuration de l'enregistrement audio.
/// </summary>
public interface IConfigurateurEnregistrementService
{
    /// <summary>
    /// Événement déclenché lorsque la configuration est modifiée.
    /// </summary>
    event EventHandler<ConfigurationEnregistrementAudio>? ConfigurationModifiee;

    /// <summary>
    /// Obtient la configuration actuelle (depuis le cache ou le fichier).
    /// </summary>
    ConfigurationEnregistrementAudio ObtenirConfiguration();

    /// <summary>
    /// Met à jour la configuration et la sauvegarde.
    /// </summary>
    Task MettreAJourConfigurationAsync(ConfigurationEnregistrementAudio configuration);

    /// <summary>
    /// Recharge la configuration depuis le fichier.
    /// </summary>
    Task<ConfigurationEnregistrementAudio> RechargerConfigurationAsync();

    /// <summary>
    /// Valide la configuration actuelle.
    /// </summary>
    List<string> ValiderConfiguration();

    /// <summary>
    /// Valide une configuration spécifique.
    /// </summary>
    List<string> ValiderConfiguration(ConfigurationEnregistrementAudio configuration);

    /// <summary>
    /// Réinitialise la configuration aux valeurs par défaut.
    /// </summary>
    Task ReinitialiserConfigurationAsync();

    /// <summary>
    /// Obtient la liste des périphériques audio disponibles.
    /// </summary>
    List<PeripheriqueAudio> ObtenirPeripheriquesDisponibles();
}
