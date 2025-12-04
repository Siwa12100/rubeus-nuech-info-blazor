namespace NuitInfo.Rubeus.Modeles.SnakeGame;

/// <summary>
/// Représente les statistiques d'une partie terminée
/// </summary>
public class GameStatistics
{
    public int FinalScore { get; set; }
    public int FinalLength { get; set; }
    public int FoodEaten { get; set; }
    public long DurationMs { get; set; }
    public GameDifficulty Difficulty { get; set; }
    public GameStatus FinalStatus { get; set; }
    public DateTime PlayedAt { get; set; }

    public GameStatistics() { }

    public GameStatistics(GameState state)
    {
        FinalScore = state.Score;
        FinalLength = state.Length;
        FoodEaten = state.FoodEaten;
        DurationMs = state.ElapsedMs;
        Difficulty = state.Difficulty;
        FinalStatus = state.Status;
        PlayedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calcule la vitesse moyenne de consommation (secondes par nourriture)
    /// </summary>
    public double GetAverageEatingPace()
    {
        if (FoodEaten == 0) return 0;
        return DurationMs / 1000.0 / FoodEaten;
    }

    /// <summary>
    /// Calcule le score moyen par seconde
    /// </summary>
    public double GetScorePerSecond()
    {
        if (DurationMs == 0) return 0;
        return (FinalScore / (DurationMs / 1000.0));
    }

    /// <summary>
    /// Obtient une description textuelle des statistiques
    /// </summary>
    public string GetSummary()
    {
        var status = FinalStatus switch
        {
            GameStatus.Victory => "🎉 VICTOIRE",
            GameStatus.GameOver => "💀 GAME OVER",
            _ => "Inachevée"
        };

        var duration = TimeSpan.FromMilliseconds(DurationMs);
        return $"{status} | Score: {FinalScore} | Longueur: {FinalLength} | " +
               $"Temps: {duration:mm\\:ss} | Difficulté: {Difficulty}";
    }

    public override string ToString() => GetSummary();
}

/// <summary>
/// Gère l'historique des parties jouées
/// </summary>
public class GameHistory
{
    private readonly List<GameStatistics> _games = [];
    public IReadOnlyList<GameStatistics> Games => _games.AsReadOnly();

    /// <summary>
    /// Ajoute une partie aux statistiques
    /// </summary>
    public void AddGame(GameState state)
    {
        var stats = new GameStatistics(state);
        _games.Add(stats);
    }

    /// <summary>
    /// Obtient le meilleur score de l'historique
    /// </summary>
    public int? GetBestScore()
    {
        return _games.Count > 0 ? _games.Max(g => g.FinalScore) : null;
    }

    /// <summary>
    /// Obtient la longueur maximale atteinte
    /// </summary>
    public int? GetMaxLength()
    {
        return _games.Count > 0 ? _games.Max(g => g.FinalLength) : null;
    }

    /// <summary>
    /// Compte les victoires
    /// </summary>
    public int GetVictoryCount()
    {
        return _games.Count(g => g.FinalStatus == GameStatus.Victory);
    }

    /// <summary>
    /// Obtient les statistiques globales
    /// </summary>
    public string GetGlobalStats()
    {
        if (_games.Count == 0)
            return "Aucune partie jouée";

        var avgScore = _games.Average(g => g.FinalScore);
        var victories = GetVictoryCount();
        var totalTime = TimeSpan.FromMilliseconds(_games.Sum(g => g.DurationMs));

        return $"Parties: {_games.Count} | Victoires: {victories} | " +
               $"Score moyen: {avgScore:F1} | Temps total: {totalTime:hh\\:mm\\:ss}";
    }

    /// <summary>
    /// Efface l'historique
    /// </summary>
    public void Clear()
    {
        _games.Clear();
    }
}
