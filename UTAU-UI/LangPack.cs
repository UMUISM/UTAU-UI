using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace UI
{
    public class LangPack
    {
        private Dictionary<int, string> langData = new Dictionary<int, string>();
        private Dictionary<int, string> originLang;
        private XmlDocument conf = new XmlDocument();
        private bool useLangFile = false;
        private string nowLangName = "";
        public LangPack(string languageFile, Config config)
        {
            conf.Load(languageFile);
            if (config.language == "auto")
            {
                nowLangName = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            }
            else
            {
                nowLangName = config.language;
            }
            this.loadConfig();
            if (langData.Count != 0)
            {
                useLangFile = true;
            }
        }

        private void loadConfig()
        {
            XmlNodeList root = this.conf.GetElementsByTagName("languages");
            if (root.Count > 0)
            {
                XmlNodeList languages = ((XmlElement)root[0]).GetElementsByTagName("lang");
                foreach (XmlElement lang in languages)
                {
                    string langName = lang.GetAttribute("name");
                    Dictionary<int, string> tempList = new Dictionary<int, string>();
                    XmlNodeList sentences = lang.GetElementsByTagName("p");
                    foreach (XmlElement sentence in sentences)
                    {
                        tempList.Add(Convert.ToInt32(sentence.GetAttribute("id")), sentence.InnerText);
                    }
                    if (langName == "original")
                    {
                        originLang = tempList;
                    }
                    else
                    {
                        string[] nameList = langName.Split(',');
                        if (nameList.Contains(this.nowLangName))
                        {
                            langData = tempList;
                        }
                    }
                }
            }
        }

        public string fetch(string origin)
        {
            int id = originLang.FirstOrDefault(q => q.Value == origin).Key;  //get first key
            if (id != 0 && useLangFile && langData.ContainsKey(id))
            {
                return langData[id];
            } else {
                return origin;
            }
        }
    }
}
