﻿using BigBallsWarVII;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BigBallsWarVII
{
    /// <summary>
    /// Ball的控制器，負責控制球體的生成，移動，攻擊等。
    /// <br>如要生成我方球體，請傳入BallsLevel列舉。</br>
    /// <br>如要生成敵方球體，請傳入BallStruct結構跟BallsType列舉。</br>
    /// </summary>
    public partial class Ball : UserControl, ILive
    {
        #region 數值們
        private MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        private DispatcherTimer moveTimer;
        private DispatcherTimer atkTimer;
        private DispatcherTimer cdTimer;
        private Stopwatch _stopWatch;//現在的時間。
        private double lastTime = 0;//上次的時間
        private double currentTime = 0;//經過的時間

        private Team team;
        public BallStruct ballStruct
        {
            get { return _ballProperties; }
            private set { }
        }//給對方看的屬性
        private BallStruct _ballProperties;//包含ATK,HP,COST跟SPEED。
        private BallsLevel ballsLevel;//從按鈕生成後，選擇要生成哪種本體用的列舉。
        private BallsType ballsType;//敵方球體的種類，用於CashManager。
        public UIElement? SHAPE//自己的形狀，不限於球體
        {
            get; private set;
        }
        private Ellipse _ball;//自己的本體(球)
        private Rectangle HPBackGround;//血條背景
        private Rectangle HPBar;//血條
        private Rectangle CDBar;//CD條
        /// <summary>
        /// 扣除血量的同時操控HPBar的寬度，請扣除血量時一定要使用此變數扣除。
        /// </summary>
        private int HP
        {
            get { return _ballProperties.HP; }
            set
            {
                /*不要這樣寫，如果血量沒剛好扣成0，你or對方就無敵了。
                 *if(value >= 0)
                 *_ballProperties.HP = value:*/
                if (value <= 0)
                    _ballProperties.HP = 0;
                else
                    _ballProperties.HP = value;
                if (HPBar != null)
                {
                    HPBar.Width = (_ballProperties.HP / maxHp) * (_ballProperties.Radius + 5);
                    Debug.WriteLine("現在的HRBar是" + HPBar.Width + "，現在的血量是" + _ballProperties.HP);
                }
            }
        }
        private double maxHp;//宣告時請註冊最大血量。
        /// <summary>
        /// 獲得對方的x座標，負責碰撞偵測。
        /// </summary>
        public double MyX
        { get { return _newX; } }
        private double _newX;//自己的新位置
        private double atkCD;//攻擊的冷卻時間
        /// <summary>
        /// 碰撞緩衝空間 = 10;
        /// </summary>
        private const double collisionBufferSpace = 10;
        public bool isAtkCastle { get; private set; } = false;//是否在攻擊城堡;
        bool isEnd = false;//球體是否正在刪除。
        #endregion
        #region 生成我方球體
        /// <summary>
        /// 生成我方球體用的，請傳入BallsLevel列舉
        /// </summary>
        /// <param name="level">球體種類的列舉</param>
        public Ball(BallsLevel level)
        {
            InitializeComponent();
            Loaded += BallsConrolLoaded;//這樣寫的話，一定會在最後才執行，不會有ATK瘋狂呼叫的問題。
            ballsLevel = level;
            team = Team.Blue;
            StartBallsControl(ballsLevel);//賦予球體數值
            CreateBall();//創造球體本身
        }
        private void StartBallsControl(BallsLevel level)
        {
            switch (level)
            {
                case BallsLevel.Small:
                    _ballProperties = new(10, 50, -60, 35);
                    break;
                case BallsLevel.Medium:
                    _ballProperties = new(25, 100, -45, 55);
                    break;
                case BallsLevel.Large:
                    _ballProperties = new(40, 300, -30, 75);
                    break;
            }
            maxHp = _ballProperties.HP;
        }
        void CreateBall()
        {
            double radius = _ballProperties.Radius;//方便賦值，不然寫太長
            _ball = ballsLevel switch
            {
                BallsLevel.Small => new Ellipse() { Width = radius, Height = radius, Fill = Brushes.Green },
                BallsLevel.Medium => new Ellipse() { Width = radius, Height = radius, Fill = Brushes.Blue },
                BallsLevel.Large => new Ellipse() { Width = radius, Height = radius, Fill = Brushes.Orange },
            };
            SHAPE = _ball;
            ballCanva.Children.Add(_ball);//將球載入畫布中。
            Canvas.SetLeft(_ball, 700 - 5);//初始位置
            Canvas.SetTop(_ball, 275 - _ball.Height);
            //血條背景
            HPBackGround = new Rectangle() { Width = radius + 5, Height = 6, Fill = Brushes.Black, Opacity = 0.7 };
            ballCanva.Children.Add(HPBackGround);//將球載入畫布中。
            Canvas.SetLeft(HPBackGround, 700 - 5);//初始位置
            Canvas.SetTop(HPBackGround, 275 - _ball.Height - 8);
            //血條本身
            HPBar = new Rectangle() { Width = radius + 5, Height = 5, Fill = Brushes.Red, Opacity = 0.7 };
            ballCanva.Children.Add(HPBar);//將球載入畫布中。
            Canvas.SetLeft(HPBar, 700 - 5);//初始位置
            Canvas.SetTop(HPBar, 275 - _ball.Height - 8);
            //CD條
            CDBar = new Rectangle() { Width = 0, Height = 5, Fill = Brushes.Black, Opacity = 0.5 };
            ballCanva.Children.Add(CDBar);//將球載入畫布中。
            Canvas.SetLeft(CDBar, 700 - 5);//初始位置
            Canvas.SetTop(CDBar, 275 - _ball.Height - 15);

            BallsManager.AddBall(this);//通知管理器增加球到自己的列表
        }
        #endregion
        #region 生成敵方球體
        /// <summary>
        /// 生成敵方球體用的，請傳入BallStruct結構
        /// </summary>
        /// <param name="ballStruct">敵方屬性，要包含HP,ATK,SPEED,COLOR跟RADIUS</param>
        /// <<param name="level">該敵方球體種類的列舉，CashManager需要他。</param>
        public Ball(BallStruct ballStruct, BallsType level)//用於敵人生成
        {
            InitializeComponent();
            team = Team.Red;
            _ballProperties = ballStruct;
            ballsType = level;
            if (_ballProperties.Color == null)
            {
                _ballProperties.Color = Brushes.Black;
                MessageBox.Show("你沒給敵人顏色啦！");
            }
            maxHp = _ballProperties.HP;
            CreateEnemyBall();//創造球體本身
            //計時器的初始化，要在_ballProperties被賦值後才可以啟動。
            Loaded += BallsConrolLoaded;
        }
        void CreateEnemyBall()
        {
            //因為沒有固定球體，由生成器決定要生成哪種類型的敵人。
            _ball = new Ellipse()
            {
                Width = _ballProperties.Radius,
                Height = _ballProperties.Radius,
                Fill = _ballProperties.Color,
            };
            SHAPE = _ball;
            ballCanva.Children.Add(_ball);//將球載入畫布
            Canvas.SetLeft(_ball, 100 - _ball.Width);//初始位置
            Canvas.SetTop(_ball, 275 - _ball.Height);
            //血條背景
            HPBackGround = new() { Width = _ballProperties.Radius + 5, Height = 6, Fill = Brushes.Black, Opacity = 0.7 };
            ballCanva.Children.Add(HPBackGround);
            Canvas.SetLeft(HPBackGround, 100 - _ball.Width);
            Canvas.SetTop(HPBackGround, 275 - _ball.Height - 8);
            //血條本身
            HPBar = new() { Width = _ballProperties.Radius + 5, Height = 5, Fill = Brushes.Blue, Opacity = 0.7 };
            ballCanva.Children.Add(HPBar);
            Canvas.SetLeft(HPBar, 100 - _ball.Width);
            Canvas.SetTop(HPBar, 275 - _ball.Height - 8);
            //CD條
            CDBar = new Rectangle() { Width = 0, Height = 5, Fill = Brushes.Black, Opacity = 0.5 };
            ballCanva.Children.Add(CDBar);//將球載入畫布中。
            Canvas.SetLeft(CDBar, 100 - _ball.Width);//初始位置
            Canvas.SetTop(CDBar, 275 - _ball.Height - 15);
        }
        #endregion
        /// <summary>
        /// 當準備顯示時，初始化計時器。
        /// </summary>
        private void BallsConrolLoaded(object sender, RoutedEventArgs e)
        {
            moveTimer = new();
            atkTimer = new();
            cdTimer = new();
            _stopWatch = new();
            moveTimer.Interval = TimeSpan.FromMilliseconds(16);//60FPS
            moveTimer.Tick += MoveTimer_Tick;
            moveTimer.Start();
            //依照攻擊除以特定數字來計算速度，攻擊力越高打越慢。
            //攻擊冷卻時間被限定在整數秒。
            atkCD = (int)Math.Log2(_ballProperties.ATK * 0.25) * 1000;
            atkTimer.Interval = TimeSpan.FromSeconds((int)atkCD * 0.001);
            atkTimer.Tick += AtkTimer_Tick;
            _stopWatch.Start();
            //單純這顆球攻擊後的冷卻時間，這是為了美觀。
            cdTimer.Interval = TimeSpan.FromMilliseconds(20);//50FPS
            cdTimer.Tick += CDTimer_Tick;
            }

        private void MoveTimer_Tick(object? sender, EventArgs e)
        {
            //移動，其實應該移動整個Canvas，但這又要測試一段時間。
            currentTime = _stopWatch.ElapsedMilliseconds;
            double deltaTime = (currentTime - lastTime) * 0.001;
            lastTime = currentTime;
            _newX = Canvas.GetLeft(_ball) + _ballProperties.SPEED * deltaTime;
            Canvas.SetLeft(_ball, _newX);
            Canvas.SetLeft(HPBackGround, _newX);
            Canvas.SetLeft(HPBar, _newX);
            Canvas.SetLeft(CDBar, _newX);

            //是否碰撞
            if (CollistionEvent())//如果球碰到敵人就攻擊
            {
                //開始先攻擊一次
                Attack();
                //攻擊後就開始計算CD
                cdTimer.Start();
                cdWatch.Start();
                endCD = cdWatch.ElapsedMilliseconds + atkCD;
                if (target != null && target.ballStruct.HP <= 0)
                {
                    target = null;
                    Debug.WriteLine("對方被我碰到就死了");
                }
                moveTimer.Stop();
                _stopWatch.Stop();
                //然後再開啟攻擊計時器，依照log2(攻擊力*0.25)決定攻擊頻率。
                atkTimer.Start();
            }
        }
        /// <summary>
        /// 目前的攻擊目標。
        /// <br>只要碰到就會存入，對方死亡為止都不會變更。</br>
        /// </summary>
        private Ball target = null;
        /// <summary>
        /// 是否碰到對方的功能，已放棄firtBall偵測。
        /// </summary>
        /// <returns>回傳是否偵測到碰撞。</returns>
        private bool CollistionEvent()
        {
            List<Ball> enemyBalls;
            switch (team)//要通知不同管理器增加球到它們自己的列表，以及碰撞偵測需要。
            {
                case Team.Blue://我們自己
                    BallsManager.UpdateBallPosition(this, _newX);
                    enemyBalls = EnemyBallsSpawner.GetAllBalls();
                    foreach (var enemy in enemyBalls)
                    {
                        //如果敵人還存在，且我已經要扁到他了。
                        double range = enemy.MyX + enemy.ballStruct.Radius;
                        if (_newX <= range && _newX > range - collisionBufferSpace)
                        {
                            //它就變成我的攻擊目標，然後回傳True碰到了。
                            target = enemy;
                            isAtkCastle = false;
                            return true;
                        }
                    }
                    if (_newX <= 115)//城堡實際在120
                    {
                        Debug.WriteLine("碰到城堡了");
                        isAtkCastle = true;//要攻擊城堡
                        return true;//碰到城堡就攻擊城堡
                    }
                    break;
                case Team.Red://敵方
                    EnemyBallsSpawner.UpdateEnemyBallPosition(this, _newX);
                    enemyBalls = BallsManager.GetAllBalls();
                    foreach (var enemy in enemyBalls)
                    {
                        //如果敵人還存在，且我已經要扁到他了。
                        //**********這裡寫法很重要，卡很久了**************
                        double range = enemy.MyX - _ballProperties.Radius;
                        if (_newX >= range && _newX < range + collisionBufferSpace)
                        {
                            //它就變成我的攻擊目標，然後回傳True碰到了。
                            target = enemy;
                            isAtkCastle = false;
                            return true;
                        }
                    }
                    if (_newX >= 675 - _ballProperties.Radius)//城堡實際在680
                    {
                        isAtkCastle = true;//要攻擊城堡
                        return true;//碰到城堡就攻擊城堡
                    }
                    break;
            }
            return false;
        }
        /// <summary>
        /// 攻擊專用的計時器，依照log2(ATK + 1.5)來計算攻擊頻率，ATK越高攻擊越慢。
        /// </summary>
        private void AtkTimer_Tick(object? sender, EventArgs e)
        {
            endCD = cdWatch.ElapsedMilliseconds + atkCD;
            cdTimer.Start();
            cdWatch.Start();
            Attack();
        }
        /// <summary>
        /// 讓玩家傳送攻擊的功能。
        /// <br>獨立Attack出來是因為，我碰觸的第一幀要攻擊一次。</br>
        /// </summary>
        private void Attack()
        {
            if (target != null)
            {
                Debug.WriteLine("攻擊了，我是" + team + " 的" + ballsLevel.ToString());
                target?.TakeDamage(this);//只要傳傷害就好。
            }
            else if (isAtkCastle)
            {
                switch (team)
                {
                    case Team.Blue:
                        EnemyBallsSpawner.RedCastleHP -= _ballProperties.ATK;
                        break;
                    case Team.Red:
                        BallsManager.BlueCastleHP -= _ballProperties.ATK;
                        break;
                }
            }
            else ResumeTimer();
        }
        double endCD;
       
        Stopwatch cdWatch = new();
        /// <summary>
        /// 單純的顯示攻擊者的CD時間。
        /// </summary>
        private void CDTimer_Tick(object? sender, EventArgs e)
        {
            double elapsedTime = -1 * (cdWatch.ElapsedMilliseconds - endCD);
            double percent = (elapsedTime / atkCD) * (_ballProperties.Radius + 5);
            if (percent > 0)
                CDBar.Width = percent;
            else
            {
                CDBar.Width = 0;
                cdTimer.Stop();
                cdWatch.Stop();
            }
        }
        public void ResumeTimer()
        {
            //只要開始移動，就代表不會攻擊了。關閉CD時間。
            CDBar.Width = 0;
            cdTimer.Stop();
            cdWatch.Stop();

            moveTimer.Start();
            _stopWatch.Start();
            atkTimer.Stop();
        }
        /// <summary>
        /// 對方攻擊我的時候，自己執行扣血量 or 依照buff扣血量的介面。
        /// </summary>
        /// <param name="ballProperties">對方的數據結構體</param>
        public void TakeDamage(Ball damager)
        {
            Action remove = team switch//根據不同的隊伍，在管理員刪除自己;
            {
                Team.Blue => () => BallsManager.RemoveBall(this),
                Team.Red => () => EnemyBallsSpawner.RemoveBall(this),
                _ => () => { }
            };
            Action resumeTimer = team switch//根據不同的隊伍，讓對方繼續移動;
            {
                Team.Blue => () => EnemyBallsSpawner.ResumeTimer(),
                Team.Red => () => BallsManager.ResumeTimer(),
                _ => () => { }
            };
            HP -= damager._ballProperties.ATK;
            if (HP <= 0 && !isEnd)
            {
                isEnd = true;
                //damager.ResumeTimer();//讓攻擊者繼續移動，先保留看以後用不用得到。
                resumeTimer.Invoke();
                remove.Invoke();
                Debug.WriteLine($"我是{team}，我死了");
                if(team == Team.Red)
                {
                    int cash = ballsType switch
                    { BallsType.Small => 10,
                        BallsType.Medium => 50,
                        BallsType.Large => 500,
                        _ => 0
                    };
                    CashSystem.IncreaseCash(cash);
                }
                EndBallsControl();
            }
        }
        private void EndBallsControl()
        {
            if (this.Parent is Panel parentPanel) { parentPanel.Children.Remove(this); }//在整個panel刪除
            moveTimer.Stop();//到這邊之後會變成null。
            _stopWatch.Stop();
            moveTimer.Tick -= MoveTimer_Tick;
            atkTimer.Stop();
            atkTimer.Tick -= AtkTimer_Tick;
            cdTimer.Stop();
            cdTimer.Tick -= CDTimer_Tick;
            Loaded -= BallsConrolLoaded;//退訂事件

            //確保所有資源都被釋放
            moveTimer = null;
            _stopWatch = null;
            _ball = null;
            SHAPE = null;
            HPBackGround = null;
            HPBar = null;
        }
        enum Team
        {
            Blue,//我們自己
            Red//敵方
        }
    }
}
public enum BallsLevel//給玩家按鈕生成用的
{
    Small,
    Medium,
    Large
}

public struct BallStruct
{
    public double Radius;
    public Brush Color;//敵人用的
    public int ATK;
    public int HP;
    public int SPEED;
    public BallStruct(int atk, int hp, int speed, double radius)
    {
        ATK = atk;
        HP = hp;
        SPEED = speed;
        Radius = radius;
    }
    public BallStruct() { }//允許建立空建構子
}
public interface ILive
{
    void TakeDamage(Ball damager);
}