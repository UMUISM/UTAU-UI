using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace UI
{
    public class Config
    {
        public XmlDocument conf;
        public string resamplerMode = "dll";
        public string resampler = "resampler.dll";
        public string wavtool = "wavtool.exe";
        public string language = "auto";
        public bool multiThread = false;
        public int threadNum = 4;
        public bool showConsole = false;
        public Config(string file)
        {
            this.conf = new XmlDocument();
            this.conf.Load(file);
            this.initConfig();
        }

        public bool initConfig()
        {
            try
            {
                XmlNodeList root = this.conf.GetElementsByTagName("config");
                if (root.Count > 0)
                {
                    XmlElement options = (XmlElement)(((XmlElement)root[0]).GetElementsByTagName("options")[0]);
                    this.resamplerMode = options.GetElementsByTagName("resampler-mode")[0].InnerText;
                    this.resampler = string.Format(options.GetElementsByTagName("resampler")[0].InnerText, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
                    this.wavtool = string.Format(options.GetElementsByTagName("wavtool")[0].InnerText, System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
                    this.multiThread = (options.GetElementsByTagName("multi-thread")[0].InnerText == "true") ? true : false;
                    this.threadNum = Convert.ToInt32(options.GetElementsByTagName("thread-num")[0].InnerText);
                    this.showConsole = (options.GetElementsByTagName("show-console")[0].InnerText == "true") ? true : false;
                    this.language = options.GetElementsByTagName("language")[0].InnerText;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
