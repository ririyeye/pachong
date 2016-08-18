using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MySql.Data;
using System.Threading;
using MySql.Data.MySqlClient;

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
            mlink = new LinkedList<List<fetch.fetchBase.watchrecord>>();
            threadpost = new Thread(postMain);
            threadpost.Start();
        }
        public void post(List<fetch.fetchBase.watchrecord> mw)
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
            List<fetch.fetchBase.watchrecord> mwriteObject;
            while (true)
            {
                try
                {
                    sqlcom = tryConnect();
                    com = new MySqlCommand();
                    com.Connection = sqlcom;

                    var node = getNode();
                    foreach (var sonNode in node)
                    {
                        com.CommandText = string.Format("show create table {0}.{1}", aimbase,sonNode.avstring);
                        try
                        {
                            com.ExecuteNonQuery();
                        }
                        catch (MySqlException)
                        {
                            com.CommandText = CreatTableString(sonNode.avstring);
                            com.ExecuteNonQuery();
                        }
                        com.CommandText = CreatTableString(sonNode.avstring);
                        com.ExecuteNonQuery();
                    }
                }
                catch (System.Exception ex)
                {
                	
                }
            }
        }

        List<fetch.fetchBase.watchrecord> getNode()
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
            string orgin= "CREATE TABLE `"+st+ "` ("
            +"`insertTime` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,"
            +"`watch` int(11) DEFAULT NULL,"
            +"`collect` int(11) DEFAULT NULL,"
            +"`coin` int(11) DEFAULT NULL,"
            +"`danmu` int(11) DEFAULT NULL,"
            +"PRIMARY KEY(`insertTime`)) "
            +"ENGINE = MyISAM DEFAULT CHARSET = utf8 COMMENT = '自动生成'";
            return orgin;
        }


    }
}
