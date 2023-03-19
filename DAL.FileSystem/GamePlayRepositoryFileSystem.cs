
using System.Text.Json;
using Domain;
using GameBrain;

namespace DAL.FileSystem;

public class GamePlayRepositoryFileSystem : IGamePlayRepository
{
     private const string FileExtension = "json";
     private readonly string _gamesDirectory = "." + Path.DirectorySeparatorChar + "games";

     // Return list of game states
     public List<string> GetAllSavedGamesNameList()
     {
         CheckOrCreateDirectory();

         var res = new List<string>();

         foreach (var fileName in Directory.GetFileSystemEntries(_gamesDirectory, "*." + FileExtension))
         {
             res.Add(Path.GetFileNameWithoutExtension(fileName));
         }

         return res;
     }

     public List<CheckersGame> GetAllGames()
     {
         throw new NotImplementedException();
     }


     public CheckersGame GetGameById(int? id)
     {
         throw new NotImplementedException();
     }

     public CheckersGame GetGameByFileName(string name)
     {
         var fileContent = File.ReadAllText(GetFileName(name));
         var game = JsonSerializer.Deserialize<CheckersGame>(fileContent);
         if (game == null)
         {
             throw new NullReferenceException($"Could not deserialize: {fileContent}");
         }
         return game;
     }
     
     
     public void SaveGame(CheckersGame game)
     {
         if (GetAllSavedGamesNameList().Contains(game.Name))
         {
             throw new ArgumentException($"Game with such name is already taken, please choose another name for the game.");
         }
         CheckOrCreateDirectory();
         var fileContent = JsonSerializer.Serialize(game);
         File.WriteAllText(GetFileName(game.Name), fileContent);
     }
    
     
     public void DeleteGame(string id)
     {
         File.Delete(GetFileName(id));
     }

     
     public void UpdateGame(CheckersGame game)
     {
         CheckOrCreateDirectory();
         var fileContent = JsonSerializer.Serialize(game);
         File.WriteAllText(GetFileName(game.Name), fileContent);
     }

     
     public void SaveGameState(CheckersGameState gameState)
     {
         var game = gameState.CheckersGame;
         game!.CheckersGameStates!.Add(gameState);
     }


     private void CheckOrCreateDirectory()
     {
         if (!Directory.Exists(_gamesDirectory))
         {
             Directory.CreateDirectory(_gamesDirectory);
         }
     }

     private string GetFileName(string id)
     {
         return _gamesDirectory +
                Path.DirectorySeparatorChar +
                id + "." + FileExtension;
     }


     // Convert jagged array to multidimensional array. We can deserialize only
     // jagged array so in that case as we get from json jagged array, it has to be
     // converted then into 2d array
     private T[,] JaggedTo2D<T>(T[][]? gameState)
     {
         try
         {
             int firstDim = gameState!.Length;
             int secondDim = gameState.GroupBy(row => row.Length).Single().Key; // throws InvalidOperationException if source is not rectangular

             var result = new T[firstDim, secondDim];
             for (int i = 0; i < firstDim; ++i)
             for (int j = 0; j < secondDim; ++j)
                 result[i, j] = gameState[i][j];

             return result;
         }
         catch (InvalidOperationException)
         {
             throw new InvalidOperationException("The given jagged array is not rectangular.");
         }
         
     }
     
     
     //Convert 2d array to Jagged array
     private T[][] ToJagged<T>(T[,] twoDimensionalArray)
     {
         int rowsFirstIndex = twoDimensionalArray.GetLowerBound(0);
         int rowsLastIndex = twoDimensionalArray.GetUpperBound(0);
         int numberOfRows = rowsLastIndex + 1;

         int columnsFirstIndex = twoDimensionalArray.GetLowerBound(1);
         int columnsLastIndex = twoDimensionalArray.GetUpperBound(1);
         int numberOfColumns = columnsLastIndex + 1;

         T[][] jaggedArray = new T[numberOfRows][];
         for (int i = rowsFirstIndex; i <= rowsLastIndex; i++)
         {
             jaggedArray[i] = new T[numberOfColumns];

             for (int j = columnsFirstIndex; j <= columnsLastIndex; j++)
             {
                 jaggedArray[i][j] = twoDimensionalArray[i, j];
             }
         }
         return jaggedArray;
     }
     
     // 2d array of integers to 2d array of enum objects.
     private EGamePiece[,] IntToEnumArray(int[,] board)
     {
         var enumBoard = new EGamePiece[board.GetLength(0), board.GetLength(1)];

         for (var i = 0; i < board.GetLength(0); i++)
         {
             for (var j = 0; j < board.GetLength(1); j++)
             {
                 enumBoard[i, j] = (EGamePiece)board[i, j];
             }
         }
         return enumBoard;
     }
     
     // 2d array of enum objects to 2d array of integers.
     private int[,] EnumToIntArray(EGamePiece[,] board)
     {
         var intBoard = new int[board.GetLength(0), board.GetLength(1)];

         for (var i = 0; i < board.GetLength(0); i++)
         {
             for (var j = 0; j < board.GetLength(1); j++)
             {
                 intBoard[i, j] = (int)board[i, j];
             }
         }

         return intBoard;
     }
}