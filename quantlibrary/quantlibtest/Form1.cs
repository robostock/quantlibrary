using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using quantlibrary;

namespace quantlibtest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] deals = System.IO.File.ReadAllLines(@"D:\Temp\wsdeals.txt",Encoding.Unicode);
            List<string> equity = new List<string>();

            Deals alldeals = new Deals();
            Portfolio port = new Portfolio("testportfolio");
            TXTMarektDataProvider txtmdp = new TXTMarektDataProvider(@"D:\Market Data\WallStreet");

            for (int i = 1; i < deals.Length; i++)
            {
                //Position;Symbol;Quantity;Entry Date;Entry Price;Exit Date;Exit Price;
                //Short;TRNFP_2016_5m;49;14.01.2016 14:50;201 600,00;19.01.2016 11:50;211 400,00;-4,92;-486 271,10 ?;268;-1 814,44 ?;sell at limit;stop buy at limit;-4,89;-0,03
                string[] items = deals[i].Split('\t');// ';');
                DateTime datetime = DateTime.Parse(items[3]);
                DateTime datetimeclose = DateTime.Parse(items[5]);

                alldeals.Add(new Deal()
                {
                    datetime = datetimeclose,
                    security = new Security() { SecCode = items[1], Price = Double.Parse(items[4].Replace(" ", "")), MarketDataProvider=txtmdp },
                    count = 0,
                    price = Double.Parse(items[4].Replace(" ", "")),
                    operationtype = items[0] == "Short" ? OperationType.Sell : OperationType.Buy
                });

                alldeals.Add(new Deal()
                {
                    datetime = datetimeclose,
                    security = new Security() { SecCode = items[1], Price = Double.Parse(items[6].Replace(" ", "")), MarketDataProvider = txtmdp },
                    count = 0,
                    price = Double.Parse(items[6].Replace(" ", "")),
                    operationtype = items[0] == "Short" ? OperationType.Buy : OperationType.Sell
                });

            }

            DateTime dtmin = alldeals.Min(p => p.datetime);
            DateTime dtmax = alldeals.Max(p=>p.datetime);

            dtmin = dtmin.Date.AddDays(-1) + new TimeSpan(0, 18, 40, 0);
            dtmax = dtmax.Date.AddDays(1) + new TimeSpan(0, 18, 40, 0);

            for (DateTime dt=dtmin;dt<=dtmax;dt = dt.AddDays(1))
            {
                List<Deal> dealitems = alldeals.Where(p => p.datetime.Date == dt.Date).ToList();
                foreach(Deal deal in dealitems)
                {
                    double liquidationvalue = port.LiquidationValue();
                    int count = (int)Math.Floor(( liquidationvalue / deal.price/8.33));

                    if (port.CurrentPositionCount(deal.security.SecCode) == 0)
                    {
                        deal.count = count;
                    }
                    else
                        deal.count = Math.Abs(port.CurrentPositionCount(deal.security.SecCode));

                    port.NewDeal(deal.datetime, deal.security, deal.price, deal.count.Value, deal.operationtype);
                }

                equity.Add(dt.ToString()+";"+port.LiquidationValue(dt).ToString());
            }

            /*for(int i=1;i<deals.Length;i++)
            {
                //Position;Symbol;Quantity;Entry Date;Entry Price;Exit Date;Exit Price;
                //Short;TRNFP_2016_5m;49;14.01.2016 14:50;201 600,00;19.01.2016 11:50;211 400,00;-4,92;-486 271,10 ?;268;-1 814,44 ?;sell at limit;stop buy at limit;-4,89;-0,03
                string[] items = deals[i].Split(';');
                DateTime datetime = DateTime.Parse(items[3]);

                int count = (int) Math.Floor((port.LiquidationValue() / Double.Parse(items[4].Replace(" ", ""))));
                count = int.Parse(items[2]);

                port.NewDeal(datetime, new Security() { SecCode = items[1], Price = Double.Parse(items[4].Replace(" ", "")) }, Double.Parse(items[4].Replace(" ","")), count, items[0] == "Short" ? OperationType.Sell : OperationType.Buy);

                datetime = DateTime.Parse(items[5]);
                port.NewDeal(datetime, new Security() { SecCode = items[1], Price = Double.Parse(items[6].Replace(" ", "")) }, Double.Parse(items[6].Replace(" ", "")), count, items[0] == "Short" ? OperationType.Buy : OperationType.Sell);

                equity.Add(count.ToString()+";"+port.LiquidationValue().ToString());
            }*/

            System.IO.File.WriteAllLines(@"d:\eq.txt",equity.ToArray());
            //TXTMarektDataProvider mdp = new TXTMarektDataProvider(@"D:\Market Data\Российские рынки\micex 5min");
            button1.Text = port.GetPortfolioItems().ToArray()[12].security.SecCode;
            this.Text = port.GetPortfolioItems().ToArray()[12].count.ToString();
        }
    }
}
