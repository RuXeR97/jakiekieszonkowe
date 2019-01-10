using jakiekieszonkowe_api.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace jakiekieszonkowe_api.Controllers
{
    public class MapStatsController : ApiController
    {
        [AcceptVerbs("GET", "POST")]
        [ActionName("GetCountryStats")]
        public object GetCountryStats(bool useFilters, int ageRangeMin, int ageRangeMax, string moneyIncludes, 
            int schoolTypeId, bool filterByMoneyIncludes)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new object();
            try
            {
                wasSuccess = true;
                resultList = GetCountryStatsDetailed(useFilters, ageRangeMin, ageRangeMax, moneyIncludes, 
                    schoolTypeId, filterByMoneyIncludes);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                countryData = resultList
            };

            return finalResult;
        }
        private object GetCountryStatsDetailed(bool useFilters, int ageRangeMin, int ageRangeMax, string moneyIncludes, 
            int schoolTypeId, bool filterByMoneyIncludes)
        {
            List<int> moneyIncludesArray = new List<int>();
            int amountOfElements = 0;
            if (useFilters)
            {
                string[] tmp = null;
                string tmpWithComma;
                string[] finalArray = null;
                if (moneyIncludes != null)
                {
                    tmp = Regex.Split(moneyIncludes, "%2C");
                    tmpWithComma = string.Join("", tmp);
                    finalArray = tmpWithComma.Split(',');
                }

                moneyIncludesArray = new List<int>();
                if (finalArray != null)
                {
                    foreach (var item in finalArray)
                    {
                        moneyIncludesArray.Add(Int32.Parse(item));
                    }
                    amountOfElements = moneyIncludesArray.Count;
                }
                
            }

            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var children = db.Children.ToList();
                    if (useFilters)
                    {
                        // age filter
                        var today = DateTime.Today;
                        children = children.Where(i => (today.Year - i.Date_of_birth.Value.Year >= ageRangeMin) &&
                        (today.Year - i.Date_of_birth.Value.Year <= ageRangeMax)).ToList();

                        // school type filter
                        if(schoolTypeId != -1)
                        {
                            children = children.Where(i => i.Id_education_stage == schoolTypeId).ToList();
                        }

                        // moneyIncludes filter
                        if (filterByMoneyIncludes)
                        {
                            List<Pocket_money_option> pocketMoneyOptions = new List<Pocket_money_option>();
                            foreach (var child in children.ToList())
                            {
                                var idPocketMoney = child.Child_pocket_money_option.Select(i => i.Id_pocket_money_option).ToList();
                                var result = moneyIncludesArray.Concat(idPocketMoney);
                                var finalResultOfSets = result.Union(moneyIncludesArray);
                                if (finalResultOfSets.Count() != amountOfElements)
                                    children.Remove(child);
                            }
                        }
                    }

                    double average, sumOfSquaresOfDifferences, sd;
                    average = sumOfSquaresOfDifferences = sd = 0;
                    if(children.Count > 0)
                    {
                        average = (double)children.Average(i => i.Current_amount_of_money);

                        sumOfSquaresOfDifferences = children.
                            Select(i => ((double)i.Current_amount_of_money - average) * ((double)i.Current_amount_of_money - average)).Sum();

                        sd = Math.Sqrt(sumOfSquaresOfDifferences / children.Count());
                    }
                    
                    var finalResult = new
                    {
                        id = 0,
                        name = "Polska",
                        avg = average,
                        std = sd,
                        count = children.Count()
                    };

                    return finalResult;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("GetProvinceStats")]
        public object GetProvinceStats(bool useFilters, int ageRangeMin, int ageRangeMax, string moneyIncludes,
            int schoolTypeId, bool filterByMoneyIncludes)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                wasSuccess = true;
                resultList = GetProvinceStatsDetailed(useFilters, ageRangeMin, ageRangeMax, moneyIncludes,
                    schoolTypeId, filterByMoneyIncludes).ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                provinceData = resultList
            };

            return finalResult;
        }
        private IEnumerable<object> GetProvinceStatsDetailed(bool useFilters, int ageRangeMin, int ageRangeMax, string moneyIncludes,
            int schoolTypeId, bool filterByMoneyIncludes)
        {
            List<int> moneyIncludesArray = new List<int>();
            List<object> finalResultList = new List<object>();
            int amountOfElements = 0;
            if (useFilters)
            {
                string[] tmp = null;
                string tmpWithComma;
                string[] finalArray = null;
                if (moneyIncludes != null)
                {
                    tmp = Regex.Split(moneyIncludes, "%2C");
                    tmpWithComma = string.Join("", tmp);
                    finalArray = tmpWithComma.Split(',');
                }

                moneyIncludesArray = new List<int>();
                if (finalArray != null)
                {
                    foreach (var item in finalArray)
                    {
                        moneyIncludesArray.Add(Int32.Parse(item));
                    }
                    amountOfElements = moneyIncludesArray.Count;
                }

            }

            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    foreach(var province in db.Provinces)
                    {
                        var children = db.Children.Where(i=>i.City.Id_province == province.Id_province).ToList();
                        if (useFilters)
                        {
                            // age filter
                            var today = DateTime.Today;
                            children = children.Where(i => (today.Year - i.Date_of_birth.Value.Year >= ageRangeMin) &&
                            (today.Year - i.Date_of_birth.Value.Year <= ageRangeMax)).ToList();

                            // school type filter
                            if (schoolTypeId != -1)
                            {
                                children = children.Where(i => i.Id_education_stage == schoolTypeId).ToList();
                            }

                            // moneyIncludes filter
                            if (filterByMoneyIncludes)
                            {
                                List<Pocket_money_option> pocketMoneyOptions = new List<Pocket_money_option>();
                                foreach (var child in children.ToList())
                                {
                                    var idPocketMoney = child.Child_pocket_money_option.Select(i => i.Id_pocket_money_option).ToList();
                                    var result = moneyIncludesArray.Concat(idPocketMoney);
                                    var finalResultOfSets = result.Union(moneyIncludesArray);
                                    if (finalResultOfSets.Count() != amountOfElements)
                                        children.Remove(child);
                                }
                            }
                        }

                        double average, sumOfSquaresOfDifferences, sd;
                        average = sumOfSquaresOfDifferences = sd = 0;
                        if (children.Count > 0)
                        {
                            average = (double)children.Average(i => i.Current_amount_of_money);

                            sumOfSquaresOfDifferences = children.
                                Select(i => ((double)i.Current_amount_of_money - average) * ((double)i.Current_amount_of_money - average)).Sum();

                            sd = Math.Sqrt(sumOfSquaresOfDifferences / children.Count());
                        }

                        var finalResult = new
                        {
                            id = province.Id_province,
                            name = province.Name.Trim(),
                            avg = average,
                            std = sd,
                            count = children.Count()
                        };
                        finalResultList.Add(finalResult);
                    }
                    
                    return finalResultList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("GetCityStats")]
        public object GetCityStats(int provinceId, bool useFilters, int ageRangeMin, int ageRangeMax, string moneyIncludes,
           int schoolTypeId, bool filterByMoneyIncludes)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                wasSuccess = true;
                resultList = GetCityStatsDetailed(provinceId, useFilters, ageRangeMin, ageRangeMax, moneyIncludes,
                    schoolTypeId, filterByMoneyIncludes).ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                cityData = resultList
            };

            return finalResult;
        }
        private IEnumerable<object> GetCityStatsDetailed(int provinceId, bool useFilters, int ageRangeMin, int ageRangeMax, string moneyIncludes,
            int schoolTypeId, bool filterByMoneyIncludes)
        {
            List<int> moneyIncludesArray = new List<int>();
            List<object> finalResultList = new List<object>();
            int amountOfElements = 0;
            if (useFilters)
            {
                string[] tmp = null;
                string tmpWithComma;
                string[] finalArray = null;
                if (moneyIncludes != null)
                {
                    tmp = Regex.Split(moneyIncludes, "%2C");
                    tmpWithComma = string.Join("", tmp);
                    finalArray = tmpWithComma.Split(',');
                }

                moneyIncludesArray = new List<int>();
                if (finalArray != null)
                {
                    foreach (var item in finalArray)
                    {
                        moneyIncludesArray.Add(Int32.Parse(item));
                    }
                    amountOfElements = moneyIncludesArray.Count;
                }
            }

            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    foreach (var city in db.Cities.Where(i=> i.Id_province == provinceId))
                    {
                        var children = db.Children.Where(i => i.Id_city == city.Id_city).ToList();
                        if (useFilters)
                        {
                            // age filter
                            var today = DateTime.Today;
                            children = children.Where(i => (today.Year - i.Date_of_birth.Value.Year >= ageRangeMin) &&
                            (today.Year - i.Date_of_birth.Value.Year <= ageRangeMax)).ToList();

                            // school type filter
                            if (schoolTypeId != -1)
                            {
                                children = children.Where(i => i.Id_education_stage == schoolTypeId).ToList();
                            }

                            // moneyIncludes filter
                            if (filterByMoneyIncludes)
                            {
                                List<Pocket_money_option> pocketMoneyOptions = new List<Pocket_money_option>();
                                foreach (var child in children.ToList())
                                {
                                    var idPocketMoney = child.Child_pocket_money_option.Select(i => i.Id_pocket_money_option).ToList();
                                    var result = moneyIncludesArray.Concat(idPocketMoney);
                                    var finalResultOfSets = result.Union(moneyIncludesArray);
                                    if (finalResultOfSets.Count() != amountOfElements)
                                        children.Remove(child);
                                }
                            }
                        }

                        double average, sumOfSquaresOfDifferences, sd;
                        average = sumOfSquaresOfDifferences = sd = 0;
                        if (children.Count > 0)
                        {
                            average = (double)children.Average(i => i.Current_amount_of_money);

                            sumOfSquaresOfDifferences = children.
                                Select(i => ((double)i.Current_amount_of_money - average) * ((double)i.Current_amount_of_money - average)).Sum();

                            sd = Math.Sqrt(sumOfSquaresOfDifferences / children.Count());
                        }

                        var finalResult = new
                        {
                            id = city.Id_city,
                            provinceId = city.Id_province,
                            name = city.Name.Trim(),
                            avg = average,
                            std = sd,
                            count = children.Count(),
                            latitude = city.Latitude,
                            longitude = city.Longitude
                        };
                        finalResultList.Add(finalResult);
                    }

                    return finalResultList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
