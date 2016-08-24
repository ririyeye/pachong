using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace WindowsFormsApplication1.fetch
{
    class htmlfetch : fetchBase
    {

        string getdataString()
        {
            DateTime currentTime = DateTime.Now;
            DateTime beforeTime = currentTime.AddDays(-7);
            return string.Format("{0:d}-{1:d}-{2:d}~{3:d}-{4:d}-{5:d}", currentTime.Year, currentTime.Month, currentTime.Day,
                beforeTime.Year, beforeTime.Month, beforeTime.Day
                );
        }

        public override int firstcatch()
        {
            string st = GetHtmlCode(getpageString(1));
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(st);
            HtmlNode node = doc.DocumentNode;

            try
            {
                var nump = node.SelectNodes("div[1] / a[10]");
                return int.Parse(nump[0].InnerHtml);
            }
            catch (System.Exception)
            {
                return 0;
            }
        }

        override public List<watchrecord> catchIndex(int index)
        {
            string st = GetHtmlCode(getpageString(index));
            
            if (st != "" && st != null)
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(st);
                HtmlNode node = doc.DocumentNode;
                var col = node.SelectNodes("ul/li");
                if (col != null)
                {
                    List<watchrecord> ret = new List<watchrecord>(20);
                    foreach (var eachanimate in col)
                    {
                        ret.Add(new watchrecord(eachanimate));
                    }
                    return ret;
                }
            }
            return null;
        }

        string getpageString(int num)
        {
            string st = getdataString();
            //http://www.bilibili.com/list/default-33-1-2016-8-10~2016-8-17.html
            return "http://www.bilibili.com/list/default-33-" + num.ToString() +"-"+st + ".html";
        }
    }
}
