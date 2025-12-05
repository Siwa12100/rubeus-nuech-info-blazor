namespace NuitInfo.Rubeus.Modeles.SnakeGame;

/// <summary>
/// Exception levée lors d'une opération invalide sur le jeu Snake
/// </summary>
public class SnakeGameException : Exception
{
    public SnakeGameException(string message) : base(message) { }
    public SnakeGameException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Exception levée lorsque le jeu est dans un état invalide
/// </summary>
public class InvalidGameStateException : SnakeGameException
{
    public InvalidGameStateException(string message) : base($"État du jeu invalide: {message}") { }
}

/// <summary>
/// Exception levée pour une position invalide
/// </summary>
public class InvalidPositionException : SnakeGameException
{
    public InvalidPositionException(Position position, int gridWidth, int gridHeight)
        : base($"Position invalide ({position.X}, {position.Y}) pour une grille {gridWidth}x{gridHeight}") { }
}

/// <summary>
/// Validateur pour les paramètres du jeu
/// </summary>
public static class SnakeGameValidator
{
    /// <summary>
    /// Valide les dimensions de la grille
    /// </summary>
    public static void ValidateGridDimensions(int width, int height)
    {
        if (width < 5 || height < 5)
            throw new ArgumentException("La grille doit être au minimum 5x5");

        if (width > 100 || height > 100)
            throw new ArgumentException("La grille ne peut pas dépasser 100x100");
    }

    /// <summary>
    /// Valide une position sur la grille
    /// </summary>
    public static void ValidatePosition(Position position, int gridWidth, int gridHeight)
    {
        if (position == null)
            throw new ArgumentNullException(nameof(position));

        if (position.X < 0 || position.X >= gridWidth || position.Y < 0 || position.Y >= gridHeight)
            throw new InvalidPositionException(position, gridWidth, gridHeight);
    }

    /// <summary>
    /// Valide un état de jeu
    /// </summary>
    public static void ValidateGameState(GameState state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));

        if (state.SnakeBody.Count == 0)
            throw new InvalidGameStateException("Le serpent doit avoir au moins un segment");

        if (state.GridWidth < 5 || state.GridHeight < 5)
            throw new InvalidGameStateException("La grille est trop petite");

        if (state.Food == null)
            throw new InvalidGameStateException("La nourriture doit être présente");
    }

    /// <summary>
    /// Valide une direction
    /// </summary>
    public static void ValidateDirection(Direction direction)
    {
        if (!Enum.IsDefined(typeof(Direction), direction))
            throw new ArgumentException($"Direction invalide: {direction}");
    }

    /// <summary>
    /// Valide une difficulté
    /// </summary>
    public static void ValidateDifficulty(GameDifficulty difficulty)
    {
        if (!Enum.IsDefined(typeof(GameDifficulty), difficulty))
            throw new ArgumentException($"Difficulté invalide: {difficulty}");
    }
}
