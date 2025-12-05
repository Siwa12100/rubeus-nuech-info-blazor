using NuitInfo.Rubeus.Modeles.SnakeGame;
using Xunit;

namespace NuitInfo.Rubeus.Tests.SnakeGame;

/// <summary>
/// Tests pour les statistiques et l'historique du jeu
/// </summary>
public class GameStatisticsTests
{
    [Fact]
    public void GameStatistics_CanBeCreatedFromGameState()
    {
        // Arrange
        var state = new GameState
        {
            Score = 100,
            SnakeBody = new List<Position>
            {
                new Position(10, 8),
                new Position(9, 8),
                new Position(8, 8),
                new Position(7, 8),
                new Position(6, 8)
            },
            FoodEaten = 10,
            Difficulty = GameDifficulty.Hard,
            Status = GameStatus.GameOver,
            ElapsedMs = 60000
        };

        // Act
        var stats = new GameStatistics(state);

        // Assert
        Assert.Equal(100, stats.FinalScore);
        Assert.Equal(5, stats.FinalLength);
        Assert.Equal(10, stats.FoodEaten);
    }

    [Fact]
    public void GameHistory_CanTrackMultipleGames()
    {
        // Arrange
        var history = new GameHistory();
        var state1 = new GameState { Score = 50, Status = GameStatus.GameOver };
        var state2 = new GameState { Score = 150, Status = GameStatus.Victory };

        // Act
        history.AddGame(state1);
        history.AddGame(state2);

        // Assert
        Assert.Equal(2, history.Games.Count);
        Assert.Equal(150, history.GetBestScore());
    }

    [Fact]
    public void GameHistory_CountsVictoriesCorrectly()
    {
        // Arrange
        var history = new GameHistory();
        history.AddGame(new GameState { Status = GameStatus.Victory });
        history.AddGame(new GameState { Status = GameStatus.Victory });
        history.AddGame(new GameState { Status = GameStatus.GameOver });

        // Act & Assert
        Assert.Equal(2, history.GetVictoryCount());
    }
}

/// <summary>
/// Tests pour les utilitaires de grille
/// </summary>
public class GridUtilitiesTests
{
    [Fact]
    public void GetNeighbors_ShouldReturnEightNeighbors()
    {
        // Act
        var neighbors = GridUtilities.GetNeighbors(new Position(5, 5));

        // Assert
        Assert.Equal(8, neighbors.Count);
    }

    [Fact]
    public void GetCardinalNeighbors_ShouldReturnFourNeighbors()
    {
        // Act
        var neighbors = GridUtilities.GetCardinalNeighbors(new Position(5, 5));

        // Assert
        Assert.Equal(4, neighbors.Count);
    }

    [Fact]
    public void FilterValid_ShouldRemoveOutOfBoundsPositions()
    {
        // Arrange
        var positions = new List<Position>
        {
            new(0, 0),
            new(5, 5),
            new(-1, 5),
            new(20, 10),
            new(10, 20)
        };

        // Act
        var valid = GridUtilities.FilterValid(positions, 10, 10);

        // Assert
        Assert.Equal(2, valid.Count);
        Assert.Contains(new Position(0, 0), valid);
        Assert.Contains(new Position(5, 5), valid);
    }

    [Fact]
    public void GetRandomFreePosition_ShouldReturnFreePosition()
    {
        // Arrange
        var occupied = new List<Position> { new(0, 0), new(1, 1) };

        // Act
        var freePos = GridUtilities.GetRandomFreePosition(10, 10, occupied);

        // Assert
        Assert.NotNull(freePos);
        Assert.False(occupied.Contains(freePos));
    }

    [Fact]
    public void GetOccupancyPercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var snakeBody = new List<Position> { new(0, 0), new(1, 0), new(2, 0) };

        // Act
        var occupancy = GridUtilities.GetOccupancyPercentage(snakeBody, 10, 10);

        // Assert
        Assert.Equal(3.0, occupancy); // 3 / 100 * 100 = 3%
    }

    [Fact]
    public void DebugVisualize_ShouldGenerateValidGrid()
    {
        // Arrange
        var state = new GameState
        {
            GridWidth = 5,
            GridHeight = 5
        };
        state.SnakeBody.Add(new Position(2, 2));
        state.Food = new Position(4, 4);

        // Act
        var visualization = GridUtilities.DebugVisualize(state);

        // Assert
        Assert.NotEmpty(visualization);
        Assert.Contains('H', visualization); // Head
        Assert.Contains('F', visualization); // Food
    }
}
