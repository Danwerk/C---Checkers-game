namespace MenuSystem;

public class MenuItem
{

    
    public MenuItem(string shortcut, string title, Func<string>? methodToRun)
    {
        Shortcut = shortcut;
        Title = title;
        MethodToRun = methodToRun;
    }
    public string Title { get; set; } = default!;
    public string Shortcut { get; set; } = default!;
    
    public Func<string>? MethodToRun { get; set; }
    
    
    public override string ToString()
    { 
        return Shortcut + ") " + Title;
        
    }
    
}