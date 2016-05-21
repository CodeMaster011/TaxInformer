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

using System.IO;

namespace Tax_Informer
{
    static class MyLog
    {
        public static string LogFilePath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/TaxInformer/log.txt";

        public static void Log(string tag, string message)
        {
            try
            {
                if (!MyGlobal.IsLogEnable) return;

                StreamWriter logStream = new StreamWriter(LogFilePath, true);
                logStream.WriteLine($"{tag}\t=\t{message}");
                logStream.Close();

            }
            catch (Exception ex) { }            
        }
        public static void Log(object sender, string message) => Log(sender, null, message);
        public static void Log(object sender, string tag, string message)
        {
            var logAttr = (LogStatusAttribute)sender.GetType().GetCustomAttributes(typeof(LogStatusAttribute), true).FirstOrDefault();
            if (logAttr == null || logAttr.IsLogEnable) Log(sender?.GetType().Name + (tag == null ? "" : (":" + tag)), message);
        }
    }

    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class LogStatusAttribute : Attribute
    {
        public string TagName { get; set; }
        public bool IsLogEnable { get; set; }
    }
}