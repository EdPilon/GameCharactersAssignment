using NLog;
using System.Reflection;
using System.Text.Json;

string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");

// load mario characters from file
string marioFileName = "mario.json";
List<Mario> marios = new List<Mario>();

if (File.Exists(marioFileName))
{
    marios = JsonSerializer.Deserialize<List<Mario>>(File.ReadAllText(marioFileName));
    logger.Info("Loaded " + marioFileName);
}

bool running = true;

while (running)
{
    // show menu
    Console.WriteLine("1) Show Mario Characters");
    Console.WriteLine("2) Add Mario Character");
    Console.WriteLine("3) Remove Mario Character");
    Console.WriteLine("Enter to quit");

    // get choice
    string? choice = Console.ReadLine();
    logger.Info("User picked: " + choice);

    if (choice == "1")
    {
        // Show characters
        foreach (var m in marios)
        {
            Console.WriteLine(m.Display());
        }
    }
    else if (choice == "2")
    {
        // Add character
        Mario newMario = new Mario();
        newMario.Id = marios.Count == 0 ? 1 : marios.Max(m => m.Id) + 1;
        InputCharacter(newMario);
        marios.Add(newMario);
        File.WriteAllText(marioFileName, JsonSerializer.Serialize(marios));
        logger.Info("Added: " + newMario.Name);
    }
    else if (choice == "3")
    {
        // Remove character
        Console.WriteLine("Enter character Id to remove:");
        if (UInt32.TryParse(Console.ReadLine(), out UInt32 idToRemove))
        {
            var toRemove = marios.FirstOrDefault(m => m.Id == idToRemove);
            if (toRemove != null)
            {
                marios.Remove(toRemove);
                File.WriteAllText(marioFileName, JsonSerializer.Serialize(marios));
                logger.Info("Removed character with Id: " + idToRemove);
            }
            else
            {
                logger.Error("Id not found");
            }
        }
        else
        {
            logger.Error("Invalid Id");
        }
    }
    else if (string.IsNullOrEmpty(choice))
    {
        running = false;
    }
    else
    {
        logger.Info("Invalid option");
    }
}

logger.Info("Program ended");

static void InputCharacter(Mario mario)
{
    PropertyInfo[] props = typeof(Mario).GetProperties();
    foreach (var prop in props)
    {
        if (prop.Name != "Id")
        {
            if (prop.PropertyType == typeof(string))
            {
                Console.WriteLine($"Enter {prop.Name}:");
                string? value = Console.ReadLine();
                prop.SetValue(mario, value);
            }
            else if (prop.PropertyType == typeof(List<string>))
            {
                List<string> items = new List<string>();
                while (true)
                {
                    Console.WriteLine($"Enter {prop.Name} item (or hit Enter to finish):");
                    string item = Console.ReadLine();
                    if (string.IsNullOrEmpty(item))
                        break;
                    items.Add(item);
                }
                prop.SetValue(mario, items);
            }
        }
    }
}
