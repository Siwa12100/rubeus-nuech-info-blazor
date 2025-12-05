namespace NuitInfo.Rubeus.Modeles.SnakeGame;

/// <summary>
/// Représente une position sur la grille du jeu Snake
/// </summary>
public class Position : IEquatable<Position>
{
    public int X { get; set; }
    public int Y { get; set; }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Position(Position other) : this(other.X, other.Y)
    {
    }

    public bool Equals(Position? other)
    {
        return other is not null && X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Position);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}
