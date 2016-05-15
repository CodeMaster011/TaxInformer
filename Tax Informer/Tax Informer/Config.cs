using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Tax_Informer
{
    static class Config
    {
        public static Dictionary<string, Core.Website> websites = new Dictionary<string, Core.Website>()
        {
            ["taxguru"] = new Websites.TaxguruWebsite(),
            ["charteredClub"] = new Websites.CharteredClubWebsite()
        };

        public static Core.Website GetWebsite(string key) => websites[key];
    }
}