using System;

namespace SQL_SERVER.Models
{
    public class TranslateRequest
    {
        private string _text;
        private string _targetLang;
        private string _sourceLang;

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public string TargetLang
        {
            get { return _targetLang; }
            set { _targetLang = value; }
        }

        public string SourceLang
        {
            get { return _sourceLang; }
            set { _sourceLang = value; }
        }
    }
}