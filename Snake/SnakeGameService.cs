using NuitInfo.Rubeus.Modeles.SnakeGame;
using NuitInfo.Rubeus.Repositories;

namespace NuitInfo.Rubeus.Snake;

/// <summary>
/// Point d'accès public pour les services Snake Game
/// Facilite l'utilisation du jeu dans d'autres parties de l'application
/// </summary>
public static class SnakeGameService
{
    /// <summary>
    /// Crée une nouvelle instance du moteur de jeu
    /// </summary>
    public static ISnakeGameEngine CreateEngine()
    {
        return new SnakeGameEngine();
    }

    /// <summary>
    /// Crée un nouvel état de jeu avec les configurations par défaut
    /// </summary>
    public static GameState CreateGameState()
    {
        return new GameState();
    }

    /// <summary>
    /// Valide si une position est valide pour une grille donnée
    /// </summary>
    public static bool IsValidPosition(Position position, int gridWidth, int gridHeight)
    {
        return position.X >= 0 && position.X < gridWidth &&
               position.Y >= 0 && position.Y < gridHeight;
    }

    /// <summary>
    /// Calcule la distance Manhattan entre deux positions
    /// </summary>
    public static int ManhattanDistance(Position pos1, Position pos2)
    {
        return Math.Abs(pos1.X - pos2.X) + Math.Abs(pos1.Y - pos2.Y);
    }

    /// <summary>
    /// Obtient tous les espaces libres sur la grille
    /// </summary>
    public static List<Position> GetFreePositions(GameState state)
    {
        var occupied = new HashSet<Position>(state.SnakeBody);
        if (state.Food is not null)
            occupied.Add(state.Food);
        if (state.SpecialFood is not null)
            occupied.Add(state.SpecialFood);

        var free = new List<Position>();
        for (int x = 0; x < state.GridWidth; x++)
        {
            for (int y = 0; y < state.GridHeight; y++)
            {
                var pos = new Position(x, y);
                if (!occupied.Contains(pos))
                {
                    free.Add(pos);
                }
            }
        }

        return free;
    }
}
