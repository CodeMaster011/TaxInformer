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
    internal sealed class Artical
    {
        public string Title { get; set; } = string.Empty;
        public Author[] Authors { get; set; } = null;
        public string Date { get; set; } = string.Empty;//TODO: Make the type of Date to int for easy comparison of latest post
        public string HtmlText { get; set; } = string.Empty;
        public ArticalOverview[] RelatedPosts { get; set; } = null;

        public Bundle ToBundle()
        {
            //TODO: Covert artical to bundle
            return null;
        }
    }
}