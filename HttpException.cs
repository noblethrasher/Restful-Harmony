using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nthings2.src.app
{
    public abstract class HttpException : Exception, IHttpHandler
    {
        public virtual bool IsReusable
        {
            get { return true; }
        }

        public abstract void ProcessRequest(HttpContext context);
    }

    public class NotFoundException : HttpException
    {

        string message;

        public NotFoundException(string message)
        {
            this.message = message;
        }


        public override void ProcessRequest(HttpContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.Write (message);
        }
    }
}