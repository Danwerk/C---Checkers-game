using System.Diagnostics.CodeAnalysis;
using Domain;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GameBrain;

public class CheckersBrain
{
    public EGamePiece?[][] GameBoard;
    private readonly CheckersOption _checkersGameOption;
    private bool _nextMoveByBlack;
    private bool _takingDone;
    private bool _gameWonByBlack;
    private bool _gameOver;
    

    public CheckersBrain(CheckersOption option)
    {
        _nextMoveByBlack = option.BlackStarts;
        _checkersGameOption = option;
        var boardWidth = option.Width;
        var boardHeight = option.Height;
        GameBoard = new EGamePiece?[boardWidth][];


        // Initialize the jagged array
        for (int i = 0; i < boardWidth; i++)
        {
            GameBoard[i] = new EGamePiece?[boardHeight];
        }

        // Set up initial board pieces
        for (int i = 0; i < boardHeight; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                if ((i == 0 && j % 2 != 0) || (i == 2 && j % 2 != 0) || (i == 1 && j % 2 == 0))
                {
                    GameBoard[j][i] = EGamePiece.WhitePiece;
                }
                else if ((i == boardHeight - 1 && j % 2 == 0) || (i == boardHeight - 3 && j % 2 == 0) ||
                         (i == boardHeight - 2 && j % 2 != 0))
                {
                    GameBoard[j][i] = EGamePiece.BlackPiece;
                }
                else if ((i + j) % 2 == 0)
                {
                    GameBoard[j][i] = EGamePiece.WhiteSquare;
                }
                else
                {
                    GameBoard[j][i] = EGamePiece.BlackSquare;
                }
            }
        }
    }


    public EGamePiece?[][] GetBoard()
    {
        var jsonStr = System.Text.Json.JsonSerializer.Serialize(GameBoard);
        return System.Text.Json.JsonSerializer.Deserialize<EGamePiece?[][]>(jsonStr)!;
    }


    public void SetGameBoard(EGamePiece?[][] board, CheckersGameState state)
    {
        try
        {
            GameBoard = board;
        }
        catch (Exception)
        {
            Console.WriteLine("There is no board");
        }

        _nextMoveByBlack = state.NextMoveByBlack;
    }

    // see sai lisatud 
    public bool BlackPiecesHasMoves()
    {
        for (int x = 0; x < _checkersGameOption.Width; x++)
        {
            for (int y = 0; y < _checkersGameOption.Height; y++)
            {
                if ((GameBoard[x][y] == EGamePiece.BlackPiece || GameBoard[x][y] == EGamePiece.BlackPiece) && PieceHasMoves(x, y))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // see sai lisatud
    public bool WhitePiecesHasMoves()
    {
        for (int x = 0; x < _checkersGameOption.Width; x++)
        {
            for (int y = 0; y < _checkersGameOption.Height; y++)
            {
                if ((GameBoard[x][y] == EGamePiece.WhitePiece || GameBoard[x][y] == EGamePiece.WhiteKing)  && PieceHasMoves(x, y))
                {
                    return true;
                }
            }
        }

        return false;
    }


    public bool PieceHasMoves(int x, int y)
    {
        if (GameBoard[x][y] == EGamePiece.BlackPiece || GameBoard[x][y] == EGamePiece.WhitePiece || 
            GameBoard[x][y] == EGamePiece.BlackKing || GameBoard[x][y] == EGamePiece.WhiteKing)
        {
            for (int tryX = 0; tryX < _checkersGameOption.Width; tryX++)
            {
                for (int tryY = 0; tryY < _checkersGameOption.Height; tryY++)
                {
                    if (PieceMoveIsPossible(x, y, tryX, tryY))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // Main method to check whether it is possible to move checker
    public bool PieceMoveIsPossible(int fromX, int fromY, int toX, int toY)
    {
        if (fromX < 0 || fromX > _checkersGameOption.Width ||
            toX < 0 || toX > _checkersGameOption.Width ||
            fromY < 0 || fromY > _checkersGameOption.Height ||
            toY < 0 || toY > _checkersGameOption.Height)
        {
            return false;
        }

        if (GameBoard[toX][toY] == EGamePiece.BlackPiece || GameBoard[toX][toY] == EGamePiece.WhitePiece ||
            GameBoard[toX][toY] == EGamePiece.BlackKing || GameBoard[toX][toY] == EGamePiece.WhiteKing)
        {
            return false;
        }


        // Check for possible movement for white and black game piece
        if (GameBoard[fromX][fromY] == EGamePiece.BlackPiece || GameBoard[fromX][fromY] == EGamePiece.WhitePiece)
        {
            // if movement is over one cell. There is white or black piece to kill
            if (Math.Abs(toX - fromX) == 2 && Math.Abs(toY - fromY) == 2)
            {
                return CanMoveWhiteOrBlackPieceOverOneCell(fromX, fromY, toX, toY);
            }

            // Movement for black piece
            if (GameBoard[fromX][fromY] == EGamePiece.BlackPiece)
            {
                return CanMoveBlackPieceRegularMove(fromX, fromY, toX, toY);
            }

            // Movement for white piece
            if (GameBoard[fromX][fromY] == EGamePiece.WhitePiece)
            {
                return CanMoveWhitePieceRegularMove(fromX, fromY, toX, toY);
            }
        }
        // Check movement possibilities for white and black king
        else
        {
            if (GameBoard[fromX][fromY] == EGamePiece.BlackKing || GameBoard[fromX][fromY] == EGamePiece.WhiteKing)
            {
                if (Math.Abs(toX - fromX) == 2 && Math.Abs(toY - fromY) == 2)
                {
                    return CanMoveWhiteOrBlackKingOverOneCell(fromX, fromY, toX, toY);
                }

                // Regular movement for black and white king
                return CanMoveWhiteOrBlackKingRegularMove(fromX, fromY, toX, toY);
            }
        }

        return false;
    }

    // Helper method for PieceMoveIsPossible. Check whether regular movement for white or black king can be done.
    private bool CanMoveWhiteOrBlackKingRegularMove(int fromX, int fromY, int toX, int toY)
    {
        try
        {
            if (toX == fromX + 1 && toY == fromY + 1)
            {
                return true;
            }

            if (toX == fromX - 1 && toY == fromY + 1)
            {
                return true;
            }

            if (toX == fromX - 1 && toY == fromY - 1)
            {
                return true;
            }

            if (toX == fromX + 1 && toY == fromY - 1)
            {
                return true;
            }
        }
        catch (IndexOutOfRangeException)
        {
            return false;
        }

        return false;
    }

    // Helper method for PieceMoveIsPossible
    private bool CanMoveWhiteOrBlackKingOverOneCell(int fromX, int fromY, int toX, int toY)
    {
        if (GameBoard[fromX][fromY] == EGamePiece.BlackKing)
        {
            if ((GameBoard[(toX + fromX) / 2][(toY + fromY) / 2] == EGamePiece.WhitePiece ||
                 GameBoard[(toX + fromX) / 2][(toY + fromY) / 2] == EGamePiece.WhiteKing) &&
                CellIsBlackAndEmpty(toX, toY))
            {
                return true;
            }

            return false;
        }

        if (GameBoard[fromX][fromY] == EGamePiece.WhiteKing)
        {
            if ((GameBoard[(toX + fromX) / 2][(toY + fromY) / 2] == EGamePiece.BlackPiece ||
                 GameBoard[(toX + fromX) / 2][(toY + fromY) / 2] == EGamePiece.BlackKing) &&
                CellIsBlackAndEmpty(toX, toY))
            {
                return true;
            }

            return false;
        }

        return false;
    }


    // Helper method for PieceMoveIsPossible. Checks whether it is possible to move white piece diagonally forward
    private bool CanMoveWhitePieceRegularMove(int fromX, int fromY, int toX, int toY)
    {
        try
        {
            // Can move diagonally right-down?
            if (toX == fromX + 1 && toY == fromY + 1)
            {
                return true;
            }

            // Can move diagonally left-down?
            if (toX == fromX - 1 && toY == fromY + 1)
            {
                return true;
            }
        }
        catch (IndexOutOfRangeException)
        {
            return false;
        }

        return false;
    }


    // Helper method for PieceMoveIsPossible. Checks whether it is possible to move black piece diagonally forward
    private bool CanMoveBlackPieceRegularMove(int fromX, int fromY, int toX, int toY)
    {
        try
        {
            // Can move diagonally left-above?
            if (toX == fromX - 1 && toY == fromY - 1)
            {
                return true;
            }

            // Can move diagonally right-above?
            if (toX == fromX + 1 && toY == fromY - 1)
            {
                return true;
            }
        }
        catch (IndexOutOfRangeException)
        {
            return false;
        }

        return false;
    }

    // Helper method for PieceMoveIsPossible
    private bool CanMoveWhiteOrBlackPieceOverOneCell(int fromX, int fromY, int toX, int toY)
    {
        if (GameBoard[fromX][fromY] == EGamePiece.BlackPiece)
        {
            if ((GameBoard[(toX + fromX) / 2][(toY + fromY) / 2] == EGamePiece.WhitePiece ||
                 GameBoard[(toX + fromX) / 2][(toY + fromY) / 2] == EGamePiece.WhiteKing) &&
                CellIsBlackAndEmpty(toX, toY))
            {
                if (toY > fromY)
                {
                    return false;
                }

                return true;
            }
        }

        else if (GameBoard[fromX][fromY] == EGamePiece.WhitePiece)
        {
            if ((GameBoard[(toX + fromX) / 2][(toY + fromY) / 2] == EGamePiece.BlackPiece ||
                 GameBoard[(toX + fromX) / 2][(toY + fromY) / 2] == EGamePiece.BlackKing) &&
                CellIsBlackAndEmpty(toX, toY))
            {
                if (toY < fromY)
                {
                    return false;
                }

                return true;
            }
        }

        return false;
    }


    public void MovePiece(int fromX, int fromY, int toX, int toY)
    {
        if (!PieceMoveIsPossible(fromX, fromY, toX, toY))
        {
            throw new ArgumentException("Cannot move");
        }


        if ((GameBoard[fromX][fromY] == EGamePiece.BlackPiece ||
             GameBoard[fromX][fromY] == EGamePiece.BlackKing) && !_nextMoveByBlack ||
            (GameBoard[fromX][fromY] == EGamePiece.WhitePiece ||
             GameBoard[fromX][fromY] == EGamePiece.WhiteKing) && _nextMoveByBlack)
        {
            throw new ArgumentException("It is not your turn" + _nextMoveByBlack);
        }

        // When black piece reaches white side, it transforms to blackKing
        if (GameBoard[fromX][fromY] == EGamePiece.BlackPiece && toX % 2 != 0 && toY == 0)
        {
            JumpOrRegularMove(fromX, fromY, toX, toY);
            GameBoard[toX][toY] = EGamePiece.BlackKing;
        }

        // When white piece reaches black side, it transforms to whiteKing
        else if (GameBoard[fromX][fromY] == EGamePiece.WhitePiece && toX % 2 == 0 && toY == GameBoard[0].Length - 1)
        {
            JumpOrRegularMove(fromX, fromY, toX, toY);
            GameBoard[toX][toY] = EGamePiece.WhiteKing;
        }

        // Actual move is done here
        else if (GameBoard[fromX][fromY] == EGamePiece.BlackPiece || GameBoard[fromX][fromY] == EGamePiece.WhitePiece ||
                 GameBoard[fromX][fromY] == EGamePiece.BlackKing || GameBoard[fromX][fromY] == EGamePiece.WhiteKing)
        {
            JumpOrRegularMove(fromX, fromY, toX, toY);
        }

        CheckIfGameIsOver();
    }

    // Helper method for MovePiece
    private void JumpOrRegularMove(int fromX, int fromY, int toX, int toY)
    {
        // Jump movement
        if (Math.Abs(fromX - toX) == 2 && Math.Abs(fromY - toY) == 2)
        {
            GameBoard[toX][toY] = GameBoard[fromX][fromY];
            GameBoard[(toX + fromX) / 2][(toY + fromY) / 2] = EGamePiece.BlackSquare;
            GameBoard[fromX][fromY] = EGamePiece.BlackSquare;
            _takingDone = true;
            if (!CanTake(toX, toY))
            {
                _nextMoveByBlack = !_nextMoveByBlack;
            }
        }
        // Regular movement
        else
        {
            GameBoard[toX][toY] = GameBoard[fromX][fromY];
            GameBoard[fromX][fromY] = EGamePiece.BlackSquare;
            _nextMoveByBlack = !_nextMoveByBlack;
            _takingDone = false;
        }
    }


    public bool CanTake(int x, int y)
    {
        if (GameBoard[x][y] == EGamePiece.BlackPiece || GameBoard[x][y] == EGamePiece.WhitePiece)
        {
            if (GameBoard[x][y] == EGamePiece.BlackPiece)
            {
                try
                {
                    if (PieceMoveIsPossible(x, y, x - 2, y - 2))
                    {
                        return true;
                    }


                    try
                    {
                        return PieceMoveIsPossible(x, y, x + 2, y - 2);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return false;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    try
                    {
                        return PieceMoveIsPossible(x, y, x + 2, y - 2);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return false;
                    }
                }
            }

            if (GameBoard[x][y] == EGamePiece.WhitePiece)
            {
                try
                {
                    if (PieceMoveIsPossible(x, y, x - 2, y + 2))
                    {
                        return true;
                    }


                    try
                    {
                        return PieceMoveIsPossible(x, y, x + 2, y + 2);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return false;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    try
                    {
                        return PieceMoveIsPossible(x, y, x + 2, y + 2);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return false;
                    }
                }
            }
        }

        return false;
    }


    public bool CellIsBlackAndEmpty(int x, int y)
    {
        if (GameBoard[x][y] == EGamePiece.WhiteSquare)
        {
            return false;
        }

        if (GameBoard[x][y] == EGamePiece.WhitePiece ||
            GameBoard[x][y] == EGamePiece.BlackPiece
            || GameBoard[x][y] == EGamePiece.WhiteKing ||
            GameBoard[x][y] == EGamePiece.BlackKing)
        {
            return false;
        }

        return true;
    }


    public void CheckIfGameIsOver()
    {
        var whitePiecesCountOnBoard = 0;
        var blackPiecesCountOnBoard = 0;
        for (int x = 0; x < _checkersGameOption.Width; x++)
        {
            for (int y = 0; y < _checkersGameOption.Height; y++)
            {
                if (GameBoard[x][y] == EGamePiece.BlackPiece || GameBoard[x][y] == EGamePiece.BlackKing)
                {
                    blackPiecesCountOnBoard++;
                }

                else if (GameBoard[x][y] == EGamePiece.WhitePiece || GameBoard[x][y] == EGamePiece.WhiteKing)
                {
                    whitePiecesCountOnBoard++;
                }
            }
        }

        if (whitePiecesCountOnBoard == 0 || !WhitePiecesHasMoves())
        {
            _gameOver = true;
            _gameWonByBlack = true;
        }

        else if (blackPiecesCountOnBoard == 0 || !BlackPiecesHasMoves())
        {
            _gameOver = true;
            _gameWonByBlack = false;
        }
    }


//   ############################## AI ######################################

    // Get all possible moves for all checkers for specified side(black/white)
    public List<Move> GetAllPossibleMoves(bool isBlack)
    {
        var moves = new List<Move>();
        var checkers = GetAllMovableCheckers(isBlack);

        foreach (var checker in checkers)
        {
            var piecePossibleMoves = GetPossibleMovesForPiece(checker.x, checker.y);

            foreach (var piecePossibleMove in piecePossibleMoves)
            {
                var move = new Move()
                {
                    FromX = piecePossibleMove.fromX,
                    FromY = piecePossibleMove.fromY,
                    ToX = piecePossibleMove.toX,
                    ToY = piecePossibleMove.toY
                };
                if (PieceMoveIsPossible(move.FromX, move.FromY, move.ToX, move.ToY))
                {
                    moves.Add(move);
                }
            }
        }

        return moves;
    }


    // Get all possible moves for specified piece. Method takes piece coordinates and returns list of possible moves.
    private List<(int fromX, int fromY, int toX, int toY)> GetPossibleMovesForPiece(int x, int y)
    {
        var possibleMoves = new List<(int fromX, int fromY, int toX, int toY)>();
        for (int y1 = 0; y1 < GameBoard[0].GetLength(0); y1++)
        {
            for (int x1 = 0; x1 < GameBoard.GetLength(0); x1++)
            {
                if (PieceMoveIsPossible(x, y, x1, y1))
                {
                    possibleMoves.Add((x, y, x1, y1));
                }
            }
        }

        return possibleMoves;
    }


    // Get list of all movable checkers for specified side(black/white)
    public List<(int x, int y)> GetAllMovableCheckers(bool isBlack)
    {
        List<(int x, int y)> movableCheckers = new List<(int x, int y)>();

        for (int y = 0; y < GameBoard[0].GetLength(0); y++)
        {
            for (int x = 0; x < GameBoard.GetLength(0); x++)
            {
                if (isBlack && (GameBoard[x][y] == EGamePiece.BlackPiece || GameBoard[x][y] == EGamePiece.BlackKing) &&
                    PieceHasMoves(x, y))
                {
                    movableCheckers.Add((x, y));
                }
                else if (!isBlack &&
                         (GameBoard[x][y] == EGamePiece.WhitePiece || GameBoard[x][y] == EGamePiece.WhiteKing) &&
                         PieceHasMoves(x, y))
                {
                    movableCheckers.Add((x, y));
                }
            }
        }

        return movableCheckers;
    }


    // If white has more pieces left, board evaluation is going to be positive. In other hands if black has more pieces,
    // then it is going to be negative.
    public double Evaluate()
    {
        var whitePieces = CountPiecesOnBoardForOneSide(false);
        var blackPieces = CountPiecesOnBoardForOneSide(true);
        return whitePieces.regular - blackPieces.regular + (whitePieces.kings * 0.5 - blackPieces.kings * 0.5);
    }

    public (int regular, int kings) CountPiecesOnBoardForOneSide(bool isBlack)
    {
        int reg = 0;
        int kin = 0;

        for (int y = 0; y < GameBoard[0].GetLength(0); y++)
        {
            for (int x = 0; x < GameBoard.GetLength(0); x++)
            {
                if (isBlack)
                {
                    if (GameBoard[x][y] == EGamePiece.BlackPiece)
                    {
                        reg++;
                    }

                    else if (GameBoard[x][y] == EGamePiece.BlackKing)
                    {
                        kin++;
                    }
                }
                else if (!isBlack)
                {
                    if (GameBoard[x][y] == EGamePiece.WhitePiece)
                    {
                        reg++;
                    }

                    else if (GameBoard[x][y] == EGamePiece.WhiteKing)
                    {
                        kin++;
                    }
                }
            }
        }

        return (reg, kin);
    }


    public void MovePieceByAi(bool isBlack)
    {
        var randomMove = MiniMax(isBlack);
        {
            MovePiece(randomMove.FromX, randomMove.FromY, randomMove.ToX, randomMove.ToY);
        }
    }

    public Move MiniMax(bool isBlack)
    {
        var newBrain = DeepCopy();
        var decision = MiniMaxLib.Minimax(4, !isBlack, newBrain);
        return decision.move;
    }

    //   ############################## AI ######################################


    // Make a deepcopy of checkers brain
    public CheckersBrain DeepCopy()
    {
        var boardCopy = GetBoard();
        var gameOver = _gameOver;
        var nextMoveByBlack = _nextMoveByBlack;
        var takingDone = _takingDone;
        var gameWonByBlack = _gameWonByBlack;


        var newBrain = new CheckersBrain(_checkersGameOption);
        newBrain._gameOver = gameOver;
        newBrain._takingDone = takingDone;
        newBrain.GameBoard = boardCopy;
        newBrain._nextMoveByBlack = nextMoveByBlack;
        newBrain._gameWonByBlack = gameWonByBlack;

        return newBrain;
    }

    public bool IsGameOver()
    {
        return _gameOver;
    }

    public bool GameWonByBlack()
    {
        return _gameWonByBlack;
    }

    public bool NextMoveByBlack()
    {
        return _nextMoveByBlack;
    }

    public bool TakingDone()
    {
        return _takingDone;
    }
    
}