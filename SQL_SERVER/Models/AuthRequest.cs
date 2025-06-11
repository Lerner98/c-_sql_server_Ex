using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_SERVER.Models
{
    public class AuthRequest
    {
        private string _email;
        private string _password;

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
    }

    public class LoginRequest : AuthRequest
    {
        // Inherits Email and Password properties from AuthRequest
    }

    public class RegisterRequest : AuthRequest
    {
        // Inherits Email and Password properties from AuthRequest
    }
}