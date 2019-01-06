using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jakiekieszonkowe_api.Other
{
    public static class DateTimeExtensions
    {
        public static DateTime? NextPaymentDate(this DateTime dateOfPayment)
        {

            if(dateOfPayment.Year <= DateTime.Now.ToLocalTime().Year && dateOfPayment.Month <= DateTime.Now.ToLocalTime().Month &&
                DateTime.Now.ToLocalTime().Day <= dateOfPayment.Day) 
            {
                return dateOfPayment;
            }
            else
            {
                int howManyMonths = ((dateOfPayment.Year - DateTime.Now.ToLocalTime().Year) * 12) + dateOfPayment.Month - DateTime.Now.ToLocalTime().Month;
                if (howManyMonths != 0)
                    return dateOfPayment.AddMonths(howManyMonths);
                else
                {
                    if (DateTime.Now.ToLocalTime().Day <= dateOfPayment.Day )
                        return dateOfPayment;
                    else
                        return dateOfPayment.AddMonths(1);
                }
            }
        }

        public static DateTime? PreviousPaymentDate(this DateTime dateOfPayment)
        {

            if (dateOfPayment.Year <= DateTime.Now.ToLocalTime().Year && dateOfPayment.Month <= DateTime.Now.ToLocalTime().Month &&
                DateTime.Now.ToLocalTime().Day <= dateOfPayment.Day)
            {
                return null;
            }
            else
            {
                return dateOfPayment.NextPaymentDate().Value.AddMonths(-1);
            }
        }
    }
}
