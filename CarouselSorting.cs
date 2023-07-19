// dotnet publish -r win10-x64 -c Release -o ./publish --self-contained=false /p:PublishSingleFile=true
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;

class Program
{
    static void Main()
    {
        string scriptDirectory = AppContext.BaseDirectory;
        string oldConfigFileName = "config.cfg";
        string newConfigFileName = "config-v3.cfg";
        string configFilePath = Path.Combine(scriptDirectory, newConfigFileName);
        if (File.Exists(Path.Combine(scriptDirectory, oldConfigFileName)))
        {
            File.Delete(Path.Combine(scriptDirectory, oldConfigFileName));
        }
        if (!File.Exists(configFilePath))
        {
            CreateConfigFile(configFilePath);
            Console.WriteLine("Config file not found...");
            Console.WriteLine("Creating one now...");
            Console.WriteLine();
            Console.WriteLine("Config file created successfully.");
            Console.WriteLine($"Please take a moment to check the config file ({newConfigFileName}) and make any changes you'd like before running the script again.");
            Sleep(30);
            Environment.Exit(0);
        }
        string searchType = "\"types_order\": ";
        string searchSort = "\"sorting_criteria\": ";
        string filePath = FindWorldOfTanksPath();
        string repType = searchType + ReadConfigValue(configFilePath, "TypesSorting") ?? string.Empty;
        string repSort = searchSort + ReadConfigValue(configFilePath, "TanksSorting") ?? string.Empty;
        if (string.IsNullOrEmpty(repType) || string.IsNullOrEmpty(repSort))
        {
            Console.WriteLine("Failed to read the config file. One or more config values may be formatted incorrectly or are not provided.");
            Console.ReadLine();
            Sleep(30);
            Environment.Exit(0);
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
                Sleep(30);
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("One or more of the lines could not be replaced. Make sure you have World of Tanks and Aslain's modpack (with XVM) installed on your machine.");
            }
        }
        catch (IOException e)
        {
            Console.WriteLine("An error occurred while reading or writing the file:");
            Console.WriteLine(e.Message);
        }
    }
    static void Sleep(int seconds)
    {
        int milliseconds = seconds * 1000;
        Thread.Sleep(milliseconds);
    }
    static void CreateConfigFile(string filePath)
    {
        try {
            File.WriteAllLines(filePath, new string[] {
                "TypesSorting=heavyTank,mediumTank,AT-SPG,lightTank,SPG",
                "TanksSorting=-premium,-level,type,marksOnGun,battles",
                "### Sorting criteria: nation, type, level, -level, maxBattleTier,",
                "### -maxBattleTier, premium, -premium, battles, -battles, winRate,",
                "### -winRate, markOfMastery, -markOfMastery, xtdb, -xtdb, xte, -xte,",
                "### wtr, -wtr, damageRating, -damageRating, marksOnGun, -marksOnGun"
            });
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred:");
            Console.WriteLine(e.Message);
        }
    }
    static string ReadConfigValue(string filePath, string configKey)
    {
        try
        {
            string[] lines = File.ReadLines(filePath).Take(2).ToArray();
            string prefix = $"{configKey}=";
            foreach (var line in lines.Where(line => line.StartsWith(prefix)))
            {
                return GenerateSortingString(line[prefix.Length..]);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred:");
            Console.WriteLine(e.Message);
        }

        return string.Empty;
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

    static string FindWorldOfTanksPath()
    {
        const string registrySearchTerm = "WorldOfTanks.exe.FriendlyAppName";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                const string keyPath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache";
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(keyPath);

                if (key == null) return string.Empty;

                var foundValueName = key.GetValueNames().FirstOrDefault(valueName => valueName.Contains(registrySearchTerm));

                if (string.IsNullOrEmpty(foundValueName)) return string.Empty;

                var cleanedValue = foundValueName.Replace(".FriendlyAppName", string.Empty);
                var directory = Path.GetDirectoryName(cleanedValue);
                
                if (string.IsNullOrEmpty(directory)) return string.Empty;

                var root = Directory.GetParent(directory)?.FullName;

                if (string.IsNullOrEmpty(root)) return string.Empty;

                return Path.Combine(root, "res_mods", "configs", "xvm", "Aslain", "carousel.xc");
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred:");
                Console.WriteLine(e.Message);
            }
        }

        return string.Empty; // If the value name is not found
    }
}