using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace nthings2.src.app
{
    public class DefaultController : Controller
    {
        public virtual Controller HandleMessage(string message, HttpContextBase context)
        {
            if (message == "")
                return this;
            else
                throw new NotFoundException (message);
        }

        public virtual bool IsReusable
        {
            get { return false; }
        }

        protected HttpContextBase GetContext()
        {
            return new HttpContextWrapper (HttpContext.Current);
        }

        protected virtual IHttpHandler GetHandler()
        {
            string path = "/views/" + this.GetType().Name + "/index.aspx";
            
            var context = ((HttpApplication)GetContext().GetService(typeof(HttpApplication))).Context;

            return PageParser.GetCompiledPageInstance (path, context.Server.MapPath (path), context);
        }

        public virtual void ProcessRequest(HttpContext context)
        {
            GetHandler ().ProcessRequest (context);
        }
    }

    public class DefaultController<T>
    {

    }
}