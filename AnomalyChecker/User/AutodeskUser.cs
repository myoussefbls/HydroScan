using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnomalyChecker.User
{
    public class AutodeskUser
    {
        private string _id;

        public AutodeskUser(string userAutodeskID) 
        {   
            _id = userAutodeskID;    
        }


        public bool IsAuthentified() 
        {
            List<string> authentifiedUsers = new List<string>()
            {
                "david.legouet",
                "educhateau",
                "fremondL4D42",
                "FSERRE92WHE",
                "Bgaultier",
                "GLAVALUDPAR",
                "Ibenabdallah",
                "isayehQ86F3",
                "spasquinelli",
                "IATINDEHOU",
                "Tjusteau",
                "Uosseni",
                "Yanisbalas",
                "GP.autodesk",
                "mohamed-youssef.krafess",
            };

            return authentifiedUsers.Contains(_id);



        }

    }
}
