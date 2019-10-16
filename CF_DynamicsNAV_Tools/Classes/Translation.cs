using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using Newtonsoft.Json;

namespace CF_DynamicsNAV_Tools.Classes
{
    /// <summary>
    /// Übersetzung mit DeepL
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("481484C0-BCF0-49AE-95BD-7B62D844D0C7")]
    class Translation : iTranslation
    {
        internal string accessKey = "";
        internal string baseUrl = "https://api.deepl.com/v2/translate";
        internal string sourceLanguage = "";
        internal string targetLanguage = "";
        internal string sourceText = "";
        internal string translatedText = "";
        internal string errorMessage = "";

        public Translation() { }

        public void SetAccessKey(string key)
        {
            accessKey = key;
        }

        public void SetSourceLanguage(string language)
        {
            sourceLanguage = language;
        }

        public void AddSourceText(string text)
        {
            sourceText += text;
        }

        public void ClearSourceText()
        {
            sourceText = "";
        }

        public int GetTranslatedLinesCount()
        {
            return 0;
        }
        public string GetTranslatedLines(int lineNumber)
        {
            return translatedText;
        }

        public string GetErrorMessage()
        {
            return errorMessage;
        }

        public bool GetTranslation(string language)
        {
            bool result = false;

            targetLanguage = language;

            errorMessage = "";

            try
            {
                WebClient wc = new WebClient();

                string response = wc.DownloadString(BuildRequestUrl());

                DeepLTranslations translation = JsonConvert.DeserializeObject<DeepLTranslations>(response);
                byte[] utf = System.Text.Encoding.Default.GetBytes(translation.translations.First().text);
                translatedText = System.Text.Encoding.UTF8.GetString(utf);

                result = true;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }

            return result;
        }

        internal string BuildRequestUrl()
        {
            string result = baseUrl;

            result += "auth_key=" + accessKey;

            if (sourceText != "")
            {
                result += "&text=" + sourceText;
            }

            if (sourceLanguage != "")
            {
                result += "&source_lang=" + sourceLanguage;
            }

            if (targetLanguage != "")
            {
                result += "&target_lang=" + targetLanguage;
            }

            result += "&tag_handling=xml";

            return result;
        }

        internal class DeepLTranslations
        {
            public IList<DeepLTranslation> translations;
        }

        internal class DeepLTranslation
        {
            public string detected_source_language;
            public string text;
        }
    }

}
