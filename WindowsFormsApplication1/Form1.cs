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
        }
        mysqlSave msss;

        void catchMain()
        {
            msss = new mysqlSave("127.0.0.1", "3500", "ririyeye", "root");
            msss.OnWaitingChange += (num) =>
            {
                textBoxWaiting.BeginInvoke(new Action(() =>
                {
                    textBoxWaiting.Text = num.ToString();
                }));
            };
            htmlfetch ht = new htmlfetch();
            ht.OnIndexComplete += (num) =>
            {
                textBoxIndex.BeginInvoke(new Action(() =>
                {
                    textBoxIndex.Text = num.ToString();
                }));
            };
            ht.OnDataGet += Ht_OnDataGet;
            ht.catchMain();

        }

        private void Ht_OnDataGet(List<fetchBase.watchrecord> mlist)
        {
            msss.post(mlist);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Thread(catchMain).Start();
        }
    }
}

