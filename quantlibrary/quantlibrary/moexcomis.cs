using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quantlibrary
{
    public interface iComission
    {
        double comission(Security sec, int count);
        double comission(Security sec, double weight);
    }

    public class moexcomis : iComission
    {
        private double _moexcomission = 0.04;
        private double _futcomission = 2;
        public double moexcomission
        {
            set { _moexcomission = value; }
            get { return _moexcomission; }
        }

        public double futcomission
        {
            set { _futcomission = value; }
            get { return _futcomission; }
        }

        public double comission(Security sec, int count)
        {
            if(sec.SecureType==SecurityType.Share)
            {
                return sec.Price * count * _moexcomission / 100;
            }
            else if(sec.SecureType == SecurityType.Futures)
            {
                return count * _futcomission;
            }
            return 0;
        }

        public double comission(Security sec, double weight)
        {
            if (sec.SecureType == SecurityType.Share)
            {
                return sec.Price * weight * _moexcomission / 100;
            }
            else if (sec.SecureType == SecurityType.Futures)
            {
                return weight * _futcomission/sec.Price;
            }
            return 0;
        }
    }
}
