using System;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Text;

namespace UI
{
    public partial class Form1 : Form
    {
        public int progress1MaxNum;
        public int progress1NowNum;
        public int progress2MaxNum;
        public int progress2NowNum;
        public string status1 = "", status2 = "";
        public Config config;
        public LangPack lang;
        public string rootDir = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        
        public Form1()
        {
            InitializeComponent();
            config = new Config(rootDir + "Config.xml");
            lang = new LangPack(rootDir + "language.xml", config);
        }

        public void SetProgress1MaxNum(int max)
        {
            progress1MaxNum = max;
        }

        public void SetProgress1NowNum(int now)
        {
            progress1NowNum = now;
            int progress = Convert.ToInt32(Math.Round(Convert.ToDouble(progress1NowNum) / Convert.ToDouble(progress1MaxNum) * 100));
            labelStatus.Text = string.Format("{0} ({1}/{2}) {3}%", status1, progress1NowNum, progress1MaxNum, progress);
            progressResampler.Value = progress;
        }

        public void SetProgress2MaxNum(int max)
        {
            progress2MaxNum = max;
        }

        public void SetProgress2NowNum(int now, bool multiThread = false)
        {
            progress2NowNum = now;
            int progress = Convert.ToInt32(Math.Round(Convert.ToDouble(this.progress2NowNum) / Convert.ToDouble(this.progress2MaxNum) * 100));
            string statusString = string.Format("{0} ({1}/{2}) {3}%", this.status2, this.progress2NowNum, this.progress2MaxNum, progress);
            if (multiThread == false)
                labelStatus.Text = statusString;
            else
                labelStatusWavtool.Text = statusString;
            progressWavtool.Value = progress;
        }

        public void SetStatus1(string status)
        {
            status1 = status;
        }

        public void SetStatus2(string status)
        {
            status2 = status;
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = lang.fetch("正在合成 - UTAU UI");
            label1.Text = lang.fetch("合成进度：");
            label2.Text = lang.fetch("拼接进度：");
            labelStatus.Text = lang.fetch("加载中……");
            CheckForIllegalCrossThreadCalls = false;
            string batFile = Environment.CurrentDirectory + "\\temp.bat";
            if (!this.config.showConsole)
            {
                Utils.hideBat();
            }
            if (File.Exists(batFile))
            {
                labelStatus.Text = lang.fetch("正在解析数据……");
                try
                {
                    // 显示bat内容
                    using (StreamReader sr = new StreamReader(batFile, Encoding.UTF8))
                    {
                        textBox1.Text = sr.ReadToEnd();
                        byte[] mybyte = Encoding.UTF8.GetBytes(textBox1.Text);
                        textBox1.Text = Encoding.UTF8.GetString(mybyte);
                    }
                    // 开始合成
                    Resampler resampler = new Resampler(batFile, this);
                    Thread thread = new Thread(new ThreadStart(resampler.synthetise));
                    thread.Start();

                }
                catch (FileNotFoundException fe)
                {
                    MessageBox.Show(string.Format("{0}{1}", lang.fetch("文件不存在："), fe.Message), lang.fetch("错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show(lang.fetch("请在UTAU中调用！"), lang.fetch("错误"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        public void stop()
        {
            this.Close();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!this.config.showConsole)
            {
                Utils.showBat();
            }
            Utils.killBat();
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("点我干什么，活着不耐烦了吗？？？？？？");
        }

        private void progressBar2_Click(object sender, EventArgs e)
        {

        }
    }
}
