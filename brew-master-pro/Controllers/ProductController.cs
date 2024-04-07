using brew_master_pro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace brew_master_pro.Controllers
{
    [RoutePrefix("api/product")]
    public class ProductController : ApiController
    {
        BrewEntities db = new BrewEntities();
        Response response = new Response();

        [HttpPost, Route("addNewProduct")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage AddNewProduct([FromBody] Product product)
        {
            try 
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized); 
                }
                product.status = "true";
                db.Products.Add(product);  
                db.SaveChanges();
                response.message = "Product added successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
