using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexifyMobile
{

    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public bool success { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string token { get; set; }

        public bool anatomy { get; set; }
        public User(string u, string t)
        {
            username = u; 
            token = t;
            anatomy = true;
        }
        public User()
        {

        }
    }

}
