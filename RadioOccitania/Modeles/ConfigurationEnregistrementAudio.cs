using System;
using System.Collections.Generic;

namespace NuitInfo.Rubeus.RadioOccitania.Modeles
{
    /// <summary>
    /// Représente la configuration complète du module d'enregistrement audio.
    /// Cette classe est bindée directement sur l'UI de configuration et persistée en JSON ou en base.
    /// </summary>
    public class ConfigurationEnregistrementAudio
    {
        /// <summary>
        /// Nom du projet d'enregistrement (ex: "Antenne Principale", "Émission Spéciale").
        /// </summary>
        public string NomProjet { get; set; } = "Mon Enregistrement";

        /// <summary>
        /// Dossier racine du projet (optionnel, peut servir pour organiser plusieurs projets).
        /// </summary>
        public string? DossierProjet { get; set; }

        /// <summary>
        /// Chemin de base où seront stockés tous les enregistrements.
        /// Exemple: C:\EnregistrementsRadio
        /// </summary>
        public string CheminBaseStockage { get; set; } = string.Empty;

        /// <summary>
        /// Préfixe ajouté au nom de chaque fichier audio (ex: "antenne", "reunion", "emission").
        /// </summary>
        public string PrefixeNomFichier { get; set; } = "enregistrement";

        /// <summary>
        /// Format de sortie du fichier audio (wav, mp3, etc.).
        /// </summary>
        public string FormatSortie { get; set; } = "wav";

        /// <summary>
        /// Fréquence d'échantillonnage en Hz (ex: 44100, 48000).
        /// </summary>
        public int FrequenceEchantillonnage { get; set; } = 44_100;

        /// <summary>
        /// Profondeur des bits audio (8, 16, 24, 32).
        /// La valeur standard est 16 bits.
        /// </summary>
        public int ProfondeurBits { get; set; } = 16;

        /// <summary>
        /// Nombre de canaux audio (1 = mono, 2 = stéréo).
        /// </summary>
        public int NombreCanaux { get; set; } = 2;

        /// <summary>
        /// Index du périphérique audio à utiliser pour l'enregistrement.
        /// -1 = périphérique par défaut du système.
        /// 0, 1, 2... = index des périphériques disponibles (obtenu via NAudio).
        /// </summary>
        public int IndexPeripheriqueAudio { get; set; } = -1;

        /// <summary>
        /// Durée maximale d'un segment d'enregistrement en minutes avant découpe automatique.
        /// 0 = pas de découpe automatique.
        /// </summary>
        public int DureeSegmentMinutes { get; set; } = 60;

        /// <summary>
        /// Durée de conservation des enregistrements en jours (TTL - Time To Live).
        /// Au-delà, les fichiers sont automatiquement supprimés.
        /// </summary>
        public int DureeConservationJours { get; set; } = 30;

        /// <summary>
        /// Alias pour la compatibilité avec le HostedService de nettoyage.
        /// Retourne la valeur de DureeConservationJours.
        /// </summary>
        public int ConserverEnregistrementsJours
        {
            get => DureeConservationJours;
            set => DureeConservationJours = value;
        }

        /// <summary>
        /// Si true, l'enregistrement démarre automatiquement au lancement de l'application.
        /// </summary>
        public bool LancerAutomatiquementAuDemarrage { get; set; } = false;

        /// <summary>
        /// Adresse email pour recevoir des alertes en cas de détection de blanc prolongé (optionnel).
        /// </summary>
        public string? AdresseMailAlerteBlanc { get; set; }

        /// <summary>
        /// Seuil de détection du silence en décibels (valeur négative).
        /// Exemple : -40 dB signifie qu'un son inférieur à -40 dB est considéré comme silence.
        /// </summary>
        public double SeuilSilenceDb { get; set; } = -40.0;

        /// <summary>
        /// Durée minimale de silence en secondes avant de considérer qu'il y a un "blanc" problématique.
        /// </summary>
        public int DureeMinSilenceSecondes { get; set; } = 10;

        /// <summary>
        /// Active la découpe automatique des enregistrements en segments.
        /// </summary>
        public bool ActiverDecoupageAutomatique { get; set; } = true;

        /// <summary>
        /// Active la détection automatique des silences pour arrêter l'enregistrement ou déclencher une alerte.
        /// </summary>
        public bool ActiverDetectionSilence { get; set; } = false;

        /// <summary>
        /// Modèle de nommage des fichiers d'enregistrement.
        /// </summary>
        public ModeleNomFichier PatronNommage { get; set; } = new ModeleNomFichier
        {
            Patron = "%prefix%_%date%_%heure%h%minute%"
        };

        /// <summary>
        /// Valide la configuration et retourne les erreurs éventuelles.
        /// </summary>
        public List<string> Valider()
        {
            var erreurs = new List<string>();

            if (string.IsNullOrWhiteSpace(NomProjet))
                erreurs.Add("Le nom du projet est obligatoire.");

            if (string.IsNullOrWhiteSpace(CheminBaseStockage))
                erreurs.Add("Le chemin de base de stockage est obligatoire.");

            if (string.IsNullOrWhiteSpace(PrefixeNomFichier))
                erreurs.Add("Le préfixe du nom de fichier est obligatoire.");

            if (FrequenceEchantillonnage <= 0)
                erreurs.Add("La fréquence d'échantillonnage doit être positive.");

            if (ProfondeurBits != 8 && ProfondeurBits != 16 && ProfondeurBits != 24 && ProfondeurBits != 32)
                erreurs.Add("La profondeur des bits doit être 8, 16, 24 ou 32.");

            if (NombreCanaux < 1 || NombreCanaux > 2)
                erreurs.Add("Le nombre de canaux doit être 1 (mono) ou 2 (stéréo).");

            if (IndexPeripheriqueAudio < -1)
                erreurs.Add("L'index du périphérique audio ne peut pas être inférieur à -1.");

            if (DureeSegmentMinutes < 0)
                erreurs.Add("La durée de segment ne peut pas être négative.");

            if (DureeConservationJours <= 0)
                erreurs.Add("La durée de conservation doit être au moins de 1 jour.");

            if (DureeMinSilenceSecondes < 0)
                erreurs.Add("La durée minimale de silence ne peut pas être négative.");

            if (SeuilSilenceDb >= 0)
                erreurs.Add("Le seuil de silence doit être une valeur négative en dB.");

            // Valider le patron de nommage
            if (PatronNommage != null)
            {
                var erreursPatron = PatronNommage.Valider();
                erreurs.AddRange(erreursPatron);
            }
            else
            {
                erreurs.Add("Le patron de nommage est obligatoire.");
            }

            return erreurs;
        }

        /// <summary>
        /// Retourne un résumé textuel de la configuration pour affichage.
        /// </summary>
        public string ObtenirResume()
        {
            return $"Projet: {NomProjet} | Stockage: {CheminBaseStockage} | " +
                   $"Format: {FormatSortie} ({FrequenceEchantillonnage} Hz, {ProfondeurBits} bits, {NombreCanaux} canaux) | " +
                   $"Conservation: {DureeConservationJours} jours";
        }

        /// <summary>
        /// Retourne les informations sur le périphérique audio sélectionné.
        /// </summary>
        public string ObtenirInfoPeripherique()
        {
            if (IndexPeripheriqueAudio == -1)
                return "Périphérique audio par défaut du système";

            return $"Périphérique audio #{IndexPeripheriqueAudio}";
        }
    }
}
