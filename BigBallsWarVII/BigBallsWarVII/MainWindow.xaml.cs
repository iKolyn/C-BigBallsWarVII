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
        #region 數值們
        DispatcherTimer cdTimer = new();
        DispatcherTimer cashTimer = new();
        Stopwatch _stopWatch = new();
        private double elapsedTime;
        private double smallLastTime, mediumLastTime, largeLastTime;//上次生成的時間
        private double smallCD = 2000, mediumCD = 5000, largeCD = 12000;//冷卻時間(毫秒)
        private bool isSmallSpawned, isMideumSpawned, isLargeSpawned;
        
        public int BlueCastleHP
        { 
            get { return _blueCastleHP; }
            set
            {
                enemyHPCurrent.Text = _blueCastleHP.ToString();
                myHPBar.Width = 120 * (_blueCastleHP / maxBlueCastleHP);
            } 
        }
        private int _blueCastleHP;
        private int maxBlueCastleHP = 10000;
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
            BallsManager.BlueCastleChanged += BlueCastleChanged;
            BallsManager.MaxBlueCastleHP = 1000;
            BallsManager.BlueCastleHP = BallsManager.MaxBlueCastleHP;
            
            EnemyBallsSpawner.addBallToCanva += AddEnemyToCanva;
            //敵方的城堡血量
            EnemyBallsSpawner.RedCastleChanged += RedCastleChanged;
            EnemyBallsSpawner.MaxRedCastleHP = 1600;
            EnemyBallsSpawner.RedCastleHP = EnemyBallsSpawner.MaxRedCastleHP;
            ChangeCountText();

            isCashEnough.Text = "";
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
        }
        /// <summary>
        /// 處理金錢增加的計時器
        /// </summary>
        private void CashTimer_Tick(object? sender, EventArgs e)
        {
            CashSystem.IncreaseCash(1);
            currentMoney.Text = CashSystem.Cash.ToString();
        }
        private void smallBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isSmallSpawned) return;
            if(CashSystem.DecreaseCash(10) == false)
            {
                ShowNotEnoughText();
                return;
            }
            isSmallSpawned = true;
            Ball ball = new(BallsLevel.Small);
            smallLastTime = _stopWatch.ElapsedMilliseconds;
            mainCanva.Children.Add(ball);
        }
        private void mediumBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isMideumSpawned) return;
            if (CashSystem.DecreaseCash(75) == false)
            {
                ShowNotEnoughText();
                return;
            }
            isMideumSpawned = true;
            Ball ball = new Ball(BallsLevel.Medium);
            mediumLastTime = _stopWatch.ElapsedMilliseconds;
            mainCanva.Children.Add(ball);
        }
        private void largeBotton_Click(object sender, RoutedEventArgs e)
        {
            if (isLargeSpawned) return;
            if (CashSystem.DecreaseCash(200) == false)
            {
                ShowNotEnoughText();
                return;
            }
            isLargeSpawned = true;
            Ball ball = new Ball(BallsLevel.Large);
            largeLastTime = _stopWatch.ElapsedMilliseconds;
            mainCanva.Children.Add(ball);
        }
        private async void ShowNotEnoughText()
        {
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
                double lerp = Math.Max(0, 90 * (elapsed / cd));
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
            mainCanva.Children.Add(ball);
        }
        void RedCastleChanged()
        {
            enemyHPCurrent.Text = EnemyBallsSpawner.RedCastleHP.ToString();
            enemyMaxHPCurrent.Text = EnemyBallsSpawner.MaxRedCastleHP.ToString();
            enemyHPBar.Width = 120 * (EnemyBallsSpawner.RedCastleHP / EnemyBallsSpawner.MaxRedCastleHP);
        }
        void BlueCastleChanged()
        {
            myMaxHPCurrent.Text = BallsManager.MaxBlueCastleHP.ToString();
            myHPCurrent.Text = BallsManager.BlueCastleHP.ToString();
            myHPBar.Width = 120 * (BallsManager.BlueCastleHP / BallsManager.MaxBlueCastleHP);
        }
    }
}
