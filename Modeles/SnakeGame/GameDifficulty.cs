namespace NuitInfo.Rubeus.Modeles.SnakeGame;

/// <summary>
/// Énumération des niveaux de difficulté du jeu
/// </summary>
public enum GameDifficulty
{
    Easy,
    Medium,
    Hard,
    Insane
}

/// <summary>
/// Configuration des difficulté
/// </summary>
public static class DifficultyConfig
{
    /// <summary>
    /// Retourne le délai (en ms) entre chaque mouvement selon la difficulté
    /// </summary>
    public static int GetTickDelayMs(this GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => 150,
            GameDifficulty.Medium => 100,
            GameDifficulty.Hard => 60,
            GameDifficulty.Insane => 30,
            _ => 100
        };
    }

    /// <summary>
    /// Retourne le multiplicateur de score selon la difficulté
    /// </summary>
    public static decimal GetScoreMultiplier(this GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => 1m,
            GameDifficulty.Medium => 1.5m,
            GameDifficulty.Hard => 2.5m,
            GameDifficulty.Insane => 5m,
            _ => 1m
        };
    }
}
