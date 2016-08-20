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
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    class mysqlSave
    {
        public delegate void WaitingChangeHandler(int [] group);
        public event WaitingChangeHandler OnWaitingChange;
        protected void trigWaitingChange(int[] group)
        {
            OnWaitingChange?.Invoke(group);
        }
        string Addr;
        string port;
        string password;
        string user;
        string aimbase;

        int parallelNum;
        SaveNode[]SaveNodes;
        int[] WaitingGroup;
        public mysqlSave(string Addr,string port,string password,string user,string aimbase = "bilibili", int parallelNum = 10)
        {
            this.Addr = Addr;
            this.port = port;
            this.password = password;
            this.user = user;
            this.parallelNum = parallelNum;
            this.aimbase = aimbase;
            
            SaveNodes = new SaveNode[20];
            WaitingGroup = new int[20];
            for (int i=0;i<parallelNum;i++)
            {
                SaveNodes[i] = new SaveNode(parallelNum, i, Addr, port, password, user,aimbase);
                SaveNodes[i].OnWaitingChange += (index,num) =>
                {
                    WaitingGroup[index] = num;
                    trigWaitingChange(WaitingGroup);
                };
            }
        }

        public void post(List<watchrecord> lwr)
        {
            if(lwr != null)
            {
                for (int i = lwr.Count - 1; i >= 0 ; i--)
                {
                    int num = lwr[i].avstring.IndexOf("av");
                    
                    if(num >= 0)
                    {
                        lwr[i].avstring = lwr[i].avstring.Substring(num + 2).Trim('/');
                    }
                    mpost(lwr[i]);
                    lwr[i] = null;
                    lwr.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// 分配算法
        /// </summary>
        /// <param name="wr"></param>
        void mpost(watchrecord wr)
        {
            int num = 0;
            try
            {
                num = int.Parse(wr.avstring);
            }
            catch (System.Exception ex)
            {
                return;
            }
            int mode = num % parallelNum;
            SaveNodes[mode].post(wr);
        }

        class SaveNode
        {
            public delegate void SWaitingChangeHandler(int index,int num);
            public event SWaitingChangeHandler OnWaitingChange;
            protected void MtrigWaitingChange(int num)
            {
                OnWaitingChange?.Invoke(index,num);
            }

            int index;
            int total;
            Thread mthread;
            /// <summary>
            /// 登入信息
            /// </summary>
            string Addr;
            string port;
            string password;
            string user;
            string aimbase;
            //Regex regdup = new Regex("Duplicate", RegexOptions.Compiled);
            //Regex regexist = new Regex("exist", RegexOptions.Compiled);

            LinkedList<watchrecord> mlink = new LinkedList<watchrecord>();
            ManualResetEventSlim mNotice;
            public SaveNode(int total,int index,string Addr, string port, string password, string user,string aimbase)
            {
                this.Addr = Addr;
                this.port = port;
                this.password = password;
                this.user = user;
                this.aimbase = aimbase;


                this.total = total;
                this.index = index;
                mNotice = new ManualResetEventSlim();
                mNotice.Reset();
                mthread = new Thread(nodeMain);
                mthread.Start();
            }

            void nodeMain()
            {
                MySqlCommand com = null;
                MySqlConnection sqlcom = null;
                watchrecord usingnode = null;
                while (true)
                {
                    usingnode = getNode();
                    if (usingnode == null)
                    {
                        continue;
                    }
                    try
                    {
                        sqlcom = tryConnect();
                        com = new MySqlCommand();
                        com.Connection = sqlcom;

                        while (true)
                        {
                            if (usingnode == null)
                            {
                                usingnode = getNode();
                                if (usingnode == null)
                                {
                                    throw new NoObjectTimeOut();
                                }
                            }

                            trywritedata(com, usingnode);

                            usingnode = null;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        try
                        {
                            if (sqlcom != null)
                            {
                                sqlcom.Close();
                                sqlcom = null;
                            }
                        }
                        catch (System.Exception)
                        {

                        }
                    }
                    finally
                    {
                        if (usingnode != null)
                        {
                            post(usingnode);
                            usingnode = null;
                        }
                    }
                }
            }

            public void post(watchrecord mw)
            {
                lock (mlink)
                {
                    mlink.AddLast(mw);
                    MtrigWaitingChange(mlink.Count);
                    mNotice.Set();
                }
            }

            watchrecord getNode()
            {
                if (mNotice.Wait(5000) == false)
                {
                    return null;
                }

                lock (mlink)
                {
                    if (mlink.Count > 0)
                    {
                        var node = mlink.First.Value;
                        mlink.RemoveFirst();
                        MtrigWaitingChange(mlink.Count);
                        if (mlink.Count == 0)
                        {
                            mNotice.Reset();
                        }
                        return node;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            void trywritedata(MySqlCommand com, watchrecord wr, int trytime = 3)
            {
                Exception innexp = null;
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        com.CommandText = addMessageString(wr);
                        com.ExecuteNonQuery();
                        return;
                    }
                    catch (MySqlException exp)
                    {
                        if (exp.Message.Contains("Duplicate") == true)
                        {
                            return;
                        }
                        else if (exp.Message.Contains("exist") == true)
                        {
                            try
                            {
                                tryCreatetable(com, wr);
                            }
                            catch (System.Exception ex)
                            {
                                innexp = ex;
                            }
                        }
                        else
                        {
                            innexp = exp;
                        }
                    }
                }
                throw new WriteFailException(wr, innexp);
            }

            class WriteFailException : Exception
            {
                watchrecord wc;
                public WriteFailException(watchrecord wr, Exception exp)
                    : base("写入失败", exp)
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
                public NoTableFoundException(watchrecord wr, Exception exp)
                    : base("没有找到数据表", exp)
                {
                    wc = wr;
                }
                public override string ToString()
                {
                    return "没有找到" + wc.avstring;
                }
            }
            void tryCreatetable(MySqlCommand com, watchrecord wr, int trytime = 3)
            {
                Exception exp = null;
                for (int i = 0; i < trytime; i++)
                {
                    try
                    {
                        com.CommandText = CreatTableString();
                        com.ExecuteNonQuery();
                        return;
                    }
                    catch (MySqlException ex)
                    {
                        exp = ex;
                    }
                }
                throw new NoTableFoundException(wr, exp);
            }

/*
            void tryfindtable(MySqlCommand com, watchrecord wr, int trytime = 1)
            {
                Exception exp = null;
                for (int i = 0; i < trytime; i++)
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
                throw new NoTableFoundException(wr, exp);
            }*/



            class NoObjectTimeOut : Exception
            {
                public NoObjectTimeOut() : base("长时间没有接收到结构体") { }

            }



            MySqlConnection tryConnect()
            {
                string st = string.Format("host={0};port= {1};User Id={2};password={3};Database={4};"
                    , Addr, port, user, password, aimbase);
                MySqlConnection sqlcom = new MySqlConnection(st);

                sqlcom.Open();

                return sqlcom;
            }

            string CreatTableString()
            {
                /*
CREATE TABLE `bilibili`.`new_table` (
  `av` INT NOT NULL,
  `insertTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `watch` INT ZEROFILL NOT NULL,
  `collect` INT ZEROFILL NOT NULL,
  `coin` INT ZEROFILL NOT NULL,
  `danmu` INT ZEROFILL NOT NULL,
  PRIMARY KEY (`av`, `insertTime`));
                 **/
                string orgin = "CREATE TABLE `"+ getTableName()
                + "` ("
                + "`av` INT NOT NULL,"
                + "`insertTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,"
                + "`watch` int(11) ZEROFILL NOT NULL,"
                + "`collect` int(11) ZEROFILL NOT NULL,"
                + "`coin` int(11) ZEROFILL NOT NULL,"
                + "`danmu` int(11) ZEROFILL NOT NULL,"
                + "PRIMARY KEY(`av`, `insertTime`)) "
                + "ENGINE = myisam DEFAULT CHARSET = utf8 COMMENT = '自动生成'";
                return orgin;
            }

            string getTableName()
            {
                return string.Format("sum-{0}-{1}-{2}", DateTime.Now.ToString("yyyy-MM-dd"), total, index);
            }

            string addMessageString(watchrecord wc)
            {
                //INSERT INTO `bilibili`.`new_table` (`av`, `watch`, `collect`, `coin`, `danmu`) VALUES ('1', '2', '3', '4', '5');
                return string.Format("INSERT INTO `bilibili`.`{0}` (`av`,`watch`, `collect`, `coin`, `danmu`) VALUES('{1}', '{2}', '{3}', '{4}','{5}');"
                    , getTableName(),wc.avstring, wc.watchCount, wc.collectCount, wc.coinCount, wc.danmuCount);
            }
        }
    }
}
