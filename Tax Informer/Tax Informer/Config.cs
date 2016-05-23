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
using System.Reflection;
namespace Tax_Informer
{
    static class Config
    {
        private static Dictionary<string, Core.Website> hiddenWebsites = new Dictionary<string, Core.Website>();

        public static Dictionary<string, Core.Website> websites = loadWebsites();

        private static Dictionary<string,Core.Website> loadWebsites()
        {
            var assemble = Assembly.GetAssembly(typeof(Core.Website));
            var allTypes = assemble?.GetTypes();
            if (allTypes == null) return null;

            var websites = new Dictionary<string, Core.Website>();
            foreach (var type in allTypes)
            {
                var websiteAtt = (Core.WebsiteAttribute)type.GetCustomAttributes(typeof(Core.WebsiteAttribute), false).FirstOrDefault();
                if (websiteAtt != null)
                    websites.Add(websiteAtt.WebsiteKey, (Core.Website)Activator.CreateInstance(type));
            }
            return websites;
        }

        public static Core.Website GetWebsite(string key) => websites.ContainsKey(key) ? websites[key] : hiddenWebsites[key];
        public static void AddHiddenWebsite(string key, Core.WebsiteGroup.ChildWebsite website) => hiddenWebsites.Add(key, website);
    }
}