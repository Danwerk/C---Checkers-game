using System.Globalization;
using Domain;
using GameBrain;
using Microsoft.EntityFrameworkCore;


namespace DAL.Db;

public class GamePlayRepositoryDb : IGamePlayRepository
{
    private readonly AppDbContext _dbContext;
    public string Name { get; set; } = "DB";

    public GamePlayRepositoryDb(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public CheckersGame GetGameById(int? id)
    {
        return _dbContext.CheckersGames
            .Include(g => g.CheckersOption)
            .Include(g => g.CheckersGameStates)
            .First(g => g.Id == id);

    }

    public CheckersGame GetGameByFileName(string name)
    {
        return _dbContext.CheckersGames
            .Include(g => g.CheckersGameStates)
            .Include(g => g.CheckersOption)
            .First(o => o.Name == name);

    }

    public List<string> GetAllSavedGamesNameList()
    {
        var res = _dbContext.CheckersGames
            .Include(g => g.CheckersGameStates)
            .OrderBy(g => g.Name)
            .ToList();
        return res.Select(o => o.Name).ToList();
    }

    public List<CheckersGame> GetAllGames()
    {
        return _dbContext.CheckersGames.Include(o => o.CheckersOption).OrderBy(o=>o.StartedAt).ToList();
    }


    public void SaveGame(CheckersGame game)
    {
        // Check if the game with given name already exists
        var gamesFromDb = _dbContext.CheckersGames.FirstOrDefault(g => g.Name == game.Name);

        if (gamesFromDb == null)
        {
            _dbContext.CheckersGames.Add(game);
            _dbContext.SaveChanges();
            return;
        }

        throw new ArgumentException($"You cannot use {game.Name} as name, because game with such name already exists");
    }


    public void DeleteGame(string name)
    {
        var gamesFromDb = GetGameByFileName(name);
        _dbContext.CheckersGames.Remove(gamesFromDb);
        _dbContext.SaveChanges();
    }

    public void UpdateGame(CheckersGame game)
    {
        var gamesFromDb = _dbContext.CheckersGames.FirstOrDefault(g => g.Name == game.Name);
        if (gamesFromDb == null)
        {
            throw new ArgumentException("Game not found in database.");
        }

        gamesFromDb.CheckersOption = game.CheckersOption;
        gamesFromDb.CheckersGameStates = game.CheckersGameStates;
        gamesFromDb.GameOverAt = game.GameOverAt;
        gamesFromDb.GameWonByPlayer = game.GameWonByPlayer;
        gamesFromDb.Player1Name = game.Player1Name;
        gamesFromDb.Player2Name = game.Player2Name;

        _dbContext.SaveChanges();
    }


    public void SaveGameState(CheckersGameState gameState)
    {
        throw new NotImplementedException();
    }
}