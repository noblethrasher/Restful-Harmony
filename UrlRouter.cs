using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace nthings2.src.app
{
    public abstract class UrlRouter<T> : IHttpModule where T : Controller, new()
    {

        static string[] excludedExtensions = new[]
            {
                ".jpg",
                ".gif",
                ".jpeg",
                ".png",
                ".css",
                ".js",
                ".cs",
            };

        public void Dispose()
        {
            
        }

        public void Init(HttpApplication context)
        {
            context.PostResolveRequestCache += delegate
            {
                var ctx = new HttpContextWrapper(context.Context);                

                if (excludedExtensions.All (x => !ctx.Request.Path.EndsWith (x, StringComparison.OrdinalIgnoreCase)))
                {
                    Controller controller = new T ();

                    foreach (var path in ctx.Request.Path.Split('/'))
                    {
                        try
                        {
                            controller = controller.HandleMessage (path, ctx);
                        }
                        catch (HttpException ex)
                        {
                            ctx.RemapHandler (ex);
                            goto stop; //yeah yeah...
                        }
                    }

                    ctx.RemapHandler (new ControllerExceptionCatcher(controller));
                }

            stop:
                return;
            };
        }

        class ControllerExceptionCatcher : IHttpHandler
        {
            Controller controller;

            public ControllerExceptionCatcher(Controller controller)
            {
                this.controller = controller;
            }

            public bool IsReusable
            {
                get { return controller.IsReusable; }
            }

            public void ProcessRequest(HttpContext context)
            {
                try
                {
                    controller.ProcessRequest (context);
                }
                catch (src.app.HttpException ex)
                {
                    ex.ProcessRequest (context);
                }
            }
        }
    }
}