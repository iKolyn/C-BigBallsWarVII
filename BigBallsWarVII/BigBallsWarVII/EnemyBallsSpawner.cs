using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        List<BallsControl> balls;        
        public BallsControl firstBall;
        private BallStruct[] ballsType = new[]
        {
            new BallStruct { ATK = 1, HP = 10, SPEED = 30, Radius = 35, Color = Brushes.Green },
            new BallStruct { ATK = 1, HP = 10, SPEED = 30, Radius = 35, Color = Brushes.Green },
            new BallStruct { ATK = 1, HP = 10, SPEED = 30, Radius = 35, Color = Brushes.Green },
        };
        public BallQueue BallQueue { get;private set; }

        private DispatcherTimer _dispatcherTimer;
        private double lastSpawnTime;
        private double elapsedTime;

        private const double regularSpawnCD = 2000;//可以用spawnCD的某個Stack來操控每顆球的生成速度。
        public EnemyBallsSpawner()
        {
            balls = [];
            BallQueue = new();
            _dispatcherTimer = new()
            {
                Interval = TimeSpan.FromMilliseconds(16)//60FPS
            };
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();
            lastSpawnTime = 0;
            elapsedTime = 0;
        }

        public int BallCount
        {
            get { return _ballCount; }
            private set { _ballCount = value; }
        }
        int _ballCount = 0;
        public void AddBall(BallsControl ball)
        {
            balls.Add(ball);
            BallCount++;
            if(firstBall == null || Canvas.GetLeft(ball) < Canvas.GetLeft(firstBall)) firstBall = ball;
        }
        public void RemoveBall(BallsControl ball)
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
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            //生成普通狀態球的邏輯
            elapsedTime += 16;
            if (elapsedTime - lastSpawnTime > regularSpawnCD)
            {
                lastSpawnTime = elapsedTime;
                BallsControl ball = new(ballsType[0]);
                AddBall(ball);
            }

        }

    }
    public enum ballType//不同敵人的種類
    {

    }
}
