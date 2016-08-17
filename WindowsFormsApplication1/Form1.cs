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
            thCatch.Start();
        }
        ActionBlock<int> proBlock;

        void catchMain()
        {
            int recod = 0;
            int comIndex = 0;

            while (true)
            {
                proBlock = new ActionBlock<int>((i) =>
                {
                    comIndex++;
                    textBoxIndex.Invoke(new Action(() =>
                    {
                        textBoxIndex.Text = comIndex.ToString();
                    }));


                    for(int trytime = 0;trytime <10;trytime ++)
                    {
                        try
                        {
                            catchIndex(i);
                            break;
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                }
                , new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 4 });
                textBoxIndex.Invoke(new Action(() =>
                {
                    textBoxcomplete.Text = recod++.ToString();
                }));
                int num = firstcatch();
                for (int i = 1; i < num; i++)
                {
                    proBlock.Post(i);
                }
                proBlock.Complete();
                proBlock.Completion.Wait();
                //for (int i=1;i<num;i++)
                //{
                //    textBoxIndex.Invoke(new Action(() =>
                //    {
                //        textBoxIndex.Text = i.ToString();
                //    }));
                //    catchIndex(i);
                //}
            }
        }
        //2016-08-10~2016-08-17.html
        string getdataString()
        {
            DateTime currentTime = DateTime.Now;
            DateTime beforeTime = currentTime.AddDays(-7);
            return string.Format("{0:d}-{1:d}-{2:d}~{3:d}-{4:d}-{5:d}", currentTime.Year, currentTime.Month, currentTime.Day,
                beforeTime.Year, beforeTime.Month, beforeTime.Day
                );
        }
        string getpageString(int num)
        {
            //http://www.bilibili.com/list/default-33-1-2016-8-10~2016-8-17.html
            return "http://www.bilibili.com/list/default-33-" + num.ToString() + "-2016-8-10~2016-8-17.html";
        }


        int firstcatch()
        {
            string st = GetHtmlCode(getpageString(1));
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(st);
            HtmlNode node = doc.DocumentNode;
            var col = node.SelectNodes("ul/li");
            if (col != null)
            {
                foreach (var eachanimate in col)
                {
                    watchrecord animateNode = new watchrecord(eachanimate);
                }
            }

            try
            {
                var nump = node.SelectNodes("div[1] / a[10]");
                return int.Parse(nump[0].InnerHtml);
            }
            catch (System.Exception ex)
            {
                return 0;
            }
        }

        List<watchrecord> catchIndex(int index)
        {
            string st = GetHtmlCode(getpageString(index));
            List<watchrecord> ret = new List<watchrecord>(20);
            if (st != "")
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(st);
                HtmlNode node = doc.DocumentNode;
                var col = node.SelectNodes("ul/li");
                if (col != null)
                {
                    foreach (var eachanimate in col)
                    {
                        ret.Add(new watchrecord(eachanimate));
                    }
                }
            }
            return ret;
        }


        class watchrecord
        {
            string title;
            string watchCount;
            string collectCount;
            string danmuCount;
            HtmlNode fatherNode;
            public watchrecord(HtmlNode fathernode)
            {
                this.fatherNode = fathernode;

                title = getMessage("div / div[1] / a[2]");
                watchCount = getMessage("div / div[2] / div[2] / span[1] / span");
                danmuCount = getMessage("div/div[2]/div[2]/span[2]/span");
                collectCount = getMessage("div/div[2]/div[2]/span[3]/span");
            }

            string getMessage(string sel)
            {
                var title = fatherNode.SelectNodes(sel);
                return title[0].InnerHtml;
            }
        }

        private string GetHtmlCode(string url)
        {
            string htmlCode;
            HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            webRequest.Timeout = 5000;
            webRequest.Method = "GET";
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36";
            webRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
            HttpWebResponse webResponse = (System.Net.HttpWebResponse)webRequest.GetResponse();
            if (webResponse.ContentEncoding.ToLower() == "gzip")//如果使用了GZip则先解压            {
                using (System.IO.Stream streamReceive = webResponse.GetResponseStream())
                {
                    using (var zipStream =
                        new System.IO.Compression.GZipStream(streamReceive, System.IO.Compression.CompressionMode.Decompress))
                    {
                        using (StreamReader sr = new System.IO.StreamReader(zipStream, Encoding.GetEncoding("utf-8")))
                        {
                            htmlCode = sr.ReadToEnd();
                        }
                    }
                }

            else
            {
                using (System.IO.Stream streamReceive = webResponse.GetResponseStream())
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(streamReceive, Encoding.Default))
                    {
                        htmlCode = sr.ReadToEnd();
                    }
                }
            }
            return htmlCode;
        }
        /*
 *    备用方案              *
 *                          *                 
//client.BrowserVersion
NHtmlUnit.WebClient client = new NHtmlUnit.WebClient();
client.Options.JavaScriptEnabled = false;
client.Options.CssEnabled = false;
client.Options.ThrowExceptionOnScriptError = false;
client.Options.Timeout = 5000;
HtmlPage page = client.GetHtmlPage("http://www.bilibili.com/video/bangumi-two-1.html");
string xml = page.AsXml();
*/
    }
}

