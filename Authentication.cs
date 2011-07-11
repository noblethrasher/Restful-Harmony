using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nthings2.src.app
{
    public class Authentication : IHttpModule
    {
        public void Dispose()
        {
            
        }

        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += delegate
            {
                var ctx = context.Context;
                var req = context.Request;

                var user = req.Form["user"];
                var pass = req.Form["pass"];
                var cookie = req.Cookies["AUTH"];

                if (user != null && pass != null)
                {
                    
                }


            };
        }
    }
}