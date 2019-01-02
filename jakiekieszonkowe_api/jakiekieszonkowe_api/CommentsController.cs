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

        public IEnumerable<Comment_city> GetAllProducts()
        {
            return comments_city;
        }

        public object GetCommentsByProvinceAndCity(int id_province, int id_city)
        {
//            obiekt {
//            success: boolean,
//message: puste, chyba że błąd,
//list: ..
            var filteredComments_city = comments_city.Where(p => p.Id_city == id_city);
            var resultList = new List<object>();

            foreach (var item in filteredComments_city)
            {
                resultList.Add(new
                {
                    id = item.Id_comment_city,
                    author = item.User.Email,
                    content = item.Content,
                    upvotes = item.Likes_amount,
                    liked = true
                });
            }
            bool successResult = true;
            var resultFinal = new
            {
                success = successResult,
                message = String.Empty,
                list = resultList
            };

            return resultFinal;
        }

        public Comment_city GetProductByCityId(int id_city)
        {
            var product = comments_city.FirstOrDefault((p) => p.Id_city == id_city);
            if (product == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return product;
        }

        public IEnumerable<object> GetProductsByAmountOfLikes(int amount_of_likes)
        {
            var filteredComments_city = comments_city.Where(p => p.Likes_amount == amount_of_likes);
            var resultList = new List<object>();
            foreach(var item in filteredComments_city)
            {
                bool isLikedByUserWhoCommented = false;
                resultList.Add(new
                {
                    id = item.Id_comment_city,
                    author = item.User.Email,
                    content = item.Content,
                    upvotes = item.Likes_amount,
                    liked = true
                });
            }

            return resultList;
        }
    }
}
