using jakiekieszonkowe_api.Database;
using jakiekieszonkowe_api.Exceptions;
using jakiekieszonkowe_api.Other;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Description;

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
                    if (db.Users.Any(i => i.Email == email))
                        throw new Exception("Podana nazwa jest zajęta! Spróbuj innej.");
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
                        if (!Security.UserTokens.Any(i => i.Key == user.Id_user))
                            Security.UserTokens.Add(user.Id_user, generatedToken);

                        user.Last_login_date = DateTime.Now;
                        db.Users.Attach(user);
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();

                        var userChildrenList = new List<object>();
                        var children = user.Child.Where(i => i.Id_user == user.Id_user);
                        List<object> moneyIncludesList;

                        foreach (var child in children)
                        {
                            moneyIncludesList = new List<object>();
                            foreach (var item in child.Pocket_money_option)
                            {
                                moneyIncludesList.Add(item.Id_pocket_money_option);
                            }

                            userChildrenList.Add(new
                            {
                                id = child.Id_child,
                                name = child.First_name.Trim(),
                                age = DateTime.Now.ToLocalTime().ToLocalTime().Year - child.Date_of_birth.Value.Year,
                                dateOfBirth = child.Date_of_birth?.ToString("yyyy-MM-dd"),
                                schoolTypeId = child.Id_education_stage,
                                quota = child.Current_amount_of_money,
                                paymentPeriodId = child.Id_payout_period,
                                paymentDate = child.Date_of_payout.ToString("yyyy-MM-dd"),
                                prevPaymentDate = child.Date_of_payout.PreviousPaymentDate()?.ToString("yyyy-MM-dd"),
                                nextPaymentDate = child.Date_of_payout.NextPaymentDate()?.ToString("yyyy-MM-dd"),
                                provinceId = child.City.Id_province,
                                cityId = child.Id_city,
                                moneyIncludes = moneyIncludesList
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
                            token = Security.UserTokens.FirstOrDefault(i => i.Key == user.Id_user).Value,
                            userData = new                                     
                            {
                                email = user.Email.Trim(),
                                accountActivationDate = user.Account_registration_date.ToString("yyyy-MM-dd"),
                                accountLastLogInDate =  user.Last_login_date?.ToString("yyyy-MM-dd"),
                                provinceId = user.City.Id_province,
                                cityId = user.Id_city,
                                province = user.City.Province.Name.Trim(),
                                city = user.City.Name.Trim(),
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
        public object AddChild(DateTime dateOfBirth, string name, double quota, int cityId, string moneyIncludes,
            DateTime paymentDate, int paymentPeriodId, int schoolTypeId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                wasSuccess = true;
                resultList = AddChildDetailed(dateOfBirth, name, quota, cityId, moneyIncludes, paymentDate, paymentPeriodId, schoolTypeId, token).ToList();
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
        private IEnumerable<object> AddChildDetailed(DateTime dateOfBirth, string name, double quota, int cityId, string moneyIncludes,
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

                List<object> userChildrenList = new List<object>();
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
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
                    List<int> moneyIncludesArray = new List<int>();
                    if(finalArray != null)
                    {
                        foreach (var item in finalArray)
                        {
                            moneyIncludesArray.Add(Int32.Parse(item));
                        }
                    }
                    
                    if (moneyIncludesArray != null)
                    {
                        foreach (int id in moneyIncludesArray)
                        {
                            Pocket_money_option pocketMoneyOption = db.Pocket_money_options.FirstOrDefault(i => i.Id_pocket_money_option == id);
                            pocketMoneyOptions.Add(pocketMoneyOption);
                        }
                        db.SaveChanges();
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

                    List<object> moneyIncludesList;

                    foreach (var singleChild in children)
                    {
                        moneyIncludesList = new List<object>();
                        foreach (var item in singleChild.Pocket_money_option)
                        {
                            moneyIncludesList.Add(item.Id_pocket_money_option);
                        }

                        userChildrenList.Add(new
                        {
                            id = singleChild.Id_child,
                            name = singleChild.First_name.Trim(),
                            age = DateTime.Now.ToLocalTime().ToLocalTime().ToLocalTime().Year - singleChild.Date_of_birth.Value.Year,
                            dateOfBirth = singleChild.Date_of_birth?.ToString("yyyy-MM-dd"),
                            schoolTypeId = singleChild.Id_education_stage,
                            quota = singleChild.Current_amount_of_money,
                            paymentPeriodId = singleChild.Id_payout_period,
                            paymentDate = singleChild.Date_of_payout.ToString("yyyy-MM-dd"),
                            prevPaymentDate = singleChild.Date_of_payout.PreviousPaymentDate()?.ToString("yyyy-MM-dd"),
                            nextPaymentDate = singleChild.Date_of_payout.NextPaymentDate()?.ToString("yyyy-MM-dd"),
                            provinceId = db.Cities.FirstOrDefault(i => i.Id_city == singleChild.Id_city).Id_province,
                            cityId = singleChild.Id_city,
                            moneyIncludes = moneyIncludesList
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

        [AcceptVerbs("GET", "POST")]
        [ActionName("EditChild")]
        public object EditChild(int childId, DateTime dateOfBirth, string name, double quota, int cityId, string moneyIncludes, 
            DateTime paymentDate,
            int paymentPeriodId, int schoolTypeId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                wasSuccess = true;
                resultList = EditChildDetailed(childId, dateOfBirth, name, quota, cityId, moneyIncludes, paymentDate, paymentPeriodId, schoolTypeId, token).ToList();
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
        private IEnumerable<object> EditChildDetailed(int childId, DateTime dateOfBirth, string name, double quota, int cityId, string moneyIncludes,
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
                    string[] tmp = Regex.Split(moneyIncludes, "%2C");
                    string tmpWithComma = string.Join("", tmp);
                    string[] finalArray = tmpWithComma.Split(',');
                    List<int> moneyIncludesArray = new List<int>();
                    foreach (var item in finalArray)
                    {
                        moneyIncludesArray.Add(Int32.Parse(item));
                    }

                    if (moneyIncludesArray != null)
                    {
                        foreach (int id in moneyIncludesArray)
                        {
                            Pocket_money_option pocketMoneyOption = db.Pocket_money_options.FirstOrDefault(i => i.Id_pocket_money_option == id);
                            pocketMoneyOptions.Add(pocketMoneyOption);
                            db.SaveChanges();
                        }
                    }
                    
                    Child child = db.Children.FirstOrDefault(i => i.Id_child == childId);
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
                    List<object> moneyIncludesList;

                    foreach (var singleChild in children)
                    {
                        moneyIncludesList = new List<object>();
                        foreach (var item in singleChild.Pocket_money_option)
                        {
                            moneyIncludesList.Add(item.Id_pocket_money_option);
                        }

                        userChildrenList.Add(new
                        {
                            id = singleChild.Id_child,
                            name = singleChild.First_name.Trim(),
                            age = DateTime.Now.ToLocalTime().ToLocalTime().ToLocalTime().Year - singleChild.Date_of_birth.Value.Year,
                            dateOfBirth = singleChild.Date_of_birth?.ToString("yyyy-MM-dd"),
                            schoolTypeId = singleChild.Id_education_stage,
                            quota = singleChild.Current_amount_of_money,
                            paymentPeriodId = singleChild.Id_payout_period,
                            paymentDate = singleChild.Date_of_payout.ToString("yyyy-MM-dd"),
                            prevPaymentDate = singleChild.Date_of_payout.PreviousPaymentDate()?.ToString("yyyy-MM-dd"),
                            nextPaymentDate = singleChild.Date_of_payout.NextPaymentDate()?.ToString("yyyy-MM-dd"),
                            provinceId = db.Cities.FirstOrDefault(i => i.Id_city == singleChild.Id_city).Id_province,
                            cityId = singleChild.Id_city,
                            moneyIncludes = moneyIncludesList
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

        [AcceptVerbs("GET", "POST")]
        [ActionName("DeleteChild")]
        public object DeleteChild(int childId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                wasSuccess = true;
                resultList = DeleteChildDetailed(childId, token).ToList();
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
        private IEnumerable<object> DeleteChildDetailed(int childId, string token)
        {
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
                    var cls = db.Children.Include(i => i.Pocket_money_option).Single(c => c.Id_child == childId);
                    db.Children.Remove(cls);
                    db.SaveChanges();            

                    var children = db.Children.Where(i => i.Id_user == userId);
                    var userChildrenList = new List<object>();
                    List<object> moneyIncludesList;

                    foreach (var singleChild in children)
                    {
                        moneyIncludesList = new List<object>();
                        foreach (var item in singleChild.Pocket_money_option)
                        {
                            moneyIncludesList.Add(item.Id_pocket_money_option);
                        }

                        userChildrenList.Add(new
                        {
                            id = singleChild.Id_child,
                            name = singleChild.First_name.Trim(),
                            age = DateTime.Now.ToLocalTime().ToLocalTime().ToLocalTime().Year - singleChild.Date_of_birth.Value.Year,
                            dateOfBirth = singleChild.Date_of_birth?.ToString("yyyy-MM-dd"),
                            schoolTypeId = singleChild.Id_education_stage,
                            quota = singleChild.Current_amount_of_money,
                            paymentPeriodId = singleChild.Id_payout_period,
                            paymentDate = singleChild.Date_of_payout.ToString("yyyy-MM-dd"),
                            prevPaymentDate = singleChild.Date_of_payout.PreviousPaymentDate()?.ToString("yyyy-MM-dd"),
                            nextPaymentDate = singleChild.Date_of_payout.NextPaymentDate()?.ToString("yyyy-MM-dd"),
                            provinceId = db.Cities.FirstOrDefault(i => i.Id_city == singleChild.Id_city).Id_province,
                            cityId = singleChild.Id_city,
                            moneyIncludes = moneyIncludesList
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

        [AcceptVerbs("GET", "POST")]
        [ActionName("ChangePassword")]
        public object ChangePassword(string newPassword, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            try
            {
                wasSuccess = true;
                ChangePasswordDetailed(newPassword, token);
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
        private void ChangePasswordDetailed(string newPassword, string token)
        {
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
                    User user = db.Users.FirstOrDefault(i => i.Id_user == userId);
                    string hashedPassword = Security.HashSHA1(newPassword + user.UserGuid.ToString());
                    user.Password = hashedPassword;
                    db.Users.Attach(user);
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("ChangeUserData")]
        public object ChangeUserData(int cityId, string token)
        {
            return ChangeUserDataDetailed(cityId, token);
        }
        private object ChangeUserDataDetailed(int cityId, string token)
        {
            string errorMessage = string.Empty;
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
                    User user = db.Users.FirstOrDefault(i => i.Id_user == userId);
                    user.Id_city = cityId;
                    db.Users.Attach(user);
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    var finalResult = new
                    {
                        success = true,
                        message = String.Empty,
                        email = user.Email.Trim(),
                        accountActivationDate = user.Account_registration_date.ToString("yyyy-MM-dd"),
                        accountLastLogInDate = user.Last_login_date?.ToString("yyyy-MM-dd"),
                        provinceId = user.City.Id_province,
                        cityId = user.Id_city,
                        province = user.City.Province.Name.Trim(),
                        city = user.City.Name.Trim(),
                    };

                    return finalResult;
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
        [ActionName("AddNotification")]
        public object AddNotification(int childId, int notificationOverLap, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                wasSuccess = true;
                resultList = AddNotificationDetailed(childId, notificationOverLap, token).ToList();
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
        private IEnumerable<object> AddNotificationDetailed(int childId, int notificationOverLap, string token)
        {
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
                    Reminder_notification reminderNotification = new Reminder_notification
                    {
                        Id_child = childId,
                        Id_user = userId,
                        Days_number = notificationOverLap
                    };
                    db.Reminder_notifications.Add(reminderNotification);
                    db.SaveChanges();

                    var reminderNotificaions = db.Reminder_notifications.Where(i => i.Id_user == userId);
                    var reminderNotificaionsList = new List<object>();

                    foreach (var singleReminderNotification in reminderNotificaions)
                    {
                        reminderNotificaionsList.Add(new
                        {
                            id = singleReminderNotification.Id_reminder_notification,
                            kidId = singleReminderNotification.Id_child,
                            notificationOverlap = singleReminderNotification.Days_number,
                        });
                    }
                    return reminderNotificaionsList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("DeleteNotification")]
        public object DeleteNotification(int notificationId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                wasSuccess = true;
                resultList = DeleteNotificationDetailed(notificationId, token).ToList();
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
        private IEnumerable<object> DeleteNotificationDetailed(int notificationId, string token)
        {
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
                    Reminder_notification reminderNotification = db.Reminder_notifications.FirstOrDefault(i => i.Id_reminder_notification == notificationId);
                    db.Reminder_notifications.Remove(reminderNotification);
                    db.SaveChanges();

                    var reminderNotificaions = db.Reminder_notifications.Where(i => i.Id_user == userId);
                    var reminderNotificaionsList = new List<object>();

                    foreach (var singleReminderNotification in reminderNotificaions)
                    {
                        reminderNotificaionsList.Add(new
                        {
                            id = singleReminderNotification.Id_reminder_notification,
                            kidId = singleReminderNotification.Id_child,
                            notificationOverlap = singleReminderNotification.Days_number,
                        });
                    }
                    return reminderNotificaionsList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("ChangeMetaNotification")]
        public object ChangeMetaNotification(bool isSubscribed, string token)
        {
            return ChangeMetaNotificationDetailed(isSubscribed, token);
        }
        private object ChangeMetaNotificationDetailed(bool isSubscribed, string token)
        {
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
                    Information_notification informationNotification = db.Information_notifications.FirstOrDefault(i => i.Id_user == userId);
                    if(informationNotification == null)
                    {
                        db.Information_notifications.Add(new Information_notification
                        {
                            Id_user = userId,
                        });
                    }
                    else if(isSubscribed == false)
                    {
                        db.Information_notifications.Remove(informationNotification);
                    }
                    db.SaveChanges();

                    var finalResult = new
                    {
                        success = true,
                        message = String.Empty,
                        userMetaNotification = isSubscribed
                    };

                    return finalResult;
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
        [ActionName("LogOut")]
        public object LogOut(string token)
        {
            return LogOutDetailed(token);
        }
        private object LogOutDetailed(string token)
        {
            string errorMessage = string.Empty;
            try
            {
                if (Security.UserTokens.Any(i => i.Value == token))
                {
                    int userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                    Security.UserTokens.Remove(userId);
                }
                else
                {
                    throw new Exception("Wystąpił problem podczas wylogowywania");
                }

                var finalResult = new
                {
                    success = true,
                    message = errorMessage
                };
                return finalResult;
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
        [ActionName("ResetPassword")]
        public object ResetPassword(string token)
        {
            return ResetPasswordDetailed(token);
        }
        private object ResetPasswordDetailed(string token)
        {
            string errorMessage = string.Empty;
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
                string newPassword;
                newPassword = Security.GeneratePassword(15);
                User user;
                User admin;
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    ChangePasswordDetailed(newPassword, token);
                    user = db.Users.FirstOrDefault(i => i.Id_user == userId);
                    admin = db.Users.FirstOrDefault(i => i.Email.Trim() == "jakiekieszonkowe@gmail.com");
                }
                string hashedPassword = Security.HashSHA1(admin.Password + admin.UserGuid);
                Email.SendEmail(user.Email.Trim(), $"Twoje nowe hasło: {newPassword}", "Zmiana hasła", hashedPassword);
                var finalResult = new
                {
                    success = true,
                    message = errorMessage
                };

                return finalResult;
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
    }
}
