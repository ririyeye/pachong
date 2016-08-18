using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApplication1.fetch
{
    class jsonfetch : fetchBase
    {
        override public void catchMain()
        {
            while(true)
            {
                Thread.Sleep(100);
            }
        }


        public static string Lostjqury(string Str)
        {
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
            throw new NotImplementedException();
        }

        public override List<watchrecord> catchIndex(int index)
        {
            throw new NotImplementedException();
        }
    }
}
