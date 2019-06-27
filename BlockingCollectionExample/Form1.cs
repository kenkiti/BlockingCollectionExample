using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Concurrent;


namespace BlockingCollectionExample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        BlockingCollection<string> bc;
        CancellationTokenSource cts;

        // Utility:呼び出し元スレッドを無視してTextBoxに文字列を追加
        void AddMessage(string msg)
        {
            textBox1.Invoke(new Action(() => {
                textBox1.AppendText(msg + Environment.NewLine);
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cts = new CancellationTokenSource();
            bc = new BlockingCollection<string>();

            Task.Run(() =>
            {
                while (cts.IsCancellationRequested == false)
                {
                    string s;
                    try
                    {
                        s = bc.Take(cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        s = "[canceled]";
                    }
                    AddMessage(DateTime.Now.ToString() + " " + s);
                }
            }).ContinueWith(t =>
            {
                cts.Dispose();
                bc.Dispose();
                button1.Invoke(new Action(() =>
                {
                    button3.Enabled = false;
                    button2.Enabled = false;
                    button1.Enabled = true;
                }));
            });
            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            cts.Cancel();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for(var i=0; i<10; i++)
            {
                bc.Add(i.ToString());
                Thread.Sleep(100);
            }
            bc.Add("[Add]");
        }
    }
}
