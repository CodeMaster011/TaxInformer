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
        public static Website currentWebsite = null;
        public static bool IsRunning = true;
        public static int NextPageContentNumber = 5;
        public static IAnalysisModule analysisModule = new AnalysisModule();
        public static IOfflineModule offlineModule = new OfflineModule();
        public static IOnlineModule onlineModule = new OnlineModule();
        public static ICache diskCache = new DiskCache(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/TaxInformer/Cache", 0);


        public static string UidGenerator() => Guid.NewGuid().ToString();
    }
}