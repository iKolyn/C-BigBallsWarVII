using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BigBallsWarVII
{
    public class BallNode
    {
        public BallsControl Data;//資料本身
        public int Priority;//優先級，越大越優先。
        public BallNode? Next;//下一個球體
        public BallNode(BallsControl data, int priority)
        {
            Data = data;
            Priority = priority;
            Next = null;
        }
        public BallNode() { }//空建構子
    }
}
