using BigBallsWarVII;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
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
    /// <br>如要生成敵方球體，請傳入BallStruct結構。</br>
    /// </summary>
    public partial class Ball : UserControl, ILive
    {
        private MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        private DispatcherTimer moveTimer;
        private DispatcherTimer atkTimer;
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
        public UIElement? SHAPE//自己的形狀，不限於球體
        {
            get; private set;
        }
        private Ellipse _ball;//自己的本體(球)
        /// <summary>
        /// 獲得對方的x座標，負責碰撞偵測。
        /// </summary>
        public double MyX
        { get { return _newX; } }
        private double _newX;//自己的新位置
        /// <summary>
        /// 碰撞緩衝空間 = 10;
        /// </summary>
        private const double collisionBufferSpace = 10;
        bool isEnd = false;//球體是否正在刪除。

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
                    _ballProperties = new(1, 10, -60, 35);
                    break;
                case BallsLevel.Medium:
                    _ballProperties = new(5, 20, -45, 55);
                    break;
                case BallsLevel.Large:
                    _ballProperties = new(10, 50, -30, 75);
                    break;
            }
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
            Canvas.SetLeft(_ball, 700 - _ball.Width);//初始位置，可以跟主程式拿當前城堡的座標
            Canvas.SetTop(_ball, 275 - _ball.Height);
            BallsManager.AddBall(this);//通知管理器增加球到自己的列表
        }
        #endregion
        #region 生成敵方球體
        /// <summary>
        /// 生成敵方球體用的，請傳入BallStruct結構
        /// </summary>
        /// <param name="ballStruct">敵方屬性，要包含HP,ATK,SPEED,COLOR跟RADIUS</param>
        public Ball(BallStruct ballStruct)//用於敵人生成
        {
            InitializeComponent();
            team = Team.Red;
            _ballProperties = ballStruct;
            if (_ballProperties.Color == null)
            {
                _ballProperties.Color = Brushes.Black;
                MessageBox.Show("你沒給敵人顏色啦！");
            }
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
            ballCanva.Children.Add(_ball);
            Canvas.SetLeft(_ball, 100 - _ball.Width);//初始位置，可以跟主程式拿當前城堡的座標
            Canvas.SetTop(_ball, 275 - _ball.Height);
        }
        #endregion
        //當準備顯示時，初始化計時器。
        private void BallsConrolLoaded(object sender, RoutedEventArgs e)
        {
            moveTimer = new();
            atkTimer = new();
            _stopWatch = new();
            moveTimer.Interval = TimeSpan.FromMilliseconds(16);//60FPS
            moveTimer.Tick += MoveTimer_Tick;
            moveTimer.Start();
            //依照攻擊除以特定數字來計算速度，攻擊力越高打越慢。
            atkTimer.Interval = TimeSpan.FromSeconds((int)Math.Log2(_ballProperties.ATK + 1.5));
            atkTimer.Tick += AtkTimer_Tick;
            //
            _stopWatch.Start();
        }

        private void MoveTimer_Tick(object? sender, EventArgs e)
        {
            //移動
            currentTime = _stopWatch.ElapsedMilliseconds;
            double deltaTime = (currentTime - lastTime) * 0.001;
            lastTime = currentTime;
            _newX = Canvas.GetLeft(_ball) + _ballProperties.SPEED * deltaTime;
            Canvas.SetLeft(_ball, _newX);

            //是否碰撞
            if (CollistionEvent())//如果球碰到敵人就攻擊
            {
                //敵人會一直回傳True一直扁我，這不正常。
                Attack();
                if (target.ballStruct.HP <= 0)
                {
                    target = null;
                    Debug.WriteLine("對方被我碰到就死了");
                }
                else
                {
                    moveTimer.Stop();
                    _stopWatch.Stop();
                    //開始先攻擊一次
                    //然後再開啟攻擊計時器，依照log2(攻擊力+1.5)決定攻擊頻率。
                    atkTimer.Start();
                    Debug.WriteLine("碰到了");
                }
            }
        }
        /// <summary>
        /// 目前的攻擊目標。
        /// <br>只要碰到就會存入，對方死亡為止都不會變更。</br>
        /// </summary>
        private Ball target = null;

        /// <summary>
        /// 是否碰到對方的功能，以放棄firtBall偵測。
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
                        if (enemy is ILive enemyTarget && (_newX <= range && _newX > range - collisionBufferSpace)) 
                        {
                            //它就變成我的攻擊目標，然後回傳True碰到了。
                            target = enemy;
                            return true;
                        }
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
                        if (enemy is ILive enemyTarget && (_newX >= range && _newX < range + collisionBufferSpace))
                        {
                            //它就變成我的攻擊目標，然後回傳True碰到了。
                            target = enemy;
                            return true;
                        }
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
                target?.TakeDamage(this, ref _ballProperties);//只要傳傷害就好。
            }
        }
        public void ResumeTimer()
        {
            moveTimer.Start();
            _stopWatch.Start();
            atkTimer.Stop();
        }
        /// <summary>
        /// 對方攻擊我的時候，自己執行扣血量 or 依照buff扣血量的介面。
        /// </summary>
        /// <param name="ballProperties">對方的數據結構體</param>
        public void TakeDamage(Ball damager ,ref BallStruct ballProperties)
        {
            Action remove = team switch
            {
                Team.Blue => () => BallsManager.RemoveBall(this),
                Team.Red => () => EnemyBallsSpawner.RemoveBall(this),
                _ => () => { }
            };
            Action resumeTimer = team switch
            {
                Team.Blue => () => EnemyBallsSpawner.ResumeTimer(),
                Team.Red =>  () => BallsManager.ResumeTimer(),
                _ => () => { }
            };
            _ballProperties.HP -= ballProperties.ATK;
            if (_ballProperties.HP <= 0 && !isEnd)
            {
                isEnd = true;
                //damager.ResumeTimer();//讓攻擊者繼續移動。
                resumeTimer.Invoke();
                remove.Invoke();
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
            Loaded -= BallsConrolLoaded;//退訂事件

            //確保所有資源都被釋放
            moveTimer = null;
            _stopWatch = null;
            _ball = null;
            SHAPE = null;
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
    public BallStruct(int atk, int hp, int speed,double radius)
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
    void TakeDamage(Ball damager,ref BallStruct ballStruct);
}