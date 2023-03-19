namespace GameBrain;

public class Move
{
    public int FromX;
    public int FromY;
    public int ToX;
    public int ToY;

    public override string ToString()
    {
        return $"{FromX}, {FromY}, {ToX}, {ToY}";
    }

}