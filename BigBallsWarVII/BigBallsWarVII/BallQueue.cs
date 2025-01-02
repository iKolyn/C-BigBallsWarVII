using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Diagnostics;

namespace BigBallsWarVII
{
    /// <summary>
    /// ballsQueue是一個管理球體的佇列，可以用於生成球體。
    /// <br>Enqueue，放入球體，裡面包含ballType,優先級。</br>
    /// <br>Dequeue，取出球體，並且生成球體。</br>
    /// <br>GetNext，查看下一個球體，這可以用於顯示在UI上。</br>
    /// <br>Clear，清空所有球體，這可以用於重置關卡 or 玩家放棄選擇。</br>
    /// </summary>
    public class BallQueue
    {
        private BallNode? Head;//頭節點
        private BallNode? Tail;//尾節點
        private const int MAX_BALLS = 20;//佇列最多可存放的數量
        public int Count { get; private set; }//球體數量
        public double cd;
        public BallQueue()
        {
            Head = null;
            Tail = null;
            Count = 0;
        }
        /// <summary>
        /// 新增球體Queue，會依照優先級排列。無須優先級別時，預設為0。
        /// </summary>
        /// <param name="data">球體本身的資料</param>
        /// <param name="priority">優先級</param>
        public void Enqueue(Ball data, double cd = 2, int priority = 0)
        {
            //如果球體數量已經達到上限，就不要再放入了。
            if (Count >= MAX_BALLS)
            {
                Console.WriteLine("佇列已經滿囉");
                return;
            }
            BallNode newNode = new(data,cd , priority);

            //如果Head是空的，就把Head換掉。
            if (Head == null)
            {
                Head = newNode;
                Tail = newNode;
            }
            //如果Head的優先級比較低，也把他換掉。
            else if (Head.Priority < priority)
            {
                newNode.Next = Head;
                Head = newNode;
            }
            else
            {
                BallNode current = Head;
                //開始遍歷節點
                while (current.Next != null && current.Next.Priority >= priority)
                    current = current.Next;
                //找到了以後
                newNode.Next= current.Next;
                current.Next = newNode;
                if (newNode.Next == null)
                    Tail = newNode;
            }
            Count++;
        }
        /// <summary>
        /// 將佇列中的球體取出。請使用計時器or按鈕click事件呼叫他。
        /// </summary>
        /// <returns></returns>
        public Ball? Dequeue()
        {
            if (Count <= 0)
            {
                Console.WriteLine("佇列已空");
                return null;
            }
            Ball data = Tail.Data;
            Tail = Tail.Next;

            if (Tail == null)
                Head = null;

            Count--;
            return data;
        }
        /// <summary>
        /// 獲得下一個球體是誰，一顆球都沒有就回傳null。
        /// </summary>
        public Ball? GetNext()
        {
            if (Tail == null)
            {
                Console.WriteLine("沒有下一個喔");
                return null;
            }
            else//有就回傳下一個。
            {
                GetNextCD();
                return Tail.Data;
            }
        }
        private double? GetNextCD()
        {
            if (Tail != null)
                return Tail.CD;
            else return null;//為了不讓他跳綠底才這樣寫，這個功能被呼叫的時候已經確定Tail不是null。
        }
        /// <summary>
        /// 清空所有queue，用於重製關卡
        /// </summary>
        public void Clear()
        {
            Head = null;
            Tail = null;
            Count = 0;
            Console.WriteLine("佇列已清空");
        }
    }
}
