using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace UI
{
    class Resampler
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
        private delegate int ResamplerProc(Int32 argc, string[] argv, byte[] wavFile, byte[] tempFile);
        private event ResamplerProc ResamplerEvent;

        public bool useResamplerDll = false;
        public IntPtr resamplerDll;
        public string resamplerPath;
        public string wavtoolPath;
        public string batfile;
        public UtauBat bat;
        public Form1 mainForm;

        private int[] threadParseList;
        private int returnedThreads = 0;
        private int synthedGens = 0;
        private int finishedGens = 0;
        private int threadNum = 0;
        private bool wavtoolIsWaitting = false;
        private bool synthetiseIsFinished = false;
        private EventWaitHandle _mainWaitHandle = new AutoResetEvent(false);
        private EventWaitHandle _wavtoolWaitHandle = new AutoResetEvent(false);

        public Resampler(string batfile, Form1 form)
        {
            this.batfile = batfile;
            this.mainForm = form;
            if (form.config.resamplerMode == "dll")
            {
                this.useResamplerDll = true;
                this.resamplerDll = LoadLibrary(form.config.resampler);
                IntPtr pAddressOfFunctionToCall = GetProcAddress(this.resamplerDll, "resampler");
                this.ResamplerEvent = (ResamplerProc)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(ResamplerProc));
            }
            else
            {
                this.ResamplerEvent = this.resampler;
                this.resamplerPath = form.config.resampler;
            }
            this.wavtoolPath = form.config.wavtool;

            if (!File.Exists(this.resamplerPath))
            {
                throw new FileNotFoundException(this.resamplerPath);
            }

            if (!File.Exists(this.wavtoolPath))
            {
                throw new FileNotFoundException(this.wavtoolPath);
            }
        }

        ~Resampler()
        {
            FreeLibrary(this.resamplerDll);
        }

        public string wavtool(string[] args){
            Process p = new Process();
            //设置要启动的应用程序
            p.StartInfo.FileName = this.wavtoolPath;
            p.StartInfo.Arguments = "\"" + string.Join("\" \"", Utils.subarr(args, 1)) + "\"";
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

            p.StandardInput.AutoFlush = true;

            //获取输出信息
            string strOuput = p.StandardOutput.ReadToEnd();
            //等待程序执行完退出进程
            p.WaitForExit();
            p.Close();
            return strOuput;
        }

        public int resampler(Int32 argc, string[] argv, byte[] wavFile, byte[] tempFile)
        {
            Process p = new Process();
            //设置要启动的应用程序
            p.StartInfo.FileName = this.resamplerPath;
            p.StartInfo.Arguments = "\"" + string.Join("\" \"", Utils.subarr(argv, 1)) + "\"";
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

            p.StandardInput.AutoFlush = true;

            //获取输出信息
            string strOuput = p.StandardOutput.ReadToEnd();
            //等待程序执行完退出进程
            p.WaitForExit();
            p.Close();
            return 0;
        }

        public void synthetise()
        {
            int step;
            bat = new UtauBat(this.batfile);
            if (this.mainForm.config.multiThread)
            {
                if (bat.resamParams.Count > 0)
                {
                    mainForm.SetStatus1(mainForm.lang.fetch("正在合成"));
                    mainForm.SetProgress1MaxNum(bat.resamParams.Count);
                    mainForm.SetProgress1NowNum(0);
                    this.synthetiseIsFinished = false;
                    //多线程合成模式
                    int i;
                    threadNum = Math.Min(this.mainForm.config.threadNum, bat.resamParams.Count);
                    Thread[] threadPool = new Thread[threadNum];
                    returnedThreads = 0;
                    synthedGens = 0;
                    threadParseList = new int[bat.resamParams.Count];
                    for (i = 0; i < threadParseList.Length; i++)
                    {
                        threadParseList[i] = 0;
                    }
                    for (i = 0; i < threadPool.Length; i++)
                    {
                        threadPool[i] = new Thread(new ThreadStart(asyncResampler));
                        threadPool[i].Start();
                    }
                }
                if (bat.toolParams.Count > 0)
                {
                    mainForm.SetStatus2(mainForm.lang.fetch("正在拼接"));
                    mainForm.SetProgress2MaxNum(bat.toolParams.Count);
                    mainForm.SetProgress2NowNum(0, true);
                    finishedGens = 0;
                    Thread threadWavtool = new Thread(new ThreadStart(asyncWavtool));
                    threadWavtool.Start();
                }
                if (bat.resamParams.Count > 0 || bat.toolParams.Count > 0)
                {
                    _mainWaitHandle.WaitOne();
                }
            }
            else
            {
                if (bat.resamParams.Count > 0)
                {
                    mainForm.SetStatus1(mainForm.lang.fetch("正在合成"));
                    mainForm.SetProgress1MaxNum(bat.resamParams.Count);
                    step = 0;
                    mainForm.SetProgress1NowNum(step);
                    foreach (resamParam param in bat.resamParams)
                    {
                        try
                        {
                            if (!File.Exists(param.temp))
                            {
                                string[] args = getResamplerParam(param);
                                this.ResamplerEvent(Convert.ToInt32(args.Length), args, Encoding.Default.GetBytes(args[1]), Encoding.Default.GetBytes(args[2]));
                            }
                            step++;
                            mainForm.SetProgress1NowNum(step);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, mainForm.lang.fetch("错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.mainForm.stop();
                            break;
                        }
                    }
                    if (bat.toolParams.Count > 0)
                    {
                        mainForm.SetStatus1(mainForm.lang.fetch("正在拼接"));
                        mainForm.SetProgress2MaxNum(bat.toolParams.Count);
                        step = 0;
                        mainForm.SetProgress2NowNum(step);
                        foreach (toolParam param in bat.toolParams)
                        {
                            try
                            {
                                string[] args = getWavtoolParam(param);
                                wavtool(args);
                                step++;
                                mainForm.SetProgress2NowNum(step);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message, mainForm.lang.fetch("错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                                this.mainForm.stop();
                                break;
                            }
                        }
                    }
                }
            }
            Utils.system(string.Format("copy /Y \"{0}.whd\" /B + \"{0}.dat\" /B \"{0}\"", bat.settings["output"]));
            File.Delete(bat.settings["output"] + ".whd");
            File.Delete(bat.settings["output"] + ".dat");
            mainForm.Close();
        }

        public void asyncResampler()
        {
            int rp;
            while (true)
            {
                Monitor.Enter(this);
                for (rp = 0; rp < this.threadParseList.Length; rp++)
                {
                    if (this.threadParseList[rp] == 0)
                    {
                        //标记为正在合成
                        this.threadParseList[rp] = 1;
                        break;
                    }
                }
                Monitor.Exit(this);
                if (rp == threadParseList.Length)
                {
                    returnedThreads++;
                    if (returnedThreads == this.mainForm.config.threadNum)
                    {
                        //表示合成已经完成
                        this.synthetiseIsFinished = true;
                    }
                    if (this.wavtoolIsWaitting)
                    {
                        this._wavtoolWaitHandle.Set();
                    }
                    break;
                }
                resamParam param = this.bat.resamParams[rp];
                if (!File.Exists(param.temp))
                {
                    string[] args = getResamplerParam(param);
                    this.ResamplerEvent(Convert.ToInt32(args.Length), args, Encoding.Default.GetBytes(args[1]), Encoding.Default.GetBytes(args[2]));
                }
                //标记为已完成
                this.threadParseList[rp] = 2;
                if (this.wavtoolIsWaitting == true)
                {
                    this._wavtoolWaitHandle.Set();
                }
                Monitor.Enter(this);
                this.synthedGens++;
                this.mainForm.SetProgress1NowNum(this.synthedGens);
                Monitor.Exit(this);
                //合成部分
            }
        }

        public void asyncWavtool()
        {
            for (int wp = 0; wp < this.bat.toolParams.Count; wp++)
            {
                toolParam param = this.bat.toolParams[wp];
                string[] args = this.getWavtoolParam(param);
                while (true)
                {
                    if (Path.GetFileNameWithoutExtension(args[2]) != "R" && this.threadParseList[param.resamParamId] != 2 && this.synthetiseIsFinished != true)
                    {
                        //文件不存在就中断
                        this.wavtoolIsWaitting = true;
                        this._wavtoolWaitHandle.WaitOne();
                    }
                    else
                    {
                        break;
                    }
                }
                string ret = wavtool(args);
                this.finishedGens++;
                this.mainForm.SetProgress2NowNum(this.finishedGens, true);
            }
            //全部合成完成后
            this._mainWaitHandle.Set();
        }

        public string[] getResamplerParam(resamParam param)
        {
            string[] args = new string[14];
            args[0] = "resampler";
            args[1] = param.genfile;
            args[2] = param.temp;
            args[3] = param.pitchPercent;
            args[4] = param.velocity.ToString();
            args[5] = param.flags;
            args[6] = param.offset.ToString();
            args[7] = param.lengthReq.ToString();
            args[8] = param.fix.ToString();
            args[9] = param.blank.ToString();
            args[10] = param.volume.ToString();
            args[11] = param.modulation.ToString();
            args[12] = param.tempo;
            args[13] = param.pit;
            return args;
        }

        public string[] getWavtoolParam(toolParam param)
        {
            string[] args = new string[param.len + 1];
            args[0] = "wavtool";
            args[1] = param.outfile;
            args[2] = param.infile;
            args[3] = param.offset.ToString();
            args[4] = param.length;
            if (param.len >= 5)
                args[5] = param.p1.ToString();
            if (param.len >= 6)
                args[6] = param.p2.ToString();
            if (param.len >= 7)
                args[7] = param.p3.ToString();
            if (param.len >= 8)
                args[8] = param.v1.ToString();
            if (param.len >= 9)
                args[9] = param.v2.ToString();
            if (param.len >= 10)
                args[10] = param.v3.ToString();
            if (param.len >= 11)
                args[11] = param.v4.ToString();
            if (param.len >= 12)
                args[12] = param.ovr.ToString();
            if (param.len >= 13)
                args[13] = param.p4.ToString();
            if (param.len >= 14)
                args[14] = param.p5.ToString();
            if (param.len >= 15)
                args[15] = param.v5.ToString();
            return args;
        }
    }
}