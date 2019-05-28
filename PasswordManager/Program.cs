using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AccountCollection;
using Newtonsoft.Json;

namespace PasswordManager
{
    class Program
    {
        static void Main(string[] args)
        {
            //display header
            Console.WriteLine("PASSWORD MANAGEMENT SYSTEM\n");
            bool show = false;
            do
            {
                Console.WriteLine("+--------------------------------------------------------------------+");
                Console.WriteLine("|                            Account Entries                         |");
                Console.WriteLine("+--------------------------------------------------------------------+");
                Collection cltn = new Collection();
                //check if JSON file exists
                if (!File.Exists(@"c:\accountCollection.json"))
                {
                    cltn.accCollection = new List<Account>();
                }
                else
                {
                    //if the JSON exists, try to read and display a list of existing accounts                
                    try
                    {
                        cltn = ReadJsonFileToCollection();//read json file
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("ERROR: Cannot read from JSON file.");
                    }
                }
                if (cltn.accCollection.Any())// if there is any existing account in collection
                {
                    int seq = 1;
                    foreach (Account acc in cltn.accCollection)
                    {
                        Console.WriteLine("|  " + seq + $". {acc.description}");
                        seq++;
                    }
                }
                Console.WriteLine("+--------------------------------------------------------------------+");
                //display command instruction
                if (cltn.accCollection.Any())// if there is any existing account in collection
                {// display this line
                    Console.WriteLine("|     Press # from above list to select an entry.");
                }
                Console.WriteLine("|     Press A to add a new entry.");
                Console.WriteLine("|     Press X to quit.");
                Console.WriteLine("+--------------------------------------------------------------------+\n");
                Console.Write("Enter a command: ");
                string command = Console.ReadLine();
                int commandNum = 0;
                string PDMCommand = "";
                bool validPDM;
                bool empty;

                // Add new entry                
                if (command.ToUpper() == "A")
                {
                    Account acc = new Account();
                    Console.WriteLine("Please key-in values for the following fields...\n");
                    do
                    {
                        Console.Write("Description:   ");
                        acc.description = Console.ReadLine();
                        empty = string.IsNullOrEmpty(acc.description);
                        if(empty)
                        {
                            Console.WriteLine("ERROR: The value should not be empty.");
                        }
                    } while (empty);

                    do
                    {
                        Console.Write("User ID:       ");
                        acc.userID = Console.ReadLine();
                        empty = string.IsNullOrEmpty(acc.userID);
                        if (empty)
                        {
                            Console.WriteLine("ERROR: The value should not be empty.");
                        }
                    } while (empty);

                    do
                    {
                        Console.Write("Password:      ");
                        acc.psWord.password = Console.ReadLine();
                        empty = string.IsNullOrEmpty(acc.psWord.password);
                        if (empty)
                        {
                            Console.WriteLine("ERROR: The value should not be empty.");
                        }                        
                    } while (empty);
                    acc.psWord.pwLastReset = DateTime.Now.ToShortDateString();
                    PasswordTester pwTester = new PasswordTester(acc.psWord.password);
                    acc.psWord.pwStrengthText = pwTester.StrengthLabel;
                    acc.psWord.pwStrengthNum = pwTester.StrengthPercent;

                    bool validURL;
                    do
                    {
                        Console.Write("Login url:     ");
                        acc.loginURL = Console.ReadLine();
                        validURL = Regex.IsMatch(acc.loginURL, @"\Ahttps?://www.+");
                        if(!validURL)
                        {
                            Console.WriteLine("ERROR: Invalid Login URL entered. Please try again.");
                        }
                    } while (!validURL);

                    Console.Write("Account #:     ");
                    acc.accountNo = Console.ReadLine();
                    cltn.addAccount(acc);
                    cltn.accCollection.Any();
                    show = true;
                }
                //View selected entry
                else if (Int32.TryParse(command, out commandNum))
                {
                    try
                    {
                        Account selectedAccount = cltn.accCollection[commandNum - 1];
                        if (commandNum > 0 && commandNum <= cltn.accCollection.Count)
                        {
                            Console.WriteLine("+--------------------------------------------------------------------+");
                            Console.WriteLine("|  " + commandNum + $". {selectedAccount.description}");
                            Console.WriteLine("+--------------------------------------------------------------------+");
                            Console.WriteLine($"| User ID:             {selectedAccount.userID}");
                            Console.WriteLine($"| Password:            {selectedAccount.psWord.password}");
                            Console.WriteLine($"| Password Strength:   {selectedAccount.psWord.pwStrengthText} ({selectedAccount.psWord.pwStrengthNum}%)");
                            Console.WriteLine($"| Password Reset:      {selectedAccount.psWord.pwLastReset}");
                            Console.WriteLine($"| Login url:           {selectedAccount.loginURL}");
                            Console.WriteLine($"| Account #:           {selectedAccount.accountNo}");
                            do
                            {
                                Console.WriteLine("+--------------------------------------------------------------------+");
                                Console.WriteLine("|     Press P to change this password.");
                                Console.WriteLine("|     Press D to delete this entry.");
                                Console.WriteLine("|     Press M to return to main menu.");
                                Console.WriteLine("+--------------------------------------------------------------------+");
                                Console.Write("Enter a command: ");
                                PDMCommand = Console.ReadLine();
                                validPDM = PDMCommand.ToUpper() == "P" || PDMCommand.ToUpper() == "D" || PDMCommand.ToUpper() == "M";
                                if (!validPDM)
                                {
                                    Console.WriteLine("ERROR: Incorrect command. Please try again!");
                                }
                            } while (!validPDM);
                            //Change Password
                            if (PDMCommand.ToUpper() == "P")
                            {
                                Console.Write("New Password:    ");
                                selectedAccount.psWord.password = Console.ReadLine();
                                PasswordTester pwTester = new PasswordTester(selectedAccount.psWord.password);
                                selectedAccount.psWord.pwStrengthText = pwTester.StrengthLabel;
                                selectedAccount.psWord.pwStrengthNum = pwTester.StrengthPercent;
                                show = true; //display main menu
                            }
                            //Delete selected entry
                            else if (PDMCommand.ToUpper() == "D")
                            {
                                Console.Write("Delete? (Y/N):   ");
                                bool delete = Console.ReadKey().KeyChar == 'y';
                                Console.WriteLine();
                                if (delete)
                                {
                                    cltn.deleteAccount(selectedAccount);
                                }
                                show = true;
                            }
                            else if (PDMCommand.ToUpper() == "M")
                            {
                                show = true; // go back to main menu
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Invalid account sequence in the list!");
                        show = true;
                    }
                    
                }
                //Exit program
                else if (command.ToUpper() == "X") 
                {
                    show = false; // exit program
                }
                else // wrong command will notify error
                {
                    Console.WriteLine("ERROR: Incorrect command. Please try again!");
                    show = true;
                }
             
                WriteDataToJsonFile(cltn);
            } while (show);
        }

        private static void WriteDataToJsonFile(Collection cltn)
        {
            string json = JsonConvert.SerializeObject(cltn);
            File.WriteAllText("c:\\accountCollection.json", json);
        }

        private static Collection ReadJsonFileToCollection()
        {
            string json = File.ReadAllText("c:\\accountCollection.json");
            return JsonConvert.DeserializeObject<Collection>(json);
        }

    }
}
