# 🐍 Snake Game - Documentation

Un excellent jeu Snake implémenté en Blazor avec une architecture propre et professionnelle.

## ✨ Caractéristiques

### Gameplay
- **Serpent contrôlable** avec 3 segments de départ
- **Nourriture normale** rouge pour gagner 10 points (x multiplicateur)
- **Nourriture spéciale** dorée pour gagner 50 points (x multiplicateur) - apparait aléatoirement
- **Collisions détectées** avec les murs et le corps du serpent
- **Victoire** possible à 50 segments
- **Pausable** à tout moment

### Difficultés
- **Facile** : 150ms/mouvement, multiplicateur 1x
- **Moyen** : 100ms/mouvement, multiplicateur 1.5x
- **Difficile** : 60ms/mouvement, multiplicateur 2.5x
- **Impossible** : 30ms/mouvement, multiplicateur 5x

### Commandes
- `↑` / `W` : Aller vers le haut
- `↓` / `S` : Aller vers le bas
- `←` / `A` : Aller à gauche
- `→` / `D` : Aller à droite
- `SPACE` : Pause/Reprendre

## 🏗️ Architecture

### Structure des fichiers

```
Snake/
├── Modeles/SnakeGame/
│   ├── Position.cs              # Représente une position (X, Y)
│   ├── Direction.cs             # Énumération des directions + utilitaires
│   ├── GameDifficulty.cs        # Niveaux de difficulté et configurations
│   ├── GameState.cs             # État complet d'une partie
│   └── SnakeGameEvent.cs        # Événements du jeu
├── Repositories/
│   └── ISnakeGameEngine.cs      # Moteur de jeu (interface + implémentation)
├── Composants/Pages/
│   ├── Snake.razor              # Page interactive du jeu
│   └── Snake.razor.css          # Styles du jeu
└── Tests/
    └── SnakeGameTests.cs        # Tests unitaires
```

### Principes SOLID appliqués

#### Single Responsibility Principle (SRP)
- `Position` : responsable uniquement de la représentation d'une coordonnée
- `Direction` : gère uniquement la logique directionnelle
- `GameState` : représente l'état, pas la logique
- `SnakeGameEngine` : gère uniquement la logique du jeu
- `Snake.razor` : affiche uniquement l'interface utilisateur

#### Open/Closed Principle (OCP)
- `ISnakeGameEngine` interface permet d'étendre le comportement sans modifier le code existant
- `Direction` utilise les switch expressions pour faciliter l'ajout de nouvelles directions

#### Liskov Substitution Principle (LSP)
- L'interface `ISnakeGameEngine` peut être remplacée par n'importe quelle implémentation

#### Interface Segregation Principle (ISP)
- `ISnakeGameEngine` expose uniquement les méthodes nécessaires au composant Razor

#### Dependency Inversion Principle (DIP)
- Le composant Razor dépend de `ISnakeGameEngine` (abstraction) et non pas de `SnakeGameEngine` (implémentation)
- Injection de dépendance via le conteneur ASP.NET Core

### Patterns utilisés

#### Strategy Pattern
- `GameDifficulty` détermine le comportement du jeu (vitesse, score)

#### State Pattern
- `GameStatus` énumère les états possibles du jeu

#### Observer Pattern (Events)
- `SnakeGameEvent` et la liste d'événements permettent au UI de réagir aux changements

#### Repository Pattern
- `ISnakeGameEngine` agit comme un service centralisé pour la logique métier

## 💻 Code Examples

### Utiliser le moteur de jeu

```csharp
// Initialiser le jeu
gameEngine.Initialize(GameDifficulty.Medium, 20, 15);

// Démarrer
gameEngine.Start();

// Changer de direction (sera validée au prochain tick)
gameEngine.SetNextDirection(Direction.Up);

// Mettre à jour l'état du jeu (appelé régulièrement)
gameEngine.Update();

// Récupérer l'état actuel
var state = gameEngine.GetCurrentState();
Console.WriteLine($"Score: {state.Score}, Longueur: {state.Length}");

// Gérer les événements
var events = gameEngine.GetEventsSinceLastUpdate();
foreach (var evt in events)
{
    Console.WriteLine(evt.Message);
}

// Pause/Reprendre
gameEngine.Pause();
gameEngine.Resume();

// Réinitialiser
gameEngine.Reset();
```

### Tester la logique

```csharp
[Fact]
public void Initialize_ShouldCreateSnakeWithThreeSegments()
{
    var engine = new SnakeGameEngine();
    engine.Initialize();
    var state = engine.GetCurrentState();
    
    Assert.Equal(3, state.Length);
}
```

## 🎨 Design visuel

### Couleurs
- **Serpent** : Vert (#00D084 pour la tête, #4CAF50 pour le corps)
- **Nourriture** : Rouge (#FF6B6B)
- **Nourriture spéciale** : Doré (#FFD700)
- **Arrière-plan** : Gradient bleu (#1e3c72 → #2a5298)

### Animations
- Pulsation de la nourriture
- Yeux animés sur la tête du serpent
- Transitions fluides des boutons
- Fade-in des événements

## 🧪 Tests

Le projet inclut une suite complète de tests unitaires couvrant :

- ✅ Initialisation du jeu
- ✅ Contrôles du jeu (démarrage, pause, reprise, réinitialisation)
- ✅ Logique directionnelle (pas de demi-tour)
- ✅ Détection des collisions
- ✅ Génération de nourriture
- ✅ Calcul des scores
- ✅ Système d'événements
- ✅ Modèles de domaine

Pour exécuter les tests :
```bash
dotnet test
```

## 🔧 Configuration

### Injection de dépendance (Program.cs)
```csharp
builder.Services.AddScoped<ISnakeGameEngine, SnakeGameEngine>();
```

### Dans le composant Razor
```csharp
@inject ISnakeGameEngine GameEngine
```

## 🚀 Performance

- **Rendu** : ~60 FPS avec SVG
- **Logique** : O(n) où n = longueur du serpent
- **Mémoire** : Optimisée avec clonage d'état uniquement quand nécessaire
- **Collision serpent** : O(n) avec recherche dans List

## 📈 Améliorations futures possibles

- [ ] Sauvegarde des meilleurs scores en BD
- [ ] Mode multijoueur
- [ ] Powerups variés
- [ ] Obstacles sur la grille
- [ ] Replay des parties
- [ ] Achievements/Trophées
- [ ] Support mobile avec touch
- [ ] Thèmes visuels personnalisables

## 🐛 Gestion des bugs connus

- Les touches doivent être pressées sur l'élément du jeu (focus automatique au premier clic)
- La nourriture spéciale peut ne pas apparaitre immédiatement (30% de chance)

## 📝 Notes de développement

### Concurrentabilité
- Le jeu n'est pas thread-safe (pas nécessaire pour usage single-user)
- Pour multi-utilisateurs, utiliser des techniques de synchronisation

### Scalabilité
- Grille configurable (20x15 par défaut)
- Peut supporter des grilles jusqu'à 50x50 sans problème de performance

### Maintainabilité
- Tous les chiffres "magiques" sont documentés
- Les méthodes sont courtes et focalisées
- Les commentaires XML documentent l'API publique

## 📄 Licence

Partie du projet NuitInfo.Rubeus 2025
