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

        Thread thCatch;
        public Form1()
        {
            this.FormClosing += (a,b) => Environment.Exit(0);

            InitializeComponent();
            thCatch = new Thread(catchMain);
            
            //catchIndex(1); 


            thCatch.Start();
        }
        mysqlSave msss;

        void catchMain()
        {
            msss = new mysqlSave("127.0.0.1", "3500", "ririyeye", "root");
            htmlfetch ht = new htmlfetch();
            ht.OnIndexComplete += (num) =>
            {
                textBoxIndex.Invoke(new Action(() =>
                {
                    textBoxIndex.Text = num.ToString();
                }));
            };
            ht.OnDataGet += Ht_OnDataGet;
            while (true)
            {
                ht.catchMain();
            }
        }

        private void Ht_OnDataGet(List<fetchBase.watchrecord> mlist)
        {
            msss.post(mlist);
        }
    }
}

