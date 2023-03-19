using Domain;
using GameBrain;

namespace ConsoleUI;

public static class UI
{
    // Main function to draw the game board.
    public static void DrawGameBoard(EGamePiece?[][] board)
    {
        int cols = board!.GetLength(0);
        int rows = board[0].GetLength(0);


        // Making a list of double letters to place them on the board.
        int numberToIncrement = 1;
        var doubleLetters = Enumerable.Range((char)65, 26)
            .SelectMany(x => Enumerable.Range((char)65, 26).Select(y => (char)x + "" + (char)y)).ToList();

        
        
        Console.WriteLine(" ");
        SetDoubleLetters(cols, doubleLetters);
        Console.WriteLine(" ");
        DrawGameBoardHorizontalLine(cols);

        
        
        for (var i = 0; i < rows; i++)
        {
            
            // printing the number on left side of the border.
            SetNumber(numberToIncrement);
            Console.Write("|");

            
            for (var j = 0; j < cols; j++)
            {
                // printing board elements
                switch (board[j][i])
                {
                    case EGamePiece.WhitePiece:
                        Console.Write(" W ");
                        break;
                    case EGamePiece.BlackPiece:
                        Console.Write(" B ");
                        break;
                    case EGamePiece.WhiteSquare:
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.Write("   ");
                        Console.BackgroundColor = ConsoleColor.Black;
                        break;
                    case EGamePiece.BlackSquare:
                        Console.Write("   ");
                        break;
                    case EGamePiece.WhiteKing:
                        Console.Write("kWk");
                        break;
                    case EGamePiece.BlackKing:
                        Console.Write("kBk");
                        break;
                }
            }

            Console.Write("|  ");
            SetNumber(numberToIncrement);
            numberToIncrement++;
            Console.WriteLine();
        }
        
        
        DrawGameBoardHorizontalLine(cols);
        SetDoubleLetters(cols, doubleLetters);
        Console.WriteLine();
        
        
    }
    

    public static void DrawGameBoardHorizontalLine(int columns)
    {
        string s = "   +";
        for (int i = 0; i < (columns * 3); i++)
        {
            s += "-";
        }

        s += "+";
        Console.WriteLine(s);
    }

    
    // Method gives double letters. E.g AA AB AC
    public static void SetDoubleLetters(int columns, List<string> doubleLetters)
    {
        Console.Write("   ");
        for (int i = 0; i < columns; i++)
        {
            Console.Write(" ");
            Console.Write(doubleLetters[i]);
        }
    }


    public static void SetNumber(int number)
    {
        if (number < 10)
        {
            Console.Write(number);
            Console.Write("  ");
        }
        else
        {
            Console.Write(number);
            Console.Write(" ");
        }
        
    }
}