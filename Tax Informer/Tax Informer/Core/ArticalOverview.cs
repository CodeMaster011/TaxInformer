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
    internal sealed class ArticalOverview
    {
        public string Title { get; set; } = string.Empty;
        public string SummaryText { get; set; } = string.Empty;
        public Author[] Authors { get; set; } = null;
        public string Date { get; set; } = string.Empty; //TODO: Make the type of Date to int for easy comparison of latest post
        public Category[] Categorys { get; set; } = null;
        public string LinkOfActualArtical { get; set; } = string.Empty;

        public Artical ToArtical()
        {
            Artical a = new Artical();
            a.Title = Title;
            a.Authors = Authors;
            a.Date = Date;
            return a;
        }
    }
}