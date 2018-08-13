using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UI
{
    public struct resamParam
    {
        public string gen;
        public string genfile;
        public string temp;
        public string pitchPercent;
        public double velocity;
        public string flags;
        public double offset;
        public double lengthReq;
        public double fix;
        public double blank;
        public double volume;
        public double modulation;
        public string tempo;
        public string pit;
    }

    public struct toolParam
    {
        public int len;
        public int resamParamId;
        public string outfile;
        public string infile;
        public double offset;
        public string length;
        public double p1;
        public double p2;
        public double p3;
        public double v1;
        public double v2;
        public double v3;
        public double v4;
        public double ovr;
        public double p4;
        public double p5;
        public double v5;
    }

    class UtauBat
    {
        public string batData;
        public List<resamParam> resamParams = new List<resamParam>();
        public List<toolParam> toolParams = new List<toolParam>();
        public Dictionary<string, string> settings = new Dictionary<string, string>();
        
        public UtauBat(string batfile)
        {
            this.batData = Utils.file_get_contents(batfile);
            this.parse();
        }

        private void parse()
        {
            string[] lines = this.batData.Replace("\r\n", "\n").Split('\n');
            foreach (string line in lines)
            {
                string[] args = Utils.parseArgs(line);
                if (args.Length > 0)
                {
                    string[] str;
                    resamParam p1;
                    toolParam p2;
                    switch (args[0])
                    {
                        case "@set":
                            //将cmd变量存入临时变量
                            string[] t = line.Substring(5).Split('=');
                            if (t.Length == 2)
                            {
                                this.settings[t[0]] = t[1].Trim('"');
                            }
                            break;
                        case "@call":
                            str = Utils.parseArgs(this.cmdFormat(string.Format("\"{0}\" \"{1}\" {2} {3} {4} {5} {6} {7} {8} {9}", args[2], settings["temp"], args[3], settings["vel"], settings["flag"], args[6], args[7], args[8], args[9], settings["params"])));
                            p1 = new resamParam();
                            p1.gen = Utils.getGen(str[0]);
                            p1.genfile = str[0];
                            p1.temp = str[1];
                            p1.pitchPercent = str[2];
                            p1.velocity = Convert.ToDouble(str[3]);
                            p1.flags = str[4];
                            p1.offset = Convert.ToDouble(str[5]);
                            p1.lengthReq = Convert.ToDouble(str[6]);
                            p1.fix = Convert.ToDouble(str[7]);
                            p1.blank = Convert.ToDouble(str[8]);
                            p1.volume = Convert.ToDouble(str[9]);
                            p1.modulation = Convert.ToDouble(str[10]);
                            p1.tempo = str[11];
                            if (str.Length > 12)
                                p1.pit = str[12];
                            else
                                p1.pit = "";
                            resamParams.Add(p1);
                            str = Utils.parseArgs(this.cmdFormat(string.Format("\"{0}\" \"{1}\" {2} {3} {4}", settings["output"], settings["temp"], settings["stp"], args[4], settings["env"])));
                            p2 = new toolParam();
                            p2.resamParamId = resamParams.Count - 1;
                            p2.len = str.Length;
                            p2.outfile = str[0];
                            p2.infile = str[1];
                            p2.offset = Convert.ToDouble(str[2]);
                            p2.length = str[3];
                            p2.p1 = Convert.ToDouble(str[4]);
                            p2.p2 = Convert.ToDouble(str[5]);
                            p2.p3 = Convert.ToDouble(str[6]);
                            p2.v1 = Convert.ToDouble(str[7]);
                            p2.v2 = Convert.ToDouble(str[8]);
                            p2.v3 = Convert.ToDouble(str[9]);
                            p2.v4 = Convert.ToDouble(str[10]);
                            if (str.Length > 11)
                                p2.ovr = Convert.ToDouble(str[11]);
                            else
                                p2.ovr = 0;
                            if (str.Length > 12)
                                p2.p4 = Convert.ToDouble(str[12]);
                            else
                                p2.p4 = 0;
                            if (str.Length > 13)
                                p2.p5 = Convert.ToDouble(str[13]);
                            else
                                p2.p5 = 0;
                            if (str.Length > 14)
                                p2.v5 = Convert.ToDouble(str[14]);
                            else
                                p2.v5 = 0;
                            toolParams.Add(p2);
                            break;
                        case "@%tool%":
                            str = Utils.parseArgs(this.cmdFormat(string.Format("\"{0}\" \"{1}\" {2} {3} {4} {5}", args[1], this.settings["oto"] + "\\R.wav", args[3], args[4], args[5], args[6])));
                            p2 = new toolParam();
                            p2.resamParamId = -1;
                            p2.len = str.Length;
                            p2.outfile = str[0];
                            p2.infile = str[1];
                            p2.offset = Convert.ToDouble(str[2]);
                            p2.length = str[3];
                            p2.p1 = Convert.ToDouble(str[4]);
                            p2.p2 = Convert.ToDouble(str[5]);
                            p2.p3 = 0;
                            p2.v1 = 0;
                            p2.v2 = 0;
                            p2.v3 = 0;
                            p2.v4 = 0;
                            p2.ovr = 0;
                            p2.p4 = 0;
                            p2.p5 = 0;
                            p2.v5 = 0;
                            toolParams.Add(p2);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public string cmdFormat(string cmd)
        {
            char[] charList = cmd.Replace("\\\\", "\\").ToCharArray();
            string tempName = "";
            bool inArg = false;
            string output = "";
            foreach (char one in charList)
            {
                if (one == '%')
                {
                    if (inArg)
                    {
                        if (this.settings.Keys.Contains(tempName))
                        {
                            output += this.settings[tempName];
                        }
                        tempName = "";
                        inArg = false;
                    }
                    else
                    {
                        inArg = true;
                    }
                }
                else
                {
                    if (inArg)
                    {
                        tempName += one;
                    }
                    else
                    {
                        output += one;
                    }
                }
            }
            return output;
        }
    }
}
