﻿using jakiekieszonkowe_api.Database;
using jakiekieszonkowe_api.Other;
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
    }
}
