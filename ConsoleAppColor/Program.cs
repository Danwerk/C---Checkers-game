namespace ConsoleAppColor;

class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var items = new List<string>()
        {
            "Aaaaaa", "Bbbbbbb", "CCC", "ddd", "dddd"
        };

        var activeLine = 0;

        var done = false;

        Console.ForegroundColor = ConsoleColor.White;

        do
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (i == activeLine)
                {
                    Console.BackgroundColor = ConsoleColor.Magenta;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                }

                Console.WriteLine(items[i]);
            }

            var key = Console.ReadKey();

            if (key.Key == ConsoleKey.UpArrow)
            {
                activeLine--;
                if (activeLine < 0) activeLine = items.Count - 1;
            }

            if (key.Key == ConsoleKey.DownArrow)
            {
                activeLine++;
                if (activeLine >= items.Count) activeLine = 0;
            }

            Console.BackgroundColor = ConsoleColor.White;

            Console.Clear();
        } while (done == false);
    }
}
