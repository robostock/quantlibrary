using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quantlibrary
{
    public interface iCommission
    {
        string FriendlyName { get; }
        string Description { get; }

        bool SetParameterValue { set; get; }
        double Calculate(SecurityType securityType,OperationType orderType, double orderPrice, int count);
        double Calculate(SecurityType securityType, OperationType orderType, double orderPrice, double weight);
        string ToString();
    }

    public class Commission : iCommission
    {
        Dictionary<string, double> parameters;

        public Commission()
        {
            parameters = new Dictionary<string, double>();

            parameters.Add("MICEX", 0.03);
            parameters.Add("FORTS", 2);
            parameters.Add("MONEY", 0.03);
        }

        public string FriendlyName
        {
            get
            {
                return "MICEX Commissions";
            }
        }

        public string Description
        {
            get
            {
                return "Объект для расчета комиссий при торговле московской бирже";
            }
        }

        public bool SetParameterValue
        {
            set
            {

            }
            get
            {
                return true;
            }
        }


        public double Calculate(SecurityType securityType,OperationType orderType, double orderPrice, int count)
        {
            switch(securityType)
            {
                case SecurityType.Share:
                    return (orderPrice * count * parameters["MICEX"]);
                    //break;
                case SecurityType.Futures:
                    return (count * parameters["FORTS"]);
                    //break;
                case SecurityType.Option:
                    break;
                case SecurityType.Money:
                case SecurityType.Rub:
                case SecurityType.USD:
                case SecurityType.Eur:
                    return (orderPrice * count * parameters["MONEY"]);
                    //break;
                default:
                    return 0;
            }
            return 0;
        }

        public double Calculate(SecurityType securityType, OperationType orderType, double orderPrice, double weight)
        {
            switch (securityType)
            {
                case SecurityType.Share:
                    return (orderPrice * weight * parameters["MICEX"]);
                //break;
                case SecurityType.Futures:
                    return (weight * parameters["FORTS"]);
                //break;
                case SecurityType.Option:
                    break;
                case SecurityType.Money:
                case SecurityType.Rub:
                case SecurityType.USD:
                case SecurityType.Eur:
                    return (orderPrice * weight * parameters["MONEY"]);
                //break;
                default:
                    return 0;
            }
            return 0;
        }

        public override string ToString()
        {
            return String.Format("Комиссия от оборота на фондовом рынке - {0}, на срочном рынке - {1}", parameters["MICEX"], parameters["FORTS"]);
        }
    }
}
