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
        private static Dictionary<string, Core.Website> hiddenWebsites = new Dictionary<string, Core.Website>();

        public static Dictionary<string, Core.Website> websites = new Dictionary<string, Core.Website>()
        {
            ["taxguru"] = new Websites.TaxguruWebsite(),
            ["charteredClub"] = new Websites.CharteredClubWebsite(),
            ["ICAI"] = new Websites.IcaiWebsiteGroup()
        };
        
        public static Core.Website GetWebsite(string key) => websites.ContainsKey(key) ? websites[key] : hiddenWebsites[key];
        public static void AddHiddenWebsite(string key, Core.WebsiteGroup.ChildWebsite website) => hiddenWebsites.Add(key, website);
    }
}