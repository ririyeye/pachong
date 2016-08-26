using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
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

        myTPLBase<int> proBlock;
        virtual public void catchMain()
        {
            int comIndex = 0;

            proBlock = new myTPLBase<int>((i) =>
            {
                bool succflag = false;

                for (int trytime = 0; trytime < 10; trytime++)
                {
                    try
                    {
                        var mlist = catchIndex(i);
                        if (mlist == null)
                        {
                            continue;
                        }
                        trigDateGet(mlist);
                        succflag = true;
                        break;
                    }
                    catch (Exception)
                    {
                        succflag = false;
                        continue;
                    }
                }
                if (succflag == true)
                {
                    trigIndexComplete(++comIndex);
                }
                else
                {
                    proBlock.Post(i);
                }
            },
            16
            //new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 8}
            );
            int num = firstcatch();

            for (int i = 1; i < num; i++)
            {
                proBlock.Post(i);
            }
        }

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


        private string tryHtmlCode(string url)
        {
            string htmlCode = null;
            HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            webRequest.Timeout = 50000;
            webRequest.ReadWriteTimeout = 50000;
            webRequest.Method = "GET";
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36";
            webRequest.Headers.Add("Accept-Encoding", "gzip, deflate");

            using (HttpWebResponse webResponse = (System.Net.HttpWebResponse)webRequest.GetResponse())
            {
                string ContentEncoding = webResponse.ContentEncoding.ToLower();
                Encoding encoding;
                try
                {
                    encoding = Encoding.GetEncoding(webResponse.CharacterSet);
                }
                catch (ArgumentException ex)
                {
                    encoding = Encoding.Default;
                }

                if (ContentEncoding.Length > 2)
                {
                    using (System.IO.Stream streamReceive = webResponse.GetResponseStream())
                    {
                        if (ContentEncoding == "gzip")//如果使用了GZip则先解压            
                        {
                            using (var zipStream =
                                new System.IO.Compression.GZipStream(streamReceive,
                                System.IO.Compression.CompressionMode.Decompress))
                            {
                                using (StreamReader sr = new System.IO.StreamReader(
                                    zipStream, encoding))
                                {
                                    htmlCode = sr.ReadToEnd();
                                }
                            }
                        }
                        else if (ContentEncoding == "deflate")
                        {
                            using (var defStream =
                                new System.IO.Compression.DeflateStream(streamReceive,
                                System.IO.Compression.CompressionMode.Decompress))
                            {
                                using (StreamReader sr = new System.IO.StreamReader(
                                    defStream, encoding))
                                {
                                    htmlCode = sr.ReadToEnd();
                                }
                            }
                        }
                        else
                        {
                            using (System.IO.StreamReader sr = new System.IO.StreamReader(streamReceive, encoding))
                            {
                                htmlCode = sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            return htmlCode;
        }
        protected string GetHtmlCode(string url)
        {
            for (int i = 0; i < 3; i++)
            {
                string st = tryHtmlCode(url);
                if (st != null)
                {
                    return st;
                }
            }
            return null;
        }

        public class watchrecord
        { 
            public string title;
            public string watchCount;
            public string collectCount;
            public string danmuCount;
            public string avstring;
            public string coinCount;
            public string gettime;
            public int deliverCount;
            public watchrecord(HtmlNode fathernode)
            {
                title = getMessage(fathernode,"div / div[1] / a[2]");
                watchCount = getMessage(fathernode,"div / div[2] / div[2] / span[1] / span");
                danmuCount = getMessage(fathernode,"div/div[2]/div[2]/span[2]/span");
                collectCount = getMessage(fathernode,"div/div[2]/div[2]/span[3]/span");
                avstring = getattribute(fathernode,"div / div[1] / a[1]");
                coinCount = "";
                gettime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                checestring(ref title);
                checestring(ref watchCount);
                checestring(ref danmuCount);
                checestring(ref collectCount);
                checestring(ref avstring);
                checestring(ref coinCount);
            }

            public watchrecord(JToken fatherjson)
            {
                title = fatherjson["title"].Value<string>();
                avstring = fatherjson["aid"].Value<string>();
                watchCount = fatherjson["stat"]["view"].Value<string>();
                danmuCount = fatherjson["stat"]["danmaku"].Value<string>();
                collectCount = fatherjson["stat"]["favorite"].Value<string>();
                coinCount = fatherjson["stat"]["coin"].Value<string>();
                gettime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                checestring(ref title);
                checestring(ref watchCount);
                checestring(ref danmuCount);
                checestring(ref collectCount);
                checestring(ref avstring);
                checestring(ref coinCount);
            }

            void checestring(ref string st)
            {
                if ((st == "") || (st == null)||(st == "--"))
                    st = "0";
            }

            string getattribute(HtmlNode fatherNode,string sel)
            {
                var title = fatherNode.SelectNodes(sel)[0];
                var avalue = title.Attributes["href"];
                return avalue.Value;
            }
            string getMessage(HtmlNode fatherNode,string sel)
            {
                var title = fatherNode.SelectNodes(sel);
                return title[0].InnerHtml;
            }
        }
    }
}
