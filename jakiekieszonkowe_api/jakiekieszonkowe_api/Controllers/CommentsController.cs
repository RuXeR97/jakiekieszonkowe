using jakiekieszonkowe_api.Database;
using jakiekieszonkowe_api.Other;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace jakiekieszonkowe_api.Controllers
{


    public class CommentsController : ApiController
    {
        [AcceptVerbs("GET", "POST")]
        [ActionName("GetComments")]
        public object GetComments(int provinceId, int cityId, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                if (!Security.UserTokens.Any(i => i.Value == token))
                    throw new Exception("Identyfikacja użytkownika nie powiodła się.");

                wasSuccess = true;
                resultList = GetCommentsDetailed(cityId, provinceId).ToList();
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
        private IEnumerable<object> GetCommentsDetailed(int cityId, int provinceId)
        {
            IEnumerable<Comment> cityComments;
            var finalResultComments = new List<object>();
            using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
            {
                if (cityId == -1 && provinceId == -1)
                    cityComments = db.Comments.Where(i => i.Id_city == null && i.Id_province == null);
                else if (cityId == -1)
                    cityComments = db.Comments.Where(i => i.Id_city == null && i.Id_province == provinceId);
                else
                    cityComments = db.Comments.Where(i => i.Id_city == cityId && i.Id_province == provinceId);

                foreach (var item in cityComments)
                {
                    bool isLiked = db.Likes.Any(i => i.Id_user == item.Id_user && i.Id_comment == item.Id_comment);
                    finalResultComments.Add(new
                    {
                        id = item.Id_comment,
                        author = item.User.Email,
                        content = item.Content.Trim(),
                        upvotes = item.Likes_amount,
                        liked = isLiked
                    });
                }
            }
            
            return finalResultComments;
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("AddComment")]
        public object AddComment(int provinceId, int cityId, string content, string token)
        {
            bool wasSuccess = false;
            string errorMessage = string.Empty;
            var resultList = new List<object>();
            try
            {
                int userId;
                if (Security.UserTokens.Any(i=> i.Value == token))
                {
                     userId = Security.UserTokens.FirstOrDefault(i => i.Value == token).Key;
                }
                else
                {
                    throw new Exception("Identyfikacja użytkownika nie powiodła się");
                }
                wasSuccess = true;
                AddCommentDetailed(cityId, provinceId, content, userId);
                resultList = GetCommentsDetailed(cityId, provinceId).ToList();
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
        private void AddCommentDetailed(int cityId, int provinceId, string content, int userId)
        {
            Comment comment = new Comment()
            {
                Content = content,
                Id_user = userId,
                Creation_date = DateTime.Now.ToLocalTime(),
                Likes_amount = 0,
            };

            if (cityId != -1)
                comment.Id_city = cityId;

            if (provinceId != -1)
                comment.Id_province = provinceId;

            int asd;

            try
            {
                using (JakieKieszonkoweEntities db = new JakieKieszonkoweEntities())
                {
                    db.Comments.Add(comment);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AcceptVerbs("GET", "POST")]
        [ActionName("ToggleCommentUpvote")]
        public object ToggleCommentUpvote(int commentId, bool isLiked, string token)
        {
            return ToggleCommentUpvoteDetailed(commentId, isLiked, token);
        }
        private object ToggleCommentUpvoteDetailed(int commentId, bool isLiked, string token)
        {
            int userId;
            try
            {
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
                    Comment comment = db.Comments.FirstOrDefault(i => i.Id_comment == commentId);
                    Like like;
                    int? cityId, provinceId;
                    if (isLiked)
                    {
                        comment.Likes_amount = comment.Likes_amount + 1;
                        db.Comments.Attach(comment);
                        db.Entry(comment).State = EntityState.Modified;

                        like = new Like
                        {
                            Amount_of_likes = 1,
                            Id_user = userId,
                            Id_comment = comment.Id_comment
                        };
                        db.Likes.Add(like);
                        db.SaveChanges();

                        like = db.Likes.FirstOrDefault(i => i.Id_comment == commentId && i.Id_user == userId);
                        cityId = like.Comment.Id_city;
                        provinceId = like.Comment.Id_province;
                    }
                    else
                    {
                        comment.Likes_amount--;
                        db.Comments.Attach(comment);
                        db.Entry(comment).State = EntityState.Modified;

                        like = db.Likes.FirstOrDefault(i => i.Id_comment == commentId && i.Id_user == userId);
                        cityId = like.Comment.Id_city;
                        provinceId = like.Comment.Id_province;
                        db.Likes.Remove(like);
                    }
                    db.SaveChanges();

                    if (cityId == null)
                        cityId = -1;

                    if (provinceId == null)
                        provinceId = -1;

                    return GetComments(cityId.Value, provinceId.Value, token);
                }
            }
            catch(Exception ex)
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
