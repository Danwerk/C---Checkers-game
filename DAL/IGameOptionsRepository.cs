using Domain;

namespace DAL;

public interface IGameOptionsRepository
{
    string Name { get; }
    List<string> GetGameOptionsList();
    
    CheckersOption GetGameOptions(string id);
    
    void SaveGameOptions(string id, CheckersOption option);
    
    void DeleteGameOptions(string id);
}