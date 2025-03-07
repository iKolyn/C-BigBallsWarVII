﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
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
    //(Remove) 3.Let Player Choose the Type of enemy balls,number of balls, and cd.

    //需要的東西：
    //ballsQueue，主要使用於讓玩家選擇的生成模式 + 城堡血量不同時的生成。
    //擁有不同的生成佇列，同時會訂閱敵方城堡的當前血量。
    public static class EnemyBallsSpawner
    {
        #region 數值們
        public static List<Ball> balls;

        //敵人們的種類
        private static BallStruct[] ballsType =
        [
            new BallStruct { ATK = 5, HP = 20, SPEED = 60, Radius = 35, Color = Brushes.Green },
            new BallStruct { ATK = 30, HP = 150, SPEED = 45, Radius = 55, Color = Brushes.Blue },
            new BallStruct { ATK = 80, HP = 400, SPEED = 30, Radius = 75, Color = Brushes.Red },
            new BallStruct { ATK = 150, HP = 650, SPEED = 10, Radius = 95, Color = Brushes.Purple }
        ];
        //城堡血量專區
        public static Action RedCastleChanged;
        public static double RedCastleHP
        {
            get { return _redCastleHP; }
            set
            {
                if (value <= 0)
                    _redCastleHP = 0;
                else
                    _redCastleHP = value;
                RedCastleChanged?.Invoke();
            }
        }
        private static double _redCastleHP;
        public static double MaxRedCastleHP
        {
            get { return _maxRedCastleHP; }
            set
            {
                _maxRedCastleHP = value;
                RedCastleChanged?.Invoke();
            }
        }
        private static double _maxRedCastleHP;
        public static BallQueue BallQueue { get; private set; }//不同城堡血量的生成佇列
        private static List<BallNode>? currentBallQueue = new();
        private static DispatcherTimer _defaultSpawnTimer;
        private static double elapsedTime;//不會停止的當前時間
        private static DispatcherTimer _specialSpawnTimer;//負責城堡血量生成。
        private static double _specialCurrentTime;//城堡血量生成的當前時間
        private static double _specialLastTime;//城堡血量生成的上次時間
        //城堡血量生成的elapsedTime要變成區域變數寫在_spawnTimer_Tick裡面

        private static Stopwatch _stopWatch;//高精度的當前執行時間
        public static Action<Ball>? addBallToCanva;//將球體數量用委派顯示到畫布上。
        public static bool isGameOver = false;
        public static MediaPlayer bossSpawnSound;
        #endregion
        static EnemyBallsSpawner()
        {
            bossSpawnSound = new();
            Start();
        }

        public static void Start()
        {
            for (int i = 0; i < balls?.Count; i++)
            {
                balls[i]?.EndBallsControl();
            }
            BallQueue?.Clear();//清空所有球體，用於重置關卡。
            balls = [];//new的意思
            BallQueue = new();//依照城堡血量生成的佇列
            _defaultSpawnTimer = new()
            {
                Interval = TimeSpan.FromMilliseconds(16)//60FPS
            };
            _defaultSpawnTimer.Tick += DefaultSpawnTimer;
            _defaultSpawnTimer.Start();
            _smallBallCDTime = 5000;
            _mediumBallCDTime = 12000;
            _largeBallCDTime = 30000;

            _stopWatch = new();
            _stopWatch.Start();
            _specialSpawnTimer = new()
            {
                Interval = TimeSpan.FromMilliseconds(16)//一開始直接生成一次。
            };
            _specialSpawnTimer.Tick += SpecialSpawnTimer_Tick;
            //不要開啟_specialSpawnTimer，因為ballQueue還沒東西，會拋出找不到參考。
            AllBallQueueRegister();//註冊特殊生成的Queue用的。
        }

        public static void Reset()
        {
            //退訂所有事件
            Debug.WriteLine("退訂所有事件。");
            _defaultSpawnTimer.Tick -= DefaultSpawnTimer;
            _specialSpawnTimer.Tick -= SpecialSpawnTimer_Tick;
            lastSmallBallSpawnTime = lastMediumBallSpawnTime = lastLargeBallSpawnTime = 0;
            level1SpecialSpawn = level2SpecialSpawn = level3SpecialSpawn = false;
            isBossSpawn = true;
            Start();
        }

        static Random random = new();
        /// <summary>
        /// 註冊特殊生成的Queue用的。
        /// </summary>
        private static void AllBallQueueRegister()
        {

            //城堡50%以下後生成的球，全用random。
            for (int i = 0; i < 10; i++)
            {
                int typeNumber = random.Next(0, 3);
                BallQueue.Enqueue(new Ball(ballsType[typeNumber], (BallsType)typeNumber), random.Next(2, 10), 1);
            }
            //城堡20%以下後生成的球全用random。
            for (int i = 0; i < 15; i++)
            {
                int typeNumber = random.Next(0, 3);
                BallQueue.Enqueue(new Ball(ballsType[typeNumber], (BallsType)typeNumber), 4, 2);
            }
            //城堡80%以下後生成的球，全用random。
            for (int i = 0; i < 5; i++)
            {
                int typeNumber = random.Next(0, 2);
                BallQueue.Enqueue(new Ball(ballsType[typeNumber], (BallsType)typeNumber), random.Next(2, 8), 0);
            }
        }
        public static int BallCount
        {
            get { return _ballCount; }
            private set { _ballCount = value; Debug.WriteLine(_ballCount); }
        }
        private static int _ballCount = 0;
        public static void AddBall(Ball ball)
        {
            Debug.WriteLine("生成了一個敵方球體。");
            balls.Add(ball);
            addBallToCanva?.Invoke(ball);

            BallCount++;
        }
        public static void RemoveBall(Ball ball)
        {
            balls.Remove(ball);
            BallCount--;
        }
        public static List<Ball> GetAllBalls()
        {
            return balls;
        }
        //處理生成邏輯
        #region 生成CD的數值們
        private static double lastSmallBallSpawnTime, lastMediumBallSpawnTime, lastLargeBallSpawnTime = 0;//小球上次生成的時間
        //小中大球的CD生成時間
        private static double _smallBallCDTime, _mediumBallCDTime, _largeBallCDTime;
        #endregion
        //public static double ElapsedTime
        //{
        //    get { return _stopWatch.ElapsedMilliseconds; }
        //}
        private static bool isBossSpawn = true;
        private static bool level1SpecialSpawn, level2SpecialSpawn, level3SpecialSpawn = false;
        private static void DefaultSpawnTimer(object? sender, EventArgs e)//CD計時器 + 城堡血量狀態計時器
        {
            if (isGameOver)
            {
                _defaultSpawnTimer.Stop();
                Debug.WriteLine("遊戲結束，生成計時器停止。");
                return;
            }
            elapsedTime = _stopWatch.ElapsedMilliseconds;
            BallsType type;
            //生成普通狀態球的邏輯們
            //如果我是普通生成，我有五種怪物。我想要其中三種怪物可以依照各自不同的CD時間生成。CD就由manager創造。
            if (elapsedTime > lastSmallBallSpawnTime + _smallBallCDTime)//如果現在的時間經過了
            {
                type = BallsType.Small;
                Ball ball = new(ballsType[(int)type], type);
                AddBall(ball);
                lastSmallBallSpawnTime = elapsedTime;
            }
            if (elapsedTime > lastMediumBallSpawnTime + _mediumBallCDTime)
            {
                type = BallsType.Medium;
                Ball ball = new(ballsType[(int)type], type);
                AddBall(ball);
                lastMediumBallSpawnTime = elapsedTime;
            }
            if (elapsedTime > lastLargeBallSpawnTime + _largeBallCDTime)
            {
                type = BallsType.Large;
                Ball ball = new(ballsType[(int)type], type);
                AddBall(ball);
                lastLargeBallSpawnTime = elapsedTime;
            }

            //生成特殊狀態球的邏輯們

            if (RedCastleHP > MaxRedCastleHP * 0.8) return;
            else
            {
                if (RedCastleHP <= MaxRedCastleHP * 0.8 && RedCastleHP > MaxRedCastleHP * 0.5)//城堡80%以下
                {
                    if (level1SpecialSpawn)
                        return;
                    level1SpecialSpawn = true;
                    currentBallQueue = BallQueue.GetQueueByPriority(0);//儲存到待生成的球體佇列
                    _specialSpawnTimer.Start();
                    if (currentBallQueue != null)
                    {
                        Debug.WriteLine("現在的球體數量是：" + currentBallQueue.Count);
                        Debug.WriteLine("如果是5，代表有乖乖地從優先級0開始算。");
                    }
                }
                if (RedCastleHP <= MaxRedCastleHP * 0.5 && RedCastleHP > MaxRedCastleHP * 0.3)//城堡50%以下
                {
                    if (level2SpecialSpawn)
                        return;
                    level2SpecialSpawn = true;
                    List<BallNode>? temp = BallQueue.GetQueueByPriority(1);//暫存待生成的球體佇列
                    if (temp != null)
                    {
                        currentBallQueue ??= new();//如果是空的就new
                        currentBallQueue.AddRange(temp);//將兩者合併
                        Debug.WriteLine("現在的球體數量是：" + currentBallQueue.Count);
                        Debug.WriteLine("如果是15，代表優先級1的都被彈出，並與當前queue合併。");
                    }
                    _specialSpawnTimer.Start();

                }
                if (RedCastleHP <= MaxRedCastleHP * 0.3)//城堡30%以下
                {

                    if (level3SpecialSpawn)
                        return;
                    if (isBossSpawn)
                    {
                        Ball ball = new(ballsType[3], BallsType.Boss);
                        AddBall(ball);
                        isBossSpawn = false;
                        bossSpawnSound.Open(new Uri("Resources/bossSound.wav", UriKind.Relative));
                        bossSpawnSound.Play();
                    }
                    level3SpecialSpawn = true;
                    List<BallNode>? temp = BallQueue.GetQueueByPriority(2);//暫存待生成的球體佇列
                    if (temp != null)
                    {
                        currentBallQueue ??= new();
                        currentBallQueue.AddRange(temp);//將兩者合併
                        Debug.WriteLine("現在的球體數量是：" + currentBallQueue.Count);
                        Debug.WriteLine("如果是30，代表優先級2的都被彈出，並與當前queue合併。");
                    }
                    _specialSpawnTimer.Start();
                }
            }
        }
        /*如果我是城堡特殊生成(一次性生成)，我想按照每個ballsQueue下一個的CD時間去生成。
        //每個queue如果都有自己的CD時間，會怎麼執行：
        //首先拿取每個Queue的下一個球體，比較CD時間。
        //當CD時間小於當前elasped的時間，就生成球體。
        //生成完球體，繼續GetNext()。當所有Queue都沒有球體時，就停止。

        //首先我需要一個計時器
        //這個計時器也需要lastTime跟當前時間，因為他會在沒有quest的情況下停止生成
        //在來需要城堡血量偵測器。
        //如果我的血量低於80以下，第一次生成。如果在這個過程馬上又低於50以下，繼續生成。
        生成方式就是直接彈出所有相對應優先級的球體陣列。第一次五顆，第二次十顆，最後一次十五顆。*/
        private static void SpecialSpawnTimer_Tick(object? sender, EventArgs e)
        {
            if (isGameOver)
            {
                _specialSpawnTimer.Stop();
                return;
            }
            
            //首先設定下一個球體的生成CD時間。
            BallNode nextBall = currentBallQueue[0];//是空的就null，否則返回第一顆球。
            if (currentBallQueue[0] == null)
            {
                return;
            }
            double nextSpawnTime = nextBall != null ? nextBall.CD : 0;

            //確保 Interval 不為 0
            if (nextSpawnTime > 0)
            {
                _specialSpawnTimer.Interval = TimeSpan.FromSeconds(nextSpawnTime);
                Debug.WriteLine(nextSpawnTime + "秒後生成球體。");

            }
            else
            {
                _specialSpawnTimer.Interval = TimeSpan.FromSeconds(1);
                Debug.WriteLine("沒有發現nextBall的時間。");
            }
            //開始生成球體
            _specialCurrentTime = _stopWatch.ElapsedMilliseconds;
            //如果當前時間，到了上一次生成球體的時間 + 下一個球體的CD時間，就生成球體。
            if (_specialCurrentTime > _specialLastTime + (nextSpawnTime * 1000) && nextBall != null)
            {
                if(nextBall!=null)
                {
                    AddBall(nextBall.Data);
                    _specialLastTime = _specialCurrentTime;
                    currentBallQueue?.Remove(nextBall);
                    Debug.WriteLine("還有 " + currentBallQueue.Count + "顆球等待生成。");
                }
            }

            //如果沒有球體可以生成了，停止計時器。
            if (currentBallQueue.Count == 0)
            {
                Debug.WriteLine("沒有球體可以生成了。");
                _specialSpawnTimer.Stop();
            }
        }
    }
    public enum BallsType//不同敵人的種類
    {
        Small,
        Medium,
        Large,
        Boss
    }
}
