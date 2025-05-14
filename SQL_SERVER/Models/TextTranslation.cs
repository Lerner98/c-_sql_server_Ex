using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_SERVER.Models
{
    public class TextTranslation
    {
        private Guid _id;
        private Guid _userId;
        private string _fromLang;
        private string _toLang;
        private string _originalText;
        private string _translatedText;
        private string _type;
        private DateTime _createdAt;

        public Guid GetId()
        {
            return this._id;
        }

        public void SetId(Guid id)
        {
            this._id = id;
        }

        public Guid GetUserId()
        {
            return this._userId;
        }

        public void SetUserId(Guid userId)
        {
            this._userId = userId;
        }

        public string GetFromLang()
        {
            return this._fromLang;
        }

        public void SetFromLang(string fromLang)
        {
            this._fromLang = fromLang;
        }

        public string GetToLang()
        {
            return this._toLang;
        }

        public void SetToLang(string toLang)
        {
            this._toLang = toLang;
        }

        public string GetOriginalText()
        {
            return this._originalText;
        }

        public void SetOriginalText(string originalText)
        {
            this._originalText = originalText;
        }

        public string GetTranslatedText()
        {
            return this._translatedText;
        }

        public void SetTranslatedText(string translatedText)
        {
            this._translatedText = translatedText;
        }

        public string GetTranslationType()
        {
            return this._type;
        }

        public void SetTranslationType(string type)
        {
            this._type = type;
        }

        public DateTime GetCreatedAt()
        {
            return this._createdAt;
        }

        public void SetCreatedAt(DateTime createdAt)
        {
            this._createdAt = createdAt;
        }
    }
}
