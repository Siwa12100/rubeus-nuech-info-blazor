namespace NuitInfo.Rubeus.Modeles.SnakeGame;

/// <summary>
/// Énumération des directions possibles pour le serpent
/// </summary>
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

/// <summary>
/// Utilitaires pour gérer les directions
/// </summary>
public static class DirectionExtensions
{
    /// <summary>
    /// Retourne le vecteur de déplacement pour une direction donnée
    /// </summary>
    public static Position GetDelta(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => new Position(0, -1),
            Direction.Down => new Position(0, 1),
            Direction.Left => new Position(-1, 0),
            Direction.Right => new Position(1, 0),
            _ => new Position(0, 0)
        };
    }

    /// <summary>
    /// Vérifie si deux directions sont opposées
    /// </summary>
    public static bool IsOpposite(this Direction current, Direction next)
    {
        return (current == Direction.Up && next == Direction.Down) ||
               (current == Direction.Down && next == Direction.Up) ||
               (current == Direction.Left && next == Direction.Right) ||
               (current == Direction.Right && next == Direction.Left);
    }
}
