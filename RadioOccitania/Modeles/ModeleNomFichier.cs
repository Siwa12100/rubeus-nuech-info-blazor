using System;
using System.Collections.Generic;
using System.IO;

namespace NuitInfo.Rubeus.RadioOccitania.Modeles
{
    /// <summary>
    /// Modèle de pattern de nommage pour les fichiers d'enregistrement.
    /// Supporte des tokens remplaçables : %prefix%, %date%, %heure%, etc.
    /// </summary>
    public class ModeleNomFichier
    {
        /// <summary>
        /// Pattern de nommage avec tokens.
        /// Exemple: "%prefix%_%date%_%heure%h%minute%"
        /// </summary>
        public string Patron { get; set; } = "%prefix%_%date%_%heure%h%minute%";

        /// <summary>
        /// Génère un nom de fichier concret à partir du patron et de la date.
        /// </summary>
        /// <param name="prefixe">Préfixe du nom de fichier (ex: "antenne").</param>
        /// <param name="dateDebut">Date et heure de début de l'enregistrement.</param>
        /// <returns>Nom de fichier sans extension.</returns>
        public string Generer(string prefixe, DateTime dateDebut)
        {
            var resultat = Patron;

            // Remplacer les tokens par leurs valeurs
            resultat = resultat.Replace("%prefix%", prefixe);
            resultat = resultat.Replace("%date%", dateDebut.ToString("yyyy-MM-dd"));
            resultat = resultat.Replace("%annee%", dateDebut.Year.ToString());
            resultat = resultat.Replace("%mois%", dateDebut.Month.ToString("D2"));
            resultat = resultat.Replace("%jour%", dateDebut.Day.ToString("D2"));
            resultat = resultat.Replace("%heure%", dateDebut.Hour.ToString("D2"));
            resultat = resultat.Replace("%minute%", dateDebut.Minute.ToString("D2"));
            resultat = resultat.Replace("%seconde%", dateDebut.Second.ToString("D2"));
            resultat = resultat.Replace("%timestamp%", dateDebut.ToString("yyyyMMdd_HHmmss"));
            resultat = resultat.Replace("%guid%", Guid.NewGuid().ToString("N")[..8]);

            // Nettoyer les caractères interdits dans les noms de fichiers
            var caracteresInterdits = Path.GetInvalidFileNameChars();
            foreach (var c in caracteresInterdits)
            {
                resultat = resultat.Replace(c, '_');
            }

            return resultat;
        }

        /// <summary>
        /// Valide le patron de nommage.
        /// </summary>
        /// <returns>Liste des erreurs (vide si valide).</returns>
        public List<string> Valider()
        {
            var erreurs = new List<string>();

            if (string.IsNullOrWhiteSpace(Patron))
            {
                erreurs.Add("Le patron de nommage ne peut pas être vide.");
                return erreurs;
            }

            // Vérifier qu'il n'y a pas de caractères interdits non tokenisés
            var patronTest = Patron;
            var tokens = new[]
            {
                "%prefix%", "%date%", "%annee%", "%mois%", "%jour%",
                "%heure%", "%minute%", "%seconde%", "%timestamp%", "%guid%"
            };

            foreach (var token in tokens)
            {
                patronTest = patronTest.Replace(token, "X");
            }

            var caracteresInterdits = Path.GetInvalidFileNameChars();
            foreach (var c in caracteresInterdits)
            {
                if (patronTest.Contains(c))
                {
                    erreurs.Add($"Le patron contient un caractère interdit : '{c}'");
                }
            }

            return erreurs;
        }

        /// <summary>
        /// Retourne un exemple de nom généré avec la date actuelle.
        /// </summary>
        public string ObtenirExemple(string prefixe = "antenne")
        {
            return Generer(prefixe, DateTime.Now);
        }
    }
}
