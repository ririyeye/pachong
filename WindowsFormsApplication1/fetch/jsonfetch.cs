using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace WindowsFormsApplication1.fetch
{
    class jsonfetch : fetchBase
    {
        myTPLBase<int> proBlock;

        override public void catchMain()
        {
            int comIndex = 0;

            proBlock = new myTPLBase<int>((i) =>
            {
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
                        break;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                }
                trigIndexComplete(++comIndex);
            },
            8
            //new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 8}
            );
            int num = firstcatch();

            for (int i = 1; i < num; i++)
            {
                proBlock.Post(i);
            }
        }


        public static string Lostjqury(string Str)
        {
            if (Str == null)
            {
                return Str;
            }
            int length = Str.Length;
            int leftnum = 0;
            for (; leftnum < length; leftnum++)
            {
                if (Str[leftnum] == '(')
                {
                    break;
                }
            }
            string newstring = Str.Remove(0, leftnum + 1).Remove(length - (leftnum + 1 + 2), 2);
            return newstring;
        }




        static DateTime dtnow = new DateTime(2016, 8, 18, 11, 25, 18);
        string timgstring(int times = 1)
        {
            DateTime dt = DateTime.Now;

            TimeSpan dtp = dt - dtnow;

            return ((int)dtp.TotalSeconds * times + 1471486755574).ToString();
        }
        string getpagejsonString(int num)
        {
            return "http://api.bilibili.com/archive_rank/getarchiverankbypartion?callback=jQuery172017288426144932756_" + timgstring() + "&type=jsonp&tid=33&pn=" + num.ToString() + "&_" + timgstring(10);
            //return "http://api.bilibili.com/archive_rank/getarchiverankbypartion?callback=jQuery172017288426144932756_1471486755574&type=jsonp&tid=33&pn=2&_=1471487245263";
        }

        public override int firstcatch()
        {
            string st = GetHtmlCode(getpagejsonString(1));
            string jsonst = Lostjqury(st);
            var js = (JObject)JsonConvert.DeserializeObject(jsonst);
            var p = js["data"]["page"];
            int total = p["count"].Value<int>();
            int size = p["size"].Value<int>();

            return total/size + 1;
        }

        public override List<watchrecord> catchIndex(int index)
        {
            string st = GetHtmlCode(getpagejsonString(index));
            if (st == null)
                return null;
            string jsonst = Lostjqury(st);
            var js = (JObject)JsonConvert.DeserializeObject(jsonst);
            var p = js["data"]["archives"].Children();
            List<watchrecord> ret = new List<watchrecord>(20);
            foreach (var node in p)
            {
                ret.Add(new watchrecord(node));
            }
            return ret;
        }
    }
}
