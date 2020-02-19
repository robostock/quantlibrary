using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quantlibrary
{
    public enum OperationType
    {
        Buy = 1,
        Sell = -1
    }

    public class PortfolioItem
    {
        public DateTime datetime { get; set; }
        public Security security { get; set; }
        public int? count { get; set; }
        public double? weight { get; set; }
    }

    public class Portfolio
    {
        private List<PortfolioItem> items = new List<PortfolioItem>();
        private string name = "defaultportfolio";
        private double money;
        private double money_weight;
        private iComission comission;

        public Portfolio(string name)
        {
            this.name = name;
            money = 10000000;
            money_weight = 1.0;
        }
        public Portfolio(string name, double startmoney)
        {
            this.name = name;
            money = startmoney;
        }

        public iComission Comission
        {
            set { comission = value; }
            get { return comission; }
        }
        public void NewDeal(DateTime datetime, Security sec, double price, int count, OperationType operationtype)
        {
            PortfolioItem pi = new PortfolioItem
            {
                datetime = datetime,
                security = sec,
                count = count*(int)operationtype
            };

            double comis = 0;
            if(comission!=null)
            {
                comis = comission.comission(sec, count);
            }

            if(items.Count(i=>i.security.SecCode==sec.SecCode)==0)
            {
                items.Add(pi);
                money -= count * price * (int)operationtype - comis;
            }
            else
            {
                var itm = items.Find(p => p.security.SecCode == sec.SecCode);
                if (itm.count.HasValue)
                {
                    itm.count = itm.count + (int)operationtype * count;
                    itm.datetime = datetime;
                    itm.security.Price = price;

                    money -= count * price * (int)operationtype - comis;
                }
                else
                {
                    throw new Exception("Попытка добавить новую сделку в портфель исчисляемый количеством, а не в долях!");
                }
            }
        }

        public void NewDeal(DateTime datetime, Security sec, double price, double weigth, OperationType operationtype)
        {
            PortfolioItem pi = new PortfolioItem
            {
                datetime = datetime,
                security = sec,
                weight = weigth
            };

            double comis = 0;
            if (comission != null)
            {
                comis = comission.comission(sec, weigth);
            }

            if (items.Count == 0)
            {
                items.Add(pi);
            }
            else
            {
                var itm = items.Find(p => p.security.SecCode == sec.SecCode);
                if (itm.weight.HasValue)
                {
                    itm.weight = itm.count + (int)operationtype * weigth;
                    itm.datetime = datetime;
                    itm.security.Price = price;

                    money_weight -= weigth - comis;
                }
                else
                {
                    throw new Exception("Попытка добавить новую сделку в портфель исчисляемый в долях, а не в количестве!");
                }
            }
        }

        public int CurrentPositionCount(string SecCode)
        {
            var itm = items.Find(p => p.security.SecCode == SecCode);
            if (itm == null)
                return 0;

            if (itm.count.HasValue)
            {
                return itm.count.Value;
            }

            return 0;
        }

        public List<PortfolioItem> GetPortfolioItems()
        {
            List<PortfolioItem> res = new List<PortfolioItem>();
            res.AddRange(items);
            res.Add(new PortfolioItem() { security = new Security() { LotSize = 1, Price = 1, SecCode = "Money" }, count = (int)money, weight = money_weight });
            return res;
        }

        public double LiquidationValue()
        {
            if (items.Count ==0 )
            {
                //необходимо добавить проверку на тип портфеля. абсолютные значения или относительные
                return money;
            }
            DateTime actualitemdate = items.OrderByDescending(p => p.datetime).First().datetime;
            double retval = money;
            double retval_weight = money_weight;
            bool ret_absolutevalue = false;
            foreach (PortfolioItem pi in items)
            {
                if(pi.security.MarketDataProvider!=null)
                {
                    if (pi.count.HasValue)
                    {
                        retval += pi.security.MarketDataProvider.GetSecurityPrice(actualitemdate,/*ref*/ pi.security) * pi.count.Value;
                        ret_absolutevalue = true;
                    }
                    else if(pi.weight.HasValue)
                    {
                        retval_weight += pi.security.MarketDataProvider.GetSecurityPrice(actualitemdate, /*ref*/ pi.security) * pi.weight.Value;
                    }

                }
                else
                {
                    if(pi.count.HasValue)
                    {
                        ret_absolutevalue = true;
                    }
                }
            }

            if (ret_absolutevalue)
                return retval;
            return retval_weight;
        }

        public double LiquidationValue(DateTime LiquidationDateTime)
        {
            if (items.Count == 0)
            {
                //необходимо добавить проверку на тип портфеля. абсолютные значения или относительные
                return money;
            }
            //DateTime actualitemdate = items.OrderByDescending(p => LiquidationDateTime).First().datetime;
            double retval = money;
            double retval_weight = money_weight;
            bool ret_absolutevalue = false;
            foreach (PortfolioItem pi in items)
            {
                if (pi.security.MarketDataProvider != null)
                {
                    if (pi.count.HasValue)
                    {
                        double price = pi.security.MarketDataProvider.GetSecurityPrice(LiquidationDateTime, /*ref*/ pi.security);

                        retval +=  price * pi.count.Value;
                        ret_absolutevalue = true;
                    }
                    else if (pi.weight.HasValue)
                    {
                        retval_weight += pi.security.MarketDataProvider.GetSecurityPrice(LiquidationDateTime, /*ref*/ pi.security) * pi.weight.Value;
                    }

                }
                else
                {
                    if (pi.count.HasValue)
                    {
                        ret_absolutevalue = true;
                    }
                }
            }

            if (ret_absolutevalue)
                return retval;
            return retval_weight;
        }
    }
}
