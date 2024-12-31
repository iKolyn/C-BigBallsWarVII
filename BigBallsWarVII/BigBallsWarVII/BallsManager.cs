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
        static List<BallsControl> balls = new();//記錄所有自己的球。
        static BallsControl firstBall = null;
        public static int BallCount
        {
            get { return _ballCount; }private set {  _ballCount = value; }
        }
        static int _ballCount = 0;
        public static Action CountChange;
        public static void AddBall(BallsControl ball)
        {
            balls.Add(ball);
            BallCount++;
            CountChange?.Invoke();
            if (firstBall == null || Canvas.GetLeft(ball) < Canvas.GetLeft(firstBall)) firstBall = ball;
        }
        public static void RemoveBall(BallsControl ball)
        {
            balls.Remove(ball);
            BallCount--;
            CountChange?.Invoke();
        }
        public static void UpdateBallPosition()
        {
            foreach (var ball in balls)
            { 
                double newX = Canvas.GetLeft(ball);
                if (newX > Canvas.GetLeft(firstBall))
                    firstBall = ball;
            }
        }
    }
    //現在需要的是敵人，敵人需要生成器、也要它的Manager偵測誰是第一個。
}
