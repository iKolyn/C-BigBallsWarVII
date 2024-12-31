using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        private DispatcherTimer moveTimer;//移動的計時器
        private DateTime lastTime;//計算deltaTime用的
        private DateTime currentTime;//計算deltaTime用的

        private BallStruct ballProperties;//包含ATK,HP,COST跟SPEED。
        private BallsLevel ballsLevel;
        private Ellipse ball;
        public BallsControl(BallsLevel level)
        {
            InitializeComponent();
            ballsLevel = level; 
            Loaded += BallsConrolLoaded;//初始化的時候
        }
        private void BallsConrolLoaded(object? sender, RoutedEventArgs e)
        {
            CreateBall();//創造球體本身
            StartBallsControl(ballsLevel);//賦予球體數值
            TimerAwake();//初始化timer
        }
        //計時器們初始化
        void TimerAwake()
        {
            lastTime = DateTime.Now;
            moveTimer = new()
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            moveTimer.Tick += MoveTimer_Tick;
            moveTimer.Start();
        }
        void CreateBall()
        {
            ball = ballsLevel switch
            {
                BallsLevel.Small => new Ellipse() { Width = 35, Height = 30, Fill = Brushes.Green },
                BallsLevel.Medium => new Ellipse() { Width = 55, Height = 50, Fill = Brushes.Blue },
                BallsLevel.Large => new Ellipse() { Width = 75, Height = 75, Fill = Brushes.Orange },
            };
            ballCanva.Children.Add(ball);//將球載入畫布中。
            Canvas.SetLeft(ball, 700 - ball.Width);//初始位置，可以跟主程式拿當前城堡的座標
            Canvas.SetTop(ball, 275 - ball.Height);
        }
        private void StartBallsControl(BallsLevel level)
        {
            switch (level)
            {
                case BallsLevel.Small:
                    ballProperties = new(1, 10, 50, 80, 2);
                    break;
                case BallsLevel.Medium:
                    ballProperties = new(2, 20, 150, 50, 6);
                    break;
                case BallsLevel.Large:
                    ballProperties = new(5, 50, 300, 30, 15);
                    break;
            }
        }
        public int GetCD()
        {
            return ballProperties.CD;
        }
        private void MoveTimer_Tick(object? sender, EventArgs e)
        {
            currentTime = DateTime.Now;
            double deltaTime = (currentTime - lastTime).TotalSeconds;
            lastTime = currentTime;
            double newX =Canvas.GetLeft(ball) + -ballProperties.SPEED * deltaTime;
            Canvas.SetLeft(ball,newX);
        }
    }
}
public enum BallsLevel
{
    Small,
    Medium,
    Large
}
public struct BallStruct
{
    public int ID;    
    public int ATK;
    public int HP;
    public int COST;
    public int SPEED;
    public int CD;
    public BallStruct(int atk, int hp, int cost, int speed,int cd)
    {
        ATK = atk;
        HP = hp;
        COST = cost;
        SPEED = speed;
        CD = cd;
    }
}