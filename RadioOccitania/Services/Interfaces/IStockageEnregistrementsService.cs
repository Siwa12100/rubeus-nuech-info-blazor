using NuitInfo.Rubeus.RadioOccitania.Modeles;

namespace NuitInfo.Rubeus.RadioOccitania.Services.Interfaces;

/// <summary>
/// Service de gestion du stockage physique des enregistrements audio.
/// Responsable de la génération des chemins, du nommage et de la gestion du cycle de vie des fichiers.
/// </summary>
public interface IStockageEnregistrementsService
{
    /// <summary>
    /// Génère le chemin complet pour un nouveau fichier d'enregistrement.
    /// Utilise la configuration actuelle (chemin de base, préfixe, pattern de nommage).
    /// </summary>
    /// <param name="dateDebut">Date et heure de début de l'enregistrement.</param>
    /// <returns>Chemin absolu du fichier à créer.</returns>
    string GenererCheminNouveauFichier(DateTime dateDebut);

    /// <summary>
    /// Génère un nom de fichier selon le pattern configuré, sans le chemin complet.
    /// </summary>
    /// <param name="dateDebut">Date et heure de début de l'enregistrement.</param>
    /// <returns>Nom du fichier avec extension.</returns>
    string GenererNomFichier(DateTime dateDebut);

    /// <summary>
    /// Liste tous les enregistrements présents dans le répertoire de stockage.
    /// Reconstruit les métadonnées à partir des fichiers et de l'index éventuel.
    /// </summary>
    /// <returns>Collection des enregistrements trouvés.</returns>
    Task<IEnumerable<EnregistrementAudio>> ListerEnregistrementsAsync();

    /// <summary>
    /// Obtient la liste de tous les enregistrements sous forme de liste ordonnée.
    /// </summary>
    Task<List<EnregistrementAudio>> ObtenirEnregistrementsAsync();

    /// <summary>
    /// Récupère un enregistrement spécifique par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant unique de l'enregistrement.</param>
    /// <returns>L'enregistrement trouvé, ou null si inexistant.</returns>
    Task<EnregistrementAudio?> ObtenirEnregistrementAsync(Guid id);

    /// <summary>
    /// Supprime tous les enregistrements dont la date d'expiration est dépassée.
    /// </summary>
    /// <returns>Nombre d'enregistrements supprimés.</returns>
    Task<int> SupprimerEnregistrementsExpiresAsync();

    /// <summary>
    /// Supprime un enregistrement spécifique (fichier + métadonnées).
    /// </summary>
    /// <param name="id">Identifiant de l'enregistrement à supprimer.</param>
    /// <returns>True si supprimé avec succès, false si non trouvé.</returns>
    Task<bool> SupprimerEnregistrementAsync(Guid id);

    /// <summary>
    /// Sauvegarde les métadonnées d'un enregistrement dans l'index.
    /// </summary>
    /// <param name="enregistrement">Enregistrement à sauvegarder.</param>
    Task SauvegarderMetadonneesAsync(EnregistrementAudio enregistrement);

    /// <summary>
    /// Calcule l'espace disque total utilisé par les enregistrements.
    /// </summary>
    /// <returns>Taille totale en octets.</returns>
    Task<long> CalculerEspaceTotalUtiliseAsync();

    /// <summary>
    /// Vérifie que le répertoire de stockage existe et est accessible en écriture.
    /// Crée le répertoire si nécessaire.
    /// </summary>
    /// <exception cref="IOException">Si le répertoire n'est pas accessible.</exception>
    Task VerifierEtCreerRepertoireStockageAsync();
}
