using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AccountCollection
{
    public class Password
    {
        public string password { get; set; }
        public int pwStrengthNum { get; set; }
        public string pwStrengthText { get; set; }
        public string pwLastReset { get; set; }

    }

    public class Account
    {
        public string userID { get; set; }
        public string description { get; set; }
        public string loginURL { get; set; }
        public string accountNo { get; set; }       
        public Password psWord { get; set; }
        public Account()
        {
            psWord = new Password();
        }             
    }

    public class Collection
    {
        public List<Account> accCollection = new List<Account>();
        public void addAccount(Account acc)
        {
            accCollection.Add(acc);
        }
        public void deleteAccount(Account acc)
        {
            accCollection.Remove(acc);
        }
    }
   
}
