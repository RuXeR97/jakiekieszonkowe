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
                        if (Security.UserTokens.Any(i => i.Value == generatedToken))
                            Security.UserTokens.Add(user.Id_user, generatedToken);

                        var userChildrenList = new List<object>();
                        var children = user.Child.Where(i => i.Id_user == user.Id_user);
                        foreach (var child in children)
                        {
                            userChildrenList.Add(new
                            {
                                id = child.Id_child,
                                name = child.First_name.Trim(),
                                age = DateTime.Now.ToLocalTime().ToLocalTime().Year - child.Date_of_birth.Value.Year,
                                dateOfBirth = child.Date_of_birth?.ToString("YYYY-MM-DD"),
                                schoolTypeId = child.Id_education_stage,
                                quota = child.Current_amount_of_money,
                                paymentPeriodId = child.Id_payout_period,
                                paymentDate = child.Date_of_payout.ToString("YYYY-MM-DD"),
                                prevPaymentDate = child.Date_of_payout.PreviousPaymentDate()?.ToString("YYYY-MM-DD"),
                                nextPaymentDate = child.Date_of_payout.NextPaymentDate()?.ToString("YYYY-MM-DD"),
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
                                email = user.Email.Trim(),
                                accountActivationDate = user.Account_registration_date.ToString("YYYY-MM-DD"),
                                accountLastLogInDate =  user.Last_login_date?.ToString("YYYY-MM-DD"),
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
                            name = singleChild.First_name.Trim(),
                            age = DateTime.Now.ToLocalTime().ToLocalTime().ToLocalTime().Year - singleChild.Date_of_birth.Value.Year,
                            dateOfBirth = singleChild.Date_of_birth?.ToString("YYYY-MM-DD"),
                            schoolTypeId = singleChild.Id_education_stage,
                            quota = singleChild.Current_amount_of_money,
                            paymentPeriodId = singleChild.Id_payout_period,
                            paymentDate = singleChild.Date_of_payout.ToString("YYYY-MM-DD"),
                            prevPaymentDate = singleChild.Date_of_payout.PreviousPaymentDate()?.ToString("YYYY-MM-DD"),
                            nextPaymentDate = singleChild.Date_of_payout.NextPaymentDate()?.ToString("YYYY-MM-DD"),
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
        public object EditChild(int childId, DateTime dateOfBirth, string name, double quota, int cityId, DateTime paymentDate,
            int paymentPeriodId, int schoolTypeId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                wasSuccess = true;
                resultList = EditChildDetailed(childId, dateOfBirth, name, quota, cityId, null, paymentDate, paymentPeriodId, schoolTypeId, token).ToList();
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
        private IEnumerable<object> EditChildDetailed(int childId, DateTime dateOfBirth, string name, double quota, int cityId, List<int> moneyIncludes,
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

                    foreach (var singleChild in children)
                    {
                        userChildrenList.Add(new
                        {
                            id = singleChild.Id_child,
                            name = singleChild.First_name.Trim(),
                            age = DateTime.Now.ToLocalTime().ToLocalTime().ToLocalTime().Year - singleChild.Date_of_birth.Value.Year,
                            dateOfBirth = singleChild.Date_of_birth?.ToString("YYYY-MM-DD"),
                            schoolTypeId = singleChild.Id_education_stage,
                            quota = singleChild.Current_amount_of_money,
                            paymentPeriodId = singleChild.Id_payout_period,
                            paymentDate = singleChild.Date_of_payout.ToString("YYYY-MM-DD"),
                            prevPaymentDate = singleChild.Date_of_payout.PreviousPaymentDate()?.ToString("YYYY-MM-DD"),
                            nextPaymentDate = singleChild.Date_of_payout.NextPaymentDate()?.ToString("YYYY-MM-DD"),
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
                    Child child = db.Children.FirstOrDefault(i => i.Id_child == childId);
                    db.Children.Remove(child);
                    db.SaveChanges();

                    var children = db.Children.Where(i => i.Id_user == userId);
                    var userChildrenList = new List<object>();

                    foreach (var singleChild in children)
                    {
                        userChildrenList.Add(new
                        {
                            id = singleChild.Id_child,
                            name = singleChild.First_name.Trim(),
                            age = DateTime.Now.ToLocalTime().ToLocalTime().ToLocalTime().Year - singleChild.Date_of_birth.Value.Year,
                            dateOfBirth = singleChild.Date_of_birth?.ToString("YYYY-MM-DD"),
                            schoolTypeId = singleChild.Id_education_stage,
                            quota = singleChild.Current_amount_of_money,
                            paymentPeriodId = singleChild.Id_payout_period,
                            paymentDate = singleChild.Date_of_payout.ToString("YYYY-MM-DD"),
                            prevPaymentDate = singleChild.Date_of_payout.PreviousPaymentDate()?.ToString("YYYY-MM-DD"),
                            nextPaymentDate = singleChild.Date_of_payout.NextPaymentDate()?.ToString("YYYY-MM-DD"),
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
                        accountActivationDate = user.Account_registration_date.ToString("YYYY-MM-DD"),
                        accountLastLogInDate = user.Last_login_date?.ToString("YYYY-MM-DD"),
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
    }
}
