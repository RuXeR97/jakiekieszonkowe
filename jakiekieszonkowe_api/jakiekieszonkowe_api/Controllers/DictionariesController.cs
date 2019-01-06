using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace jakiekieszonkowe_api.Controllers
{
    public class DictionariesController : ApiController
    {
        //[AcceptVerbs("GET", "POST")]
        //[ActionName("RegisterUser")]
        //public object RegisterUser(string email, string password, int cityId)
        //{
        //    bool wasSuccess = false;
        //    string errorMessage = string.Empty;
        //    var resultList = new List<object>();
        //    try
        //    {
        //        wasSuccess = true;
        //        RegisterUserDetailed(email, password, cityId);
        //    }
        //    catch (Exception ex)
        //    {
        //        errorMessage = ex.Message;
        //        wasSuccess = false;
        //    }

        //    var finalResult = new
        //    {
        //        success = wasSuccess,
        //        message = errorMessage,
        //    };

        //    return finalResult;
        //}
        //private void RegisterUserDetailed(string email, string password, int cityId)
        //{
        //    Guid userGuid = System.Guid.NewGuid();
        //    string hashedPassword = Security.HashSHA1(password + userGuid.ToString());

        //    User comment = new User()
        //    {
        //        Email = email,
        //        Password = hashedPassword,
        //        UserGuid = userGuid,
        //        Id_city = cityId,
        //        Account_registration_date = DateTime.Now.ToLocalTime().ToLocalTime(),
        //    };

        //    try
        //    {
        //        using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
        //        {
        //            db.Users.Add(comment);
        //            db.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
    }
}
