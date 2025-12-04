Parfait, on va te cadrer ça comme si tu avais un mini “plan de bataille” pour tes 3h.
Je te donne :

* une **roadmap chronologique** (dans quel ordre faire les choses),
* la **liste des fichiers** à créer (dossiers, classes, pages),
* pour chaque fichier : **rôle** + **logique métier** (sans code concret).

Je pars sur un contexte Blazor Server .NET 9, monolithe, Windows (NAudio possible), avec IA en *stub*.

---

## 🕒 Vue d’ensemble des étapes (3h)

### Phase 1 – Modélisation (20–30 min)

1. Créer les **modèles** métier (config, enregistrement, statut IA).
2. Prévoir les **DTO / ViewModels** pour les pages Blazor.

### Phase 2 – Services cœur métier (60–75 min)

3. Service de **stockage des enregistrements** (chemins, nommage, TTL).
4. Service d’**enregistrement audio** (start/stop, découpe logique).
5. Service de **nettoyage** des anciens fichiers.
6. Services “IA” en **stubs** (transcription, résumé, détection de blanc).

### Phase 3 – Infrastructure & démarrage (30–40 min)

7. HostedService éventuel pour lancement auto / nettoyage périodique.
8. Intégration dans `Program.cs` (DI, config, logging).

### Phase 4 – UI Blazor (40–60 min)

9. Page de **configuration** (`/audio/config`).
10. Page de **liste & restitution** (`/audio/enregistrements`).
11. Petits composants réutilisables si tu as le temps (lecture audio, badge statut IA).

---

# 🧱 Structure de projet proposée

### 📁 Dossiers / Fichiers

```text
/Models
    ConfigurationEnregistrementAudio.cs
    ModeleNomFichier.cs
    EnregistrementAudio.cs
    StatutTraitementIA.cs

/Services
    Interfaces
        IConfigurateurEnregistrementService.cs
        IStockageEnregistrementsService.cs
        IEnregistreurAudioService.cs
        IAnalyseSilencesService.cs
        ITranscriptionService.cs
        ISyntheseService.cs
    Impl
        ConfigurateurEnregistrementService.cs
        StockageEnregistrementsService.cs
        EnregistreurAudioService.cs
        AnalyseSilencesServiceStub.cs
        TranscriptionServiceStub.cs
        SyntheseServiceStub.cs

/HostedServices
    NettoyageEnregistrementsHostedService.cs
    (optionnel) EnregistrementAutoHostedService.cs

/Pages
    AudioConfig.razor
    AudioEnregistrements.razor

/Components
    EnregistrementCard.razor
    StatutIAChip.razor

appsettings.json
```

Maintenant on détaille chaque bloc.

---

## 1️⃣ Les modèles (Models)

### 1.1 `ConfigurationEnregistrementAudio.cs`

**Rôle :**
Représente TOUTE la configuration modifiable depuis l’UI pour le module d’enregistrement.

**Champs typiques :**

* `string NomProjet`
* `string DossierProjet` *(si besoin)*
* `string CheminBaseStockage` (ex : `C:\EnregistrementsRadio`)
* `string PrefixeNomFichier` (texte libre, ex. `antenne`, `reunion`)
* `string FormatSortie` (`"wav"`, `"mp3"`, etc.)
* `int FrequenceEchantillonnage` (44_100, etc. si tu exposes ça)
* `int DureeSegmentMinutes` (durée max d’un fichier avant découpe)
* `int DureeConservationJours` (TTL)
* `bool LancerAutomatiquementAuDemarrage`
* `string AdresseMailAlerteBlanc` (optionnel)
* `double SeuilSilenceDb` (param pour futur algo de blanc)
* `int DureeMinSilenceSecondes` (param pour futur algo)

**Responsabilité :**

* Classe purement **POCO**.
* Sert :

  * à être **persistée** dans un fichier JSON ou en base
  * à être **bindée** directement sur la page Blazor de config.

---

### 1.2 `ModeleNomFichier.cs` (optionnel mais propre)

**Rôle :**
Représente un **pattern de nommage** de fichier audio.

Exemple de pattern côté UI :
`%prefix%_%date%_%heure%h%minute%`

**Champs :**

* `string Patron` (le texte du pattern)
* éventuellement des aides/flags si tu veux éviter de parser des choses partout.

**Responsabilité :**

* Rester un modèle simple.
* La logique de “remplacer les tokens par des valeurs” sera dans `IStockageEnregistrementsService`.

---

### 1.3 `EnregistrementAudio.cs`

**Rôle :**
Représente un **fichier audio concret** (enregistré).

**Champs :**

* `Guid Id` (ou string)
* `string CheminFichier` (absolu)
* `string NomFichier` (friendly)
* `DateTime DateDebut`
* `DateTime DateFin` (ou nullable si en cours)
* `long TailleOctets`
* `DateTime DateExpiration` (calculée à partir de TTL)
* `StatutTraitementIA StatutTranscription`
* `StatutTraitementIA StatutSynthese`
* `string? ResumeTexte` (si synthèse réalisée)
* `string? CheminTranscription` (si tu stockes la transcription à part)

**Responsabilité :**

* Base d’info pour :

  * affichage dans l’UI,
  * décisions de nettoyage,
  * état IA.

Tu peux au début **ne pas persister** ça en base et simplement le reconstruire à partir des fichiers + un petit index JSON si tu n’as pas le temps pour EF.

---

### 1.4 `StatutTraitementIA.cs` (enum)

**Rôle :**
Enum pour suivre l’état de la transcription / synthèse.

* `NonDemarre`
* `EnCours`
* `Termine`
* `Erreur`

**Responsabilité :**

* Rester simple.
* Te permettre dans l’UI de dire :

  * “Non traité”
  * “Traitement en cours…”
  * “Synthèse disponible”
  * “Erreur IA”

---

## 2️⃣ Les services – Interfaces

Tu exposes des **interfaces** claires, les implémentations peuvent rester simples / stubées.

---

### 2.1 `IConfigurateurEnregistrementService.cs`

**Rôle :**
Gestion centralisée de la **configuration**.

**Responsabilités :**

* Charger la configuration au démarrage (depuis `appsettings`, `config.json`, ou `IConfiguration` + éventuellement `.env`).
* Sauvegarder la configuration modifiée depuis l’UI.
* Fournir la config actuelle aux autres services.

**Méthodes typiques (en pseudo-signatures) :**

* `ConfigurationEnregistrementAudio ObtenirConfiguration();`
* `Task MettreAJourConfigurationAsync(ConfigurationEnregistrementAudio nouvelleConfig);`

---

### 2.2 `IStockageEnregistrementsService.cs`

**Rôle :**
Gérer **où** sont stockés les fichiers, **comment** ils s’appellent, et leur **durée de vie**.

**Responsabilités :**

* Construire le **chemin complet** pour un nouveau fichier en fonction de :

  * la config,
  * la date/heure de début,
  * le pattern de nommage.
* Lister tous les enregistrements présents.
* Supprimer les enregistrements expirés.

**Méthodes typiques :**

* `string GenererCheminNouveauFichier(DateTime dateDebut);`

  * utilise `ConfigurationEnregistrementAudio` + `ModeleNomFichier` + `CheminBaseStockage`.
* `IEnumerable<EnregistrementAudio> ListerEnregistrements();`

  * parcourt le dossier, reconstruit les métadonnées.
* `Task SupprimerEnregistrementsExpirésAsync();`

---

### 2.3 `IEnregistreurAudioService.cs`

**Rôle :**
Cœur de la capture audio. C’est lui qui fait le **start/stop** et sait sur **quel fichier** écrire.

**Responsabilités :**

* Démarrer un enregistrement :

  * créer le fichier via `IStockageEnregistrementsService`,
  * initialiser la capture (NAudio/ffmpeg),
  * mémoriser l’heure de début.
* Arrêter l’enregistrement :

  * fermer le flux,
  * mémoriser l’heure de fin.
* Gérer la “découpe” (rotation) si tu veux le faire ici, ou laisser un HostedService gérer le timer.

**Méthodes :**

* `Task DemarrerEnregistrementAsync();`
* `Task ArreterEnregistrementAsync();`
* `bool EstEnCours { get; }`
* `EnregistrementAudio? EnregistrementActuel { get; }` (méta du fichier courant)

---

### 2.4 `IAnalyseSilencesService.cs` (IA “blancs” – stub possible)

**Rôle :**
Service dédié à la **détection de silences** dans un enregistrement, et plus tard : “silence naturel vs non naturel”.

**Responsabilités :**

* Fournir un **point d’entrée unique** pour la logique de détection de blancs :

  * à partir d’un fichier audio déjà enregistré,
  * ou à partir de segments audio pendant la capture (plus avancé).
* Retourner soit :

  * une simple info booléenne; ou
  * une liste de “segments de silence” (timestamps début/fin + classification).

**Méthodes possibles :**

* `Task<bool> ContientBlancSuspectAsync(EnregistrementAudio enregistrement);`
* (plus avancé) `Task<IReadOnlyList<SegmentSilence>> DetecterSilencesAsync(EnregistrementAudio enregistrement);`

En V1, `AnalyseSilencesServiceStub` peut juste renvoyer `false` et loguer “TODO”.

---

### 2.5 `ITranscriptionService.cs`

**Rôle :**
Point d’entrée pour convertir **audio → texte**.

**Responsabilités :**

* Encapsuler l’appel à un moteur STT (future IA).
* Gérer la sauvegarde éventuelle de la transcription (fichier texte, JSON, base).

**Méthode :**

* `Task<string> TranscrireAsync(EnregistrementAudio enregistrement);`

En V1, `TranscriptionServiceStub` :

* renvoie `"Transcription non implémentée (stub)"`,
* marque le `StatutTranscription` sur l’enregistrement.

---

### 2.6 `ISyntheseService.cs`

**Rôle :**
Prend un texte (transcription) et produit un **résumé**.

**Responsabilités :**

* Appel à un LLM / API IA (plus tard).
* Logique de format du résumé (par ex. :

  * résumé global + points clés + décisions).

**Méthode :**

* `Task<string> GenererSyntheseAsync(string transcription);`

En V1, `SyntheseServiceStub` renvoie un texte fixe expliquant que la synthèse n’est pas implémentée.

---

## 3️⃣ Services – Implémentations

Tu les mets dans `/Services/Impl`.

---

### 3.1 `ConfigurateurEnregistrementService.cs`

**Rôle pratique :**

* Lecture/écriture de la config.
* Par exemple : stocker dans un fichier `config-enregistrement.json` dans le projet.

**Logique :**

* Au premier appel, charger depuis disque ou utiliser des valeurs par défaut.
* Quand l’UI sauvegarde, écrire le JSON.

---

### 3.2 `StockageEnregistrementsService.cs`

**Logique clé :**

* *Génération de nom de fichier* :

  * Récupère la config (`CheminBaseStockage`, `PrefixeNomFichier`, `FormatSortie`).
  * Crée le dossier s’il n’existe pas.
  * Applique un pattern type :
    `"{prefix}_{yyyy-MM-dd_HH-mm-ss}.{ext}"`.
* *Listing* :

  * `Directory.GetFiles(CheminBaseStockage, "*.wav" | "*.mp3"…)`.
  * Pour chaque fichier :

    * `File.GetCreationTime`, `FileInfo.Length`…
  * Construit des `EnregistrementAudio`.
* *Nettoyage* :

  * Sur chaque `EnregistrementAudio`, compare `DateExpiration` à `DateTime.Now`.
  * Supprime fichiers expirés.

---

### 3.3 `EnregistreurAudioService.cs`

**Logique centrale :**

* Gérer l’**état courant** :

  * bool `EstEnCours`
  * l’enregistrement courant (référence vers `EnregistrementAudio`).
* Sur `DemarrerEnregistrementAsync` :

  * demander au `StockageEnregistrementsService` un nouveau chemin,
  * initialiser la capture audio (via NAudio ou autre),
  * stocker `DateDebut`.
* Sur `ArreterEnregistrementAsync` :

  * stopper la capture,
  * compléter `DateFin`,
  * éventuellement notifier un autre service (ex : `IAnalyseSilencesService`) pour lancer une analyse asynchrone.

Tu peux garder la logique de **découpe horaire** dans un HostedService qui fait :

* toutes les X minutes :
  `EnregistreurAudioService.ArreterEnregistrementAsync()` puis `DemarrerEnregistrementAsync()`.

---

### 3.4 `AnalyseSilencesServiceStub.cs`, `TranscriptionServiceStub.cs`, `SyntheseServiceStub.cs`

**Rôle :**

* Offrir des points d’entrée déjà intégrés dans l’archi,
* mais retourner des résultats simplistes (stub) :

  * logs,
  * texte fixe.

Ça montre que tu as pensé à l’extensibilité sans perdre du temps sur de l’IA compliquée.

---

## 4️⃣ HostedServices

Dossier `/HostedServices`.

---

### 4.1 `NettoyageEnregistrementsHostedService.cs`

**Rôle :**

* Tâche de fond qui nettoie les vieux enregistrements périodiquement.

**Logique :**

* Sur `StartAsync`, lancer une boucle (timer) qui :

  * toutes les X minutes :

    * appelle `IStockageEnregistrementsService.SupprimerEnregistrementsExpirésAsync()`.
* Sur `StopAsync`, stopper proprement la boucle.

Tu peux paramétrer l’intervalle via `appsettings.json`.

---

### 4.2 `(Optionnel) EnregistrementAutoHostedService.cs`

Si tu veux que l’antenne soit **toujours enregistrée** dès le démarrage :

**Rôle :**

* Au démarrage de l’app :

  * lire la config,
  * si `LancerAutomatiquementAuDemarrage == true` :

    * appeler `EnregistreurAudioService.DemarrerEnregistrementAsync()`.

Possibilité aussi de gérer ici la **découpe** régulière des fichiers (rotation).

---

## 5️⃣ Pages Blazor

Dossier `/Pages`.

---

### 5.1 `AudioConfig.razor`

**Rôle :**
Page d’administration de la configuration du module.

**Affiche :**

* Formulaire avec :

  * chemin de stockage,
  * format,
  * durée segment,
  * durée conservation,
  * préfixe,
  * email d’alerte,
  * case “enregistrer automatiquement au démarrage”.
* Boutons :

  * “Enregistrer la configuration”
  * “Tester l’enregistrement” (éventuellement)

**Logique :**

* Injecter `IConfigurateurEnregistrementService`.
* Charger la configuration au `OnInitializedAsync`.
* Data-binding bi-directionnel.
* Au clic sur “Enregistrer” → appeler `MettreAJourConfigurationAsync`.

---

### 5.2 `AudioEnregistrements.razor`

**Rôle :**
Page principale de **restitution**.

**Affiche :**

* Boutons en haut :

  * “Démarrer l’enregistrement” / “Arrêter l’enregistrement”
* Informations :

  * état courant (“Enregistrement en cours”, “Arrêté”)
* Liste d’enregistrements (tableau ou cartes) :

  * Date / Heure de début
  * Durée (DateFin – DateDebut)
  * Taille
  * Date d’expiration
  * Statut transcription / synthèse (via `StatutIAChip`)
  * Actions :

    * “Écouter”
    * “Télécharger”
    * “Transcrire” (appelle `ITranscriptionService`)
    * “Synthèse” (appelle `ISyntheseService`)

**Logique :**

* Injecter :

  * `IEnregistreurAudioService`
  * `IStockageEnregistrementsService`
  * `ITranscriptionService`
  * `ISyntheseService`
* Au chargement :

  * récupérer la liste des enregistrements
* Boutons :

  * Démarrer → `EnregistreurAudioService.DemarrerEnregistrementAsync()`, puis recharger la liste.
  * Arrêter → `ArreterEnregistrementAsync()`, recharger.

---

## 6️⃣ Composants UI (facultatifs mais clean)

Dossier `/Components`.

---

### 6.1 `EnregistrementCard.razor`

**Rôle :**

* Composant réutilisable pour afficher un `EnregistrementAudio` avec actions.

**Paramètres :**

* `[Parameter] public EnregistrementAudio Enregistrement { get; set; }`
* `[Parameter] public EventCallback OnDemanderTranscription { get; set; }` etc.

---

### 6.2 `StatutIAChip.razor`

**Rôle :**

* Afficher un petit badge coloré en fonction de `StatutTraitementIA`.

---

## 7️⃣ Intégration & configuration

### 7.1 `appsettings.json`

Ajouter une section :

```json
"ModuleEnregistrementAudio": {
  "CheminBaseStockage": "C:\\EnregistrementsRadio",
  "FormatSortie": "wav",
  "DureeSegmentMinutes": 60,
  "DureeConservationJours": 30,
  "LancerAutomatiquementAuDemarrage": false
}
```

Tu peux initialiser `ConfigurationEnregistrementAudio` à partir de cette section au démarrage.

---

### 7.2 `Program.cs`

**Responsabilités :**

* Enregistrer tes services dans le conteneur DI :

  * `IConfigurateurEnregistrementService`
  * `IStockageEnregistrementsService`
  * `IEnregistreurAudioService`
  * `IAnalyseSilencesService` (stub)
  * `ITranscriptionService` (stub)
  * `ISyntheseService` (stub)
* Ajouter les HostedServices :

  * `NettoyageEnregistrementsHostedService`
  * `(optionnel) EnregistrementAutoHostedService`

---

## 🧭 En résumé : ce que tu peux raisonnablement faire en 3h

1. **Créer tous les modèles & interfaces** (Models + Interfaces services).
2. **Implémenter 3 services clés** :

   * Configurateur
   * Stockage des enregistrements
   * Enregistreur audio (même si la “vraie” capture audio est simplifiée ou TODO)
3. **Ajouter 1 HostedService** simple pour le nettoyage.
4. **Créer 2 pages Blazor** :

   * `AudioConfig.razor` (config)
   * `AudioEnregistrements.razor` (liste + start/stop + boutons IA)
5. **Mettre les services IA et analyse de silence en stub**, mais parfaitement intégrés dans l’architecture.

Même si certains bouts sont marqués `TODO : implémenter la capture via NAudio / ffmpeg`, ton correcteur verra :

* une **architecture claire**,
* une **séparation nette des responsabilités**,
* la **préparation** pour IA, détection de blancs, etc.

Si tu veux, je peux te faire ensuite un “schéma texte” d’architecture (qui appelle qui, dans quel sens) pour que tu l’expliques à l’oral / dans le rapport.
