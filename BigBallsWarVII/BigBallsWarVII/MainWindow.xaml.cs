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

namespace BigBallsWarVII
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer cdTimer = new();
        private DateTime lastSpawnTime;//上次生成的時間
        public MainWindow()
        {
            InitializeComponent();
            cdTimer.Interval = TimeSpan.FromMilliseconds(16);
            cdTimer.Tick += CdTimer_Tick;
        }
        /// <summary>
        /// 負責處理CD的計時器
        /// </summary>
        private void CdTimer_Tick(object? sender, EventArgs e)
        {
            
        }
        private void smallBotton_Click(object sender, RoutedEventArgs e)
        {
            BallsControl ball = new BallsControl(BallsLevel.Small);
            mainCanva.Children.Add(ball);
        }
        private void mediumBotton_Click(object sender, RoutedEventArgs e)
        {
            BallsControl ball = new BallsControl(BallsLevel.Medium);
            mainCanva.Children.Add(ball);
        }
        private void largeBotton_Click(object sender, RoutedEventArgs e)
        {
            BallsControl ball = new BallsControl(BallsLevel.Large);
            mainCanva.Children.Add(ball);
        }
    }
}