using System.Data;
using System.Numerics;
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
        private DateTime smallLastTime, mediumLastTime, largeLastTime;//上次生成的時間
        private double smallCD = 2, mediumCD = 6, largeCD = 20;//冷卻時間
        private bool isSmallSpawned, isMideumSpawned, isLargeSpawned;
        public MainWindow()
        {
            InitializeComponent();
            cdTimer.Interval = TimeSpan.FromMilliseconds(16);//60FPS
            cdTimer.Tick += CdTimer_Tick;
            cdTimer.Start();
        }
        /// <summary>
        /// 負責處理CD的計時器
        /// </summary>
        private void CdTimer_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            UpdateCooldownTime(smallBottonSlider, smallCD, smallLastTime, now, isSmallSpawned);
            UpdateCooldownTime(mediumBottonSlider, mediumCD, mediumLastTime, now, isMideumSpawned);
            UpdateCooldownTime(largeBottonSlider, largeCD, largeLastTime, now, isLargeSpawned);
            if (isSmallSpawned && now > smallLastTime)
            {
                isSmallSpawned = false;
            }
            if (isMideumSpawned && now > mediumLastTime)
            {
                isMideumSpawned = false;
            }
            if (isLargeSpawned && now > largeLastTime)
            {
                isLargeSpawned = false;
            }
        }
        private void smallBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isSmallSpawned) return;
            
            isSmallSpawned = true;
            BallsControl ball = new(BallsLevel.Small);
            smallLastTime = DateTime.Now + TimeSpan.FromSeconds(smallCD);
            mainCanva.Children.Add(ball);
        }
        private void mediumBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isMideumSpawned) return;

            isMideumSpawned = true;
            BallsControl ball = new BallsControl(BallsLevel.Medium);
            mediumLastTime = DateTime.Now + TimeSpan.FromSeconds(mediumCD);
            mainCanva.Children.Add(ball);

        }
        private void largeBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isLargeSpawned) return;

            isLargeSpawned = true;
            BallsControl ball = new BallsControl(BallsLevel.Large);
            largeLastTime = DateTime.Now + TimeSpan.FromSeconds(largeCD);
            mainCanva.Children.Add(ball);
        }
        void UpdateCooldownTime(Rectangle rect, double cd,DateTime lastTime,DateTime now,bool isSpawned)
        {
            if (isSpawned)
            {
                double elapsedTime = (lastTime - now).TotalSeconds;
                double newID = Math.Max(0, 90 * (elapsedTime / cd));
                rect.Width = newID;
            }
            else
            {
                rect.Width = 0;
            }
        }
    }
}