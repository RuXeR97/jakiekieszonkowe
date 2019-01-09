using jakiekieszonkowe_api.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace jakiekieszonkowe_api.Controllers
{
    public class AdminController : ApiController
    {
        [AcceptVerbs("GET", "POST")]
        [ActionName("GetCityDictionary")]
        public object GetCityDictionary(int id)
        {
            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    var resultList = db.Cities;
                    var finalResult = new
                    {
                        success = true,
                        message = String.Empty,
                        list = resultList
                    };

                    return finalResult;
                }
            }
            catch (Exception ex)
            {
                var finalResult = new
                {
                    success = false,
                    message = ex.Message,
                };

                return finalResult;
            }
        }
    }
}
