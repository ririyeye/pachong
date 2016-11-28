using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WindowsFormsApplication1.fetch
{
    class danmuFetch : fetchBase
    {
        public override List<watchrecord> catchIndex(int index)
        {
            throw new NotImplementedException();
        }

        public override int firstcatch()
        {
            throw new NotImplementedException();
        }

        public override void catchMain()
        {
            string st = GetHtmlCode("http://www.bilibili.com/video/av6061280/index_2.html");
            Regex re = new Regex(@"(?<=cid=)\d+");
            var mat = re.Match(st);
            string st2 = mat.ToString();
        }
    }

}
