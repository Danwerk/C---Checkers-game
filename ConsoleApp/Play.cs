using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using ConsoleUI;
using DAL;
using Domain;
using GameBrain;

namespace ConsoleApp;

public class Play
{
    private IGamePlayRepository _repo = default!;
    private CheckersGame _game = default!;
    private CheckersBrain _brain = default!;
    private bool _wonByBlack;

    // Actual game runner that is used in consoleApp 
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH",
        MessageId = "type: System.Nullable`1[GameBrain.EGamePiece][]")]
    public void RunGame(int id, IGamePlayRepository repo, CheckersBrain brain)
    {
        _repo = repo;
        _brain = brain;

        var game = _repo.GetGameById(id);
        if (game == null || game.CheckersOption == null || game.CheckersGameStates == null)
        {
            throw new Exception("Game not found!");
        }

        _game = game;

        do
        {
            var gameState = _game.CheckersGameStates.Last();
            var board = JsonSerializer.Deserialize<EGamePiece?[][]>(gameState.SerializedGameState)!;
            _brain.SetGameBoard(board, gameState);
            
            Console.WriteLine();
            Console.WriteLine();
            
            ShowWhoMoves(game);

            UI.DrawGameBoard(_brain.GetBoard());


            // If it is AI turn
            if (gameState.NextMoveByBlack && _game.Player2Type == EPlayerType.Ai)
            {
                ShowWhoMoves(game);
                MovePieceByAiAndSaveState(gameState);
                UI.DrawGameBoard(_brain.GetBoard());
            }
            // If it is AI turn
            else if (!gameState.NextMoveByBlack && _game.Player1Type == EPlayerType.Ai)
            {
                ShowWhoMoves(game);
                MovePieceByAiAndSaveState(gameState);
                UI.DrawGameBoard(_brain.GetBoard());
            }
            // If it is human turn. Ask player start and dest coords to move
            // Get movement info from player
            else
            {
                bool error;
                do
                {
                    try
                    {
                        error = false;
                        var coords = AskForUserCoords();
                        var fromX = coords[0];
                        var fromY = coords[1];
                        var toX = coords[2];
                        var toY = coords[3];


                        if (board[fromX][fromY] != EGamePiece.WhiteSquare && _brain.PieceHasMoves(fromX, fromY))
                        {
                            if (_brain.NextMoveByBlack() && (board[fromX][fromY] == EGamePiece.BlackPiece ||
                                                             board[fromX][fromY] == EGamePiece.BlackKing))
                            {
                                MovePieceAndSaveState(fromX, fromY, toX, toY);
                            }

                            else if (!_brain.NextMoveByBlack() && (board[fromX][fromY] == EGamePiece.WhitePiece ||
                                                                   board[fromX][fromY] == EGamePiece.WhiteKing))
                            {
                                MovePieceAndSaveState(fromX, fromY, toX, toY);
                            }
                            else
                            {
                                AskForUserCoords();
                            }
                        }
                        else if (!brain.PieceHasMoves(fromX, fromY))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Cannot make a move, please choose another coordinates to move from!");
                            Console.ResetColor();
                        }
                    }
                    catch (Exception e)
                    {
                        error = true;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("BAD CHOICE!!! Try again!");
                        Console.ResetColor();
                    }
                } while (error);
            }


            if (_brain.IsGameOver())
            {
                _game.GameWonByPlayer = _brain.GameWonByBlack() ? _game.Player2Name : _game.Player1Name;
                _game.GameOverAt = DateTime.Now;
            }

            _repo.UpdateGame(_game);


            if (game.GameOverAt != null)
            {
                _wonByBlack = _game.GameWonByPlayer == _game.Player2Name;
            }

            
            
        } while (game.GameOverAt == null);
        
        var wonByBlack = WonByBlack();
        Console.WriteLine();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(
            $"Game won by: {(wonByBlack ? game.Player2Name + " - (BLACK) " : game.Player1Name + " - (WHITE) ")}");
        Console.ResetColor();
        UI.DrawGameBoard(brain.GetBoard());
    }


    // Is used to convert letters to int type coords
    private int ConvertLettersToNumber(string lettersAsCoord)
    {
        var doubleLetters = Enumerable.Range((char)65, 26)
            .SelectMany(x => Enumerable.Range((char)65, 26).Select(y => (char)x + "" + (char)y)).ToList();

        for (int i = 0; i < _brain.GameBoard.GetLength(0); i++)
        {
            if (doubleLetters[i] == lettersAsCoord.ToUpper())
            {
                return i;
            }
        }

        throw new Exception("INCORRECT LETTERS");
    }

    private List<int> AskForUserCoords()
    {
        List<int> coords = new List<int>();
        int fromX;
        int fromY;
        int toX;
        int toY;

        // Check whether player who plays on white side, gives correct input coords
        if (!_brain.NextMoveByBlack())
        {
            var error = false;
            do
            {
                Console.WriteLine($"Choose piece to move: ");
                Console.WriteLine($"FROM x-coordinate (letters): ");
                var fromXstring = Console.ReadLine()!;
                fromX = ConvertLettersToNumber(fromXstring);

                Console.WriteLine("FROM y-coordinate (numbers): ");
                var fromYstring = Console.ReadLine()!;
                fromY = int.Parse(fromYstring) - 1;

                var boardPiece = _brain.GameBoard[fromX][fromY];
                if (!_brain.NextMoveByBlack() &&
                    (boardPiece == EGamePiece.BlackPiece || boardPiece == EGamePiece.BlackKing))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("It is black piece. You can't move this piece! Try once again!!");
                    Console.ResetColor();
                }
                else if (boardPiece != EGamePiece.BlackKing && boardPiece != EGamePiece.WhiteKing &&
                         boardPiece != EGamePiece.WhitePiece && boardPiece != EGamePiece.BlackPiece)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("There is no piece to move! Try once again!!");
                    Console.ResetColor();
                }
                else if (!_brain.NextMoveByBlack() &&
                         (boardPiece == EGamePiece.WhitePiece || boardPiece == EGamePiece.WhiteKing) &&
                         !_brain.PieceHasMoves(fromX, fromY))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Can't move this piece! choose another one");
                    Console.ResetColor();
                }
            } while ((_brain.GameBoard[fromX][fromY] == EGamePiece.BlackPiece && !_brain.NextMoveByBlack()) ||
                     !_brain.PieceHasMoves(fromX, fromY) && error);

            coords.Add(fromX);
            coords.Add(fromY);
        }
        // Check whether player who plays on black side, gives correct input coords
        else
        {
            do
            {
                Console.WriteLine($"Piece to move: ");
                Console.WriteLine($"FROM x-coordinate (letters): ");
                var fromXstring = Console.ReadLine()!;
                fromX = ConvertLettersToNumber(fromXstring);

                Console.WriteLine("FROM y-coordinate (numbers): ");
                var fromYstring = Console.ReadLine()!;
                fromY = int.Parse(fromYstring) - 1;

                var boardPiece = _brain.GameBoard[fromX][fromY];
                if (_brain.NextMoveByBlack() &&
                    (boardPiece == EGamePiece.WhitePiece || boardPiece == EGamePiece.WhiteKing))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("It is white piece. You can't move this piece! Try once again!!");
                    Console.ResetColor();
                }
                else if (boardPiece != EGamePiece.BlackKing && boardPiece != EGamePiece.WhiteKing &&
                         boardPiece != EGamePiece.WhitePiece && boardPiece != EGamePiece.BlackPiece)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("There is no piece to move! Try once again!!");
                    Console.ResetColor();
                }
                else if (_brain.NextMoveByBlack() &&
                         (boardPiece == EGamePiece.BlackPiece || boardPiece == EGamePiece.BlackKing) &&
                         !_brain.PieceHasMoves(fromX, fromY))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Can't move this piece! choose another one");
                    Console.ResetColor();
                }
            } while ((_brain.GameBoard[fromX][fromY] == EGamePiece.WhitePiece && _brain.NextMoveByBlack()) ||
                     !_brain.PieceHasMoves(fromX, fromY));

            coords.Add(fromX);
            coords.Add(fromY);
        }


        // Check for correct move-to coordinates
        while (true)
        {
            Console.WriteLine($"TO x-coordinate (letters): ");
            var toXstring = Console.ReadLine()!;
            toX = ConvertLettersToNumber(toXstring);

            Console.WriteLine("TO y-coordinate (numbers): ");
            var toYstring = Console.ReadLine()!;
            toY = int.Parse(toYstring) - 1;

            if (_brain.PieceMoveIsPossible(fromX, fromY, toX, toY))
            {
                coords.Add(toX);
                coords.Add(toY);
                break;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Cannot move there!!! Try again!");
            Console.ResetColor();
        }

        return coords;
    }


    public void MovePieceAndSaveState(int fromX, int fromY, int toX, int toY)
    {
        _brain.MovePiece(fromX, fromY, toX, toY);
        var newState = new CheckersGameState
        {
            SerializedGameState = JsonSerializer.Serialize(_brain.GetBoard()),
            NextMoveByBlack = _brain.NextMoveByBlack()
        };
        _game.CheckersGameStates!.Add(newState);
        _repo.UpdateGame(_game);
    }

    public void MovePieceByAiAndSaveState(CheckersGameState state)
    {
        _brain.MovePieceByAi(state.NextMoveByBlack);
        var newState = new CheckersGameState
        {
            SerializedGameState = JsonSerializer.Serialize(_brain.GetBoard()),
            NextMoveByBlack = _brain.NextMoveByBlack()
        };

        _game.CheckersGameStates!.Add(newState);
        _repo.UpdateGame(_game);
    }

    public void ShowWhoMoves(CheckersGame game)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"{game.Player1Name}[{game.Player1Type}] vs {game.Player2Name}[{game.Player2Type}]");
        var nextMove = _brain.NextMoveByBlack() ? $"{game.Player2Name} - (BLACK)" : $"{game.Player1Name} - (WHITE)";
        Console.WriteLine($"Your turn: " + nextMove);
        Console.ResetColor();

    }

    public bool WonByBlack()
    {
        return _wonByBlack;
    }
}