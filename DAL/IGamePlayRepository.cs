using Domain;
using GameBrain;

namespace DAL;

public interface IGamePlayRepository
{
    
    CheckersGame? GetGameById(int? id);

    CheckersGame GetGameByFileName(string name);
    
    List<string> GetAllSavedGamesNameList();
    List<CheckersGame> GetAllGames();
    void SaveGame(CheckersGame game);
    void DeleteGame(string id);

    void UpdateGame(CheckersGame game);
    void SaveGameState(CheckersGameState gameState);



}