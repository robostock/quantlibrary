﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace quantlibrary
{
    public struct DOHLCV
    {
        public DateTime Date;
        public DateTime Time;
        //тип данных о ценнах место для оптимизации
        //в 64-битных системах оптимизации по памяти, но не по скорости
        public double Open;
        public double High;
        public double Low;
        public double Close;
        public double Volume;
    }

    public struct MarketDataItem
    {
        public string name;
        public DOHLCV[] bars;
    }

    public delegate void MarketDataUpdate(ref Security security, ref DOHLCV dohlcv);

    public interface iMarketDataProvider
    {
        bool SubscribeMarketData(ref Security security,int timeframe = 5);
        double GetSecurityPrice(DateTime datetime,/*ref*/ Security security);
        event MarketDataUpdate MarketDataUpdateEvent; 
    }

    public class TXTMarektDataProvider : iMarketDataProvider
    {
        private MarketDataUpdate marketdateupdateevent;
        private string marketdatapath = "";
        private char separator = ',';
        private char point = '.';
        private string[] files;
        private string dateformat = "MM/dd/yyyy"/*"yyyyMMdd"*/, timeformat = /*"HHmmss"*/"HH:mm";
        private bool ignorefirstline = true, ignorelastline = false;

        private List<MarketDataItem> mditems = new List<MarketDataItem>();

        private bool writedebuginfo = false;

        public TXTMarektDataProvider(string path)
        {
            if (Directory.Exists(path))
            {
                marketdatapath = path;
                files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    MarketDataItem mdi = new MarketDataItem();
                    mdi.name = Path.GetFileNameWithoutExtension(file);
                    mdi.bars = new DOHLCV[0];

                    string[] lines = File.ReadAllLines(file);

                    string decimal_sep = System.Globalization.NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;
                    Array.Resize(ref mdi.bars, lines.Length);

                    for (int i = ignorefirstline ? 1 : 0; i < (ignorelastline ? (lines.Length - 1) : lines.Length); i++)
                    {
                        string[] items = lines[i].Split(separator);
                        //Array.Resize(ref mdi.bars, mdi.bars.Length + 1);
                        mdi.bars[i/*mdi.bars.Length - 1*/].Date = DateTime.ParseExact(items[0], dateformat, CultureInfo.InvariantCulture);
                        mdi.bars[i/*mdi.bars.Length - 1*/].Time = DateTime.ParseExact(items[1], timeformat, CultureInfo.InvariantCulture);
                        
                        mdi.bars[i/*mdi.bars.Length - 1*/].Open = double.Parse(items[2].Replace(point.ToString(), decimal_sep));
                        mdi.bars[i/*mdi.bars.Length - 1*/].High = double.Parse(items[3].Replace(point.ToString(), decimal_sep));
                        mdi.bars[i/*mdi.bars.Length - 1*/].Low = double.Parse(items[4].Replace(point.ToString(), decimal_sep));
                        mdi.bars[i/*mdi.bars.Length - 1*/].Close = double.Parse(items[5].Replace(point.ToString(), decimal_sep));
                        mdi.bars[i/*mdi.bars.Length - 1*/].Volume = double.Parse(items[6].Replace(point.ToString(), decimal_sep));
                    }
                    mditems.Add(mdi);
                }
            }
        }

        public TXTMarektDataProvider(string path, char separator, char point, string dateformat, string timeformat, bool ignorefirstline = true, bool ignorelastline = false)
        {
            this.separator = separator;
            this.point = point;
            this.dateformat = dateformat;
            this.timeformat = timeformat;
            this.ignorefirstline = ignorefirstline;
            this.ignorelastline = ignorelastline;


        }

        public event MarketDataUpdate MarketDataUpdateEvent
        {
            add
            {
                marketdateupdateevent += value;
            }

            remove
            {
                marketdateupdateevent -= value;
            }
        }

        public int myBinarySearch(DOHLCV[] list, DateTime dt)
        {
            if (list.Length == 0)
                throw new InvalidOperationException("Item not found");

            int inttime = 0;
            int intmidtime = 0;

            int min = 0;
            int max = list.Length-1;
            int mid=0;
            while (min<max)
            {
                mid = min + ((max - min) / 2);
                DOHLCV midItem = list[mid];

                inttime = dt.Hour * 1000 + dt.Minute;
                intmidtime = midItem.Time.Hour * 1000 + midItem.Time.Minute;

                if((dt.Date<midItem.Date))
                {
                    max = mid - 1;
                }
                else if((dt.Date>midItem.Date))
                {
                    min = mid + 1;
                }
                else                
                {
                    if (inttime < intmidtime)
                    {
                        max = mid - 1;
                    }
                    else if (inttime > intmidtime)
                    {
                        min = mid + 1;
                    }
                    else
                        return mid;
                }
            }
            inttime = dt.Hour * 1000 + dt.Minute;
            intmidtime = list[mid].Time.Hour * 1000 + list[mid].Time.Minute;
            if (min == max &&
                min < (list.Length-1) &&
                ((dt.Date == list[mid].Date) && (inttime == intmidtime) ))
            {
                return min;
            }
            return -1;
        }

        public double GetSecurityPrice(DateTime datetime, /*ref*/ Security security)
        {
            List<string> ll = new List<string>();
            int year = datetime.Year;
            foreach (MarketDataItem mdi in mditems)
            {
                if (mdi.name.Contains(security.SecCode))
                {                    
                    int idx = myBinarySearch(mdi.bars, datetime);

                    if (idx > 0)
                    {
                        double price = mdi.bars[idx].Close;
                        if (writedebuginfo)
                        {
                            ll.Add(datetime.ToString() + ";" + price.ToString());
                            System.IO.File.AppendAllLines(@"d:\ll.txt", ll.ToArray());
                        }
                        return price;
                    }
                    else
                    {
                        DateTime tmpdatetime = datetime;
                        for (int c = 1; c < 20; c++)
                        {
                            tmpdatetime = tmpdatetime.AddDays(-1);
                            //idx = Array.BinarySearch(mdi.bars, tmpdatetime);
                            idx = myBinarySearch(mdi.bars, tmpdatetime);

                            if (idx > 0)
                            {
                                double price = mdi.bars[idx].Close; 

                                if (writedebuginfo)
                                {
                                    ll.Add(datetime.ToString() + ";" + price.ToString());
                                    System.IO.File.AppendAllLines(@"d:\ll.txt", ll.ToArray());
                                }

                                return price;
                            }
                        }
                        return 0;
                    }
                }
            }
            return 0;
        }

        public double _GetSecurityPrice(DateTime datetime, /*ref*/ Security security)
        {
            List<string> ll = new List<string>();
            int year = datetime.Year;
            foreach (MarketDataItem mdi in mditems)
            {
                if (mdi.name.Contains(security.SecCode))
                {
                    var tmp = mdi.bars.Where(p => p.Date == datetime.Date && p.Time.TimeOfDay == datetime.TimeOfDay);
                    /*int idx = Array.BinarySearch(mdi.bars, new DOHLCV() { Date = datetime });
                    //tmp = (new IEnumerable<quantlibrary.DOHLCV>()).Add(mdi.bars.ElementAt(idx));/**/

                    if (tmp.Count() > 0)
                    {
                        double price = tmp.First().Close;
                        if (writedebuginfo)
                        {
                            ll.Add(datetime.ToString() + ";" + price.ToString());
                            System.IO.File.AppendAllLines(@"d:\ll.txt", ll.ToArray());
                        }
                        return price;
                    }
                    else
                    {
                        DateTime tmpdatetime = datetime;
                        for(int c=1;c<20;c++)
                        {
                            tmpdatetime = tmpdatetime.AddDays(-1);
                            tmp = mdi.bars.Where(p => p.Date == tmpdatetime.Date && p.Time.TimeOfDay == tmpdatetime.TimeOfDay);
                            
                            if (tmp.Count() > 0)
                            {
                                double price = tmp.First().Close;

                                if (writedebuginfo)
                                {
                                    ll.Add(datetime.ToString() + ";" + price.ToString());
                                    System.IO.File.AppendAllLines(@"d:\ll.txt", ll.ToArray());
                                }

                                return price;
                            }
                        }
                        return 0;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// подписка к маркет дате.
        /// таймфрейм указывается в минутах
        /// </summary>
        /// <param name="security"></param>
        /// <param name="timeframe"></param>
        /// <returns></returns>
        public bool SubscribeMarketData(ref Security security,int timeframe = 5) 
        {
            return false;
        }
    }
}
