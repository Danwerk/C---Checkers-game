namespace GameBrain;

public class MiniMaxLib
{
    public static (double evaluation, Move move) Minimax(int depth, bool maxPlayer, CheckersBrain brain, Move? move = null)
    {
        if (depth == 0 || brain.IsGameOver() && move != null)
        {
            return (brain.Evaluate(), move!);
        }
        
        

        if (maxPlayer)
        {
            var maxEval = (double) int.MinValue;
            Move? bestMove = null;

            foreach (var m in brain.GetAllPossibleMoves(brain.NextMoveByBlack()))
            {
                var newBrain = SimulateMove(brain, m);
                var eval = Minimax(depth - 1, !newBrain.NextMoveByBlack(), newBrain, m).evaluation;
                maxEval = Math.Max(maxEval, eval);
                if (maxEval <= eval)
                {
                    bestMove = m;
                }
            }
            return (brain.Evaluate(), bestMove)!;
        }

        else
        {
            var minEval = (double) int.MaxValue;
            Move? bestMove = null;

            foreach (var m in brain.GetAllPossibleMoves(brain.NextMoveByBlack()))
            {
                var newBrain = SimulateMove(brain, m);
                var eval = Minimax(depth - 1, !newBrain.NextMoveByBlack(), newBrain, m).evaluation;
                minEval = Math.Min(minEval, eval);
                if (minEval >= eval)
                {
                    bestMove = m;
                }
            }
            return (brain.Evaluate(), bestMove)!;
        }
    }

    // Make a deepcopy of brain and move piece.
    public static CheckersBrain SimulateMove(CheckersBrain cBrain, Move? move)
    {
        var newBrain = cBrain.DeepCopy();
        if (move != null) newBrain.MovePiece(move.FromX, move.FromY, move.ToX, move.ToY);
        return newBrain;
    }
}