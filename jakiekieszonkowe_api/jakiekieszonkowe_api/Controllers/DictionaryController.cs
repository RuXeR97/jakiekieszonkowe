using jakiekieszonkowe_api.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace jakiekieszonkowe_api.Controllers
{
    public class DictionaryController : ApiController
    {
        [AcceptVerbs("GET","POST")]
        [ActionName("GetProvinceDictionary")]
        public IEnumerable<object> GetProvinceDictionary()
        {
            List<object> finalResultList = new List<object>();
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    foreach (var item in db.Provinces)
                    {
                        finalResultList.Add(new
                        {
                            id = item.Id_province,
                            name = item.Name
                        });
                    }

                    return finalResultList;
                }
            }
            catch(Exception)
            {
                return null;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("GetCityDictionary")]
        public IEnumerable<object> GetCityDictionary()
        {
            List<object> finalResultList = new List<object>();
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    foreach (var item in db.Cities)
                    {
                        finalResultList.Add(new
                        {
                            id = item.Id_city,
                            provinceId = item.Id_province,
                            name = item.Name
                        });
                    }

                    return finalResultList;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("GetPaymentPeriodDictionary")]
        public IEnumerable<object> GetPaymentPeriodDictionary()
        {
            List<object> finalResultList = new List<object>();
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    foreach (var item in db.Payout_periods)
                    {
                        finalResultList.Add(new
                        {
                            id = item.Id_payout_period,
                            days = item.Days,
                            name = item.Name
                        });
                    }

                    return finalResultList;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("GetSchoolTypeDictionary")]
        public IEnumerable<object> GetSchoolTypeDictionary()
        {
            List<object> finalResultList = new List<object>();
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    foreach (var item in db.Education_stages)
                    {
                        finalResultList.Add(new
                        {
                            id = item.Id_education_stage,
                            name = item.Name
                        });
                    }

                    return finalResultList;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("GetMoneyIncludes")]
        public IEnumerable<object> GetMoneyIncludes()
        {
            List<object> finalResultList = new List<object>();
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    foreach (var item in db.Pocket_money_options)
                    {
                        finalResultList.Add(new
                        {
                            id = item.Id_pocket_money_option,
                            name = item.Name
                        });
                    }

                    return finalResultList;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
