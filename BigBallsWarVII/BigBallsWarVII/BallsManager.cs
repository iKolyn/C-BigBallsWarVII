using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BigBallsWarVII
{
    public static class BallsManager
    {
        static BallsManager()
        {
            EnemyBallsSpawner.isFirstBallDie += ResumeTimer;
            Debug.WriteLine("訂閱成功");
        }
        static List<Ball> balls = new();//記錄所有自己的球。
        public static Ball? FirstBall//預計讓mainCanva可以顯示當前第一顆球是誰。
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
        /// <summary>
        /// 通知對方管理員第一顆球死了，讓所有對方的球可以移動。
        /// </summary>
        public static Action isFirstBallDie;
        public static double GetFirstBallPosition()
        {
            return FirstBall != null && FirstBall.SHAPE != null ? Canvas.GetLeft(FirstBall.SHAPE) : 0;
        }
        public static void AddBall(Ball ball)
        {
            balls.Add(ball);
            addBallToCanva?.Invoke(ball);
            if (_firstBall == null)
            {
                Debug.WriteLine("我是剛生成的第一顆球");
                _firstBall = ball;
            }
            BallCount++;
            CountChange?.Invoke();
        }
        public static void RemoveBall(Ball ball)
        {
            balls.Remove(ball);
            _firstBall = null;//移除firstBall
            BallCount--;
            CountChange?.Invoke();
        }
        public static List<Ball> GetAllBalls()
        {
            return balls;
        }
        /// <summary>
        /// 每一顆球會偵測自己跟firstBall的位置，自己比較前面，就取代firstBall。
        /// </summary>
        /// <param name="ball">我自己</param>
        /// <param name="myX">我的座標</param>
        public static void UpdateBallPosition(Ball ball, double myX)
        {
            if (_firstBall == null)
            {
                Debug.WriteLine("第一顆球掛掉，被取代了");
                _firstBall = ball;//如果firstBall是空的，就直接取代。
            }
            //如果已經有第一顆球，而且我比他前面，就取代他。-1是避免過於接近而短期間互換太多次。
            if (_firstBall != null && _firstBall.SHAPE != null && myX < Canvas.GetLeft(_firstBall.SHAPE) - 1)
            {
                Debug.WriteLine("被超過了！第一顆球取代");
                _firstBall = ball;
            }
        }
        public static void ResumeTimer()
        {
            foreach (Ball ball in balls)
            {
                ball.ResumeTimer();
            }
        }
    }
}
