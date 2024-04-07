﻿using brew_master_pro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace brew_master_pro.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        BrewEntities db = new BrewEntities();

        [HttpPost, Route("signup")]
        public HttpResponseMessage Signup([FromBody] User user)
        {
            try
            {
                User existingUser = db.Users.Where(u => u.email == user.email).FirstOrDefault();
                if (existingUser == null)
                {
                    user.role = "user";
                    user.status = "false";
                    db.Users.Add(user);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = "Successfully Registered" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = "Email already Exist" });
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        [HttpPost, Route("login")]
        public HttpResponseMessage Login([FromBody] User user)
        {
            try
            {
                User existingUser = db.Users.Where(u => (u.email == user.email && u.password == user.password)).FirstOrDefault();
                if (existingUser != null)
                {
                    if (existingUser.status == "true")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { token = TokenManager.GenerateToken(existingUser.email, existingUser.role) });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "Wait for Admin Approval" });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "Incorrect Username or Password" });
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }
        [HttpGet, Route("checkToken")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage checkToken()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { message = "true" });
        }

        [HttpGet, Route("getAllUser")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetAllUser() 
        {
            try 
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                var result = db.Users.Select(u => new { u.id, u.name, u.contactNumber, u.email, u.status, u.role }).Where(x => (x.role == "user")).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}





