using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
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
    /// BallsControl.xaml 的互動邏輯
    /// </summary>
    public partial class Ball : UserControl
    {
        private MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        private DispatcherTimer moveTimer;
        private DispatcherTimer atkTimer;
        private Stopwatch _stopWatch;//現在的時間。
        private double lastTime = 0;//上次的時間
        private double currentTime = 0;//經過的時間

        private Team team;
        private BallStruct ballProperties;//包含ATK,HP,COST跟SPEED。
        private BallsLevel ballsLevel;
        public UIElement Shape//自己的形狀
        {
            get; private set;
        }
        private Ellipse _ball;//自己的本體(球)
        private double _newX;//自己的新位置
     
        #region 生成我方球體
        public Ball(BallsLevel level)
        {
            InitializeComponent();
            Loaded += BallsConrolLoaded;
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
                    ballProperties = new(1, 10, -60, 35);
                    break;
                case BallsLevel.Medium:
                    ballProperties = new(2, 20, -45, 55);
                    break;
                case BallsLevel.Large:
                    ballProperties = new(5, 50, -30, 75);
                    break;
            }
        }
        void CreateBall()
        {
            double radius = ballProperties.Radius;
            _ball = ballsLevel switch
            {
                BallsLevel.Small => new Ellipse() { Width = radius, Height = radius, Fill = Brushes.Green },
                BallsLevel.Medium => new Ellipse() { Width = radius, Height = radius, Fill = Brushes.Blue },
                BallsLevel.Large => new Ellipse() { Width = radius, Height = radius, Fill = Brushes.Orange },
            };
            Shape = _ball;
            ballCanva.Children.Add(_ball);//將球載入畫布中。
            Canvas.SetLeft(_ball, 700 - _ball.Width);//初始位置，可以跟主程式拿當前城堡的座標
            Canvas.SetTop(_ball, 275 - _ball.Height);
        }
        #endregion
        #region 生成敵方球體
        /// <summary>
        /// 生成敵方球體用的
        /// </summary>
        /// <param name="ballStruct">敵方屬性，要包含HP,ATK,SPEED,COLOR跟RADIUS</param>
        public Ball(BallStruct ballStruct)//用於敵人生成
        {
            InitializeComponent();
            BallsConrolLoaded(this,new RoutedEventArgs());//計時器的初始化
            team = Team.Red;
            ballProperties = ballStruct;
            if(ballProperties.Color == null)
            {
                ballProperties.Color = Brushes.Black;
                MessageBox.Show("你沒給敵人顏色啦！");
            }
            CreateEnemyBall();//創造球體本身
        }
        void CreateEnemyBall()
        {
            //因為沒有固定球體，由生成器決定要生成哪種類型的敵人。
            _ball = new Ellipse()
            {
                Width = ballProperties.Radius,
                Height = ballProperties.Radius,
                Fill = ballProperties.Color,
            };
            Shape = _ball;
            ballCanva.Children.Add(_ball);
            Canvas.SetLeft(_ball, 100 - _ball.Width);//初始位置，可以跟主程式拿當前城堡的座標
            Canvas.SetTop(_ball, 275 - _ball.Height);
        }
        #endregion
        private void BallsConrolLoaded(object sender, RoutedEventArgs e)
        {
            moveTimer = new();
            atkTimer = new();
            _stopWatch = new();
            moveTimer.Interval = TimeSpan.FromMilliseconds(16);//60FPS
            moveTimer.Tick += MoveTimer_Tick;
            moveTimer.Start();
            //依照攻擊除以特定數字來計算速度，攻擊力越高打越慢。
            atkTimer.Interval = TimeSpan.FromSeconds((int)Math.ILogB(ballProperties.ATK + 1.5));
            //
            _stopWatch.Start();
        }
        //Remove DispatcherTimer and use Update() instead.Manage Will call this Method.
        private void MoveTimer_Tick(object? sender,EventArgs e)
        {
            currentTime = _stopWatch.ElapsedMilliseconds;
            double deltaTime = (currentTime - lastTime) / 1000;
            lastTime = currentTime;
            _newX = Canvas.GetLeft(_ball) + ballProperties.SPEED * deltaTime;
            Canvas.SetLeft(_ball, _newX);
            if (CollistionEvent())//如果球碰到敵人就攻擊
            {
                moveTimer.Stop();
                _stopWatch.Stop();
                //開始攻擊
            }
        }
        private bool CollistionEvent()
        {            
            double enemyX;
            switch (team)//要通知不同管理器增加球到它們自己的列表，以及碰撞偵測需要。
            {
                case Team.Blue:
                    BallsManager.UpdateBallPosition(this, _newX);//更新第一個球是誰(攻擊目標)
                    if (EnemyBallsSpawner.FirstBall != null)
                    {
                        enemyX = Canvas.GetLeft(EnemyBallsSpawner.FirstBall.Shape) + EnemyBallsSpawner.FirstBall.ballProperties.Radius;
                        Debug.WriteLine($"EnemyX : {enemyX}");
                    }
                    else
                        enemyX = 0;
                    if (_newX <= enemyX)//Collision Event
                    {
                        Debug.WriteLine("碰到敵人了");
                        return true;
                    }
                    break;
                case Team.Red:
                    EnemyBallsSpawner.UpdateEnemyBallPosition(this, _newX);//更新第一個球是誰(攻擊目標)
                    if (BallsManager.FirstBall != null)
                    {
                        enemyX = Canvas.GetLeft(BallsManager.FirstBall.Shape) - BallsManager.FirstBall.ballProperties.Radius;
                        Debug.WriteLine($"myX : {enemyX}");
                    }
                    else
                        enemyX = double.MaxValue;
                    if (_newX >= enemyX)//Collision Event
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }
        private void EndBallsControl()
        {
            if (this.Parent is Panel parentPanel) { parentPanel.Children.Remove(this); }//在整個panel刪除
            moveTimer.Stop();
            _stopWatch.Stop();
            moveTimer.Tick -= MoveTimer_Tick;
            Loaded -= BallsConrolLoaded;//退訂事件

            // 確保所有資源都被釋放
            moveTimer = null;
            _stopWatch = null;
            _ball = null;
            Shape = null;
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