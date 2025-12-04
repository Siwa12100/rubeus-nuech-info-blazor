namespace NuitInfo.Rubeus.Modeles.SnakeGame;

/// <summary>
/// Énumération des états possibles du jeu
/// </summary>
public enum GameStatus
{
    NotStarted,
    Playing,
    Paused,
    GameOver,
    Victory
}

/// <summary>
/// Représente l'état complet d'une partie de Snake
/// </summary>
public class GameState
{
    public int GridWidth { get; set; } = 20;
    public int GridHeight { get; set; } = 15;

    public List<Position> SnakeBody { get; set; } = [];
    public Position? Food { get; set; }
    public Position? SpecialFood { get; set; }

    public Direction CurrentDirection { get; set; } = Direction.Right;
    public Direction NextDirection { get; set; } = Direction.Right;

    public int Score { get; set; }
    public int FoodEaten { get; set; }
    public GameStatus Status { get; set; } = GameStatus.NotStarted;
    public GameDifficulty Difficulty { get; set; } = GameDifficulty.Medium;

    public long ElapsedMs { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? GameOverTime { get; set; }

    /// <summary>
    /// Obtient la tête du serpent
    /// </summary>
    public Position? Head => SnakeBody.Count > 0 ? SnakeBody[0] : null;

    /// <summary>
    /// Obtient la queue du serpent
    /// </summary>
    public Position? Tail => SnakeBody.Count > 0 ? SnakeBody[^1] : null;

    /// <summary>
    /// Obtient la longueur du serpent
    /// </summary>
    public int Length => SnakeBody.Count;

    /// <summary>
    /// Vérifie si le serpent s'est heurté à lui-même
    /// </summary>
    public bool IsCollidingWithSelf => Head is not null && SnakeBody.Skip(1).Contains(Head);

    /// <summary>
    /// Vérifie si le serpent est sorti de la grille
    /// </summary>
    public bool IsOutOfBounds => Head is null || Head.X < 0 || Head.X >= GridWidth || Head.Y < 0 || Head.Y >= GridHeight;

    /// <summary>
    /// Crée une copie de l'état actuel
    /// </summary>
    public GameState Clone()
    {
        return new GameState
        {
            GridWidth = GridWidth,
            GridHeight = GridHeight,
            SnakeBody = [..SnakeBody.Select(p => new Position(p))],
            Food = Food is not null ? new Position(Food) : null,
            SpecialFood = SpecialFood is not null ? new Position(SpecialFood) : null,
            CurrentDirection = CurrentDirection,
            NextDirection = NextDirection,
            Score = Score,
            FoodEaten = FoodEaten,
            Status = Status,
            Difficulty = Difficulty,
            ElapsedMs = ElapsedMs,
            StartTime = StartTime,
            GameOverTime = GameOverTime
        };
    }
}
