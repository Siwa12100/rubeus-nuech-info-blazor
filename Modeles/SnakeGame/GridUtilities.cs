namespace NuitInfo.Rubeus.Modeles.SnakeGame;

/// <summary>
/// Utilitaires et extensions pour la grille de jeu
/// </summary>
public static class GridUtilities
{
    /// <summary>
    /// Obtient tous les voisins d'une position (8 directions)
    /// </summary>
    public static List<Position> GetNeighbors(Position position)
    {
        return new List<Position>
        {
            new(position.X - 1, position.Y - 1), // Top-left
            new(position.X, position.Y - 1),     // Top
            new(position.X + 1, position.Y - 1), // Top-right
            new(position.X - 1, position.Y),     // Left
            new(position.X + 1, position.Y),     // Right
            new(position.X - 1, position.Y + 1), // Bottom-left
            new(position.X, position.Y + 1),     // Bottom
            new(position.X + 1, position.Y + 1)  // Bottom-right
        };
    }

    /// <summary>
    /// Obtient les 4 voisins cardinaux (haut, bas, gauche, droite)
    /// </summary>
    public static List<Position> GetCardinalNeighbors(Position position)
    {
        return new List<Position>
        {
            new(position.X, position.Y - 1),     // Top
            new(position.X, position.Y + 1),     // Bottom
            new(position.X - 1, position.Y),     // Left
            new(position.X + 1, position.Y)      // Right
        };
    }

    /// <summary>
    /// Filtre les positions valides dans une liste
    /// </summary>
    public static List<Position> FilterValid(IEnumerable<Position> positions, int gridWidth, int gridHeight)
    {
        return positions
            .Where(p => p.X >= 0 && p.X < gridWidth && p.Y >= 0 && p.Y < gridHeight)
            .ToList();
    }

    /// <summary>
    /// Trouve le chemin le plus court entre deux positions (BFS simplifié)
    /// </summary>
    public static List<Position> FindPath(Position start, Position goal, int gridWidth, int gridHeight, List<Position> obstacles)
    {
        var queue = new Queue<(Position, List<Position>)>();
        var visited = new HashSet<Position> { start };
        var obstacleSet = new HashSet<Position>(obstacles);

        queue.Enqueue((start, new List<Position> { start }));

        while (queue.Count > 0)
        {
            var (current, path) = queue.Dequeue();

            if (current.Equals(goal))
                return path;

            foreach (var neighbor in GetCardinalNeighbors(current))
            {
                if (!visited.Contains(neighbor) &&
                    neighbor.X >= 0 && neighbor.X < gridWidth &&
                    neighbor.Y >= 0 && neighbor.Y < gridHeight &&
                    !obstacleSet.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    var newPath = new List<Position>(path) { neighbor };
                    queue.Enqueue((neighbor, newPath));
                }
            }
        }

        return new List<Position>(); // Pas de chemin trouvé
    }

    /// <summary>
    /// Calcule la zone de couverture du serpent (pour éviter de le bloquer)
    /// </summary>
    public static List<Position> GetSnakeCoverage(List<Position> snakeBody)
    {
        var coverage = new HashSet<Position>(snakeBody);

        // Ajoute les voisins directs pour prédire où le serpent peut aller
        foreach (var segment in snakeBody.Take(3))
        {
            foreach (var neighbor in GetCardinalNeighbors(segment))
            {
                coverage.Add(neighbor);
            }
        }

        return coverage.ToList();
    }

    /// <summary>
    /// Génère une position aléatoire libre sur la grille
    /// </summary>
    public static Position? GetRandomFreePosition(int gridWidth, int gridHeight, List<Position> occupied)
    {
        var occupiedSet = new HashSet<Position>(occupied);
        var random = new Random();

        // Essaie 50 fois avant d'abandonner
        for (int i = 0; i < 50; i++)
        {
            var pos = new Position(random.Next(0, gridWidth), random.Next(0, gridHeight));
            if (!occupiedSet.Contains(pos))
                return pos;
        }

        // Fallback : cherche une position libre dans la grille entière
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                var pos = new Position(x, y);
                if (!occupiedSet.Contains(pos))
                    return pos;
            }
        }

        return null; // Grille complètement remplie
    }

    /// <summary>
    /// Calcule le pourcentage de la grille occupée par le serpent
    /// </summary>
    public static double GetOccupancyPercentage(List<Position> snakeBody, int gridWidth, int gridHeight)
    {
        var totalCells = gridWidth * gridHeight;
        return (snakeBody.Count / (double)totalCells) * 100;
    }

    /// <summary>
    /// Génère une grille visuelle pour debug (représentation textuelle)
    /// </summary>
    public static string DebugVisualize(GameState state)
    {
        var grid = new char[state.GridHeight][];
        for (int i = 0; i < state.GridHeight; i++)
        {
            grid[i] = Enumerable.Repeat('.', state.GridWidth).ToArray();
        }

        // Nourriture
        if (state.Food != null && state.Food.X >= 0 && state.Food.X < state.GridWidth &&
            state.Food.Y >= 0 && state.Food.Y < state.GridHeight)
        {
            grid[state.Food.Y][state.Food.X] = 'F';
        }

        // Nourriture spéciale
        if (state.SpecialFood != null && state.SpecialFood.X >= 0 && state.SpecialFood.X < state.GridWidth &&
            state.SpecialFood.Y >= 0 && state.SpecialFood.Y < state.GridHeight)
        {
            grid[state.SpecialFood.Y][state.SpecialFood.X] = '*';
        }

        // Serpent
        for (int i = state.SnakeBody.Count - 1; i >= 0; i--)
        {
            var segment = state.SnakeBody[i];
            if (segment.X >= 0 && segment.X < state.GridWidth && segment.Y >= 0 && segment.Y < state.GridHeight)
            {
                grid[segment.Y][segment.X] = i == 0 ? 'H' : 's';
            }
        }

        // Convertir en string
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < state.GridHeight; i++)
        {
            result.AppendLine(new string(grid[i]));
        }

        return result.ToString();
    }
}
