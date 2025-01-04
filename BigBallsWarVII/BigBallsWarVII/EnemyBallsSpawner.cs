using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace BigBallsWarVII
{
    //How does Enemy Spwan?(Spawn Mode)
    //1.by reguler cd time
    //2.Spawn different type of balls queue,incouding the castle's hp range.
    //3.Let Player Choose the Type of enemy balls,number of balls, and cd.

    //需要的東西：
    //ballsQueue，主要使用於讓玩家選擇的生成模式 + 城堡血量不同時的生成。
    //擁有不同的生成佇列，同時會訂閱敵方城堡的當前血量。
    public static class EnemyBallsSpawner
    {
        //因為希望有不同關卡，所以不用Staitc;
        public static List<Ball> balls;        
        public static Ball? FirstBall 
        {
            get { return _firstBall; }
            private set { _firstBall = value; FirstBallChangeEvent?.Invoke(); }
        }
        static Ball? _firstBall;
        public static Action FirstBallChangeEvent;//顯示誰是第一顆球用的事件;
        //敵人們的種類
        private static BallStruct[] ballsType =
        [
            new BallStruct { ATK = 1, HP = 10, SPEED = 60, Radius = 35, Color = Brushes.Green },
            new BallStruct { ATK = 2, HP = 20, SPEED = 45, Radius = 55, Color = Brushes.Blue },
            new BallStruct { ATK = 3, HP = 30, SPEED = 30, Radius = 75, Color = Brushes.Red },
        ];
        public static BallQueue BallQueue { get;private set; }//不同城堡血量的生成佇列

        private static DispatcherTimer _dispatcherTimer;
        private static double elapsedTime;
        private static Stopwatch _stopWatch;//高精度的當前執行時間
        public static Action<Ball>? addBallToCanva;//將球體數量用委派顯示到畫布上。
        static EnemyBallsSpawner()
        {
            balls = [];//new的意思
            BallQueue = new();//依照城堡血量生成的佇列
            _firstBall = null;
            _dispatcherTimer = new()
            {
                Interval = TimeSpan.FromMilliseconds(16)//60FPS
            };
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();
            _stopWatch = new();
            _stopWatch.Start();
        }

        public static int BallCount
        {
            get { return _ballCount; }
            private set { _ballCount = value; }
        }
        private static int _ballCount = 0;
        public static void AddBall(Ball ball)
        {
            balls.Add(ball);
            addBallToCanva?.Invoke(ball);
            if (_firstBall == null)
                FirstBall = ball;
            BallCount++;
        }
        public static void RemoveBall(Ball ball)
        {
            balls.Remove(ball);
            BallCount--;
        }
        public static void UpdateEnemyBallPosition(Ball ball,double myX)
        {
            //如果我是第一顆球，或者我比第一顆球前面，就取代第一顆球。
            if (_firstBall != null && _firstBall.Shape != null && myX < Canvas.GetLeft(_firstBall.Shape))
                _firstBall = ball;
        }
        //處理生成邏輯
        #region 生成CD的數值們
        private static double lastSmallBallSpawnTime, lastMediumBallSpawnTime = 0;//小球上次生成的時間
        //小球的CD時間設定功能
        public static double SmallBallCDTime
        {
            get { return _smallBallCDTime; }
            set
            {
                _smallBallCDTime = value < 0 ? 0 : value;//不讓CD時間變成負數
            }
        }
        private static double _smallBallCDTime = 5000;//小球的CD時間;
        //中球的CD時間設定功能
        public static double MediumBallCDTime
        {
            get { return _mediumBallCDTime; }
            set
            {
                _mediumBallCDTime = value < 0 ? 0 : value;//不讓CD時間變成負數
            }
        }
        private static double _mediumBallCDTime = 12000;//中球的CD時間;
        static double[] 指定的CD生成時間 = new double[20];
        #endregion
        public static double ElapsedTime
        {
            get { return _stopWatch.ElapsedMilliseconds; }
        }
        private static void DispatcherTimer_Tick(object? sender, EventArgs e)//CD計時器
        {
            elapsedTime = _stopWatch.ElapsedMilliseconds;
            //生成普通狀態球的邏輯們
            //如果我是普通生成，我有五種怪物。我想要其中三種怪物可以依照各自不同的CD時間生成。CD就由manager創造。
            if (elapsedTime > lastSmallBallSpawnTime + SmallBallCDTime)//如果現在的時間經過了
            {
                Ball ball = new(ballsType[(int)ballType.Small]);
                AddBall(ball);
                lastSmallBallSpawnTime = elapsedTime;
            }
            if (elapsedTime > lastMediumBallSpawnTime + MediumBallCDTime)
            {
                Ball ball = new(ballsType[(int)ballType.Medium]);
                AddBall(ball);
                lastMediumBallSpawnTime = elapsedTime;
            }
            //如果我是城堡特殊生成(一次性生成)，我想按照每個ballsQueue下一個的CD時間去生成。
            //每個queue如果都有自己的CD時間，會怎麼執行：
            //首先拿取每個Queue的下一個球體，比較CD時間。
            //當CD時間小於當前elasped的時間，就生成球體。
            //生成完球體，繼續GetNext()。當所有Queue都沒有球體時，就停止。
        }
    }
    public enum ballType//不同敵人的種類
    {
        Small,
        Medium,
        Large
    }
}
