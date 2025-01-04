using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BigBallsWarVII
{
    public static class BallsManager
    {
        static List<Ball> balls = new();//記錄所有自己的球。
        public static Ball FirstBall//預計讓mainCanva可以顯示當前第一顆球是誰。
        {
            get
            { return _firstBall; }
            set
            {
                FirstBallChangeEvent?.Invoke();//只要firstBall改變，就觸發事件。訂閱者模式。
            }
        }
        static Ball? _firstBall = null;
        public static Action? FirstBallChangeEvent;//問號?是允許值為空，不想讓他一直跳綠底很煩
        //位置相關
        public static Action<Ball>? addBallToCanva;//將球體數量用委派顯示到畫布上。
        public static int BallCount
        {
            get { return _ballCount; }
            private set { _ballCount = value; }
        }
        static int _ballCount = 0;
        public static Action? CountChange;

        public static void AddBall(Ball ball)
        {
            balls.Add(ball);
            addBallToCanva?.Invoke(ball);
            if (_firstBall == null)
                FirstBall = ball;
            BallCount++;
            CountChange?.Invoke();
        }
        public static void RemoveBall(Ball ball)
        {
            balls.Remove(ball);
            BallCount--;
            CountChange?.Invoke();
        }
        /// <summary>
        /// 每一顆球會偵測自己跟firstBall的位置，自己比較前面，就取代firstBall。
        /// </summary>
        /// <param name="ball">我自己</param>
        /// <param name="myX">我的座標</param>
        public static void UpdateBallPosition(Ball ball,double myX)
        {
            if (_firstBall != null && _firstBall.Shape != null && myX > Canvas.GetLeft(_firstBall.Shape))
            {
                _firstBall = ball;
            }
        }
    }
}
