using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quantlibrary
{
    public enum SecurityType {
        Money = 0,
        Share = 1,
        Futures = 2,
        Option = 3,
        Rub = 643,  //iso 4217
        USD = 840,  //iso 4217
        Eur = 978,   //iso 4217
    }

    public class Security
    {
        private string seccode = "";
        private string secboard = "";
        private double price = -1;
        private int lotsize = 1;
        private SecurityType securetype = SecurityType.Share;

        private iMarketDataProvider marketdataprovider;

        public SecurityType SecureType
        {
            get { return securetype; }
            set { securetype = value; }
        }

        public string SecCode
        {
            get { return seccode; }
            set { seccode = value; }
        }
        public string SecBoard
        {
            get { return secboard; }
            set { secboard = value; }
        }

        public double Price
        {
            get { return price; }
            set { price = value; }
        }

        public int LotSize
        {
            get { return lotsize; }
            set { lotsize = value; }
        }

        public iMarketDataProvider MarketDataProvider
        {
            get { return marketdataprovider; }
            set { marketdataprovider = value; }
        }
    }
}
