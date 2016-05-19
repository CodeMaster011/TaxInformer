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

namespace Tax_Informer.Core
{
    internal sealed class Category: Java.Lang.Object
    {
        public string Name { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;

        public Bundle ToBundle()
        {
            //TODO: Convert Category to bundle
            return null;
        }
    }
}