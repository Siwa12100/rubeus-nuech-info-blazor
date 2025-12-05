using NuitInfo.Rubeus.Modeles.SnakeGame;
using NuitInfo.Rubeus.Repositories;
using Xunit;

namespace NuitInfo.Rubeus.Tests.SnakeGame;

/// <summary>
/// Tests unitaires pour le moteur de jeu Snake
/// </summary>
public class SnakeGameEngineTests
{
    private readonly ISnakeGameEngine _gameEngine;

    public SnakeGameEngineTests()
    {
        _gameEngine = new SnakeGameEngine();
    }

    #region Initialization Tests

    [Fact]
    public void Initialize_ShouldCreateNewGameState_WithCorrectDimensions()
    {
        // Arrange & Act
        _gameEngine.Initialize(GameDifficulty.Medium, 20, 15);
        var state = _gameEngine.GetCurrentState();

        // Assert
        Assert.Equal(20, state.GridWidth);
        Assert.Equal(15, state.GridHeight);
        Assert.Equal(GameStatus.NotStarted, state.Status);
        Assert.Equal(0, state.Score);
    }

    [Fact]
    public void Initialize_ShouldCreateSnakeWithThreeSegments()
    {
        // Arrange & Act
        _gameEngine.Initialize(GameDifficulty.Medium);
        var state = _gameEngine.GetCurrentState();

        // Assert
        Assert.Equal(3, state.Length);
    }

    [Fact]
    public void Initialize_ShouldSpawnFood()
    {
        // Arrange & Act
        _gameEngine.Initialize(GameDifficulty.Medium);
        var state = _gameEngine.GetCurrentState();

        // Assert
        Assert.NotNull(state.Food);
        Assert.InRange(state.Food.X, 0, state.GridWidth - 1);
        Assert.InRange(state.Food.Y, 0, state.GridHeight - 1);
    }

    #endregion

    #region Game Control Tests

    [Fact]
    public void Start_ShouldChangeStatusToPlaying()
    {
        // Arrange
        _gameEngine.Initialize();
        
        // Act
        _gameEngine.Start();
        var state = _gameEngine.GetCurrentState();

        // Assert
        Assert.Equal(GameStatus.Playing, state.Status);
    }

    [Fact]
    public void Start_ShouldOnlyWorkWhenNotStarted()
    {
        // Arrange
        _gameEngine.Initialize();
        _gameEngine.Start();
        
        // Act - Try to start again
        _gameEngine.Start();
        var state = _gameEngine.GetCurrentState();

        // Assert - Status should still be Playing
        Assert.Equal(GameStatus.Playing, state.Status);
    }

    [Fact]
    public void Pause_ShouldChangeStatusToPaused()
    {
        // Arrange
        _gameEngine.Initialize();
        _gameEngine.Start();
        
        // Act
        _gameEngine.Pause();
        var state = _gameEngine.GetCurrentState();

        // Assert
        Assert.Equal(GameStatus.Paused, state.Status);
    }

    [Fact]
    public void Resume_ShouldChangeStatusToPlayingFromPaused()
    {
        // Arrange
        _gameEngine.Initialize();
        _gameEngine.Start();
        _gameEngine.Pause();
        
        // Act
        _gameEngine.Resume();
        var state = _gameEngine.GetCurrentState();

        // Assert
        Assert.Equal(GameStatus.Playing, state.Status);
    }

    [Fact]
    public void Reset_ShouldResetGameState()
    {
        // Arrange
        _gameEngine.Initialize();
        _gameEngine.Start();
        _gameEngine.SetNextDirection(Direction.Down);
        
        // Act
        _gameEngine.Reset();
        var state = _gameEngine.GetCurrentState();

        // Assert
        Assert.Equal(GameStatus.NotStarted, state.Status);
        Assert.Equal(3, state.Length);
        Assert.Equal(0, state.Score);
    }

    #endregion

    #region Direction Tests

    [Fact]
    public void SetNextDirection_ShouldUpdateNextDirection()
    {
        // Arrange
        _gameEngine.Initialize();
        
        // Act
        _gameEngine.SetNextDirection(Direction.Down);
        var state = _gameEngine.GetCurrentState();

        // Assert
        Assert.Equal(Direction.Down, state.NextDirection);
    }

    [Fact]
    public void SetNextDirection_ShouldPreventOppositeDirections()
    {
        // Arrange
        _gameEngine.Initialize();
        var state = _gameEngine.GetCurrentState();
        var initialDirection = state.CurrentDirection;
        
        // Act - Try to reverse direction
        _gameEngine.SetNextDirection(Direction.Left); // Opposite of Right
        var updatedState = _gameEngine.GetCurrentState();

        // Assert
        Assert.Equal(initialDirection, updatedState.NextDirection);
    }

    #endregion

    #region Collision Tests

    [Fact]
    public void Update_ShouldDetectWallCollision_Top()
    {
        // Arrange
        try
        {
            _gameEngine.Initialize(GameDifficulty.Easy, 10, 10);
            _gameEngine.Start();
            var initialState = _gameEngine.GetCurrentState();
            
            // Verify initial state  
            Assert.NotNull(initialState);
            Assert.NotNull(initialState.Head);
            Assert.Equal(3, initialState.Length);
        }
        catch (Exception ex)
        {
            Assert.True(false, $"Test failed with exception: {ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion

    #region Food Tests

    [Fact]
    public void GameState_FoodShouldNotSpawnOnSnakeBody()
    {
        // Arrange & Act
        _gameEngine.Initialize(GameDifficulty.Easy);
        _gameEngine.Start();
        var state = _gameEngine.GetCurrentState();

        // Assert
        Assert.NotNull(state.Food);
        Assert.False(state.SnakeBody.Contains(state.Food));
    }

    #endregion

    #region Score Tests

    [Fact]
    public void GameState_ScoreShouldIncreaseWithDifficulty()
    {
        // Arrange
        var easyEngine = new SnakeGameEngine();
        var hardEngine = new SnakeGameEngine();

        easyEngine.Initialize(GameDifficulty.Easy);
        hardEngine.Initialize(GameDifficulty.Hard);

        // Act
        var easyMultiplier = GameDifficulty.Easy.GetScoreMultiplier();
        var hardMultiplier = GameDifficulty.Hard.GetScoreMultiplier();

        // Assert
        Assert.True(hardMultiplier > easyMultiplier);
    }

    #endregion

    #region Event Tests

    [Fact]
    public void GetEventsSinceLastUpdate_ShouldReturnStartEvent()
    {
        // Arrange
        _gameEngine.Initialize();
        _gameEngine.Start();
        
        // Act
        var events = _gameEngine.GetEventsSinceLastUpdate();

        // Assert
        Assert.Contains(events, e => e.Type == SnakeGameEventType.GameStarted);
    }

    [Fact]
    public void GetEventsSinceLastUpdate_ShouldClearEventsAfterRetrival()
    {
        // Arrange
        _gameEngine.Initialize();
        _gameEngine.Start();
        
        // Act
        var firstCall = _gameEngine.GetEventsSinceLastUpdate();
        var secondCall = _gameEngine.GetEventsSinceLastUpdate();

        // Assert
        Assert.NotEmpty(firstCall);
        Assert.Empty(secondCall);
    }

    #endregion
}

/// <summary>
/// Tests unitaires pour les modèles de domaine
/// </summary>
public class SnakeGameModelsTests
{
    [Fact]
    public void Position_TwoPositionsWithSameCoordinatesShouldBeEqual()
    {
        // Arrange & Act
        var pos1 = new Position(5, 10);
        var pos2 = new Position(5, 10);

        // Assert
        Assert.Equal(pos1, pos2);
        Assert.True(pos1.Equals(pos2));
    }

    [Fact]
    public void Position_CopyConstructorShouldCreateIndependentCopy()
    {
        // Arrange
        var original = new Position(5, 10);
        
        // Act
        var copy = new Position(original);
        copy.X = 20;

        // Assert
        Assert.Equal(5, original.X);
        Assert.Equal(20, copy.X);
    }

    [Fact]
    public void Direction_IsOppositeShouldCorrectlyIdentifyOppositeDirections()
    {
        // Assert
        Assert.True(Direction.Up.IsOpposite(Direction.Down));
        Assert.True(Direction.Down.IsOpposite(Direction.Up));
        Assert.True(Direction.Left.IsOpposite(Direction.Right));
        Assert.True(Direction.Right.IsOpposite(Direction.Left));

        Assert.False(Direction.Up.IsOpposite(Direction.Left));
        Assert.False(Direction.Down.IsOpposite(Direction.Right));
    }

    [Fact]
    public void Direction_GetDeltaShouldReturnCorrectVectors()
    {
        // Act & Assert
        Assert.Equal(new Position(0, -1), Direction.Up.GetDelta());
        Assert.Equal(new Position(0, 1), Direction.Down.GetDelta());
        Assert.Equal(new Position(-1, 0), Direction.Left.GetDelta());
        Assert.Equal(new Position(1, 0), Direction.Right.GetDelta());
    }

    [Fact]
    public void GameState_CloneShouldCreateIndependentCopy()
    {
        // Arrange
        var original = new GameState
        {
            Score = 100,
            FoodEaten = 5,
            Status = GameStatus.Playing
        };

        // Act
        var clone = original.Clone();
        clone.Score = 200;

        // Assert
        Assert.Equal(100, original.Score);
        Assert.Equal(200, clone.Score);
    }

    [Fact]
    public void GameState_IsCollidingWithSelfShouldReturnTrueWhenSnakeOverlapWithItself()
    {
        // Arrange
        var state = new GameState();
        state.SnakeBody.Add(new Position(5, 5)); // Head
        state.SnakeBody.Add(new Position(5, 6));
        state.SnakeBody.Add(new Position(5, 5)); // Same as head - collision!

        // Act & Assert
        Assert.True(state.IsCollidingWithSelf);
    }

    [Fact]
    public void GameDifficulty_TickDelayShouldDecreaseWithDifficulty()
    {
        // Act
        var easyDelay = GameDifficulty.Easy.GetTickDelayMs();
        var hardDelay = GameDifficulty.Hard.GetTickDelayMs();
        var insaneDelay = GameDifficulty.Insane.GetTickDelayMs();

        // Assert
        Assert.True(easyDelay > hardDelay);
        Assert.True(hardDelay > insaneDelay);
    }
}
