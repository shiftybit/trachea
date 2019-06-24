using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Net.Security;

namespace trachea
{
    public partial class Form1 : Form
    {
        delegate void TextBoxDelegate(string text);
        private TcpClient tcpClient;
        private SslStream ns;
        private StreamReader reader;
        private StreamWriter writer;
        private bool Listening;
// private static Action<string> OutputLog;

        public Form1()
        {
            InitializeComponent();
        }
        public void OutputLog(string text)
        {
            if (!richTextBox1.InvokeRequired)
            {
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.SelectionLength = 0;
                richTextBox1.SelectionColor = Color.DarkBlue;
                richTextBox1.AppendText(text + "\r\n");
                richTextBox1.SelectionColor = Color.Black;
            }
            else
            {
                TextBoxDelegate d = OutputLog;
                this.Invoke(d, new object[] { text });
            }
        }

        public void InputLog(string text)
        {
            if (!richTextBox1.InvokeRequired)
            {
                richTextBox1.SelectionStart = richTextBox1.TextLength;
                richTextBox1.SelectionLength = 0;
                richTextBox1.SelectionColor = Color.DarkRed;
                richTextBox1.AppendText(text + "\r\n");
                richTextBox1.SelectionColor = Color.Black;
            }
            else
            {
                TextBoxDelegate d = InputLog;
                this.Invoke(d, new object[] { text });
            }
        }
        private void Write(string line)
        {
            try
            {
                writer.WriteLine(line);
                writer.Flush();
                OutputLog(line);
            }
            catch (System.IO.IOException)
            {

            }

        }
        private void BridgeMain()
        {
            var input = string.Empty;
            try
            {
                while ((input = reader.ReadLine()) != null)
                {
                    if(Listening) InputLog(input);
                    var tokens = input.Split(' ');
                    if (tokens[0].ToUpper() == "PING")
                    {
                        Write("PONG " + tokens[1]);
                    }
                    else
                        switch (tokens[1].ToUpper())
                        {
                            case "PRIVMSG":
                                if (tokens[2].ToLower() == "zerocool" && tokens[3].ToLower().Contains("version"))
                                {
                                    //Write("PRIVMSG #TOP wazzup");
                                    string nick = tokens[0].Split('!')[0].Split(':')[1];
                                    Write($"NOTICE {nick} :VERSION mIRC v7.43");
                                }
                                if(tokens[2].ToLower() == "#blackhat")  InputLog(input); 
                                break;
                            case "001":
                                Write("JOIN #blackhat"); // 
                                //Write("JOIN #chokey");
                                break;
                            default:
                                break;
                        }
                }
            }
            catch (Exception err)
            {

            }
        }
        public void StartBridge() {
            CheckConnection();
            Write($"USER ZeroCool * * :Is that all you got?");
            Write("NICK ZeroCool");
            BridgeMain();
        }
        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            StartBridge();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Listening = true ;
            OutputLog("Starting Timer");
            backgroundWorker1.RunWorkerAsync();
            timer1.Interval = 9000;
            timer1.Start();
        }
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        public void CheckConnection()
        {
            if (tcpClient == null)
            {
                tcpClient = new TcpClient("167.114.221.137", 6697);
                ns = new SslStream(tcpClient.GetStream(),false, new RemoteCertificateValidationCallback (ValidateServerCertificate));
                //ns = tcpClient?.GetStream();
                ns.AuthenticateAsClient("breaking");
                reader = new StreamReader(ns);
                writer = new StreamWriter(ns);
            }
        }

        private void BtnSend2_Click(object sender, EventArgs e)
        {
            Write($"PRIVMSG {txtRoom.Text} {txtMessage.Text}");
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                Write($"PRIVMSG {txtRoom.Text} {txtMessage.Text}");
                txtMessage.Clear();
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            Listening = false;
            OutputLog("Stopping Timer");
        }

        private void BtnRaw_Click(object sender, EventArgs e)
        {
            Write(txtRaw.Text);
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked) Listening = true;
            else Listening = false;

        }
    }
}
