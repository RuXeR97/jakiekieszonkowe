using jakiekieszonkowe_api.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace jakiekieszonkowe_api.Controllers
{
    
    public class CommentsController : ApiController
    {
        Comment_city[] comments_city = new Comment_city[]
        {
            new Comment_city { Id_city = 1, Content = "Przykladowy komentarz", Creation_date = DateTime.Now, Id_comment_city = 2, Id_user = 5, Likes_amount = 3, User = new User() { Id_user = 2, Email = "mail123@smiw.com"} },
            new Comment_city { Id_city = 2, Content = "Przykladowy komentarz2", Creation_date = DateTime.Now, Id_comment_city = 3, Id_user = 4, Likes_amount =53, User =  new User() { Id_user = 23, Email = "tetete3@smiw.com"}}
        };

        public object GetComments(int provinceId, int cityId)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                wasSuccess = true;
                if (provinceId == -1 && cityId == -1)
                {
                    resultList = GetCountryComments().ToList();
                }
                else if (cityId == -1)
                {
                    resultList = GetProvinceComments(provinceId).ToList();
                }
                else
                {
                    resultList = GetCityComments(cityId).ToList();
                }
            }
            catch(Exception ex)
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
        private IEnumerable<object> GetCityComments(int cityId)
        {
            IEnumerable<Comment_city> cityComments;
            var finalResultComments = new List<object>();
            using (DatabaseEntities db = new DatabaseEntities())
            {
                cityComments = db.Comments_city.Where(i => i.Id_city == cityId);

                foreach (var item in cityComments)
                {
                    finalResultComments.Add(new
                    {
                        id = item.Id_comment_city,
                        author = item.User.Email,
                        content = item.Content,
                        upvotes = item.Likes_amount,
                        liked = true
                    });
                }
            }

            

            return finalResultComments;
        }
        private IEnumerable<object> GetProvinceComments(int provinceId)
        {
            IEnumerable<Comment_province> provinceComments;
            var finalResultComments = new List<object>();
            using (DatabaseEntities db = new DatabaseEntities())
            {
                provinceComments = db.Comments_province.Where(i => i.Id_province == provinceId);

                foreach (var item in provinceComments)
                {
                    finalResultComments.Add(new
                    {
                        id = item.Id_comment_province,
                        author = item.User.Email,
                        content = item.Content,
                        upvotes = item.Likes_amount,
                        liked = true
                    });
                }
            }

            

            return finalResultComments;
        }
        private IEnumerable<object> GetCountryComments()
        {
            IEnumerable<Comment_country> countryComments;
            var finalResultComments = new List<object>();
            using (DatabaseEntities db = new DatabaseEntities())
            {
                countryComments = db.Comments_country;

                foreach (var item in countryComments)
                {
                    finalResultComments.Add(new
                    {
                        id = item.Id_comment_country,
                        author = item.User.Email,
                        content = item.Content,
                        upvotes = item.Likes_amount,
                        liked = true
                    });
                }
            }

           

            return finalResultComments;
        }


        public object AddComment(int provinceId, int cityId, string content, int userId)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                wasSuccess = true;
                if (provinceId == -1 && cityId == -1)
                {
                    AddCountryComment(content, userId);
                    resultList = GetCountryComments().ToList();
                }
                else if (cityId == -1)
                {
                    AddProvinceComment(provinceId, content, userId);
                    resultList = GetProvinceComments(provinceId).ToList();
                }
                else
                {
                    AddCityComment(cityId, content, userId);
                    resultList = GetCityComments(cityId).ToList();
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            var finalResult = new
            {
                success = wasSuccess,
                message = errorMessage,
                list = resultList
            };

            return finalResult;
        }
        private void AddCityComment(int cityId, string content, int userId)
        {
            Comment_city commentCity = new Comment_city()
            {
                Content = content,
                Id_user = userId,
                Id_city = cityId,
                Creation_date = DateTime.Now,
            };

            try
            {
                using (DatabaseEntities db = new DatabaseEntities())
                {
                    db.Comments_city.Add(commentCity);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void AddProvinceComment(int provinceId, string content, int userId)
        {
            Comment_province commentProvince = new Comment_province()
            {
                Content = content,
                Id_user = userId,
                Id_province = provinceId,
                Creation_date = DateTime.Now,
            };

            try
            {
                using (DatabaseEntities db = new DatabaseEntities())
                {
                    db.Comments_province.Add(commentProvince);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void AddCountryComment(string content, int userId)
        {
            Comment_country commentCountry = new Comment_country()
            {
                Content = content,
                Id_user = userId,
                Creation_date = DateTime.Now,
            };

            try
            {
                using (DatabaseEntities db = new DatabaseEntities())
                {
                    db.Comments_country.Add(commentCountry);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
