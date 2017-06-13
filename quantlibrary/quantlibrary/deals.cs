using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quantlibrary
{
    public class Deal
    {
        public Deal()
        {

        }

        public DateTime datetime;
        public Security security;
        public OperationType operationtype;
        public double price;
        public int? count;
        public double? weight;
    }

    public class Deals:List<Deal>
    {
    }
}
