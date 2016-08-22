using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Windows.Threading;
using System.Threading.Tasks.Dataflow;
using System.Text.RegularExpressions;
using WindowsFormsApplication1.fetch;
//using NHtmlUnit;
//using NHtmlUnit.Html;
namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.FormClosing += (a,b) => Environment.Exit(0);
            InitializeComponent();

            msss = new mysqlSave("127.0.0.1", "3500", "ririyeye", "root");
            msss.OnWaitingChange += (nums) =>
            {
                if (nums != null)
                {
                    waitingNum = nums.Sum();
                }
            };

            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                    textBoxWaiting.Invoke(new Action(() =>
                    {
                        textBoxWaiting.Text = waitingNum.ToString();
                        textBoxIndex.Text = completeNum.ToString();
                    }));
                };
            }).Start();
        }
        mysqlSave msss;
        int waitingNum;
        int completeNum;
        void catchMain()
        {
            jsonfetch ht = new jsonfetch();
            ht.OnIndexComplete += (num) =>
            {
                completeNum = num;
            };
            ht.OnDataGet += (List<fetchBase.watchrecord> mlist) => msss.post(mlist); ;
            ht.catchMain();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                while (true)
                {
                    new Thread(catchMain).Start();
                    Thread.Sleep(60000);
                }
            }).Start();
        }
    }
}

