using jakiekieszonkowe_api.Database;
using jakiekieszonkowe_api.Other;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

namespace jakiekieszonkowe_api.Controllers
{
    public class AdminController : ApiController
    {
        [AcceptVerbs("GET", "POST")]
        [ActionName("AddProvince")]
        public object AddProvince(string provinceName, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                AddProvinceDetailed(provinceName, userId);
                resultList = dictionaryController.GetProvinceDictionary().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetProvinceDictionary().ToList()
            };

            return finalResult;
        }
        private void AddProvinceDetailed(string provinceName, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if(!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    db.Provinces.Add(new Province { Name = provinceName });
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("EditProvince")]
        public object EditProvince(string provinceName, int provinceId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                EditProvinceDetailed(provinceName, provinceId, userId);
                resultList = dictionaryController.GetProvinceDictionary().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetProvinceDictionary().ToList()
            };

            return finalResult;
        }
        private void EditProvinceDetailed(string provinceName, int provinceId, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    Province province = db.Provinces.FirstOrDefault(i => i.Id_province == provinceId);
                    province.Name = provinceName;
                    db.Provinces.Attach(province);
                    db.Entry(province).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("DeleteProvince")]
        public object DeleteProvince(int provinceId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                DeleteProvinceDetailed(provinceId, userId);
                resultList = dictionaryController.GetProvinceDictionary().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetProvinceDictionary().ToList()
            };

            return finalResult;
        }
        private void DeleteProvinceDetailed(int provinceId, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    Province province = db.Provinces.FirstOrDefault(i => i.Id_province == provinceId);

                    var children = db.Children.Where(i => i.City.Id_province == provinceId).Include(i => i.Pocket_money_option);
                    foreach (var child in children)
                    {
                        db.Children.Remove(child);
                    }

                    db.Cities.RemoveRange(db.Cities.Where(i => i.Id_province == provinceId));
                    db.Provinces.Remove(province);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("AddCity")]
        public object AddCity(string cityName, int provinceId, double longitude, double latitude, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                AddCityDetailed(cityName, provinceId, longitude, latitude, userId);
                resultList = dictionaryController.GetCityDictionary().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetCityDictionary().ToList()
            };

            return finalResult;
        }
        private void AddCityDetailed(string cityName, int provinceId, double longitude, double latitude, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    db.Cities.Add(new City
                    {
                        Name = cityName,
                        Id_province = provinceId,
                        Longitude = longitude,
                        Latitude = latitude
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("EditCity")]
        public object EditCity(string cityName, int provinceId, int cityId, double longitude, double latitude, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                EditCityDetailed(cityName, provinceId, cityId, longitude, latitude, userId);
                resultList = dictionaryController.GetCityDictionary().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetCityDictionary().ToList()
            };

            return finalResult;
        }
        private void EditCityDetailed(string cityName, int provinceId, int cityId, double longitude, double latitude, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    City city = db.Cities.FirstOrDefault(i => i.Id_city == cityId);
                    city.Name = cityName;
                    city.Id_province = provinceId;
                    city.Longitude = longitude;
                    city.Latitude = latitude;
                    db.Cities.Attach(city);
                    db.Entry(city).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("DeleteCity")]
        public object DeleteCity(int cityId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                DeleteCityDetailed(cityId, userId);
                resultList = dictionaryController.GetCityDictionary().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetCityDictionary().ToList()
            };

            return finalResult;
        }
        private void DeleteCityDetailed(int cityId, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    City city = db.Cities.FirstOrDefault(i => i.Id_city == cityId);

                    var children = db.Children.Where(i => i.Id_city== city.Id_city).Include(i => i.Pocket_money_option);
                    foreach (var child in children)
                    {
                        db.Children.Remove(child);
                    }
                    db.Cities.Remove(city);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("AddSchoolType")]
        public object AddSchoolType(string schoolTypeName, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                AddProvinceDetailed(schoolTypeName, userId);
                resultList = dictionaryController.GetSchoolTypeDictionary().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetSchoolTypeDictionary().ToList()
            };

            return finalResult;
        }
        private void AddSchoolTypeDetailed(string schoolTypeName, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    db.Education_stages.Add(new Education_stage { Name = schoolTypeName });
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("EditSchoolType")]
        public object EditSchoolType(string schoolTypeName, int schoolTypeId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                EditSchoolTypeDetailed(schoolTypeName, schoolTypeId, userId);
                resultList = dictionaryController.GetSchoolTypeDictionary().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetSchoolTypeDictionary().ToList()
            };

            return finalResult;
        }
        private void EditSchoolTypeDetailed(string schoolTypeName, int schoolTypeId, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    Education_stage educationStage = db.Education_stages.FirstOrDefault(i => i.Id_education_stage == schoolTypeId);
                    educationStage.Name = schoolTypeName;
                    db.Education_stages.Attach(educationStage);
                    db.Entry(educationStage).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("DeleteSchoolType")]
        public object DeleteSchoolType(int schoolTypeId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                DeleteSchoolTypeDetailed(schoolTypeId, userId);
                resultList = dictionaryController.GetSchoolTypeDictionary().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetSchoolTypeDictionary().ToList()
            };

            return finalResult;
        }
        private void DeleteSchoolTypeDetailed(int schoolTypeId, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    Education_stage educationStage = db.Education_stages.FirstOrDefault(i => i.Id_education_stage == schoolTypeId);

                    var children = db.Children.Where(i => i.Id_education_stage == schoolTypeId).Include(i => i.Pocket_money_option);
                    foreach (var child in children)
                    {
                        db.Children.Remove(child);
                    }
                    
                    db.Education_stages.Remove(educationStage);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("AddMoneyIncludes")]
        public object AddMoneyIncludes(string moneyIncludesName, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                AddMoneyIncludesDetailed(moneyIncludesName, userId);
                resultList = dictionaryController.GetMoneyIncludes().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetMoneyIncludes().ToList()
            };

            return finalResult;
        }
        private void AddMoneyIncludesDetailed(string moneyIncludesName, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    db.Pocket_money_options.Add(new Pocket_money_option { Name = moneyIncludesName });
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("EditMoneyIncludes")]
        public object EditMoneyIncludes(string moneyIncludesName, int moneyIncludesId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                EditMoneyIncludesDetailed(moneyIncludesName, moneyIncludesId, userId);
                resultList = dictionaryController.GetMoneyIncludes().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetMoneyIncludes().ToList()
            };

            return finalResult;
        }
        private void EditMoneyIncludesDetailed(string moneyIncludesName, int moneyIncludesId, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    Pocket_money_option pocketMoneyOption = db.Pocket_money_options.FirstOrDefault(i => i.Id_pocket_money_option == moneyIncludesId);
                    pocketMoneyOption.Name = moneyIncludesName;
                    db.Pocket_money_options.Attach(pocketMoneyOption);
                    db.Entry(pocketMoneyOption).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("DeleteMoneyIncludes")]
        public object DeleteMoneyIncludes(int moneyIncludesId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                DeleteMoneyIncludesDetailed(moneyIncludesId, userId);
                resultList = dictionaryController.GetMoneyIncludes().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetMoneyIncludes().ToList()
            };

            return finalResult;
        }
        private void DeleteMoneyIncludesDetailed(int moneyIncludesId, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    Pocket_money_option pocketMoneyOption = db.Pocket_money_options.FirstOrDefault(i => i.Id_pocket_money_option == moneyIncludesId);

                    var children = db.Children.Where(i => i.Pocket_money_option.Any(j=> j.Id_pocket_money_option == moneyIncludesId)).Include(i => i.Pocket_money_option);
                    foreach (var child in children)
                    {
                        db.Children.Remove(child);
                    }

                    db.Pocket_money_options.Remove(pocketMoneyOption);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("AddPaymentPeriod")]
        public object AddPaymentPeriod(string paymentPeriodName, int days, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                AddPaymentPeriodDetailed(paymentPeriodName, days, userId);
                resultList = dictionaryController.GetPaymentPeriodDictionary().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetPaymentPeriodDictionary().ToList()
            };

            return finalResult;
        }
        private void AddPaymentPeriodDetailed(string paymentPeriodName, int days, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    db.Payout_periods.Add(new Payout_period
                    {
                        Name = paymentPeriodName,
                        Days = days
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("EditPaymentPeriod")]
        public object EditPaymentPeriod(string paymentPeriodName, int days, int paymentPeriodId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                EditPaymentPeriodDetailed(paymentPeriodName, days, paymentPeriodId, userId);
                resultList = dictionaryController.GetPaymentPeriodDictionary().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetPaymentPeriodDictionary().ToList()
            };

            return finalResult;
        }
        private void EditPaymentPeriodDetailed(string paymentPeriodName, int days, int paymentPeriodId, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    Payout_period payoutPeriod = db.Payout_periods.FirstOrDefault(i => i.Id_payout_period == paymentPeriodId);
                    payoutPeriod.Name = paymentPeriodName;
                    payoutPeriod.Days = days;
                    db.Payout_periods.Attach(payoutPeriod);
                    db.Entry(payoutPeriod).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("DeletePaymentPeriod")]
        public object DeletePaymentPeriod(int paymentPeriodId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            DictionaryController dictionaryController = new DictionaryController();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                DeletePaymentPeriodDetailed(paymentPeriodId, userId);
                resultList = dictionaryController.GetPaymentPeriodDictionary().ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = dictionaryController.GetPaymentPeriodDictionary().ToList()
            };

            return finalResult;
        }
        private void DeletePaymentPeriodDetailed(int paymentPeriodId, int userId)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var isAdmin = db.Users.FirstOrDefault(i => i.Id_user == userId).IsAdmin;
                    if (!isAdmin)
                    {
                        throw new Exception("Nie posiadasz wystarczających uprawnień.");
                    }
                    Payout_period payoutPeriod = db.Payout_periods.FirstOrDefault(i => i.Id_payout_period == paymentPeriodId);

                    var children = db.Children.Where(i => i.Id_payout_period == paymentPeriodId).Include(i => i.Pocket_money_option);
                    foreach (var child in children)
                    {
                        db.Children.Remove(child);
                    }
                    
                    db.Payout_periods.Remove(payoutPeriod);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
