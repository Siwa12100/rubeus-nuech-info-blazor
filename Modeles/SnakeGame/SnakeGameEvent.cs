namespace NuitInfo.Rubeus.Modeles.SnakeGame;

/// <summary>
/// Énumération des événements qui peuvent survenir durant une partie
/// </summary>
public enum SnakeGameEventType
{
    FoodEaten,
    SpecialFoodEaten,
    Collision,
    DirectionChanged,
    GameStarted,
    GamePaused,
    GameResumed,
    GameOver,
    GameReset
}

/// <summary>
/// Représente un événement survenant durant une partie de Snake
/// </summary>
public class SnakeGameEvent
{
    public SnakeGameEventType Type { get; set; }
    public long Timestamp { get; set; }
    public Position? Position { get; set; }
    public int? ScoreGained { get; set; }
    public string? Message { get; set; }

    public SnakeGameEvent(SnakeGameEventType type, long timestamp)
    {
        Type = type;
        Timestamp = timestamp;
    }

    public override string ToString()
    {
        return $"[{Type}] {Timestamp}ms - {Message}";
    }
}
