using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQL_SERVER.Models
{
    public class LanguageStat
    {
        private string _fromLang;
        private string _toLang;
        private int _translationCount;

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

        public int GetTranslationCount()
        {
            return this._translationCount;
        }

        public void SetTranslationCount(int count)
        {
            this._translationCount = count;
        }
    }
}

