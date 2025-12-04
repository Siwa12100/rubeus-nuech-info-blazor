namespace NuitInfo.Rubeus.Modeles.SnakeGame;

/// <summary>
/// Configuration centralisée du jeu Snake
/// Permet de modifier les comportements sans changer le code
/// </summary>
public static class SnakeGameConfig
{
    /// <summary>
    /// Dimensionde la grille par défaut
    /// </summary>
    public static class GridDefaults
    {
        public const int Width = 20;
        public const int Height = 15;
    }

    /// <summary>
    /// Configuration des segments du serpent
    /// </summary>
    public static class SnakeDefaults
    {
        /// <summary>
        /// Longueur initiale du serpent
        /// </summary>
        public const int InitialLength = 3;

        /// <summary>
        /// Longueur maximale pour remporter la partie
        /// </summary>
        public const int VictoryLength = 50;
    }

    /// <summary>
    /// Configuration des points
    /// </summary>
    public static class ScoringConfig
    {
        /// <summary>
        /// Points pour une nourriture normale
        /// </summary>
        public const int NormalFoodPoints = 10;

        /// <summary>
        /// Points pour une nourriture spéciale
        /// </summary>
        public const int SpecialFoodPoints = 50;
    }

    /// <summary>
    /// Configuration de la nourriture spéciale
    /// </summary>
    public static class SpecialFoodConfig
    {
        /// <summary>
        /// Probabilité d'apparition de nourriture spéciale (0-100)
        /// </summary>
        public const int SpawnChancePercent = 30;
    }

    /// <summary>
    /// Configuration des délais de jeu (en millisecondes)
    /// </summary>
    public static class TickDelays
    {
        public const int EasyMs = 150;
        public const int MediumMs = 100;
        public const int HardMs = 60;
        public const int InsaneMs = 30;
    }

    /// <summary>
    /// Configuration des multiplicateurs de score
    /// </summary>
    public static class ScoreMultipliers
    {
        public const decimal Easy = 1m;
        public const decimal Medium = 1.5m;
        public const decimal Hard = 2.5m;
        public const decimal Insane = 5m;
    }

    /// <summary>
    /// Configuration UI
    /// </summary>
    public static class UIConfig
    {
        /// <summary>
        /// Intervalle de rafraîchissement de l'UI (FPS)
        /// </summary>
        public const int UpdateIntervalMs = 16; // ~60 FPS

        /// <summary>
        /// Nombre maximum d'événements à afficher
        /// </summary>
        public const int MaxEventsDisplay = 5;
    }

    /// <summary>
    /// Messages du jeu
    /// </summary>
    public static class Messages
    {
        public const string GameStarted = "Partie démarrée";
        public const string GamePaused = "Partie mise en pause";
        public const string GameResumed = "Partie reprise";
        public const string GameReset = "Partie réinitialisée";
        public const string NormalFoodEaten = "Nourriture mangée! +{0} points";
        public const string SpecialFoodEaten = "Nourriture spéciale mangée! +{0} points";
        public const string WallCollision = "Collision avec un mur!";
        public const string SelfCollision = "Collision avec le corps du serpent!";
        public const string Victory = "🎉 VICTOIRE! Vous avez atteint la limite du serpent!";
    }
}
