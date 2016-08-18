using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MySql.Data;
using System.Threading;
using MySql.Data.MySqlClient;
using static WindowsFormsApplication1.fetch.fetchBase;

namespace WindowsFormsApplication1
{
    class mysqlSave
    {
        //ActionBlock<List<fetch.fetchBase.watchrecord>> processBlock;
        LinkedList<List<fetch.fetchBase.watchrecord>> mlink;

        ManualResetEventSlim mNotice;

        string Addr;
        string port;
        string password;
        string user;

        string aimbase;

        int parallel_num;
        Thread threadpost;
        public mysqlSave(string Addr,string port,string password,string user,int parallelNum = 5)
        {
            this.Addr = Addr;
            this.port = port;
            this.password = password;
            this.user = user;
            this.parallel_num = parallelNum;
            this.aimbase = "bilibili";
            mNotice = new ManualResetEventSlim();
            mlink = new LinkedList<List<watchrecord>>();
            //threadpost = new Thread(postMain);
            //threadpost.Start();
            new Thread(postMain).Start();
            new Thread(postMain).Start();
            new Thread(postMain).Start();
            new Thread(postMain).Start();
            new Thread(postMain).Start();
            new Thread(postMain).Start();
            new Thread(postMain).Start();
            new Thread(postMain).Start();
        }
        public void post(List<watchrecord> mw)
        {
            lock (mlink)
            {
                mlink.AddLast(mw);
                mNotice.Set();
            }    
        }

        void postMain()
        {
            MySqlCommand com;
            MySqlConnection sqlcom;
            List<watchrecord> usingnode = null;
            while (true)
            {
                try
                {
                    sqlcom = tryConnect();
                    com = new MySqlCommand();
                    com.Connection = sqlcom;

                    usingnode = getNode();

                    for (int i = usingnode.Count - 1; i >= 0; i--)
                    {
                        com.CommandText = string.Format("show create table {0}.{1}", aimbase, usingnode[i].avstring);
                        tryfindtable(com, usingnode[i]);
                        trywritedata(com, usingnode[i]);
                        usingnode.RemoveAt(i);
                    }
                    usingnode = null;
                }
                catch (System.Exception ex)
                {
                    if (usingnode != null)
                    {
                        post(usingnode);
                        usingnode = null;
                    }
                }
            }
        }

        void trywritedata(MySqlCommand com, watchrecord wr, int trytime = 3)
        {
            Exception innexp = null;
            for (int i=0;i<3;i++)
            {
                try
                {
                    com.CommandText = addMessageString(wr);
                    com.ExecuteNonQuery();
                    return;
                }
                catch (MySqlException exp)
                {
                    innexp = exp;
                }
            }
            if (wr.avstring == "av5855395")
            {
            }
            throw new WriteFailException(wr, innexp);
        }

        class WriteFailException : Exception
        {
            watchrecord wc;
            public WriteFailException(watchrecord wr,Exception exp)
                :base("写入失败",exp)
            {
                wc = wr;
            }
            public override string ToString()
            {
                return "写入失败@" + wc.avstring;
            }
        }

        class NoTableFoundException : Exception
        {
            watchrecord wc;
            public NoTableFoundException(watchrecord wr,Exception exp )
                :base("没有找到数据表",exp)
            {
                wc = wr;
            }
            public override string ToString()
            {
                return "没有找到" + wc.avstring;
            }
        }

        void tryfindtable(MySqlCommand com, watchrecord wr,int trytime = 3)
        {
            Exception exp = null;
            for (int i=0;i<trytime;i++)
            {
                try
                {
                    com.ExecuteNonQuery();
                    return;
                }
                catch (MySqlException)
                {
                    try
                    {
                        com.CommandText = CreatTableString(wr.avstring);
                        com.ExecuteNonQuery();
                        return;
                    }
                    catch (MySqlException ex)
                    {
                        exp = ex;
                    }
                }
            }
            throw new NoTableFoundException(wr,exp);
        }


        List<watchrecord> getNode()
        {
            mNotice.Wait();
            lock (mlink)
            {
                if (mlink.Count > 0)
                {
                    var node = mlink.First.Value;
                    mlink.RemoveFirst();
                    return node;
                }
                mNotice.Set();
                return null;
            }
        }

        MySqlConnection tryConnect()
        {
            string st = string.Format("host={0};port= {1};User Id={2};password={3};Database={4};"
                , Addr, port, user,password, aimbase);
            MySqlConnection  sqlcom = new MySqlConnection(st);

            sqlcom.Open();

            return sqlcom;
        }

        string CreatTableString(string st)
        {
            /*
CREATE TABLE `av112` (
`insertTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
`watch` int(11) ZEROFILL NOT NULL ,
`collect` int(11)  ZEROFILL NOT NULL ,
`coin` int(11)  ZEROFILL NOT NULL ,
`danmu` int(11)  ZEROFILL NOT NULL ,
PRIMARY KEY(`insertTime`)) 
ENGINE = MyISAM DEFAULT CHARSET = utf8 COMMENT = '自动生成'
             **/
            string orgin = "CREATE TABLE `"+st+ "` ("
            +"`insertTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,"
            + "`watch` int(11) ZEROFILL NOT NULL,"
            + "`collect` int(11) ZEROFILL NOT NULL,"
            + "`coin` int(11) ZEROFILL NOT NULL,"
            + "`danmu` int(11) ZEROFILL NOT NULL,"
            + "PRIMARY KEY(`insertTime`)) "
            +"ENGINE = MyISAM DEFAULT CHARSET = utf8 COMMENT = '自动生成'";
            return orgin;
        }

        string addMessageString(watchrecord wc)
        {
            return string.Format("INSERT INTO `bilibili`.`{0}` (`watch`, `collect`, `coin`, `danmu`) VALUES('{1}', '{2}', '{3}', '{4}');"
                ,wc.avstring, wc.watchCount, wc.collectCount, wc.coinCount, wc.danmuCount);
        }
    }
}
