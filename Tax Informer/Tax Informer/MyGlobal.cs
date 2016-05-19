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
using Tax_Informer.Core;

namespace Tax_Informer
{
    internal static class MyGlobal
    {
        public static bool IsRunning = true;
        public static int NextPageContentNumber = 5;
        public static IAnalysisModule analysisModule = new AnalysisModule();
        public static IOfflineModule offlineModule = new OfflineModule();
        public static IOnlineModule onlineModule = new OnlineModule();
        public static ICache diskCache = new DiskCache(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/TaxInformer/Cache", 0);
        public static IDatabaseModule database = new DatabaseModule(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/TaxInformer/database.db");

        public static string UidGenerator() => Guid.NewGuid().ToString();

        public static string GetHumanReadableDate(string formatedDate)
        {
            if (formatedDate == null || formatedDate == string.Empty) return null;

            int currentDate = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

            var req = int.Parse(formatedDate);
            var diff = currentDate - req;
            switch (diff)
            {
                case 0:
                    return "Today";
                case 1:
                    return "Yesterday";
                case 2:
                    return "2 day ago";
                case 3:
                    return "3 day ago";
                default:
                    break;
            }
            var dd = req % 100;
            var mm = ((req - dd) / 100) % 100;
            if (currentDate.ToString().Substring(0, 4) == formatedDate.Substring(0, 4))
                return Helper.monthArray[mm - 1] + " " + dd.ToString();
            else
                return $"{dd} {Helper.monthArray[mm - 1]} {formatedDate.Substring(0, 4)}";
        }

        public static void ChangeStatusBarColor(Window window, string color)
        {
            try
            {
                // clear FLAG_TRANSLUCENT_STATUS flag:
                window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                // add FLAG_DRAWS_SYSTEM_BAR_BACKGROUNDS flag to the window
                window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                // finally change the color
                window.SetStatusBarColor(Android.Graphics.Color.ParseColor(color));
            }
            catch (Exception) { }
        }

        public static void StartActivityArtical(Context context, ArticalOverview overview, string websiteKey, bool isOfflineArtical = false)
        {
            var b = overview.ToBundle();
            Intent intent = new Intent(context, typeof(Activities.ArticalActivity));
            intent.PutExtra(Activities.ArticalActivity.PassArticalOverviewObj, b );
            intent.PutExtra(Activities.ArticalActivity.PassWebsiteKey, websiteKey);
            intent.PutExtra(Activities.ArticalActivity.PassIsOffline, isOfflineArtical);
            context.StartActivity(intent);
        }
        public static void StartActivityOffline(Context context)
        {
            Intent intent = new Intent(context, typeof(Activities.OfflineActivity));
            context.StartActivity(intent);
        }
        public static void StartActivityOverview(Context context, string websiteKey)
        {
            Intent intent = new Intent(context, typeof(Activities.OverviewActivityWithFragments));
            intent.PutExtra(Activities.OverviewActivityWithFragments.PassWebsiteKey, websiteKey);
            context.StartActivity(intent);
        }
    }
}