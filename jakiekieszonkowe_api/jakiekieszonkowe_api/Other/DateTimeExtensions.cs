﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jakiekieszonkowe_api.Other
{
    public static class DateTimeExtensions
    {
        public static DateTime? NextPaymentDate(this DateTime dateOfPayment, int periodTimeInDays)
        {

            if(dateOfPayment.Year <= DateTime.Now.ToLocalTime().Year && dateOfPayment.Month <= DateTime.Now.ToLocalTime().Month &&
                DateTime.Now.ToLocalTime().Day <= dateOfPayment.Day) 
            {
                return dateOfPayment;
            }
            else
            {
                int amountOfLoops = (int)(DateTime.Now - dateOfPayment).TotalDays;
                amountOfLoops /= periodTimeInDays;
                amountOfLoops++;
                return dateOfPayment.AddDays(amountOfLoops * periodTimeInDays);
                //int howManyMonths = ((DateTime.Now.ToLocalTime().Year - dateOfPayment.Year) * 12) + (DateTime.Now.ToLocalTime().Month - dateOfPayment.Month);
                //if (howManyMonths != 0)
                //    return dateOfPayment.AddMonths(howManyMonths);
                //else
                //{
                //    if (DateTime.Now.ToLocalTime().Day <= dateOfPayment.Day )
                //        return dateOfPayment;
                //    else
                //        return dateOfPayment.AddMonths(1);
                //}
            }
        }

        public static DateTime? PreviousPaymentDate(this DateTime dateOfPayment, int periodTimeInDays)
        {

            if (dateOfPayment.Year <= DateTime.Now.ToLocalTime().Year && dateOfPayment.Month <= DateTime.Now.ToLocalTime().Month &&
                DateTime.Now.ToLocalTime().Day <= dateOfPayment.Day)
            {
                return null;
            }
            else
            {
                return dateOfPayment.NextPaymentDate(periodTimeInDays).Value.AddDays(-periodTimeInDays);
            }
        }
    }
}
