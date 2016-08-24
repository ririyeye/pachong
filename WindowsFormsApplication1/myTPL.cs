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
        public int parallelNum { get; }
        public int currentRunningThreadNum { get; private set; }
        public int releaseTick { get;}
        Action<T> act;
        LinkedList<T> mlink;
        SemaphoreSlim mNotice;
        public myTPLBase(Action<T> act,int parallelNum = 8,int releaseTick = 5000)
        {
            this.parallelNum = parallelNum;
            currentRunningThreadNum = 0;
            this.act = act;
            this.releaseTick = releaseTick;
            mlink = new LinkedList<T>();
            mNotice = new SemaphoreSlim(0);
        }

        class LongTimeNoNodeException : Exception
        {
            public LongTimeNoNodeException()
                :base("长时间没有结构体")
                {
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
                catch (LongTimeNoNodeException )
                {
                    return;
                }
                catch (System.Exception)
                {
                    Post(node);
                }
                finally
                {
                    node = default(T);
                }
            }
        }

        public void setNewThread()
        {
            if (currentRunningThreadNum < parallelNum)
            {
                currentRunningThreadNum++;
                var task = new Task(NodeMain);
                task.Start();
            }
        }

        public void Post(T value)
        {
            lock (mlink)
            {
                mlink.AddLast(value);
                mNotice.Release();
            }
            if (mlink.Count > 0)
            {
                setNewThread();
            }
        }

        public T getNode()
        {
            bool flag = mNotice.Wait(5000);

            if (flag == false)
            {
                throw new LongTimeNoNodeException();
            }

            lock (mlink)
            {
                if (mlink.Count > 0)
                {
                    var node = mlink.First.Value;
                    mlink.RemoveFirst();
                    return node;
                }   
            }
            return getNode();
        }
    }
}
