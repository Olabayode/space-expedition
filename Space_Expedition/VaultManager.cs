namespace Space_Expedition
{
    internal class VaultManager
    {
        static readonly char[] Original = 
        {
            'A','B','C','D','E','F','G','H','I','J','K','L','M',
            'N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
        };
        static readonly char[] Mapped = 
        {
            'H','Z','A','U','Y','E','K','G','O','T','I','R','J',
            'V','W','N','M','F','Q','S','D','B','X','L','C','P'
        };

        static void ShowMenu()
        {
            Console.WriteLine("Galactic Vault Menu");
            Console.WriteLine("1. Add Artifact");
            Console.WriteLine("2. View Inventory");
            Console.WriteLine("3. Save and Exit");
            Console.Write("Select from 1 - 3: ");
        }

        public static void Run()
        {
            int count;
            Artifact[] vault = LoadVault("galactic_vault.txt", out count);
            SortArtifacts(vault, count);

            bool running = true;

            while (running)
            {
                ShowMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddArtifact(ref vault, ref count);
                        break;

                    case "2":
                        ViewInventory(vault, count);
                        break;

                    case "3":
                        SaveVault("expedition_summary.txt", vault, count);
                        Console.WriteLine("Artifacts Saved");
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Invalid Selection.");
                        break;
                }
            }
        }
        static Artifact[] LoadVault(string filePath, out int count)
        {
            count = 0;
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File not found: {filePath}");
                    return new Artifact[0];
                }
                    string[] lines = File.ReadAllLines(filePath);
                    Artifact[] artifacts = new Artifact[lines.Length];
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] fields = lines[i].Split(',');

                        if (fields.Length < 5)
                        {
                            Console.WriteLine($"Invalid data format in line {i + 1}");
                            continue;
                        }

                        Artifact artifact = new Artifact();
                        artifact.EncodedName = fields[0].Trim();
                        artifact.DecodedName = DecodeName(artifact.EncodedName);
                        artifact.Planet = fields[1].Trim();
                        artifact.DiscoveryDate = fields[2].Trim();
                        artifact.StorageLocation = fields[3].Trim();
                        artifact.Description = fields[4].Trim();

                        artifacts[count++] = artifact;
                    }

                    return artifacts;
                }
                catch (UnauthorizedAccessException)
                {
                    
                    Console.WriteLine("Access denied when trying to read the file.");
                }
                catch(IOException)
                {
                    Console.WriteLine("An Error occured while reading the file.");
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid Encoded name format");
                }
                return new Artifact[0];
        }

        
        //recursion
        static string DecodeName(string encoded)
        {
            string decoded = "";
            string[] parts = encoded.Split('|');

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                char letter = part[0];
                int level = int.Parse(part.Substring(1)); 
                decoded += DecodeChar(letter, level);
            }

            return decoded;
        }

        static char DecodeChar(char c, int level)
        {
            if (level == 1)
            {
                // Base case
                int index = Array.IndexOf(Original, c);
                return Original[25 - index]; //Alphabet inverse
            }

            // Recursive mapping
            int mappedIndex = Array.IndexOf(Original, c);
            char mappedChar = Mapped[mappedIndex];

            return DecodeChar(mappedChar, level - 1);
        }

        
        //sorting
        static void SortArtifacts(Artifact[] artifacts, int count)
        {
            for (int i = 1; i < count; i++)
            {
                Artifact key = artifacts[i];
                int j = i - 1;

                while (j >= 0 && string.Compare(artifacts[j].DecodedName, key.DecodedName) > 0)
                {
                    artifacts[j + 1] = artifacts[j];
                    j--;
                }

                artifacts[j + 1] = key;
            }
        }

        
        //binary search
        static int BinarySearch(Artifact[] artifacts, int count, string decodedName)
        {
            int left = 0;
            int right = count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                int comparison = string.Compare(artifacts[mid].DecodedName, decodedName);

                if (comparison == 0)
                    return mid;
                else if (comparison < 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return -1;
        }

        
        //insertion sort
        static void InsertArtifact(ref Artifact[] artifacts, ref int count, Artifact newArtifact)
        {
            Artifact[] newArray = new Artifact[artifacts.Length + 1];
            int i = 0;

            while (i < count && string.Compare(artifacts[i].DecodedName, newArtifact.DecodedName) < 0)
            {
                newArray[i] = artifacts[i];
                i++;
            }

            newArray[i] = newArtifact;

            for (int j = i; j < count; j++)
            {
                newArray[j + 1] = artifacts[j];
            }

            artifacts = newArray;
            count++;
        }

        
        //add artifact
        static void AddArtifact(ref Artifact[] vault, ref int count)
        {
            Console.Write("Enter artifact file name: ");
            string fileName = Console.ReadLine().Trim();

            try
            {
                int tempCount;
                Artifact[] temp = LoadVault(fileName, out tempCount);
                Artifact newArtifact = temp[0];

                if (BinarySearch(vault, count, newArtifact.DecodedName) == -1)
                {
                    InsertArtifact(ref vault, ref count, newArtifact);
                    Console.WriteLine("Artifact added successfully.");
                }
                else
                {
                    Console.WriteLine("Artifact already exists.");
                }
            }
            catch
            {
                Console.WriteLine("Error reading artifact file.");
            }
        }

        //view
        static void ViewInventory(Artifact[] vault, int count)
        {
            Console.WriteLine("Galactic Vault Inventory");
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(vault[i].DecodedName + " | " + vault[i].Planet + " | " + vault[i].DiscoveryDate);
            }
        }

        static void SaveVault(string filePath,  Artifact[] artifacts, int count)
        {
            string[] lines = new string[count];
            for(int i = 0; i < count; i++)
            {
                lines[i] = artifacts[i].EncodedName + ", " +
                    artifacts[i].Planet + ", " +
                    artifacts[i].DiscoveryDate + ", " +
                    artifacts[i].StorageLocation + ", " +
                    artifacts[i].Description;
            }
            File.WriteAllLines(filePath, lines);
        }        

    }
}

