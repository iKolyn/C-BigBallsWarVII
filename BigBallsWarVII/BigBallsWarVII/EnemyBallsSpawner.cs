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
    public class EnemyBallsSpawner
    {
        //因為希望有不同關卡，所以不用Staitc;
        List<Ball> balls;        
        public Ball? firstBall;
        private BallStruct[] ballsType = new[]
        {
            new BallStruct { ATK = 1, HP = 10, SPEED = 60, Radius = 35, Color = Brushes.Green },
            new BallStruct { ATK = 2, HP = 20, SPEED = 45, Radius = 55, Color = Brushes.Blue },
            new BallStruct { ATK = 3, HP = 30, SPEED = 30, Radius = 75, Color = Brushes.Red },
        };
        public BallQueue BallQueue { get;private set; }

        private DispatcherTimer _dispatcherTimer;
        private double elapsedTime;
        private Stopwatch _stopWatch;
        public static Action<Ball>? addBallToCanva;
        public EnemyBallsSpawner()
        {
            balls = [];//new的意思
            BallQueue = new();//依照城堡血量生成的佇列
            firstBall = null;
            _dispatcherTimer = new()
            {
                Interval = TimeSpan.FromMilliseconds(16)//60FPS
            };
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();
            _stopWatch = new();
            _stopWatch.Start();
        }

        public int BallCount
        {
            get { return _ballCount; }
            private set { _ballCount = value; }
        }
        int _ballCount = 0;
        public void AddBall(Ball ball)
        {
            balls.Add(ball);
            addBallToCanva?.Invoke(ball);
            BallCount++;
            if(firstBall == null || Canvas.GetLeft(ball) < Canvas.GetLeft(firstBall)) firstBall = ball;
        }
        public void RemoveBall(Ball ball)
        {
            balls.Remove(ball);
            BallCount--;
        }
        public void UpdateEnemyBallPosition()
        {
            foreach (var ball in balls)
            {
                double newX = Canvas.GetLeft(ball);
                if (newX < Canvas.GetLeft(firstBall))
                    firstBall = ball;
            }
        }
        //處理生成邏輯
        #region 生成CD的數值們
        private double lastSmallBallSpawnTime, lastMediumBallSpawnTime = 0;//小球上次生成的時間
        //小球的CD時間設定功能
        public double SmallBallCDTime
        {
            get { return _smallBallCDTime; }
            set
            {
                _smallBallCDTime = value < 0 ? 0 : value;//不讓CD時間變成負數
            }
        }
        double _smallBallCDTime = 5000;//小球的CD時間;
        //中球的CD時間設定功能
        public double MediumBallCDTime
        {
            get { return _mediumBallCDTime; }
            set
            {
                _mediumBallCDTime = value < 0 ? 0 : value;//不讓CD時間變成負數
            }
        }
        private double _mediumBallCDTime = 12000;//中球的CD時間;
        double[] 指定的CD生成時間 = new double[20];
        #endregion
        public double ElapsedTime
        {
            get { return _stopWatch.ElapsedMilliseconds; }
        }
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
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
