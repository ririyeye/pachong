using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WindowsFormsApplication1.fetch
{
    abstract class fetchBase
    {
        abstract public int firstcatch();
        abstract public List<watchrecord> catchIndex(int index);
        abstract public void catchMain();

        public delegate void IndexHandler(int num);
        public event IndexHandler OnIndexComplete;
        protected void trigIndexComplete(int num)
        {
            OnIndexComplete?.Invoke(num);
        }

        public delegate void RoundHandler(int num);
        public event RoundHandler OnRoundComplete;
        protected void trigRoundComplete(int num)
        {
            OnRoundComplete?.Invoke(num);
        }

        public delegate void DataHandler(List<watchrecord> mlist);
        public event DataHandler OnDataGet;
        protected void trigDateGet(List<watchrecord> mlist)
        {
            OnDataGet?.Invoke(mlist);
        }



        protected string GetHtmlCode(string url)
        {
            string htmlCode;
            HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            webRequest.Timeout = 5000;
            webRequest.Method = "GET";
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36";
            webRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
            HttpWebResponse webResponse = (System.Net.HttpWebResponse)webRequest.GetResponse();
            if (webResponse.ContentEncoding.ToLower() == "gzip")//如果使用了GZip则先解压            
            {
                using (System.IO.Stream streamReceive = webResponse.GetResponseStream())
                {
                    using (var zipStream =
                        new System.IO.Compression.GZipStream(streamReceive, System.IO.Compression.CompressionMode.Decompress))
                    {
                        using (StreamReader sr = new System.IO.StreamReader(zipStream, Encoding.GetEncoding("UTF-8")))
                        {
                            htmlCode = sr.ReadToEnd();
                        }
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

        public class watchrecord
        {
            protected static Regex reg = new Regex(@"av\d+");
            public string title;
            public string watchCount;
            public string collectCount;
            public string danmuCount;
            public string avstring;
            HtmlNode fatherNode;
            public watchrecord(HtmlNode fathernode)
            {
                this.fatherNode = fathernode;

                title = getMessage("div / div[1] / a[2]");
                watchCount = getMessage("div / div[2] / div[2] / span[1] / span");
                danmuCount = getMessage("div/div[2]/div[2]/span[2]/span");
                collectCount = getMessage("div/div[2]/div[2]/span[3]/span");
                avstring = getattribute("div / div[1] / a[1]");
            }

            string getattribute(string sel)
            {
                var title = fatherNode.SelectNodes(sel)[0];

                var avalue = title.Attributes["href"];
                return reg.Match(avalue.Value).ToString();
            }
            string getMessage(string sel)
            {
                var title = fatherNode.SelectNodes(sel);
                return title[0].InnerHtml;
            }
        }
    }
}
