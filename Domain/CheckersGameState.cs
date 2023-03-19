using System.Text.Json.Serialization;
using GameBrain;

namespace Domain;

public class CheckersGameState
{
    public int Id { get; set; }

    // public EGamePiece[,] GameBoard = default!;
    // public int[][] JaggedGameBoard { get; set; } = default!;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool NextMoveByBlack { get; set; }
    public string SerializedGameState { get; set; } = default!;

    public int CheckersGameId { get; set;  }
    
    public CheckersGame? CheckersGame { get; set; }
    
    public override string ToString()
    {
        return $"State id: {Id}, State created at: {CreatedAt}, Next move by black: {NextMoveByBlack}, serialized game state: {SerializedGameState}";
    }
}

