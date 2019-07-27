/*
 Author: Kiet Nguyen
 Date: June 09, 2019
 Program: This program simulates a password manager console
 It allows user to add more account info, delete, update 
 password.
 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AccountCollection;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;


namespace PasswordManager
{
    class Program
    {
        static void Main(string[] args)
        {
            //display header
            Console.WriteLine("PASSWORD MANAGEMENT SYSTEM\n");
            bool show = false;
            string path_json_data = RelativeToAbsolutePath("accountCollection.json");
            string path_json_schema = RelativeToAbsolutePath("json-schema-password-manager.json");
            do
            {
                Console.WriteLine("+--------------------------------------------------------------------+");
                Console.WriteLine("|                            Account Entries                         |");
                Console.WriteLine("+--------------------------------------------------------------------+");
                Collection cltn = new Collection();
                //check if JSON file exists
                if (!File.Exists(path_json_data))
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
                bool inputAgain;

                // Add new entry                
                if (command.ToUpper() == "A")
                {
                    Account acc = new Account();
                    do
                    {
                        Console.WriteLine("Please key-in values for the following fields...\n");
                        Console.Write("Description:   ");
                        acc.description = Console.ReadLine();
                       
                        Console.Write("User ID:       ");
                        acc.userID = Console.ReadLine();

                        do
                        {
                            Console.Write("Password:      ");
                            acc.psWord.password = Console.ReadLine();
                            empty = string.IsNullOrEmpty(acc.psWord.password);
                            if (empty)
                            {
                                Console.WriteLine("ERROR: The password should not be empty.");
                            }                     
                        } while (empty);                            
                        acc.psWord.pwLastReset = DateTime.Now.ToShortDateString();
                        PasswordTester pwTester = new PasswordTester(acc.psWord.password);
                        acc.psWord.pwStrengthText = pwTester.StrengthLabel;
                        acc.psWord.pwStrengthNum = pwTester.StrengthPercent;

                        Console.Write("Login url:     ");
                        acc.loginURL = Console.ReadLine();
                        if(acc.loginURL == "")
                        {
                            acc.loginURL = null;// if user does not provide login url, set the value to null to make it valid when validate with format URI
                        }
                        
                        Console.Write("Account #:     ");
                        acc.accountNo = Console.ReadLine();
                        cltn.addAccount(acc);
                        //write data to Json file, Read the file and validate against schema
                        WriteDataToJsonFile(cltn);
                        string json_data = File.ReadAllText(path_json_data);
                        string json_schema = File.ReadAllText(path_json_schema);
                        IList<string> messages;
                        if (ValidateAccountData(json_data, json_schema, out messages)) // if validation is successful
                        {
                            show = true; //show main menu 
                            inputAgain = false; // don't ask for re-enter account info
                        }
                        else // unsuccessful validation against schema
                        {
                            Console.WriteLine("ERROR: Invalid account information entered. Please try again.");
                            foreach (string msg in messages)
                                Console.WriteLine($"\t{msg}"); // output all error msg
                            if (cltn.accCollection.Any())
                            {
                                cltn.deleteAccount(acc); // delete the account with invalid input
                            }
                            WriteDataToJsonFile(cltn);
                            inputAgain = true; //ask for re-enter account info
                        }
                    } while (inputAgain);                                           
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
                                do { 
                                    Console.Write("New Password:    ");
                                    selectedAccount.psWord.password = Console.ReadLine();
                                    empty = string.IsNullOrEmpty(selectedAccount.psWord.password);
                                    if (empty)
                                    {
                                        Console.WriteLine("ERROR: The password should not be empty.");
                                    }
                                } while(empty);
                                PasswordTester pwTester = new PasswordTester(selectedAccount.psWord.password);
                                selectedAccount.psWord.pwStrengthText = pwTester.StrengthLabel;
                                selectedAccount.psWord.pwStrengthNum = pwTester.StrengthPercent;
                                selectedAccount.psWord.pwLastReset = DateTime.Now.ToShortDateString();
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
            string path = RelativeToAbsolutePath("accountCollection.json");
            try
            {
                File.WriteAllText(path, json);
            }
            catch(IOException)
            {
                Console.WriteLine("ERROR: Cannot write to JSON file");
            }
        }

        private static Collection ReadJsonFileToCollection()
        {
            string path = RelativeToAbsolutePath("accountCollection.json");
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Collection>(json);
        }
        private static bool ValidateAccountData(string json_data, string json_schema, out IList<string>messages)
        {
            JSchema schema = JSchema.Parse(json_schema);
            JObject accounts = JObject.Parse(json_data);
            return accounts.IsValid(schema, out messages);
        }

        static string RelativeToAbsolutePath(string path)
        {
            var projectFolder = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            return Path.Combine(projectFolder, "..", @path);
        }
    }
}
