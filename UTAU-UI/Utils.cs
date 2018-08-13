using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;


namespace UI
{
    class Utils
    {
        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        public static string tempTitle = "fastw4u - " + GetRandomString(10, true, true, true, false, "");
        public static void hideBat()
        {
            Console.Title = tempTitle;
            IntPtr intptr = FindWindow("ConsoleWindowClass", tempTitle);
            if (intptr != IntPtr.Zero)
            {
                ShowWindow(intptr, 0);//隐藏这个窗口
            }
        }

        public static void showBat()
        {
            Console.Title = tempTitle;
            IntPtr intptr = FindWindow("ConsoleWindowClass", tempTitle);
            if (intptr != IntPtr.Zero)
            {
                ShowWindow(intptr, 1);//隐藏这个窗口
            }
        }

        public static void killBat()
        {
            Console.Title = tempTitle;
            Process[] procList = Process.GetProcessesByName("cmd");
            foreach (Process p in procList)
            {
                if (p.MainWindowTitle == tempTitle)
                {
                    p.Kill();
                }
            }
        }

        public static string getGen(string filename)
        {
            return Path.GetFileNameWithoutExtension(filename);
        }

        public static void setProgress(int now, int max){
            Console.Write("\r");
            Console.Write("{0}/{1}", now, max);
        }

        public static string file_get_contents(string filename)
        {
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open);//初始化文件流
                byte[] array = new byte[fs.Length];//初始化字节数组
                fs.Read(array, 0, array.Length);//读取流中数据到字节数组中
                fs.Close();//关闭流
                string str = Encoding.Default.GetString(array);//将字节数组转化为字符串
                return str;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                return "";
            }
        }

        public static string[] parseArgs(string cmd)
        {
            List<string> args = new List<string>();
            char[] charList = cmd.ToCharArray();
            bool inStringFlag = false;
            string tempArg = "";
            foreach (char one in charList)
            {
                switch (one)
                {
                    case '"':
                        inStringFlag = !inStringFlag;
                        break;
                    case ' ':
                        if (inStringFlag)
                        {
                            tempArg += ' ';
                        }
                        else
                        {
                            args.Add(tempArg);
                            tempArg = "";
                        }
                        break;
                    default:
                        tempArg += one;
                        break;
                }
            }
            args.Add(tempArg);
            return args.ToArray();
        }

        public static string GetRandomString(int length, bool useNum, bool useLow, bool useUpp, bool useSpe, string custom)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        } 

        public static string system(string cmd)
        {
            Process p = new Process();
            //设置要启动的应用程序
            p.StartInfo.FileName = "cmd.exe";
            //是否使用操作系统shell启动
            p.StartInfo.UseShellExecute = false;
            // 接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardInput = true;
            //输出信息
            p.StartInfo.RedirectStandardOutput = true;
            // 输出错误
            p.StartInfo.RedirectStandardError = true;
            //不显示程序窗口
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
            //启动程序
            p.Start();

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(cmd + "&exit");

            p.StandardInput.AutoFlush = true;

            //获取输出信息
            string strOuput = p.StandardOutput.ReadToEnd();
            //等待程序执行完退出进程
            p.WaitForExit();
            p.Close();
            return strOuput;
        }

        public static string[] subarr(string[] arr, int start)
        {
            string[] output = new string[arr.Length - start];
            int index = 0;
            for (int i = start; i < arr.Length; i++)
            {
                output[index++] = arr[i];
            }
            return output;
        }
    }
}
