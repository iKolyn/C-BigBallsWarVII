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
            StartBallsControl(ballsLevel);//賦予球體數值
            CreateBall();//創造球體本身
            BallsManager.AddBall(this);
            Loaded += BallsConrolLoaded;//初始化的時候
        }
        public BallsControl(BallStruct ballStruct)//用於敵人生成
        {
            InitializeComponent();
            StartEnemyBallsControl(ballStruct);//賦予球體數值
            CreateEnemyBall();//創造球體本身
            BallsManager.AddBall(this);
            Loaded += BallsConrolLoaded;//初始化的時候
        }
        private void BallsConrolLoaded(object? sender, RoutedEventArgs e)
        {  
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
        private void StartEnemyBallsControl(BallStruct ballStruct)
        {
            ballProperties = ballStruct;
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
        void CreateEnemyBall()
        {

        }
        private void MoveTimer_Tick(object? sender, EventArgs e)
        {
            currentTime = DateTime.Now;
            double deltaTime = (currentTime - lastTime).TotalSeconds;
            lastTime = currentTime;
            double newX =Canvas.GetLeft(ball) + ballProperties.SPEED * deltaTime;
            Canvas.SetLeft(ball,newX);
            BallsManager.UpdateBallPosition();
            if (Canvas.GetLeft(ball) < 20 - ball.Width)
            {
                BallsManager.RemoveBall(this);
                EndBallsControl();
            }
        }
        private void EndBallsControl()
        {
            if (this.Parent is Panel parentPanel) { parentPanel.Children.Remove(this); }//在整個panel刪除
            moveTimer.Stop();
            moveTimer.Tick -= MoveTimer_Tick;
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
    public double Radius;
    public SolidColorBrush Color;//敵人用的
    public int ATK;
    public int HP;
    public int SPEED;
    public BallStruct(int atk, int hp, int speed,double Radius)
    {
        ATK = atk;
        HP = hp;
        SPEED = speed;
    }
}