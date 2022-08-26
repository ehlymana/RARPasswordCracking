using System.Diagnostics;

#region Parameter setup

Console.WriteLine("Enter your PC username:");
string username = Console.ReadLine() ?? "Ehlimana";
Console.WriteLine("Choose dataset: 1 for automatically generated, 2 for RockYou, 3 for Password Strength");
int dataset = int.Parse(Console.ReadLine() ?? "1");
Console.WriteLine("Choose mode: 1 for John the Ripper, 2 for Hashcat");
int mode = int.Parse(Console.ReadLine() ?? "1");
Console.WriteLine("Choose RAR archive type: 3 for RAR3, 5 for RAR5");
int rarType = int.Parse(Console.ReadLine() ?? "3");
Console.WriteLine("Use 5-minute-long timeout? Y - yes, N - no");
string timeoutString = Console.ReadLine() ?? "Y";
bool timeout = timeoutString == "Y"? true : false;

#endregion

#region Global variables

// set up parameters for tool and file locations
string rockYouLocation = @"C:\Users\" + username + @"\Downloads\rockyou.txt",
       passwordStrengthLocation = @"C:\Users\" + username + @"\Downloads\data.csv",
       exampleFileLocation = @"C:\Users\" + username + @"\Desktop\file.txt",
       archiveLocation = @"C:\Users\" + username + @"\Desktop\rarArchive.rar",
       hashLocation = @"C:\Users\" + username + @"\Desktop\hash.txt",
       johnLocation = @"C:\Users\" + username + @"\Downloads\john-1.9.0-jumbo-1-win64\run",
       rarLocation = @"C:\Program Files\WinRAR",
       resultsLocation = @"C:\Users\" + username + @"\Desktop\results",
       // for RockYou:
       // dictionaryFolder = @"C:\Users\" + username + @"\Downloads\SecLists-master\Passwords\top5",
       // for randomly generated passwords:
       dictionaryFolder = @"C:\Users\" + username + @"\Downloads\SecLists-master\Passwords\top20",
       hashcatLocation = @"C:\Users\" + username + @"\Downloads\hashcat-6.2.5",
       hashcatOutputLocation = @"C:\Users\" + username + @"\Desktop\guess.txt",
       statisticalAnalysisLocation = @"C:\Users\" + username + @"\Desktop\statistics.txt";
List<string> dictionaries = Directory.GetFiles(dictionaryFolder).ToList(),
             resultFiles = new List<string>()
             {
                @"C:\Users\" + username + @"\Desktop\results_john (RAR3).txt",
                @"C:\Users\" + username + @"\Desktop\results_john (RAR5).txt",
                @"C:\Users\" + username + @"\Desktop\results_hashcat (RAR3).txt",
                @"C:\Users\" + username + @"\Desktop\results_hashcat (RAR5).txt"
             };

#endregion

#region Functions

/// <summary>
/// Function that generates a new password by using random characters.
/// Type 1: only a-z is used
/// Type 2: a-z and A-Z is used
/// Type 3: a-z, A-Z, 0-9 is used
/// Type 4: a-z, A-Z, 0-9, *-? is used
/// Type 5: extra characters (č, ć, š, ž, đ) are used
/// </summary>
string GeneratePassword(int noOfCharacters, int type)
{
    List<List<char>> passwordCharacters = new List<List<char>>()
    {
        new List<char>()
        { 
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' 
        },
        new List<char>()
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        },
        new List<char>()
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        },
        new List<char>()
        {
            '!', '"', '#', '$', '%', '&', '/', '(', ')', '=', '\'', '?', '+',
            '*', ',', ';', '.', ':', '-', '_', '<', '>', '@', '{', '}', '[',
            ']', '\\'
        },
        new List<char>()
        {
            'č', 'ć', 'đ', 'š', 'ž'
        }
    };
    string password = "";
    for (int i = 0; i < noOfCharacters; i++)
    {
        Random random = new Random();
        int listNumber = random.Next(0, type - 1);
        int characterNumber = random.Next(0, passwordCharacters[listNumber].Count - 1);

        password += passwordCharacters[listNumber][characterNumber];
    }
    return password;
}

/// <summary>
/// Function that creates a RAR archive by using an example TXT file
/// and encrypting the archive by using the provided password
/// </summary>
void CreateRAR(string password, int rarType)
{
    Process p = new Process();

    p.StartInfo.UseShellExecute = false;
    p.StartInfo.RedirectStandardOutput = true;
    p.StartInfo.FileName = rarLocation + @"\Rar.exe";
    if (rarType == 5)
        p.StartInfo.Arguments = "a -ep -m3 -ma5 -p" + password + " -r " + archiveLocation + " " + exampleFileLocation;
    else
        p.StartInfo.Arguments = "a -ep -m3 -p" + password + " -r " + archiveLocation + " " + exampleFileLocation;
    p.Start();
    p.WaitForExit();

    string output = p.StandardOutput.ReadToEnd();
    if (!output.Contains("OK"))
        throw new Exception("Could not create RAR file!");
}

/// <summary>
/// Function that generates hash for the previously generated RAR archive.
/// This hash can be used by John the Ripper to crack the password.
/// </summary>
string GetPasswordHash()
{
    Process p = new Process();
    
    p.StartInfo.UseShellExecute = false;
    p.StartInfo.RedirectStandardOutput = true;
    p.StartInfo.FileName = johnLocation + @"\rar2john.exe";
    p.StartInfo.Arguments = archiveLocation;
    p.Start();
    p.WaitForExit();

    string output = p.StandardOutput.ReadToEnd();
    output = output.Substring(output.IndexOf("$"));
    int delimiter = 0, i = 0;
    while (delimiter < 2 && i < output.Length)
    {
        if (output[i] == ':')
            delimiter++;
        i++;
    }
    if (delimiter > 0)
        i -= 3;
    return output.Substring(0, i);
}

/// <summary>
/// Function that does everything necessary to prepare the RAR file for cracking.
/// </summary>
void PrepareRAR(string newPassword, int rarType)
{
    // save example TXT file to a RAR archive
    // using the generated password so that Rar to John tool
    // can access it and decode the password hash
    CreateRAR(newPassword, rarType);

    // extract hashes for the generated RAR file
    string hash = GetPasswordHash();
    Console.WriteLine("Password hash to be cracked: " + hash);

    // delete created RAR file
    File.Delete(archiveLocation);

    // save hash to TXT file so that the tools can decrypt it
    using (StreamWriter writer2 = new StreamWriter(hashLocation))
        writer2.WriteLine(hash);
}

/// <summary>
/// Function that begins the password cracking process
/// by using John the Ripper tool
/// </summary>
void CrackThePasswordJohn(int noOfCharacters, int rarType, bool timeout)
{
    Process p = new Process();
    p.StartInfo.UseShellExecute = false;
    p.StartInfo.RedirectStandardOutput = true;
    p.StartInfo.FileName = johnLocation + @"\john.exe";
    string type = rarType == 3 ? "rar" : "RAR5";
    p.StartInfo.Arguments = hashLocation + " --incremental --format=" + type +
                            " --min-length=" + noOfCharacters +
                            " --max-length=" + noOfCharacters;
    p.Start();
    if (timeout)
    {
        if (!p.WaitForExit(5 * 60 * 1000))
            p.Kill();
    }
    else
        p.WaitForExit();
}

/// <summary>
/// Function that begins the password cracking process
/// by using Hashcat tool
/// </summary>
void CrackThePasswordHashcat(string dictionary, int rarType, bool timeout)
{
    string rar = rarType == 3 ? "23800" : "13000";
    Process p = new Process();
    p.StartInfo.UseShellExecute = true;
    p.StartInfo.WorkingDirectory = hashcatLocation;
    p.StartInfo.FileName = "hashcat.exe";
    p.StartInfo.Arguments = "-m " + rar + " -a 0 --hwmon-disable -o " +
                            hashcatOutputLocation + " " + hashLocation +
                            " " + dictionary;
    p.Start();
    if (timeout)
    {
        if (!p.WaitForExit(5 * 60 * 1000))
            p.Kill();
    }
    else
        p.WaitForExit();
}

/// <summary>
/// Function that checks whether John the Ripper has cracked the
/// password successfully or not
/// </summary>
bool PasswordCrackedJohn(string originalPassword)
{
    Process p = new Process();

    p.StartInfo.UseShellExecute = false;
    p.StartInfo.RedirectStandardOutput = true;
    p.StartInfo.FileName = johnLocation + @"\john.exe";
    p.StartInfo.Arguments = @"--show " + hashLocation;
    p.Start();
    p.WaitForExit();

    string output = p.StandardOutput.ReadToEnd();
    return output.Contains(originalPassword);
}

/// <summary>
/// Function that checks whether Hashcat has cracked the
/// password successfully or not
/// </summary>
bool PasswordCrackedHashcat()
{
    if (!File.Exists(hashcatOutputLocation))
        return false;

    using (StreamReader reader = new StreamReader(hashcatOutputLocation))
    {
        return reader.ReadToEnd().Length > 0;
    }
}

/// <summary>
/// Function that performs all actions of password cracking.
/// </summary>
void PerformAttack(int mode, int rarType, int j, string newPassword, bool timeout, string strength = "")
{
    Stopwatch stopwatch = new Stopwatch();

    // do brute force attack (J2R)
    if (mode == 1)
    {
        // begin measuring time of execution
        stopwatch.Reset();
        stopwatch.Start();

        // crack the password by using John the Ripper
        CrackThePasswordJohn(newPassword.Length, rarType, timeout);

        // stop measuring time of execution
        stopwatch.Stop();

        // check if the password has been cracked successfully
        int executionTime = -1;
        if (PasswordCrackedJohn(newPassword))
            executionTime = (int)stopwatch.ElapsedMilliseconds;

        Console.WriteLine("Execution time for password length: " + newPassword.Length + ": " + executionTime + " ms");

        // write results to a file
        string resultFile = resultsLocation + "_john.txt";
        using (StreamWriter writer = new StreamWriter(resultFile, true))
        {
            writer.WriteLine("Original password: " + newPassword + ", mode: " + j +
                             ", password length: " + newPassword.Length +
                             ", processing time (ms): " + executionTime +
                             ", password strength: " + strength +
                             ", type of RAR: " + rarType);
        }
    }

    // iterate through every dictionary provided (HC)
    else
    {
        foreach (string dictionary in dictionaries)
        {
            // begin measuring time of execution
            stopwatch.Reset();
            stopwatch.Start();

            // crack the password by using Hashcat
            CrackThePasswordHashcat(dictionary, rarType, timeout);

            // stop measuring time of execution
            stopwatch.Stop();

            // check if the password has been cracked successfully
            int executionTime = -1;
            if (PasswordCrackedHashcat())
                executionTime = (int)stopwatch.ElapsedMilliseconds;

            Console.WriteLine("Execution time for dictionary " + dictionary + ": " + executionTime + " ms");

            // write results to a file
            string resultFile = resultsLocation + "_hashcat.txt";
            using (StreamWriter writer = new StreamWriter(resultFile, true))
            {
                writer.WriteLine("Original password: " + newPassword + ", mode: " + j +
                                 ", password length: " + newPassword.Length +
                                 ", dictionary: " + dictionary + 
                                 ", processing time (ms): " + executionTime + 
                                 ", password strength: " + strength +
                                 ", type of RAR: " + rarType);
            }

            // delete the hashcat results file if it exists
            if (File.Exists(hashcatOutputLocation))
                File.Delete(hashcatOutputLocation);
        }
    }
}

/// <summary>
/// Function that imports the desired dataset.
/// RockYou dataset is TXT and only contains passwords.
/// Password Strength dataset is CSV and contains passwords and their strengths.
/// This information is imported along with the password because it might be useful
/// for analysis.
/// </summary>
List<string> ImportDataset(int type)
{
    List<string> password = new List<string>();

    // insert the RockYou dataset (TXT type, one-column)
    if (type == 1)
    {
        password = File.ReadAllLines(rockYouLocation).ToList();
    }

    // insert the Password Strength dataset (CSV type, two-column)
    else
        using (var reader = new StreamReader(passwordStrengthLocation))
        {
            while (!reader.EndOfStream)
                password.Add(reader.ReadLine() ?? "");
        }

    return password;
}

/// <summary>
/// Function that takes the results of the cracking process and calculates
/// statistically important results.
/// </summary>
void StatisticalAnalysis()
{
    
    foreach (string file in resultFiles)
    {
        List<string> allLines = File.ReadAllLines(file).ToList();

        // create lists sorted by processing times:
        // from 0 s to 1 min, to 2 min, to 3 min, to 4 min, to 5 min, not found
        List<List<Tuple<int, int>>> processingTimesByLength = new List<List<Tuple<int, int>>>()
                                    {
                                        new List<Tuple<int, int>>(),
                                        new List<Tuple<int, int>>(),
                                        new List<Tuple<int, int>>(),
                                        new List<Tuple<int, int>>(),
                                        new List<Tuple<int, int>>(),
                                        new List<Tuple<int, int>>()
                                    },
                                    processingTimesByStrength = new List<List<Tuple<int, int>>>()
                                    {
                                        new List<Tuple<int, int>>(),
                                        new List<Tuple<int, int>>(),
                                        new List<Tuple<int, int>>(),
                                        new List<Tuple<int, int>>(),
                                        new List<Tuple<int, int>>(),
                                        new List<Tuple<int, int>>()
                                    };
        List<int> processingTimesByDictionary = new List<int>()
        {
            0, 0, 0, 0, 0
        };

        // the statistic is different for hashcat (one password in five rows)
        if (!file.Contains("hashcat"))
        {
            // count the relevant statistic for every password
            foreach (string line in allLines)
            {
                // first extract the relevant information
                string[] result = line.Split(",");
                int passwordLength = int.Parse(string.Concat(result[2].Where(Char.IsDigit)) ?? "0");
                double processingTime = double.Parse(string.Concat(result[3].Where(Char.IsDigit)) ?? "1");
                if (processingTime == 1)
                    processingTime = -1;
                string strength = string.Concat(result[4].Where(Char.IsDigit)) ?? "";

                // determine which element in the list we are going to append
                // if the password has not been found, processing time is assumed to be
                // more than 5 minutes (element no. 5)
                int listElement = 5;
                if (processingTime != -1)
                    listElement = (int)(processingTime / 60000);

                // check if the required password length has already been counted
                var element = processingTimesByLength[listElement].Find(time => time.Item1 == passwordLength);
                if (element != null)
                {
                    var newElement = new Tuple<int, int>(element.Item1, element.Item2 + 1);
                    processingTimesByLength[listElement].Remove(element);
                    processingTimesByLength[listElement].Add(newElement);
                }
                else
                    processingTimesByLength[listElement].Add(new Tuple<int, int>(passwordLength, 1));

                // also attempt to append information about password strength
                // (only present in one dataset)
                if (strength.Length > 0)
                {
                    var element2 = processingTimesByStrength[listElement].Find(time => time.Item1 == int.Parse(strength));
                    if (element2 != null)
                    {
                        var newElement2 = new Tuple<int, int>(element2.Item1, element2.Item2 + 1);
                        processingTimesByStrength[listElement].Remove(element2);
                        processingTimesByStrength[listElement].Add(newElement2);
                    }
                    else
                        processingTimesByStrength[listElement].Add(new Tuple<int, int>(int.Parse(strength), 1));
                }
            }
        }
        else
        {
            for (int i = 0; i < allLines.Count; i += 5)
            {
                // first we need to find for which dictionary the processing time
                // is different than -1
                int j = i;
                while( j < i + 5)
                {
                    string[] result1 = allLines[j].Split(",");
                    double processingTime1 = double.Parse(string.Concat(result1[4].Where(Char.IsDigit)) ?? "1");
                    if (processingTime1 == 1)
                    {
                        j++;
                        continue;
                    }
                    else
                        break;
                }
                int dictionary = j - i;
                if (dictionary < 5)
                    processingTimesByDictionary[dictionary]++;
                // the password was not cracked - insert last info into results file
                else
                    dictionary = 4;
                // first extract the relevant information
                string[] result = allLines[i + dictionary].Split(",");
                int passwordLength = int.Parse(string.Concat(result[2].Where(Char.IsDigit)) ?? "0");
                double processingTime = double.Parse(string.Concat(result[4].Where(Char.IsDigit)) ?? "1");
                if (processingTime == 1)
                    processingTime = -1;
                string strength = string.Concat(result[5].Where(Char.IsDigit)) ?? "";

                // determine which element in the list we are going to append
                // if the password has not been found, processing time is assumed to be
                // more than 5 minutes (element no. 5)
                int listElement = 5;
                if (processingTime != -1)
                    listElement = (int)(processingTime / 60000);

                // check if the required password length has already been counted
                var element = processingTimesByLength[listElement].Find(time => time.Item1 == passwordLength);
                if (element != null)
                {
                    var newElement = new Tuple<int, int>(element.Item1, element.Item2 + 1);
                    processingTimesByLength[listElement].Remove(element);
                    processingTimesByLength[listElement].Add(newElement);
                }
                else
                    processingTimesByLength[listElement].Add(new Tuple<int, int>(passwordLength, 1));

                // also attempt to append information about password strength
                // (only present in one dataset)
                if (strength.Length > 0)
                {
                    var element2 = processingTimesByStrength[listElement].Find(time => time.Item1 == int.Parse(strength));
                    if (element2 != null)
                    {
                        var newElement2 = new Tuple<int, int>(element2.Item1, element2.Item2 + 1);
                        processingTimesByStrength[listElement].Remove(element2);
                        processingTimesByStrength[listElement].Add(newElement2);
                    }
                    else
                        processingTimesByStrength[listElement].Add(new Tuple<int, int>(int.Parse(strength), 1));
                }
            }
        }

        // done processing a single file - write the information to a file
        using (StreamWriter writer = new StreamWriter(statisticalAnalysisLocation, true))
        {
            writer.WriteLine("******************************************");
            writer.WriteLine("The following file was analyzed: " + file);
            for (int i = 0; i < 5; i++)
            {
                writer.WriteLine("Processing time less than " + (i + 1) + " min");
                writer.WriteLine("(Password length), (Count)");
                foreach (var element in processingTimesByLength[i])
                    writer.WriteLine(element.Item1 + ", " + element.Item2);

                if (processingTimesByStrength[i].Count > 0)
                {
                    writer.WriteLine("Strengths of passwords");
                    writer.WriteLine("(Password strength), (Count)");
                    foreach (var element in processingTimesByStrength[i])
                        writer.WriteLine(element.Item1 + ", " + element.Item2);
                }
            }
            writer.WriteLine("Passwords which were not successfully recovered during 5 min period");
            writer.WriteLine("(Password length), (Count)");
            foreach (var element in processingTimesByLength[5])
                writer.WriteLine(element.Item1 + ", " + element.Item2);

            if (processingTimesByStrength[5].Count > 0)
            {
                writer.WriteLine("Strengths of passwords");
                writer.WriteLine("(Password strength), (Count)");
                foreach (var element in processingTimesByStrength[5])
                    writer.WriteLine(element.Item1 + ", " + element.Item2);
            }

            // additional dictionary info for hashcat analysis
            if (file.Contains("hashcat"))
            {
                writer.WriteLine("Dictionaries in which the passwords were recovered");
                writer.WriteLine("(Number of dictionary), (Count)");
                for (int i = 0; i < processingTimesByDictionary.Count; i++)
                    writer.WriteLine(i + ", " + processingTimesByDictionary[i]);
            }

            writer.WriteLine("******************************************");
        }
    }
}

#endregion

#region Password cracking

Console.WriteLine("Print statistical analysis? Y - yes, N - no");
string statString = Console.ReadLine() ?? "Y";
bool stat = statString == "Y" ? true : false;
if (stat)
{
    StatisticalAnalysis();
}

else
{
    // automatically generated password mode
    if (dataset == 1)
    {
        // pasword modes and lengths can only be setup if they are automatically generated
        Console.WriteLine("Starting mode (1-5):");
        int startingMode = int.Parse(Console.ReadLine() ?? "1");
        Console.WriteLine("Starting password length (1-20):");
        int startingLength = int.Parse(Console.ReadLine() ?? "1");

        // create new passwords up to 20 characters
        for (int i = startingLength; i < 21; i++)
        {
            // create new passwords for all modes
            for (int j = startingMode; j < 6; j++)
            {
                // generate textual representations for passwords
                string newPassword = GeneratePassword(i, j);
                Console.WriteLine("Password to be cracked: " + newPassword);

                PrepareRAR(newPassword, rarType);

                PerformAttack(mode, rarType, j, newPassword, timeout);
            }
        }
    }

    // dataset mode
    else
    {
        List<string> datasetPasswords = ImportDataset(dataset - 1);

        foreach (string password in datasetPasswords)
        {
            string newPassword = password,
                   strength = "";

            if (dataset == 3)
            {
                string[] content = password.Split(",");
                newPassword = content[0];
                strength = content[1];
            }

            Console.WriteLine("Password to be cracked: " + newPassword);

            PrepareRAR(newPassword, rarType);

            PerformAttack(mode, rarType, -1, newPassword, timeout, strength);
        }
    }
}

#endregion

#region Cleanup

// delete created hash file (don't leave trash)
File.Delete(hashLocation);

#endregion