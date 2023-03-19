namespace MenuSystem;

public class Menu
{
    private readonly EMenuLevel _level;


    private const string ShortcutExit = "X";
    private const string ShortcutGoBack = "B";
    private const string ShortcutGoToMain = "M";
    public string Title { get; set; }

    private readonly Dictionary<string, MenuItem> _menuItems = new Dictionary<string, MenuItem>();
    private readonly MenuItem _menuItemExit = new MenuItem(ShortcutExit, "Exit", null);
    private readonly MenuItem _menuItemGoBack = new MenuItem(ShortcutGoBack, "Back", null);
    private readonly MenuItem _menuItemGoToMain = new MenuItem(ShortcutGoToMain, "Main Menu", null);


    public Menu(EMenuLevel level, string title, List<MenuItem> menuItems)
    {
        _level = level;
        Title = title;
        foreach (var menuItem in menuItems)
        {
            _menuItems.Add(menuItem.Shortcut, menuItem);
        }

        if (_level != EMenuLevel.Main)
            _menuItems.Add(ShortcutGoBack, _menuItemGoBack);


        if (_level != EMenuLevel.Other)
            _menuItems.Add(ShortcutGoToMain, _menuItemGoToMain);


        _menuItems.Add(ShortcutExit, _menuItemExit);
    }

    public string RunMenu()
    {
        Console.Clear();
        bool menuDone = false;
        var userChoice = "";
        do
        {
            Console.WriteLine(Title);
            Console.WriteLine("===============");
            foreach (var menuItem in _menuItems.Values)
            {
                Console.WriteLine(menuItem);
            }


            Console.WriteLine("---------------");
            Console.Write("Your choice:");
            userChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";

            Console.WriteLine();
            Console.WriteLine();


            if (_menuItems.ContainsKey(userChoice))
            {
                string? runReturnValue = null;
                if (_menuItems[userChoice].MethodToRun != null)
                {
                    if (userChoice == "N")
                    {
                        menuDone = true;
                    }

                    runReturnValue = _menuItems[userChoice].MethodToRun!();
                    
                }

                if (userChoice == ShortcutGoBack)
                {
                    menuDone = true;
                }


                if (runReturnValue == ShortcutExit || userChoice == ShortcutExit)
                {
                    userChoice = runReturnValue ?? userChoice;
                    menuDone = true;
                }

                if ((userChoice == ShortcutGoToMain || runReturnValue == ShortcutGoToMain) && _level != EMenuLevel.Main)
                {
                    userChoice = runReturnValue ?? userChoice;
                    menuDone = true;
                }
            }
            else
            {
                Console.WriteLine("What the hell man?");
            }
        } while (menuDone == false);
        Console.Clear();
        return userChoice;
    }
}