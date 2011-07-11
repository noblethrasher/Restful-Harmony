using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace nthings2.src.app
{
    public static class Utils
    {
        public static T GetView<T>(this string s, HttpContext context) where T : IHttpHandler
        {
            return (T)PageParser.GetCompiledPageInstance (s, context.Server.MapPath (s), context);
        }

    }


    public static class StringUtils
    {
        public static string Capitalize(this string s)
        {
            if (s != null && s.Length > 0)
            {
                var chars = s.ToCharArray ();

                chars[0] = char.ToUpper (chars[0]);

                return new string (chars);
            }

            return s;
        }        
    }
}