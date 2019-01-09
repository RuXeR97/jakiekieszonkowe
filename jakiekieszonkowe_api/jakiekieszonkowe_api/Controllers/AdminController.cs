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
    }
}
