using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nthings2.src.app
{
    public interface Controller : IHttpHandler
    {
        Controller HandleMessage(string message, HttpContextBase context);
    }
}