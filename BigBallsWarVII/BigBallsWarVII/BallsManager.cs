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
        #region 數值們
        static List<Ball> balls = new();//記錄所有自己的球。
        //城堡相關
        public static Action BlueCastleChanged;
        public static double BlueCastleHP
        {
            get { return _blueCastleHP; }
            set
            {
                if (value <= 0)
                    _blueCastleHP = 0;
                else
                    _blueCastleHP = value;
                BlueCastleChanged?.Invoke();
            }
        }
        private static double _blueCastleHP;
        public static double MaxBlueCastleHP
        {
            get { return _maxBlueCastleHP; }
            set
            {
                _maxBlueCastleHP = value;
                BlueCastleChanged?.Invoke();
            }
        }
        private static double _maxBlueCastleHP;
        //位置相關
        public static int BallCount
        {
            get { return _ballCount; }
            private set { _ballCount = value; }
        }
        static int _ballCount = 0;
        public static Action? CountChange;

        public static bool isGameOver = false;
        #endregion
        public static void AddBall(Ball ball)
        {
            balls.Add(ball);
            BallCount++;
            CountChange?.Invoke();
        }
        public static void RemoveBall(Ball ball)
        {
            balls.Remove(ball);
            BallCount--;
            CountChange?.Invoke();
        }
        public static List<Ball> GetAllBalls()
        {
            return balls;
        }
        public static void Reset()
        {
            foreach (var ball in balls)
            {
                ball?.EndBallsControl();
            }
            balls.Clear();
            BallCount = 0;
        }
    }
}
