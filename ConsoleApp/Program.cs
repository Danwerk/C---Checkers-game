// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using ConsoleApp;
using ConsoleUI;
using DAL;
using DAL.Db;
using DAL.FileSystem;
using Domain;
using GameBrain;
using MenuSystem;
using Microsoft.EntityFrameworkCore;


var gameOptions = new CheckersOption();
gameOptions.Name = "Standard game";
CheckersBrain brain = new CheckersBrain(gameOptions);


var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite("Data Source=C:/Users/danyi/RiderProjects/temp/checkers.db").Options;
var ctx = new AppDbContext(dbOptions);

// Apply any non-applied migrations.
ctx.Database.Migrate();

// Default settings for gameOptions repo.
IGameOptionsRepository optionsRepoDb = new GameOptionsRepositoryDb(ctx);
IGameOptionsRepository optionsRepoFs = new GameOptionsRepositoryFileSystem();
IGameOptionsRepository optionsRepo = optionsRepoDb; // here we can choose between implementations (filesystem or DB)

// To avoid gameOptions duplications in DB 
if (!optionsRepo.GetGameOptionsList().Contains(gameOptions.Name))
{
    optionsRepo.SaveGameOptions(gameOptions.Name, gameOptions);
}
else
{
    gameOptions = optionsRepo.GetGameOptions(gameOptions.Name);
}

// Default settings for gameState repo
IGamePlayRepository gameRepoFs = new GamePlayRepositoryFileSystem();
IGamePlayRepository gameRepoDb = new GamePlayRepositoryDb(ctx);
IGamePlayRepository gameRepo = gameRepoDb;


var optionsMenu = new Menu(EMenuLevel.Second, "Checkers Options", new List<MenuItem>()
{
    new MenuItem("C", "Create options", CreateGameOptions),
    new MenuItem("O", "List saved options", ListGameOptions),
    new MenuItem("L", "Load options", LoadGameOptions),
    new MenuItem("D", "Delete options", DeleteGameOptions),
    new MenuItem("S", "Save current options", SaveCurrentOptions),
    new MenuItem("P", "Persistence method swap", SwapPersistenceEngine)
});


var mainMenu = new Menu(EMenuLevel.Main, "Checkers", new List<MenuItem>()
{
    new MenuItem("N", "New Game", DoNewGame),
    new MenuItem("L", "Load Game", LoadGame),
    new MenuItem("D", "Delete Game", DeleteGame),
    new MenuItem("O", "Options", optionsMenu.RunMenu)
});


mainMenu.RunMenu();


// Do new game and get the board
string DoNewGame()
{
    Console.WriteLine($"Game will be started with current loaded options {gameOptions}");
    WaitUserInput();
    string gameName;
    string playerName;
    ConsoleKey key;

    Console.Clear();
    brain = new CheckersBrain(gameOptions);


    var newGame = new CheckersGame
    {
        CheckersOption = gameOptions
    };


    do
    {
        Console.Clear();
        Console.WriteLine("Enter a name for the game: ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        gameName = Console.ReadLine()!;
        Console.ResetColor();
    } while (gameName == "" &&
             !(gameName.Length > 0) &&
             gameName.Contains('"'));

    newGame.Name = gameName;

    // Set up for player1
    do
    {
        Console.Clear();
        Console.WriteLine("Please enter Player1 name:");
        Console.ForegroundColor = ConsoleColor.Yellow;
        playerName = Console.ReadLine()!;
        Console.ResetColor();
    } while (playerName.Length > 0 && playerName.Contains('"'));

    newGame.Player1Name = playerName;


    do
    {
        Console.Clear();
        Console.WriteLine($"Press A if Player1 [{newGame.Player1Name}] is AI or H, if human");
        key = Console.ReadKey(true).Key;
    } while (key != ConsoleKey.A && key != ConsoleKey.H);

    switch (key)
    {
        case ConsoleKey.A:
            newGame.Player1Type = EPlayerType.Ai;
            break;
        case ConsoleKey.H:
            newGame.Player1Type = EPlayerType.Human;
            break;
    }


    // Set up for player2
    do
    {
        Console.Clear();
        Console.WriteLine("Please enter Player2 name:");
        Console.ForegroundColor = ConsoleColor.Yellow;
        playerName = Console.ReadLine()!;
        Console.ResetColor();
    } while (playerName.Length > 0 && playerName.Contains('"'));

    newGame.Player2Name = playerName;


    do
    {
        Console.Clear();
        Console.WriteLine($"Press A if Player2 [{newGame.Player2Name}] is AI or H, if human");
        key = Console.ReadKey(true).Key;
    } while (key != ConsoleKey.A && key != ConsoleKey.H);

    switch (key)
    {
        case ConsoleKey.A:
            newGame.Player2Type = EPlayerType.Ai;
            break;
        case ConsoleKey.H:
            newGame.Player2Type = EPlayerType.Human;
            break;
    }

    newGame.CheckersGameStates = new List<CheckersGameState>();


    var gameState = new CheckersGameState
    {
        NextMoveByBlack = gameOptions.BlackStarts,
        SerializedGameState = JsonSerializer.Serialize(brain.GameBoard)
    };

    newGame.CheckersGameStates.Add(gameState);
    gameRepo.SaveGame(newGame);

    Console.Clear();

    DoGamePlay(newGame);
    return "...";
}


// Actual gameplay is done here
void DoGamePlay(CheckersGame game)
{
    if (game.CheckersOption != null) brain = new CheckersBrain(game.CheckersOption);
    Play gamePlay = new Play();


    if (game.GameOverAt != null)
    {
        // If game has been ended, show just winner message with the board state.
        var gameState = game.CheckersGameStates!.Last();
        var board = JsonSerializer.Deserialize<EGamePiece?[][]>(gameState.SerializedGameState)!;
        brain.SetGameBoard(board, gameState);

        var isBlackWinner = BlackWon(game, board, gameState);
        SuccessMessage(
            $"This game has been ended. Game won by {game.GameWonByPlayer}{(isBlackWinner ? " - (BLACK)" : " - (WHITE)")}");

        UI.DrawGameBoard(brain.GetBoard());
    }

    // Load the game and do gameplay
    else
    {
        gamePlay.RunGame(game.Id, gameRepo, brain);
       
    }
}


// User chooses from list the game to load and play
string LoadGame()
{
    Console.Clear();

    Console.WriteLine("List of saved games: ");
    Console.ForegroundColor = ConsoleColor.Yellow;

    var games = gameRepo.GetAllGames();

    foreach (var game in games)
    {
        Console.WriteLine("<< " + game.Name + " >>");
    }

    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine("Choose game to load (type the name of the game): ");
    Console.ForegroundColor = ConsoleColor.Yellow;
    var choice = Console.ReadLine();
    Console.ResetColor();

    if (choice != null)
    {
        if (!gameRepo.GetAllSavedGamesNameList().Contains(choice))
        {
            WarningMessage("Game with such name does not exist");
        }
        else
        {
            CheckersGame g = gameRepo.GetGameByFileName(choice);
            DoGamePlay(g);
        }
    }

    //      WaitUserInput();
    return "...";
}


// List of all game options
string ListGameOptions()
{
    Console.Clear();
    Console.WriteLine("List of game options: ");
    Console.ForegroundColor = ConsoleColor.Yellow;
    foreach (var name in optionsRepo.GetGameOptionsList())
    {
        Console.WriteLine("<< " + name + " >>");
    }

    Console.ResetColor();
    WaitUserInput();

    return "...";
}


string ListGames()
{
    Console.Clear();
    Console.WriteLine("List of games: ");
    Console.ForegroundColor = ConsoleColor.Yellow;
    foreach (var name in gameRepo.GetAllSavedGamesNameList())
    {
        Console.WriteLine("<< " + name + " >>");
    }

    Console.ResetColor();
    WaitUserInput();

    return "...";
}


// Load game options
string LoadGameOptions()
{
    ListGameOptions();
    Console.Write("Enter options name: ");
    Console.ForegroundColor = ConsoleColor.Yellow;
    var optionsName = Console.ReadLine();

    if (OptionExist(optionsName))
    {
        gameOptions = optionsRepo.GetGameOptions(optionsName);
        Console.ResetColor();


        Console.Write("\nOptions");
        SuccessMessage($"[{gameOptions}]");
        Console.ResetColor();
        Console.Write("loaded successfully!\n\n");
        WaitUserInput();
    }
    else
    {
        WarningMessage("Whoops! Option with such name does not exist!");
        WaitUserInput();
    }

    return "...";
}


// Delete options
string DeleteGameOptions()
{
    ListGameOptions();

    Console.Write("Options name to delete: ");

    Console.ForegroundColor = ConsoleColor.Yellow;
    var optionsName = Console.ReadLine();
    Console.ResetColor();


    //Check if there is option with such name
    if (OptionExist(optionsName!))
    {
        // If some game is using given options, dont't let them delete
        int optionsId = optionsRepo.GetGameOptions(optionsName).Id;
        if (IsGamesUsingGivenOptions(optionsId))
        {
            WarningMessage("Can't delete this options, because some games are using them!");
        }
        else
        {
            optionsRepo.DeleteGameOptions(optionsName);

            //check whether the deleting operation was successful
            if (OptionExist(optionsName))
            {
                WarningMessage("Deleted unsuccessfully!");
            }
            else
            {
                SuccessMessage($"Options named: [{optionsName}] successfully deleted!");
            }
        }
    }
    else
    {
        WarningMessage($"Whoops! Option with such name: [{optionsName}] does not exist!");
    }

    WaitUserInput();
    return "...";
}


string DeleteGame()
{
    ListGames();

    Console.Write("Game to delete: ");

    Console.ForegroundColor = ConsoleColor.Yellow;
    var gameName = Console.ReadLine();
    Console.ResetColor();

    if (gameRepo.GetAllSavedGamesNameList().Contains(gameName))
    {
        gameRepo.DeleteGame(gameName);
        if (gameRepo.GetAllSavedGamesNameList().Contains(gameName))
        {
            WarningMessage("Deleted unsuccessfully!");
        }
        else
        {
            SuccessMessage($"Game: [{gameName}] successfully deleted!");
        }
    }
    else
    {
        WarningMessage($"Whoops! Game with such name: [{gameName}] does not exist!");
    }

    return "...";
}


// Create game options according to player's input.
string CreateGameOptions()
{
    var checkersOptions = new CheckersOption();
    var boardSizesOk = false;

    int boardWidth;
    int boardHeight;

    do
    {
        //Ask from player for board sizes
        Console.Write("Choose table width -> ONLY even number allowed (min 4): ");
        var userChoiceWidth = Console.ReadLine();
        Console.Write("Choose table height -> ONLY even number allowed (min 8): ");
        var userChoiceHeight = Console.ReadLine();

        boardWidth = Convert.ToInt32(userChoiceWidth);
        boardHeight = Convert.ToInt32(userChoiceHeight);

        if (boardWidth % 2 == 0 && boardHeight % 2 == 0 && boardWidth >= 4 && boardHeight >= 8)
        {
            boardSizesOk = true;
        }
        else
        {
            Console.WriteLine("Board sizes are not OKAY, please try again!");
            Console.WriteLine();
        }
    } while (!boardSizesOk);


    checkersOptions.Width = boardWidth;
    checkersOptions.Height = boardHeight;

    Console.WriteLine();

    Console.WriteLine("Press Y (yes) if taking is mandatory or N (no) if not");
    ConsoleKey key = Console.ReadKey(true).Key;
    checkersOptions.TakingIsMandatory = key switch
    {
        ConsoleKey.N => false,
        ConsoleKey.Y => true,
        _ => checkersOptions.TakingIsMandatory
    };

    Console.WriteLine();


    Console.WriteLine("Press Y (yes) if black checkers start the game or N (no) if not");
    key = Console.ReadKey(true).Key;
    checkersOptions.BlackStarts = key switch
    {
        ConsoleKey.N => false,
        ConsoleKey.Y => true,
        _ => checkersOptions.BlackStarts
    };

    Console.Write("Options name: ");
    var optionsName = Console.ReadLine();

    checkersOptions.Name = optionsName!;

    optionsRepo.SaveGameOptions(optionsName!, checkersOptions);
    SuccessMessage($"Game options {optionsName} created!");
    WaitUserInput();
    return "...";
}


string SaveCurrentOptions()
{
    Console.Clear();
    Console.WriteLine("Current game option:");
    Console.WriteLine(gameOptions);
    Console.WriteLine("Give the name for current options: ");
    Console.ForegroundColor = ConsoleColor.Yellow;
    var currentOptionsName = Console.ReadLine();
    Console.ResetColor();
    gameOptions.Name = currentOptionsName!;
    optionsRepo.SaveGameOptions(currentOptionsName!, gameOptions);
    SuccessMessage("Game options saved!");
    WaitUserInput();

    return "...";
}


string SwapPersistenceEngine()
{
    if (optionsRepo == optionsRepoDb && gameRepo == gameRepoDb)
    {
        optionsRepo = optionsRepoFs;
        gameRepo = gameRepoFs;
    }
    else
    {
        optionsRepo = optionsRepoDb;
        gameRepo = gameRepoDb;
    }

    Console.WriteLine("Persistence engine: " + optionsRepo.Name);
    return optionsRepoDb.Name;
}


void WaitUserInput()
{
    Console.WriteLine("\nPress Enter to continue...\n");
    ConsoleKey userKey;
    do
    {
        userKey = Console.ReadKey(true).Key;
    } while (userKey != ConsoleKey.Enter);
}


// Helper method to check whether option with given name exists in options list
bool OptionExist(string o)
{
    var exist = false;
    foreach (var option in optionsRepo.GetGameOptionsList())
    {
        if (option == o)
        {
            exist = true;
            break;
        }
    }

    if (exist) return true;
    return false;
}

// Helper method to print warning message
void WarningMessage(string s)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"{s}");
    Console.ResetColor();
}


// Helper method to print success message
void SuccessMessage(string s)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"{s}");
    Console.ResetColor();
}


bool IsGamesUsingGivenOptions(int optionsId)
{
    var isUsing = false;
    var games = gameRepo.GetAllGames();
    foreach (var game in games)
    {
        if (game.CheckersOption != null && game.CheckersOption.Id == optionsId)
        {
            isUsing = true;
            break;
        }
    }

    return isUsing;
}


// Check whether player on black side has won the game.
bool BlackWon(CheckersGame game, EGamePiece?[][] board, CheckersGameState gameState)
{
    if (game.CheckersGameStates != null)
    {
        gameState = game.CheckersGameStates.Last();
        board = JsonSerializer.Deserialize<EGamePiece?[][]>(gameState.SerializedGameState)!;
        brain.SetGameBoard(board, gameState);
    }

    var whitePieces = brain.CountPiecesOnBoardForOneSide(false);
    var blackPieces = brain.CountPiecesOnBoardForOneSide(true);

    if (!brain.WhitePiecesHasMoves() || (whitePieces.kings == 0 && whitePieces.regular == 0))
    {
        return true;
    }

    return false;
}