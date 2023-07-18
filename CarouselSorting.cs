class Program
{
    static void Main()
    {
        string scriptDirectory = AppContext.BaseDirectory;
        string configFilePath = Path.Combine(scriptDirectory, "config.cfg");

        if (!File.Exists(configFilePath))
        {
            CreateConfigFile(configFilePath);
        }

        string filePath = ReadConfigValue(configFilePath, "filePath") ?? string.Empty;
        string searchLineSorting = ReadConfigValue(configFilePath, "searchLineSorting") ?? string.Empty;
        string replacementLineSorting = ReadConfigValue(configFilePath, "replacementLineSorting") ?? string.Empty;
        string searchLineType = ReadConfigValue(configFilePath, "searchLineType") ?? string.Empty;
        string replacementLineType = ReadConfigValue(configFilePath, "replacementLineType") ?? string.Empty;

        if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(searchLineSorting) || string.IsNullOrEmpty(replacementLineSorting) || string.IsNullOrEmpty(searchLineType) || string.IsNullOrEmpty(replacementLineType))
        {
            Console.WriteLine("Failed to read the config file or one or more config values are not provided.");
            Console.ReadLine();
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            bool lineReplacedSorting = false;
            bool lineReplacedType = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (!lineReplacedSorting && lines[i].Contains(searchLineSorting))
                {
                    lines[i] = replacementLineSorting;
                    lineReplacedSorting = true;
                }

                if (!lineReplacedType && lines[i].Contains(searchLineType))
                {
                    lines[i] = replacementLineType;
                    lineReplacedType = true;
                }

                if (lineReplacedSorting && lineReplacedType)
                    break;  // Assuming only one occurrence of each line
            }

            if (lineReplacedSorting && lineReplacedType)
            {
                File.WriteAllLines(filePath, lines);
                Console.WriteLine("Replacement completed successfully!");
            }
            else
            {
                Console.WriteLine("One or more of the lines to replace were not found in the file.");
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
            "filePath=C:\\Games\\World_of_Tanks_NA\\res_mods\\configs\\xvm\\Aslain\\carousel.xc",
            "searchLineSorting=\"sorting_criteria\":",
            "replacementLineSorting=\"sorting_criteria\": [\"-premium\", \"level\", \"type\", \"nation\"],",
            "searchLineType=\"types_order\":",
            "replacementLineType=\"types_order\": [\"heavyTank\", \"mediumTank\", \"AT-SPG\", \"lightTank\", \"SPG\"]"
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
                foreach (string line in lines)
                {
                    if (line.StartsWith(configKey + "="))
                    {
                        return line[(configKey.Length + 1)..];  // Assuming the format is "key=value"
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred:");
                Console.WriteLine(e.Message);
            }
        }

        return string.Empty;  // Return an empty string if the config file doesn't exist or the config value is not found
    }
}
