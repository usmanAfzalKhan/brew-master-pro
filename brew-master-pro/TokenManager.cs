﻿using brew_master_pro.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace brew_master_pro
{
    public class TokenManager
    {
        public static string Secret = "pucsyodirqbzjevwomtkdnrzifbxpgvmxfwrsedihpnaorvctejxyuwnkbmxyagcmtldwqjlhwmxbkrrkxqcsyvpfhjebzwtvmsoytidzbwqmctupjxysvspwltcjzixteiqkgnxruubayxfsvlcikcxnwrytueogmxlnrzhrqkjvctwzedozrpvcwnmfyvoztbnulxgzldrpakbhtqjagvienwdqsybifrnlnmsvrtxkorwxjedqbnfzgqpjxdmhoajnzrmtoxdybpmfxqwakscudljgtmrpuomhzfsnkviyqenbmwtzqaxrthvfknbwusdlkajecqfsvzjxomgavkqyxnzjxqrzdngoyluekqbnvhi...";

        public static string GenerateToken(string email, string role)
        {
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email), new Claim(ClaimTypes.Role, role) }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }

        public static ClaimsPrincipal GetPrincipal(string token) 
        {
            try 
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                if(jwtToken == null) 
                {
                    return null;
                }
                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret))
                };

                SecurityToken securityToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out securityToken);
                return principal;
            }
            catch (Exception ex) 
            {  
                return null;
            }
        }

        public static TokenClaim ValidateToken(string RawToken)
        {
            string[] array = RawToken.Split(' ');
            var token = array[1];
            ClaimsPrincipal principal = GetPrincipal(token);
            if (principal == null)
                return null;
            ClaimsIdentity identity = null;
            try 
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch(Exception ex) 
            {
                return null;
            }
            TokenClaim tokenClaim = new TokenClaim();
            var temp = identity.FindFirst(ClaimTypes.Email);
            tokenClaim.Email = temp.Value;
            temp = identity.FindFirst(ClaimTypes.Role);
            tokenClaim.Role = temp.Value;
            return tokenClaim;
        }
    }
}