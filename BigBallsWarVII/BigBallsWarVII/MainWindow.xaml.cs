using System.Data;
using System.Diagnostics;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace BigBallsWarVII
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer cdTimer = new();
        Stopwatch _stopWatch = new();
        private double elapsedTime;
        private double smallLastTime, mediumLastTime, largeLastTime;//上次生成的時間
        private double smallCD = 2000, mediumCD = 6000, largeCD = 20000;//冷卻時間(毫秒)
        private bool isSmallSpawned, isMideumSpawned, isLargeSpawned;
        
        public MainWindow()
        {
            InitializeComponent();
            cdTimer.Interval = TimeSpan.FromMilliseconds(16);//60FPS
            cdTimer.Tick += CdTimer_Tick;
            cdTimer.Start();
            _stopWatch.Start();
            BallsManager.CountChange += ChangeCountText;
            EnemyBallsSpawner.addBallToCanva += AddEnemyToCanva;
            ChangeCountText();
        }
        /// <summary>
        /// 負責處理CD的計時器
        /// </summary>
        private void CdTimer_Tick(object? sender, EventArgs e)
        {
            elapsedTimeText.Text = EnemyBallsSpawner.ElapsedTime.ToString();
            myFirstBallPosition.Text = BallsManager.FirstBall == null ? "0" : Canvas.GetLeft(BallsManager.FirstBall).ToString();
            enemyFirstBallPosition.Text = EnemyBallsSpawner.FirstBall == null ? "0" : Canvas.GetLeft(EnemyBallsSpawner.FirstBall).ToString();
            elapsedTime = _stopWatch.ElapsedMilliseconds;//更精準判斷經過的時間。
            UpdateCooldownTime(smallBottonSlider, smallCD, smallLastTime, isSmallSpawned);
            UpdateCooldownTime(mediumBottonSlider, mediumCD, mediumLastTime, isMideumSpawned);
            UpdateCooldownTime(largeBottonSlider, largeCD, largeLastTime, isLargeSpawned);
            if (isSmallSpawned && elapsedTime > smallLastTime + smallCD)
            {
                isSmallSpawned = false;
                MessageBox.Show("被關掉了");
            }
            if (isMideumSpawned && elapsedTime > mediumLastTime + mediumCD)
            {
                isMideumSpawned = false;
            }
            if (isLargeSpawned && elapsedTime > largeLastTime + largeCD)
            {
                isLargeSpawned = false;
            }
        }
        private void smallBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isSmallSpawned) return;
            
            isSmallSpawned = true;
            Ball ball = new(BallsLevel.Small);
            smallLastTime = _stopWatch.ElapsedMilliseconds;
            mainCanva.Children.Add(ball);
            BallsManager.AddBall(ball);
        }
        private void mediumBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isMideumSpawned) return;

            isMideumSpawned = true;
            Ball ball = new Ball(BallsLevel.Medium);
            mediumLastTime = _stopWatch.ElapsedMilliseconds;
            mainCanva.Children.Add(ball);
            BallsManager.AddBall(ball);

        }
        private void largeBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isLargeSpawned) return;

            isLargeSpawned = true;
            Ball ball = new Ball(BallsLevel.Large);
            largeLastTime = _stopWatch.ElapsedMilliseconds;
            mainCanva.Children.Add(ball);
            BallsManager.AddBall(ball);
        }
        void UpdateCooldownTime(Rectangle rect, double cd, double lastTime, bool isSpawned)
        {
            double now = _stopWatch.ElapsedMilliseconds;
            if (isSpawned)
            {
                double elapsed = (lastTime + cd) - now;
                double lerp = Math.Max(0, 90 * (elapsed / cd));
                rect.Width = lerp;
            }
            else
            {
                rect.Width = 0;
            }
        }
        void ChangeCountText()
        {
            myBallsCount.Text = BallsManager.BallCount.ToString();
        }
        void AddEnemyToCanva(Ball ball)
        {
            mainCanva.Children.Add(ball);
        }
        void AddToCanva(Ball ball)
        {
            mainCanva.Children.Add(ball);
        }
    }
}