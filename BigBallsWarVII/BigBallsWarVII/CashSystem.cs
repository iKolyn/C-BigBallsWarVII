using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBallsWarVII
{
    public static class CashSystem
    {
        private static int _cash = 3000;
        private const int limitCash = 5000;
        public static int Cash // 顯示金額
        {
            get { return _cash; }
        }
        public static bool DecreaseCash(int value) //如果有成功花錢回傳(true)，反之(false)
        {
            if (value <= _cash) // 錢足夠
            {
                _cash -= value;
                return true;
            }
            else //錢不夠
                return false;
        }
        public static void IncreaseCash(int value) // 收到數值後增加金錢，有上限
        {
            if (_cash == limitCash) // 到剛好達到上現金額
                return;
            if (_cash + value > 5000) // 超過上限金額定為上限
            {
                _cash = 5000;
                return;
            }
            _cash += value;
        }
    }
}
