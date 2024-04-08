using brew_master_pro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace brew_master_pro.Controllers
{
    [RoutePrefix("api/dashboard")]
    public class DashboardController : ApiController
    {
        BrewEntities db = new BrewEntities();
        [HttpGet, Route("details")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetDetails()
        {
            try
            {
                var data = new
                {
                    category = db.Categories.Count(),
                    product = db.Products.Count(),
                    bill = db.Bills.Count(),
                    user = db.Users.Count(),
                };
                return Request.CreateResponse(HttpStatusCode.OK, data);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }

        }

    }
}
