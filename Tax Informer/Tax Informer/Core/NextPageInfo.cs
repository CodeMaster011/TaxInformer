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
    internal class NextPageInfo
    {
        public OverviewType overviewType { get; set; } = OverviewType.UNKNOWN;
        public string Link { get; set; } = string.Empty;
    }
}