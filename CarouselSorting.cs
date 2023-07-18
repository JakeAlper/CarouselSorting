// dotnet publish -r win10-x64 -c Release -o ./publish --self-contained=false /p:PublishSingleFile=true
using System.Text;

class Program
{
    static void Main()
    {
        string scriptDirectory = AppContext.BaseDirectory;
        string configFilePath = Path.Combine(scriptDirectory, "config.cfg");

        if (!File.Exists(configFilePath))
        {
            CreateConfigFile(configFilePath);
            
            // Exits the application with status code 0 (success)
            Environment.Exit(0);
        }

        string searchSort = "\"sorting_criteria\": ";
        string searchType = "\"types_order\": ";
        string filePath = ReadConfigValue(configFilePath, "carouselPath") ?? string.Empty;
        string repSort = searchSort + ReadConfigValue(configFilePath, "TanksSorting") ?? string.Empty;
        string repType = searchType + ReadConfigValue(configFilePath, "TypesSorting") ?? string.Empty;

        if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(searchSort) || string.IsNullOrEmpty(repSort) || string.IsNullOrEmpty(searchType) || string.IsNullOrEmpty(repType))
        {
            Console.WriteLine("Failed to read the config file. One or more config values may be incorrect or are not provided.");
            Console.ReadLine();
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            bool flagRepSort = false;
            bool flagRepType = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (!flagRepSort && lines[i].Contains(searchSort))
                {
                    int countLeadingSpaces = lines[i].TakeWhile(Char.IsWhiteSpace).Count();
                    lines[i] = repSort.PadLeft(repSort.Length + countLeadingSpaces);
                    flagRepSort = true;
                }

                if (!flagRepType && lines[i].Contains(searchType))
                {
                    int countLeadingSpaces = lines[i].TakeWhile(Char.IsWhiteSpace).Count();
                    lines[i] = repType.PadLeft(repType.Length + countLeadingSpaces);
                    flagRepType = true;
                }
            }
            if (flagRepSort && flagRepType)
            {
                File.WriteAllLines(filePath, lines);
                Console.WriteLine("Both replacements completed successfully!");
            }
            else
            {
                Console.WriteLine("One or more of the lines could not be replaced.");
            }
        }
        catch (IOException e)
        {
            Console.WriteLine("An error occurred while reading or writing the file:");
            Console.WriteLine(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred:");
            Console.WriteLine(e.Message);
        }

        Console.ReadLine();
    }

    static void CreateConfigFile(string filePath)
    {
        string[] lines = {
            "carouselPath=C:\\Games\\World_of_Tanks_NA\\res_mods\\configs\\xvm\\Aslain\\carousel.xc",
            "TanksSorting=-premium,level,type,nation",
            "TypesSorting=heavyTank,mediumTank,AT-SPG,lightTank,SPG"
        };

        File.WriteAllLines(filePath, lines);
    }

    static string ReadConfigValue(string filePath, string configKey)
    {
        if (File.Exists(filePath))
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                string prefix = configKey + "=";
                string line = lines.FirstOrDefault(l => l.StartsWith(prefix)) ?? string.Empty;

                if (line.Contains(','))
                {
                    return GenerateSortingString(line[(prefix.Length)..]);
                }
                if (line != null)
                {
                    return line[(prefix.Length)..];
                }
                else { return string.Empty; }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred:");
                Console.WriteLine(e.Message);
            }
        }

        return string.Empty;  // Return an empty string if the config file doesn't exist or the config value is not found
    }

    static string GenerateSortingString(string sortingString)
    {
        List<string> splitStringList = sortingString.Split(new[] { "," }, StringSplitOptions.None).ToList();
        StringBuilder sb = new();
        sb.Append('[');

        for (int i = 0; i < splitStringList.Count; i++)
        {
            sb.Append('"').Append(splitStringList[i]).Append('"');

            if (i < splitStringList.Count - 1)
            {
                sb.Append(", ");
            }
        }

       sb.Append("],");

        return sb.ToString();
    }
}
