using System;
using System.Collections.Generic;
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
    public partial class BallsControl : UserControl
    {
        private DispatcherTimer moveTimer;
        private DateTime lastTime;//計算deltaTime用的
        private DateTime currentTime;//計算deltaTime用的

        private Team team;
        private BallStruct ballProperties;//包含ATK,HP,COST跟SPEED。
        private BallsLevel ballsLevel;        
        private Ellipse ball;

        public BallsControl(BallsLevel level)
        {
            InitializeComponent();
            ballsLevel = level;
            team = Team.Blue;
            StartBallsControl(ballsLevel);//賦予球體數值
            CreateBall();//創造球體本身
            lastTime = DateTime.Now;
            BallsManager.AddBall(this);
            Loaded += BallsConrolLoaded;
        }

        private void BallsConrolLoaded(object sender, RoutedEventArgs e)
        {
            moveTimer = new();
            moveTimer.Interval = TimeSpan.FromMilliseconds(16);//60FPS
            moveTimer.Tick += MoveTimer_Tick;
            moveTimer.Start();

        }
        #region 生成blue球體
        private void StartBallsControl(BallsLevel level)
        {
            switch (level)
            {
                case BallsLevel.Small:
                    ballProperties = new(1, 10, -80, 35);
                    break;
                case BallsLevel.Medium:
                    ballProperties = new(2, 20, -50, 55);
                    break;
                case BallsLevel.Large:
                    ballProperties = new(5, 50, -30, 75);
                    break;
            }
        }
        void CreateBall()
        {
            double radius = ballProperties.Radius;
            ball = ballsLevel switch
            {
                BallsLevel.Small => new Ellipse() { Width = radius, Height = radius, Fill = Brushes.Green },
                BallsLevel.Medium => new Ellipse() { Width = radius, Height = radius, Fill = Brushes.Blue },
                BallsLevel.Large => new Ellipse() { Width = radius, Height = radius, Fill = Brushes.Orange },
            };
            ballCanva.Children.Add(ball);//將球載入畫布中。
            Canvas.SetLeft(ball, 700 - ball.Width);//初始位置，可以跟主程式拿當前城堡的座標
            Canvas.SetTop(ball, 275 - ball.Height);
        }
        #endregion
        #region 生成敵方球體
        /// <summary>
        /// 生成敵方球體用的
        /// </summary>
        /// <param name="ballStruct">敵方屬性，要包含HP,ATK,SPEED,COLOR跟RADIUS</param>
        public BallsControl(BallStruct ballStruct)//用於敵人生成
        {
            InitializeComponent();
            team = Team.Red;
            ballProperties = ballStruct;
            if(ballProperties.Color == null)
            {
                ballProperties.Color = Brushes.Black;
                MessageBox.Show("你沒給敵人顏色啦！");
            }
            CreateEnemyBall();//創造球體本身
            lastTime = DateTime.Now;
            BallsManager.AddBall(this);
        }
        void CreateEnemyBall()
        {
            //因為沒有固定球體，由生成器決定要生成哪種類型的敵人。
            ball = new Ellipse()
            {
                Width = ballProperties.Radius,
                Height = ballProperties.Radius,
                Fill = ballProperties.Color,
            };
            ballCanva.Children.Add(ball);
            Canvas.SetLeft(ball, 100 - ball.Width);//初始位置，可以跟主程式拿當前城堡的座標
            Canvas.SetTop(ball, 275 - ball.Height);
        }
        #endregion
        //Remove DispatcherTimer and use Update() instead.Manage Will call this Method.
        private void MoveTimer_Tick(object? sender,EventArgs e)
        {
            currentTime = DateTime.Now;
            double deltaTime = (currentTime - lastTime).TotalSeconds;
            lastTime = currentTime;
            double newX = Canvas.GetLeft(ball) + ballProperties.SPEED * deltaTime;
            Canvas.SetLeft(ball, newX);

            switch (team)//要通知不同管理器增加球到它們自己的列表，以及碰撞偵測需要。
            {
                case Team.Blue:
                    BallsManager.UpdateBallPosition();
                    if (Canvas.GetLeft(ball) < 20 - ball.Width)
                    {
                        BallsManager.RemoveBall(this);
                        EndBallsControl();
                    }
                    break;
                case Team.Red:
                    if (Canvas.GetLeft(ball) > 780 + ball.Width)
                    {
                        BallsManager.RemoveBall(this);
                        EndBallsControl();
                    }
                    break;
            }
        }

        private void EndBallsControl()
        {
            if (this.Parent is Panel parentPanel) { parentPanel.Children.Remove(this); }//在整個panel刪除
            moveTimer.Stop();
            moveTimer.Tick -= MoveTimer_Tick;
            Loaded -= BallsConrolLoaded;//退訂事件
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