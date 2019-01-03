using jakiekieszonkowe_api.Database;
using jakiekieszonkowe_api.Exceptions;
using jakiekieszonkowe_api.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace jakiekieszonkowe_api.Controllers
{
    public class UserController : ApiController
    {
        [AcceptVerbs("GET", "POST")]
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
                Account_registration_date = DateTime.Now,
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
                        var userChildrenList = new List<object>();
                        var children = user.Child.Where(i => i.Id_user == user.Id_user);
                        foreach (var child in children)
                        {
                            userChildrenList.Add(new
                            {
                                id = child.Id_child,
                                name = child.First_name,
                                age = DateTime.Now.Year - child.Date_of_birth.Value.Year,
                                schoolTypeId = child.Id_education_stage,
                                quota = child.Current_amount_of_money,
                                //paymentPeriodId = child.Id
                                //paymentDate = child.Reminder_notification.FirstOrDefault(i=> i.Id_child == child.Id_child).
                                //            quota: int,
                                //            paymentPeriodId: int,
                                //            paymentDate:                    //"YYYY-MM-DD",
                                //            prevPaymentDate:                //"YYYY-MM-DD",
                                //nextPaymentDate:                //"YYYY-MM-DD",
                                provinceId = child.City.Id_province,
                                cityId = child.Id_city,
                                //moneyIncludes:  
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

                            userData = new                                      // done
                            {
                                email = user.Email,
                                accountActivationDate = user.Account_registration_date.ToString("yyyy-MM-dd"),
                                //accountLastLogInDate =  user.Last_login_date.ToString("yyyy-MM-dd"),                       //"YYY -MM-DD",
                                provinceId = user.City.Id_province,
                                cityId = user.Id_city,
                                province = user.City.Province.Name,
                                city = user.City.Name
                            },
                            userKids = userChildrenList,
                            userNotifications = reminderNotificationsList,      // done
                            userMetaNotification = userMetaNotificationElement  // done
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
    }
}
