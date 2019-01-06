using jakiekieszonkowe_api.Database;
using jakiekieszonkowe_api.Exceptions;
using jakiekieszonkowe_api.Other;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

namespace jakiekieszonkowe_api.Controllers
{
    public class UserController : ApiController
    {
        [AcceptVerbs("GET", "POST")]
        [ActionName("RegisterUser")]
        public object RegisterUser(string email, string password, int cityId)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                wasSuccess = true;
                RegisterUserDetailed(email, password, cityId);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                wasSuccess = false;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
            };

            return finalResult;
        }
        private void RegisterUserDetailed(string email, string password, int cityId)
        {
            Guid userGuid = System.Guid.NewGuid();
            string hashedPassword = Security.HashSHA1(password + userGuid.ToString());

            User comment = new User()
            {
                Email = email,
                Password = hashedPassword,
                UserGuid = userGuid,
                Id_city = cityId,
                Account_registration_date = DateTime.Now.ToLocalTime().ToLocalTime(),
            };

            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    db.Users.Add(comment);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("ValidateUser")]
        public object ValidateUser(string email, string password)
        {
            return ValidateUserDetailed(email.Trim(), password.Trim());
        }
        private object ValidateUserDetailed(string email, string password)
        {
            string errorMessage = string.Empty;
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    User user = db.Users.FirstOrDefault(i => i.Email == email);
                    if (user == null)
                        throw new UserNotFoundException("Nie ma takiego użytkownika.");

                    string hashedPassword = Security.HashSHA1(password + user.UserGuid);

                    // if its correct password the result of the hash is the same as in the database
                    if (user.Password.Trim() == hashedPassword)
                    {
                        // The password is correct
                        string generatedToken = Security.GenerateToken(user.Email);
                        Security.UserTokens.Add(user.Id_user, generatedToken);
                        var userChildrenList = new List<object>();
                        var children = user.Child.Where(i => i.Id_user == user.Id_user);
                        foreach (var child in children)
                        {
                            userChildrenList.Add(new
                            {
                                id = child.Id_child,
                                name = child.First_name,
                                age = DateTime.Now.ToLocalTime().ToLocalTime().Year - child.Date_of_birth.Value.Year,
                                schoolTypeId = child.Id_education_stage,
                                quota = child.Current_amount_of_money,
                                paymentPeriodId = child.Id_payout_period,
                                paymentDate = child.Date_of_payout,
                                prevPaymentDate = child.Date_of_payout.PreviousPaymentDate(),
                                nextPaymentDate = child.Date_of_payout.NextPaymentDate(),
                                provinceId = child.City.Id_province,
                                cityId = child.Id_city,
                                moneyIncludes = child.Pocket_money_option
                            });
                        }

                        var reminderNotifications = user.Reminder_notification.Where(i => i.Id_user == user.Id_user);
                        List<object> reminderNotificationsList = new List<object>();
                        foreach (var reminderNotification in reminderNotifications)
                        {
                            reminderNotificationsList.Add(new
                            {
                                id = reminderNotification.Id_reminder_notification,
                                kidId = reminderNotification.Id_child,
                                notificationOverlap = reminderNotification.Days_number,
                            });
                        }

                        var userMetaNotificationElement = user.Information_notification.Any(i => i.Id_user == user.Id_user);

                        var finalResult = new
                        {
                            success = true,
                            message = String.Empty,
                            isValidated = true,
                            isAdmin = user.IsAdmin,
                            token = generatedToken,
                            userData = new                                     
                            {
                                email = user.Email,
                                accountActivationDate = user.Account_registration_date.ToString("yyyy-MM-dd"),
                                accountLastLogInDate =  user.Last_login_date,                       //"YYY -MM-DD",
                                provinceId = user.City.Id_province,
                                cityId = user.Id_city,
                                province = user.City.Province.Name,
                                city = user.City.Name
                            },
                            userKids = userChildrenList,
                            userNotifications = reminderNotificationsList,      
                            userMetaNotification = userMetaNotificationElement  
                        };
                        return finalResult;
                    }
                    else
                    {
                        var finalResult = new
                        {
                            success = false,
                            message = "Błędne hasło"
                        };
                        return finalResult;
                    }
                }
            }
            catch (Exception ex)
            {
                var finalResult = new
                {
                    success = false,
                    message = ex.Message
                };
                return finalResult;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("AddChild")]
        public object AddChild(DateTime dateOfBirth, string name, double quota, int cityId, DateTime paymentDate,
            int paymentPeriodId, int schoolTypeId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                wasSuccess = true;
                resultList = AddChildDetailed(dateOfBirth, name, quota, cityId, null, paymentDate, paymentPeriodId, schoolTypeId, token).ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                wasSuccess = false;
                
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = resultList
            };

            return finalResult;
        }
        private IEnumerable<object> AddChildDetailed(DateTime dateOfBirth, string name, double quota, int cityId, List<int> moneyIncludes,
            DateTime paymentDate, int paymentPeriodId, int schoolTypeId, string token)
        {
            List<Pocket_money_option> pocketMoneyOptions = new List<Pocket_money_option>();
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

                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    if(moneyIncludes != null)
                    {
                        foreach (int id in moneyIncludes)
                        {
                            Pocket_money_option pocketMoneyOption = db.Pocket_money_options.FirstOrDefault(i => i.Id_pocket_money_option == id);
                            pocketMoneyOptions.Add(pocketMoneyOption);
                        }
                    }
                    Child child = new Child()
                    {
                        First_name = name,
                        Current_amount_of_money = (decimal)quota,
                        Id_city = cityId,
                        Id_user = userId,
                        Date_of_birth = dateOfBirth,
                        Id_education_stage = schoolTypeId,
                        Pocket_money_option = pocketMoneyOptions,
                        Id_payout_period = paymentPeriodId,
                        Date_of_payout = paymentDate
                    };
                    db.Children.Add(child);
                    db.SaveChanges();
                    var children = db.Children.Where(i => i.Id_user == userId);
                    var userChildrenList = new List<object>();

                    foreach (var singleChild in children)
                    {
                        userChildrenList.Add(new
                        {
                            id = singleChild.Id_child,
                            name = singleChild.First_name,
                            age = DateTime.Now.ToLocalTime().ToLocalTime().ToLocalTime().Year - singleChild.Date_of_birth.Value.Year,
                            schoolTypeId = singleChild.Id_education_stage,
                            quota = singleChild.Current_amount_of_money,
                            paymentPeriodId = singleChild.Id_payout_period,
                            paymentDate = singleChild.Date_of_payout,
                            prevPaymentDate = singleChild.Date_of_payout.PreviousPaymentDate(),
                            nextPaymentDate = singleChild.Date_of_payout.NextPaymentDate(),
                            provinceId = singleChild.City.Id_province,
                            cityId = singleChild.Id_city,
                            moneyIncludes = singleChild.Pocket_money_option
                        });
                    }


                    return userChildrenList;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("EditChild")]
        public object EditChild(DateTime dateOfBirth, string name, double quota, int cityId, DateTime paymentDate,
            int paymentPeriodId, int schoolTypeId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                wasSuccess = true;
                resultList = EditChildDetailed(dateOfBirth, name, quota, cityId, null, paymentDate, paymentPeriodId, schoolTypeId, token).ToList();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                wasSuccess = false;

            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = resultList
            };

            return finalResult;
        }
        private IEnumerable<object> EditChildDetailed(DateTime dateOfBirth, string name, double quota, int cityId, List<int> moneyIncludes,
            DateTime paymentDate, int paymentPeriodId, int schoolTypeId, string token)
        {
            List<Pocket_money_option> pocketMoneyOptions = new List<Pocket_money_option>();
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

                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    if (moneyIncludes != null)
                    {
                        foreach (int id in moneyIncludes)
                        {
                            Pocket_money_option pocketMoneyOption = db.Pocket_money_options.FirstOrDefault(i => i.Id_pocket_money_option == id);
                            pocketMoneyOptions.Add(pocketMoneyOption);
                        }
                    }
                    
                    Child child = db.Children.FirstOrDefault(i => i.Id_user == userId && i.First_name == name);
                    child.First_name = name;
                    child.Current_amount_of_money = (decimal)quota;
                    child.Id_city = cityId;
                    child.Date_of_birth = dateOfBirth;
                    child.Id_education_stage = schoolTypeId;
                    child.Pocket_money_option = pocketMoneyOptions;
                    child.Id_payout_period = paymentPeriodId;
                    child.Date_of_payout = paymentDate;
                    db.Children.Attach(child);
                    db.Entry(child).State = EntityState.Modified;
                    db.SaveChanges();

                    var children = db.Children.Where(i => i.Id_user == userId);
                    var userChildrenList = new List<object>();

                    foreach (var singleChild in children)
                    {
                        userChildrenList.Add(new
                        {
                            id = singleChild.Id_child,
                            name = singleChild.First_name,
                            age = DateTime.Now.ToLocalTime().ToLocalTime().ToLocalTime().Year - singleChild.Date_of_birth.Value.Year,
                            schoolTypeId = singleChild.Id_education_stage,
                            quota = singleChild.Current_amount_of_money,
                            paymentPeriodId = singleChild.Id_payout_period,
                            paymentDate = singleChild.Date_of_payout,
                            prevPaymentDate = singleChild.Date_of_payout.PreviousPaymentDate(),
                            nextPaymentDate = singleChild.Date_of_payout.NextPaymentDate(),
                            provinceId = singleChild.City.Id_province,
                            cityId = singleChild.Id_city,
                            moneyIncludes = singleChild.Pocket_money_option
                        });
                    }

                    return userChildrenList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
