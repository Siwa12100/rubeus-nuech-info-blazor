using NuitInfo.Rubeus.Modeles.SnakeGame;

namespace NuitInfo.Rubeus.Repositories;

/// <summary>
/// Interface pour le service de gestion du jeu Snake
/// </summary>
public interface ISnakeGameEngine
{
    GameState GetCurrentState();
    void Initialize(GameDifficulty difficulty = GameDifficulty.Medium, int gridWidth = 20, int gridHeight = 15);
    void Start();
    void Pause();
    void Resume();
    void Reset();
    void SetNextDirection(Direction direction);
    void Update();
    List<SnakeGameEvent> GetEventsSinceLastUpdate();
    bool IsGameActive { get; }
}

/// <summary>
/// Implémentation du moteur de jeu Snake
/// </summary>
public class SnakeGameEngine : ISnakeGameEngine
{
    private GameState _gameState;
    private readonly Random _random = new();
    private readonly List<SnakeGameEvent> _events = [];
    private long _ticksSinceLastUpdate = 0;
    private DateTime _lastUpdateTime = DateTime.UtcNow;

    public GameState GetCurrentState() => _gameState.Clone();
    public bool IsGameActive => _gameState.Status == GameStatus.Playing;

    public SnakeGameEngine()
    {
        _gameState = new GameState();
    }

    /// <summary>
    /// Initialise une nouvelle partie
    /// </summary>
    public void Initialize(GameDifficulty difficulty = GameDifficulty.Medium, int gridWidth = 20, int gridHeight = 15)
    {
        _gameState = new GameState
        {
            GridWidth = gridWidth,
            GridHeight = gridHeight,
            Difficulty = difficulty,
            Status = GameStatus.NotStarted,
            Score = 0,
            FoodEaten = 0,
            ElapsedMs = 0,
            StartTime = DateTime.UtcNow
        };

        InitializeSnakeBody();
        SpawnFood();
        _events.Clear();
        _ticksSinceLastUpdate = 0;
        _lastUpdateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Initialise le corps du serpent au centre de la grille
    /// </summary>
    private void InitializeSnakeBody()
    {
        _gameState.SnakeBody.Clear();

        // Crée un serpent de 3 segments au centre
        int startX = _gameState.GridWidth / 2;
        int startY = _gameState.GridHeight / 2;

        for (int i = 0; i < 3; i++)
        {
            _gameState.SnakeBody.Add(new Position(startX - i, startY));
        }

        _gameState.CurrentDirection = Direction.Right;
        _gameState.NextDirection = Direction.Right;
    }

    /// <summary>
    /// Démarre la partie
    /// </summary>
    public void Start()
    {
        if (_gameState.Status == GameStatus.NotStarted)
        {
            _gameState.Status = GameStatus.Playing;
            _gameState.StartTime = DateTime.UtcNow;
            _lastUpdateTime = DateTime.UtcNow;

            var startEvent = new SnakeGameEvent(SnakeGameEventType.GameStarted, _gameState.ElapsedMs)
            {
                Message = "Partie démarrée"
            };
            _events.Add(startEvent);
        }
    }

    /// <summary>
    /// Met la partie en pause
    /// </summary>
    public void Pause()
    {
        if (_gameState.Status == GameStatus.Playing)
        {
            _gameState.Status = GameStatus.Paused;
            var pauseEvent = new SnakeGameEvent(SnakeGameEventType.GamePaused, _gameState.ElapsedMs)
            {
                Message = "Partie mise en pause"
            };
            _events.Add(pauseEvent);
        }
    }

    /// <summary>
    /// Reprend la partie
    /// </summary>
    public void Resume()
    {
        if (_gameState.Status == GameStatus.Paused)
        {
            _gameState.Status = GameStatus.Playing;
            _lastUpdateTime = DateTime.UtcNow;
            var resumeEvent = new SnakeGameEvent(SnakeGameEventType.GameResumed, _gameState.ElapsedMs)
            {
                Message = "Partie reprise"
            };
            _events.Add(resumeEvent);
        }
    }

    /// <summary>
    /// Réinitialise la partie
    /// </summary>
    public void Reset()
    {
        var resetEvent = new SnakeGameEvent(SnakeGameEventType.GameReset, _gameState.ElapsedMs)
        {
            Message = "Partie réinitialisée"
        };
        _events.Add(resetEvent);

        var difficulty = _gameState.Difficulty;
        var width = _gameState.GridWidth;
        var height = _gameState.GridHeight;

        Initialize(difficulty, width, height);
    }

    /// <summary>
    /// Définit la prochaine direction du serpent
    /// </summary>
    public void SetNextDirection(Direction direction)
    {
        // Empêche le serpent de faire demi-tour directement
        if (_gameState.CurrentDirection.IsOpposite(direction))
            return;

        _gameState.NextDirection = direction;

        var directionEvent = new SnakeGameEvent(SnakeGameEventType.DirectionChanged, _gameState.ElapsedMs)
        {
            Message = $"Direction changée vers {direction}"
        };
        _events.Add(directionEvent);
    }

    /// <summary>
    /// Met à jour l'état du jeu
    /// </summary>
    public void Update()
    {
        if (_gameState.Status != GameStatus.Playing)
            return;

        var now = DateTime.UtcNow;
        var elapsed = (now - _lastUpdateTime).TotalMilliseconds;
        _lastUpdateTime = now;

        _gameState.ElapsedMs = (long)(now - _gameState.StartTime).TotalMilliseconds;

        int tickDelayMs = _gameState.Difficulty.GetTickDelayMs();
        _ticksSinceLastUpdate += (long)elapsed;

        // Effectue un tick si assez de temps s'est écoulé
        if (_ticksSinceLastUpdate >= tickDelayMs)
        {
            TickGame();
            _ticksSinceLastUpdate = 0;
        }
    }

    /// <summary>
    /// Effectue un tick du jeu (un mouvement du serpent)
    /// </summary>
    private void TickGame()
    {
        // Applique la nouvelle direction
        _gameState.CurrentDirection = _gameState.NextDirection;

        // Calcule la nouvelle position de la tête
        var delta = _gameState.CurrentDirection.GetDelta();
        var newHeadPosition = new Position(
            _gameState.Head!.X + delta.X,
            _gameState.Head.Y + delta.Y
        );

        // Vérifie les collisions avec les murs
        if (newHeadPosition.X < 0 || newHeadPosition.X >= _gameState.GridWidth ||
            newHeadPosition.Y < 0 || newHeadPosition.Y >= _gameState.GridHeight)
        {
            EndGame("Collision avec un mur!");
            return;
        }

        // Vérifie les collisions avec le corps
        if (_gameState.SnakeBody.Contains(newHeadPosition))
        {
            EndGame("Collision avec le corps du serpent!");
            return;
        }

        // Ajoute la nouvelle tête
        _gameState.SnakeBody.Insert(0, newHeadPosition);

        // Vérifie si le serpent a mangé de la nourriture
        bool foodEaten = false;
        if (newHeadPosition.Equals(_gameState.Food))
        {
            var scoreGained = CalculateFoodScore(false);
            _gameState.Score += scoreGained;
            _gameState.FoodEaten++;
            foodEaten = true;

            var foodEvent = new SnakeGameEvent(SnakeGameEventType.FoodEaten, _gameState.ElapsedMs)
            {
                Position = newHeadPosition,
                ScoreGained = scoreGained,
                Message = $"Nourriture mangée! +{scoreGained} points"
            };
            _events.Add(foodEvent);

            SpawnFood();
        }
        // Vérifie si le serpent a mangé la nourriture spéciale
        else if (_gameState.SpecialFood is not null && newHeadPosition.Equals(_gameState.SpecialFood))
        {
            var scoreGained = CalculateFoodScore(true);
            _gameState.Score += scoreGained;
            _gameState.FoodEaten++;
            foodEaten = true;

            var specialFoodEvent = new SnakeGameEvent(SnakeGameEventType.SpecialFoodEaten, _gameState.ElapsedMs)
            {
                Position = newHeadPosition,
                ScoreGained = scoreGained,
                Message = $"Nourriture spéciale mangée! +{scoreGained} points"
            };
            _events.Add(specialFoodEvent);

            _gameState.SpecialFood = null;

            // Chance de générer une nouvelle nourriture spéciale
            if (_random.Next(0, 100) < 30)
            {
                SpawnSpecialFood();
            }
        }

        // Si aucune nourriture n'a été mangée, enlève la queue
        if (!foodEaten)
        {
            _gameState.SnakeBody.RemoveAt(_gameState.SnakeBody.Count - 1);
        }

        // Vérifie la victoire (longueur arbitraire de 50)
        if (_gameState.Length >= 50)
        {
            _gameState.Status = GameStatus.Victory;
            _gameState.GameOverTime = DateTime.UtcNow;

            var victoryEvent = new SnakeGameEvent(SnakeGameEventType.GameOver, _gameState.ElapsedMs)
            {
                Message = "Victoire! Vous avez atteint la limite du serpent!"
            };
            _events.Add(victoryEvent);
        }
    }

    /// <summary>
    /// Termine la partie
    /// </summary>
    private void EndGame(string reason)
    {
        _gameState.Status = GameStatus.GameOver;
        _gameState.GameOverTime = DateTime.UtcNow;

        var gameOverEvent = new SnakeGameEvent(SnakeGameEventType.GameOver, _gameState.ElapsedMs)
        {
            Message = reason
        };
        _events.Add(gameOverEvent);
    }

    /// <summary>
    /// Calcule les points gagnés en mangeant de la nourriture
    /// </summary>
    private int CalculateFoodScore(bool isSpecialFood)
    {
        var baseScore = isSpecialFood ? 50 : 10;
        var multiplier = _gameState.Difficulty.GetScoreMultiplier();
        return (int)(baseScore * multiplier);
    }

    /// <summary>
    /// Génère une nouvelle nourriture à une position aléatoire
    /// </summary>
    private void SpawnFood()
    {
        Position? newFood;
        do
        {
            newFood = new Position(
                _random.Next(0, _gameState.GridWidth),
                _random.Next(0, _gameState.GridHeight)
            );
        } while (_gameState.SnakeBody.Contains(newFood));

        _gameState.Food = newFood;
    }

    /// <summary>
    /// Génère une nouvelle nourriture spéciale
    /// </summary>
    private void SpawnSpecialFood()
    {
        Position? newSpecialFood;
        do
        {
            newSpecialFood = new Position(
                _random.Next(0, _gameState.GridWidth),
                _random.Next(0, _gameState.GridHeight)
            );
        } while (_gameState.SnakeBody.Contains(newSpecialFood) || newSpecialFood.Equals(_gameState.Food));

        _gameState.SpecialFood = newSpecialFood;
    }

    /// <summary>
    /// Retourne les événements depuis la dernière mise à jour
    /// </summary>
    public List<SnakeGameEvent> GetEventsSinceLastUpdate()
    {
        var result = new List<SnakeGameEvent>(_events);
        _events.Clear();
        return result;
    }
}
