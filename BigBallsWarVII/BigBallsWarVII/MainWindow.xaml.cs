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
        private readonly DispatcherTimer cdTimer = new();
        private readonly DispatcherTimer cashTimer = new();
        private readonly Stopwatch _stopWatch = new();
        private double elapsedTime;
        private double smallLastTime, mediumLastTime, largeLastTime, triangleLastTime, squareLastTime;//上次生成的時間
        private const double smallCD = 2000, mediumCD = 6000, largeCD = 18000, triangleCD = 5000, squareCD = 22000;//冷卻時間(毫秒)
        private bool isSmallSpawned, isMideumSpawned, isLargeSpawned, isTriangleSpawned, isSquareSpawned;
        private bool isMyBallLimit = false;
        private bool isGameOver = false;
        private readonly MediaPlayer backgroundMusicPlayer;
        private readonly MediaPlayer soundEffectPlayer;

        #endregion
        public MainWindow()
        {
            InitializeComponent();
            isGameOver = true;

            //設定計時器
            cdTimer.Interval = TimeSpan.FromMilliseconds(16);//60FPS
            cdTimer.Tick += CdTimer_Tick;
            cashTimer.Interval = TimeSpan.FromSeconds(.1);
            cashTimer.Tick += CashTimer_Tick;

            myBallsCount.Text = "";
            currentMoney.Text = "";
            isCashEnough.Text = "";
            gameOverLabel.Content = "";
            isGameOverMask.Visibility = Visibility.Hidden;
            restartButton.Visibility = Visibility.Hidden;

            //初始化音樂播放器
            backgroundMusicPlayer = new();
            soundEffectPlayer = new();
        }

        /// <summary>
        /// 遊戲從初始化狀態、結束狀態開始
        /// </summary>
        private void GameStart()
        {
            cdTimer.Start();
            CdReset();
            _stopWatch.Start();
            cashTimer.Start();
            CashInterval = 1;
            moneyUpgrateQuestPrice = 200;

            //我的城堡血量
            BallsManager.isGameOver = false;

            BallsManager.Reset();
            BallsManager.MaxBlueCastleHP = 2000;
            BallsManager.BlueCastleHP = BallsManager.MaxBlueCastleHP;
            BallsManager.BlueCastleChanged += BlueCastleChanged;
            BlueCastleChanged();
            BallsManager.CountChange += ChangeCountText;//我的球體數量改變的事件

            EnemyBallsSpawner.addBallToCanva += AddEnemyToCanva;
            //敵方的城堡血量
            EnemyBallsSpawner.isGameOver = false;

            EnemyBallsSpawner.Reset();
            EnemyBallsSpawner.MaxRedCastleHP = 2000;
            EnemyBallsSpawner.RedCastleHP = EnemyBallsSpawner.MaxRedCastleHP;
            EnemyBallsSpawner.RedCastleChanged += RedCastleChanged;
            RedCastleChanged();
            gameOverLabel.Content = "";

            ChangeCountText();

            //初始化金錢
            CashSystem.Reset();
            howMuchUpgrateText.Text = "200元";

            //設定音樂
            backgroundMusicPlayer.Open(new Uri("Resources/backGroundMusic.wav", UriKind.Relative));//使用相對路徑抓
            //這一段是在描述，當音樂播放完畢以後充新播放。使用拉姆達表示法來執行。
            backgroundMusicPlayer.MediaEnded += (s, e) =>
            {
                backgroundMusicPlayer.Position = TimeSpan.Zero;//音樂重頭播放
                backgroundMusicPlayer.Play();//繼續播放音樂。
            };
            backgroundMusicPlayer.Play();

            isGameOver = false;
        }

        /// <summary>
        /// 負責處理CD的計時器
        /// </summary>
        private void CdTimer_Tick(object? sender, EventArgs e)
        {
            elapsedTime = _stopWatch.ElapsedMilliseconds;//更精準判斷經過的時間。
            UpdateCooldownTime(smallBottonSlider, smallCD, smallLastTime, isSmallSpawned);
            UpdateCooldownTime(mediumBottonSlider, mediumCD, mediumLastTime, isMideumSpawned);
            UpdateCooldownTime(largeBottonSlider, largeCD, largeLastTime, isLargeSpawned);
            UpdateCooldownTime(triangleBottonSlider, triangleCD, triangleLastTime, isTriangleSpawned);
            UpdateCooldownTime(squareBottonSlider, squareCD, squareLastTime, isSquareSpawned);
            if (BallsManager.BallCount >= 15)
            {
                if (!isMyBallLimit)
                {
                    isMyBallLimit = true;
                    myBallsCount.Foreground = Brushes.Red;
                }
            }
            else
            {
                if (isMyBallLimit)
                {
                    isMyBallLimit = false;
                    myBallsCount.Foreground = Brushes.Aquamarine;
                }
            }
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
        private void CdReset()
        {
            smallLastTime = 0;
            mediumLastTime = 0;
            largeLastTime = 0;
            triangleLastTime = 0;
            squareLastTime = 0;
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
        private int moneyUpgrateQuestPrice = 200;
        /// <summary>
        /// 處理金錢增加的計時器
        /// </summary>
        private void CashTimer_Tick(object? sender, EventArgs e)
        {
            CashSystem.IncreaseCash(CashInterval);
            currentMoney.Text = CashSystem.Cash.ToString();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            gameStartBox.Visibility = Visibility.Hidden;
            GameStart();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            //跟遊戲結束相關的UI都要隱藏
            isGameOverMask.Visibility = Visibility.Hidden;
            restartButton.Visibility = Visibility.Hidden;
            GameStart();
        }

        #region 金錢 + 按鈕事件
        /// <summary>
        /// 金錢升級按鈕的點擊事件
        /// </summary>
        private void moneyUpgrateBackGround_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isGameOver) return;
            if (moneyUpgrateQuestPrice >= 600)
            {
                return;
            }
            if (CashSystem.DecreaseCash(moneyUpgrateQuestPrice) == false)
            {
                ShowNotEnoughText();
                return;
            }

            CashInterval++;
            moneyUpgrateQuestPrice = moneyUpgrateQuestPrice + 200;
            if (moneyUpgrateQuestPrice >= 600)
                howMuchUpgrateText.Text = "最高等";
            else
                howMuchUpgrateText.Text = $"{moneyUpgrateQuestPrice}元";
            soundEffectPlayer.Open(new Uri("Resources/moneyUpgrateSound.wav", UriKind.Relative));
            soundEffectPlayer.Play();
        }

        private void smallBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isSmallSpawned || isMyBallLimit || isGameOver)
            {
                soundEffectPlayer.Open(new Uri("Resources/cantDo.wav", UriKind.Relative));
                soundEffectPlayer.Play();
                return;
            }
            if (CashSystem.DecreaseCash(10) == false)
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
            if (isMideumSpawned || isMyBallLimit || isGameOver)
            {
                soundEffectPlayer.Open(new Uri("Resources/cantDo.wav", UriKind.Relative));
                soundEffectPlayer.Play();
                return;
            }
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
            if (isLargeSpawned || isMyBallLimit || isGameOver)
            {
                soundEffectPlayer.Open(new Uri("Resources/cantDo.wav", UriKind.Relative));
                soundEffectPlayer.Play();
                return;
            }
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
            if (isTriangleSpawned || isMyBallLimit || isGameOver)
            {
                soundEffectPlayer.Open(new Uri("Resources/cantDo.wav", UriKind.Relative));
                soundEffectPlayer.Play();
                return;
            }
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
            if (isSquareSpawned || isMyBallLimit || isGameOver)
            {
                soundEffectPlayer.Open(new Uri("Resources/cantDo.wav", UriKind.Relative));
                soundEffectPlayer.Play();
                return;
            }
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
            if (isGameOver) return;

            soundEffectPlayer.Open(new Uri("Resources/cantDo.wav", UriKind.Relative));
            soundEffectPlayer.Play();
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
            myBallsCount.Text = "我方出戰數量：" + BallsManager.BallCount.ToString();
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
            if (EnemyBallsSpawner.RedCastleHP <= 0 && !isGameOver)
            {
                EnemyBallsSpawner.isGameOver = true;
                BallsManager.isGameOver = true;
                GameOver(true);
            }
        }
        void BlueCastleChanged()
        {
            myMaxHPCurrent.Text = BallsManager.MaxBlueCastleHP.ToString();
            myHPCurrent.Text = BallsManager.BlueCastleHP.ToString();
            myHPBar.Width = 120 * (BallsManager.BlueCastleHP / BallsManager.MaxBlueCastleHP);

            if (BallsManager.BlueCastleHP <= 0 && !isGameOver)
            {
                EnemyBallsSpawner.isGameOver = true;
                BallsManager.isGameOver = true;
                GameOver(false);
            }
        }

        /// <summary>
        /// 判定遊戲結束是誰贏誰輸的功能，並且顯示相對應的UI。
        /// </summary>
        /// <param name="isPlayerWin">true則為玩家贏，false則是對方贏。</param>
        void GameOver(bool isPlayerWin)
        {
            if(isGameOver) return;
            isGameOver = true;

            Debug.WriteLine("GameOver了");
            gameOverLabel.Content = isPlayerWin ? "全勝！" : "慘敗！";
            isGameOverMask.Visibility = Visibility.Visible;
            restartButton.Visibility = Visibility.Visible;
            GradientBrush brush = new LinearGradientBrush();
            brush.SetCurrentValue(LinearGradientBrush.StartPointProperty, new Point(0, 0));
            brush.SetCurrentValue(LinearGradientBrush.EndPointProperty, new Point(0, 1));
            brush.GradientStops.Add(new GradientStop((isPlayerWin ? Colors.MediumBlue : Colors.Red), 0));
            brush.GradientStops.Add(new GradientStop((isPlayerWin ? Colors.DarkBlue : Colors.DarkRed), 1));
            gameOverLabel.Foreground = brush;
        }
    }
}
