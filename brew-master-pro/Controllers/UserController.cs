using brew_master_pro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Data.Common;

namespace brew_master_pro.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        BrewEntities db = new BrewEntities();
        Response response = new Response();

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
                if (tokenClaim.Role != "admin")
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
        [HttpPost, Route("updateUserStatus")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage UpdateUserStatus(User user)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                User existingUser = db.Users.Find(user.id);
                if (existingUser == null)
                {
                    response.message = "User ID is not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                existingUser.status = user.status;
                db.Entry(existingUser).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.message = "User Status Updated Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost, Route("changePassword")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage ChangePassworded(ChangePassword changePassword)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                User existingUser = db.Users.Where(x => x.email == tokenClaim.Email && x.password == changePassword.OldPassword).FirstOrDefault();


                if (existingUser != null)
                {
                    existingUser.password = changePassword.NewPassword;
                    db.Entry(existingUser).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    response.message = "Password Updated Successfully";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    response.message = "Incorrecnt Old Password";
                    return Request.CreateResponse(HttpStatusCode.BadRequest, response);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        private string createEmailBody(string email, string password)
        {
            try
            {
                string body = string.Empty;
                using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("/Template/forgot-password.html")))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{email}", email);
                body = body.Replace("{password}", password);
                body = body.Replace("{frontendUrl}", "http://localhost:4200/");
                return body;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        [HttpPost, Route("forgotPassword")]
        public async Task<HttpResponseMessage> ForgotPassword([FromBody] User user)
        {
            User existingUser = db.Users.Where(x => x.email == user.email).FirstOrDefault();
            response.message = "Password sent successfully to your email";
            if (existingUser == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            var message = new MailMessage();
            message.To.Add(new MailAddress(user.email));
            message.Subject = "Password by Brew Master Pro";
            message.Body = createEmailBody(user.email, existingUser.password);
            message.IsBodyHtml = true;
            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);
                await Task.FromResult(0);
            }
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}






