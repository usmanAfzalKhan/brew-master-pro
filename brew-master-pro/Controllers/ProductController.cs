using brew_master_pro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
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
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                product.status = "true";
                db.Products.Add(product);
                db.SaveChanges();
                response.message = "Product added successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost, Route("getAllProduct")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetAllProduct([FromBody] Product product)
        {
            try
            {
                var result = from Products in db.Products
                             join Category in db.Categories
                             on Products.categoryId equals Category.id
                             select new
                             {
                                 Products.id,
                                 Products.name,
                                 Products.description,
                                 Products.price,
                                 Products.status,
                                 categoryId = Category.id,
                                 categoryName = Category.name
                             };
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost, Route("getProductByCategory/{id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetProductByCategory(int id)
        {
            try
            {
                var result = db.Products.Where(x => x.categoryId == id && x.status == "true").Select(x => new { x.id, x.name }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }

        }
        [HttpPost, Route("getProductById/{id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetProductById(int id)
        {
            try
            {
                Product productObj = db.Products.Find(id);
                return Request.CreateResponse(HttpStatusCode.OK, productObj);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        [HttpPost, Route("updateProduct")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage UpdateProduct([FromBody] Product product)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product productObj = db.Products.Find(product.id);
                if (productObj == null)
                {
                    response.message = "Product id is not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                productObj.name = product.name;
                productObj.categoryId = product.categoryId;
                productObj.description = product.description;
                productObj.price = product.price;
                db.Entry(productObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.message = "Product updates successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost, Route("deleteProduct/{id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage DeleteProduct(int id) 
        {
            try 
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product productObj = db.Products.Find(id);
                if (productObj == null) 
                {
                    response.message = "Product id is not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                db.Products.Remove(productObj);
                db.SaveChanges();
                response.message = "Product Deleted Successfully";
                return Request.CreateResponse(HttpStatusCode.OK,response);
            }
            catch (Exception ex) 
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}

