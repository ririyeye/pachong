using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    class myTPLBase<T>
    {
        int parallelNum { get; }
        public int releaseTick { get; set; }
        Thread []mthreads;
        Action<T> act;
        LinkedList<T> mlink;
        ManualResetEventSlim mNotice;
        public myTPLBase(Action<T> act,int parallelNum = 8,int releaseTick = 5000)
        {
            this.parallelNum = parallelNum;
            this.act = act;
            this.releaseTick = releaseTick;
            mlink = new LinkedList<T>();
            mthreads = new Thread[parallelNum];

            mNotice = new ManualResetEventSlim();
            mNotice.Reset();

            for (int i=0;i<parallelNum;i++)
            {
                mthreads[i] = new Thread(NodeMain);
                mthreads[i].Start();
            } 
        }

        public void NodeMain()
        {
            T node = default(T);
            while (true)
            {
                try
                {
                    node = getNode();
                    act(node);
                }
                catch (System.Exception ex)
                {
                	
                }
                finally
                {
                    node = default(T);
                }
            }
        }

        public void Post(T value)
        {
            lock (mlink)
            {
                mlink.AddLast(value);
                mNotice.Set();
            }
        }

        public T getNode()
        {
            int enterTime = Environment.TickCount;
            bool flag = false;
            while (true)
            {
                flag = mNotice.Wait(100);
                if (flag == false)
                {
                    if (Environment.TickCount - enterTime > releaseTick)
                    {
                        return default(T);
                    }
                }

                lock (mlink)
                {
                    if (mlink.Count > 0)
                    {
                        var node = mlink.First.Value;
                        mlink.RemoveFirst();
                        if (mlink.Count == 0)
                        {
                            mNotice.Reset();
                        }
                        return node;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
    }
}
