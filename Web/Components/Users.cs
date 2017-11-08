using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDemo.Components
{
    public class Users
    {
        public static List<User> LUsers = new List<User>() { 
            new User(){ Name="admin", Pass="1"}, 
            new User(){ Name="", Pass="1"}, 
            new User(){ Name="", Pass="1"}, 
            new User(){ Name="", Pass="1"}, 
            new User(){ Name="", Pass="1"}, 
            new User(){ Name="", Pass="1"},
            };

        public static bool bfLogon(string name, string pass)
        {
            return false;
        }

    }

    public class User
    {
        public string Name { get; set; }
        public string Pass { get; set; }
    }

}
