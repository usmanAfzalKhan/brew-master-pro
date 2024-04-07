using brew_master_pro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace brew_master_pro.Controllers
{

    [RoutePrefix("api/category")]
    public class CategoryController : ApiController
    {
        BrewEntities db = new BrewEntities();
        Response response = new Response();

        [HttpPost, Route("addNewCategory")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage AddNewMessage([FromBody]Category category)
        {
            try 
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                db.Categories.Add(category);
                db.SaveChanges();
                response.message = "Category Added Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch  (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
