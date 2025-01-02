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
        static Ball firstBall = null;
        public static int BallCount
        {
            get { return _ballCount; }private set {  _ballCount = value; }
        }
        static int _ballCount = 0;
        public static Action CountChange;
        public static void AddBall(Ball ball)
        {
            balls.Add(ball);
            BallCount++;
            CountChange?.Invoke();
            if (firstBall == null || Canvas.GetLeft(ball) < Canvas.GetLeft(firstBall)) firstBall = ball;
        }
        public static void RemoveBall(Ball ball)
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
}
