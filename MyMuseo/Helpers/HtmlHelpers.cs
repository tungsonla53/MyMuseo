using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Helpers
{
    public static class HtmlHelpers
    {
        public static string BoolToYesNo(this System.Web.Mvc.HtmlHelper helper, bool value)
        {
            if (value == true)
            {
                return "Yes";
            }
            else
            {
                return "No";
            }
        }

        public static string BoolToNoYes(this System.Web.Mvc.HtmlHelper helper, bool value)
        {
            if (value == true)
            {
                return "No";
            }
            else
            {
                return "Yes";
            }
        }
    }

}