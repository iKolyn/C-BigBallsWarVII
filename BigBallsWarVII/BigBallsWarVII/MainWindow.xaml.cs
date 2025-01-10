using System.Data;
using System.Diagnostics;
using System.Media;
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
        #region 數值們
        DispatcherTimer cdTimer = new();
        DispatcherTimer cashTimer = new();
        Stopwatch _stopWatch = new();
        private double elapsedTime;
        private double smallLastTime, mediumLastTime, largeLastTime, triangleLastTime, squareLastTime;//上次生成的時間
        private double smallCD = 2000, mediumCD = 6000, largeCD = 18000, triangleCD = 5000, squareCD = 22000;//冷卻時間(毫秒)
        private bool isSmallSpawned, isMideumSpawned, isLargeSpawned, isTriangleSpawned, isSquareSpawned;
        private bool isGameOver = false;
        private MediaPlayer backgroundMusicPlayer;
        private MediaPlayer soundEffectPlayer;

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            cdTimer.Interval = TimeSpan.FromMilliseconds(16);//60FPS
            cdTimer.Tick += CdTimer_Tick;
            cdTimer.Start();
            _stopWatch.Start();
            cashTimer.Interval = TimeSpan.FromSeconds(.1);
            cashTimer.Tick += CashTimer_Tick;
            cashTimer.Start();

            BallsManager.CountChange += ChangeCountText;
            //我的城堡血量
            BallsManager.MaxBlueCastleHP = 3000;
            BallsManager.BlueCastleHP = BallsManager.MaxBlueCastleHP;
            BallsManager.BlueCastleChanged += BlueCastleChanged;
            BlueCastleChanged();

            EnemyBallsSpawner.addBallToCanva += AddEnemyToCanva;
            //敵方的城堡血量
            EnemyBallsSpawner.MaxRedCastleHP = 3000;
            EnemyBallsSpawner.RedCastleHP = EnemyBallsSpawner.MaxRedCastleHP;
            EnemyBallsSpawner.RedCastleChanged += RedCastleChanged;
            RedCastleChanged();

            ChangeCountText();

            isCashEnough.Text = "";
            gameOverLabel.Content = "";

            backgroundMusicPlayer = new();
            backgroundMusicPlayer.Open(new Uri("Resources/backGroundMusic.wav", UriKind.Relative));//使用相對路徑抓
            //這一段是在描述，當音樂播放完畢以後充新播放。使用拉姆達表示法來執行。
            backgroundMusicPlayer.MediaEnded += (s, e) =>
            {
                backgroundMusicPlayer.Position = TimeSpan.Zero;//音樂重頭播放
                backgroundMusicPlayer.Play();//繼續播放音樂。
            };
            backgroundMusicPlayer.Play();

            soundEffectPlayer = new();
        }
        /// <summary>
        /// 負責處理CD的計時器
        /// </summary>
        private void CdTimer_Tick(object? sender, EventArgs e)
        {
            elapsedTimeText.Text = EnemyBallsSpawner.ElapsedTime.ToString();    
            elapsedTime = _stopWatch.ElapsedMilliseconds;//更精準判斷經過的時間。
            UpdateCooldownTime(smallBottonSlider, smallCD, smallLastTime, isSmallSpawned);
            UpdateCooldownTime(mediumBottonSlider, mediumCD, mediumLastTime, isMideumSpawned);
            UpdateCooldownTime(largeBottonSlider, largeCD, largeLastTime, isLargeSpawned);
            UpdateCooldownTime(triangleBottonSlider, triangleCD, triangleLastTime, isTriangleSpawned);
            UpdateCooldownTime(squareBottonSlider, squareCD, squareLastTime, isSquareSpawned);
            if (isSmallSpawned && elapsedTime > smallLastTime + smallCD)
            {
                isSmallSpawned = false;
            }
            if (isMideumSpawned && elapsedTime > mediumLastTime + mediumCD)
            {
                isMideumSpawned = false;
            }
            if (isLargeSpawned && elapsedTime > largeLastTime + largeCD)
            {
                isLargeSpawned = false;
            }
            if (isTriangleSpawned && elapsedTime > triangleLastTime + triangleCD)
            {
                isTriangleSpawned = false;
            }
            if (isSquareSpawned && elapsedTime > squareLastTime + squareCD)
            {
                isSquareSpawned = false;
            }
        }
        /// <summary>
        /// 封裝用，怕CashTimer沒得到設定
        /// </summary>
        public int CashInterval
        {
            get { return _cashInterval; }
            set
            {
                _cashInterval = value;
            }
        }
        private int _cashInterval = 1;
        private int moneyUpgrateQuestPrice = 150;
        /// <summary>
        /// 處理金錢增加的計時器
        /// </summary>
        private void CashTimer_Tick(object? sender, EventArgs e)
        {
            CashSystem.IncreaseCash(CashInterval);
            currentMoney.Text = CashSystem.Cash.ToString();
        }
        #region 金錢 + 按鈕事件
        /// <summary>
        /// 金錢升級按鈕的點擊事件
        /// </summary>
        private void moneyUpgrateBackGround_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(isGameOver) return;
            if (moneyUpgrateQuestPrice >= 450) 
            {
                return;
            }   
            if(CashSystem.DecreaseCash(moneyUpgrateQuestPrice) == false)
            {
                ShowNotEnoughText();
                return;
            }
            else
            {
                CashInterval ++;
                moneyUpgrateQuestPrice = moneyUpgrateQuestPrice + 150;
                if(moneyUpgrateQuestPrice >= 450)
                    howMuchUpgrateText.Text = "最高等";
                else
                    howMuchUpgrateText.Text = $"{moneyUpgrateQuestPrice}元";
                soundEffectPlayer.Open(new Uri("Resources/moneyUpgrateSound.wav", UriKind.Relative));
                soundEffectPlayer.Play();
            }
        }

        private void smallBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isSmallSpawned || isGameOver) return;
            if(CashSystem.DecreaseCash(10) == false)
            {
                ShowNotEnoughText();
                return;
            }
            isSmallSpawned = true;
            Ball ball = new(BallsLevel.Small);
            smallLastTime = _stopWatch.ElapsedMilliseconds;
            mainCanva.Children.Add(ball);

            soundEffectPlayer.Open(new Uri("Resources/clickSound.wav", UriKind.Relative));
            soundEffectPlayer.Play();
        }
        private void mediumBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isMideumSpawned || isGameOver) return;
            if (CashSystem.DecreaseCash(75) == false)
            {
                ShowNotEnoughText();
                return;
            }
            isMideumSpawned = true;
            Ball ball = new Ball(BallsLevel.Medium);
            mediumLastTime = _stopWatch.ElapsedMilliseconds;
            mainCanva.Children.Add(ball);

            soundEffectPlayer.Open(new Uri("Resources/clickSound.wav", UriKind.Relative));
            soundEffectPlayer.Play();
        }
        private void largeBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isLargeSpawned || isGameOver) return;
            if (CashSystem.DecreaseCash(250) == false)
            {
                ShowNotEnoughText();
                return;
            }
            isLargeSpawned = true;
            Ball ball = new Ball(BallsLevel.Large);
            largeLastTime = _stopWatch.ElapsedMilliseconds;
            mainCanva.Children.Add(ball);

            soundEffectPlayer.Open(new Uri("Resources/clickSound.wav", UriKind.Relative));
            soundEffectPlayer.Play();
        }
        private void triangleBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isTriangleSpawned || isGameOver) return;
            if (CashSystem.DecreaseCash(30) == false)
            {
                ShowNotEnoughText();
                return;
            }
            isTriangleSpawned = true;
            Ball ball = new Ball(BallsLevel.Triangle);
            triangleLastTime = _stopWatch.ElapsedMilliseconds;
            mainCanva.Children.Add(ball);

            soundEffectPlayer.Open(new Uri("Resources/clickSound.wav", UriKind.Relative));
            soundEffectPlayer.Play();
        }
        private void squareBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isSquareSpawned || isGameOver) return;
            if (CashSystem.DecreaseCash(150) == false)
            {
                ShowNotEnoughText();
                return;
            }
            isSquareSpawned = true;
            Ball ball = new Ball(BallsLevel.Square);
            squareLastTime = _stopWatch.ElapsedMilliseconds;
            mainCanva.Children.Add(ball);

            soundEffectPlayer.Open(new Uri("Resources/clickSound.wav", UriKind.Relative));
            soundEffectPlayer.Play();
        }
        #endregion
        private async void ShowNotEnoughText()
        {
            if(isGameOver) return;

            isCashEnough.Text = "錢不夠！";
            await Task.Delay(2000);
            isCashEnough.Text = "";
        }
        void UpdateCooldownTime(Rectangle rect, double cd, double lastTime, bool isSpawned)
        {
            double now = _stopWatch.ElapsedMilliseconds;
            if (isSpawned)
            {
                double elapsed = (lastTime + cd) - now;
                double lerp = Math.Max(0, 100 * (elapsed / cd));
                rect.Width = lerp;
            }
            else rect.Width = 0;
        }
        void ChangeCountText()
        {
            myBallsCount.Text = BallsManager.BallCount.ToString();
        }
        void AddEnemyToCanva(Ball ball)
        {
            //如果球已經加到主畫面上了，就別再加了。
            if (ball.Parent != null)
            {
                return;
            }
            mainCanva.Children.Add(ball);
        }
        void RedCastleChanged()
        {
            enemyHPCurrent.Text = EnemyBallsSpawner.RedCastleHP.ToString();
            enemyMaxHPCurrent.Text = EnemyBallsSpawner.MaxRedCastleHP.ToString();
            enemyHPBar.Width = 120 * (EnemyBallsSpawner.RedCastleHP / EnemyBallsSpawner.MaxRedCastleHP);
            if (EnemyBallsSpawner.RedCastleHP <= 0)
            {
                isGameOver = true;
                EnemyBallsSpawner.isGameOver = true;
                BallsManager.isGameOver = true;
                gameOverLabel.Content = "你贏了！";
                GradientBrush brush = new LinearGradientBrush();
                brush.SetCurrentValue(LinearGradientBrush.StartPointProperty, new Point(0, 0));
                brush.SetCurrentValue(LinearGradientBrush.EndPointProperty, new Point(0, 1));
                brush.GradientStops.Add(new GradientStop(Colors.MediumBlue, 0));
                brush.GradientStops.Add(new GradientStop(Colors.DarkBlue, 1));
                gameOverLabel.Foreground = brush;
            }
        }
        void BlueCastleChanged()
        {
            myMaxHPCurrent.Text = BallsManager.MaxBlueCastleHP.ToString();
            myHPCurrent.Text = BallsManager.BlueCastleHP.ToString();
            myHPBar.Width = 120 * (BallsManager.BlueCastleHP / BallsManager.MaxBlueCastleHP);

            if(BallsManager.BlueCastleHP <= 0)
            {
                isGameOver = true;
                EnemyBallsSpawner.isGameOver = true;
                BallsManager.isGameOver = true;
                gameOverLabel.Content = "你輸了！";
                GradientBrush brush = new LinearGradientBrush();
                brush.SetCurrentValue(LinearGradientBrush.StartPointProperty, new Point(0, 0));
                brush.SetCurrentValue(LinearGradientBrush.EndPointProperty, new Point(0, 1));
                brush.GradientStops.Add(new GradientStop(Colors.Red, 0));
                brush.GradientStops.Add(new GradientStop(Colors.DarkRed, 1));
                gameOverLabel.Foreground = brush;
            }
        }
    }
}
