using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_SERVER.Models
{
    public class User
    {
        private Guid _id;
        private string _email;
        private string _passwordHash;
        private string _defaultFromLang;
        private string _defaultToLang;
        private DateTime _createdAt;
        private DateTime? _lastLogin;

        public Guid GetId()
        {
            return this._id;
        }

        public void SetId(Guid id)
        {
            this._id = id;
        }

        public string GetEmail()
        {
            return this._email;
        }

        public void SetEmail(string email)
        {
            this._email = email;
        }

        public string GetPasswordHash()
        {
            return this._passwordHash;
        }

        public void SetPasswordHash(string hash)
        {
            this._passwordHash = hash;
        }

        public string GetDefaultFromLang()
        {
            return this._defaultFromLang;
        }

        public void SetDefaultFromLang(string lang)
        {
            this._defaultFromLang = lang;
        }

        public string GetDefaultToLang()
        {
            return this._defaultToLang;
        }

        public void SetDefaultToLang(string lang)
        {
            this._defaultToLang = lang;
        }

        public DateTime GetCreatedAt()
        {
            return this._createdAt;
        }

        public void SetCreatedAt(DateTime createdAt)
        {
            this._createdAt = createdAt;
        }

        public DateTime? GetLastLogin()
        {
            return this._lastLogin;
        }

        public void SetLastLogin(DateTime? lastLogin)
        {
            this._lastLogin = lastLogin;
        }
    }
}
