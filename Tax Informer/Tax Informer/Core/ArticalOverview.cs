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
        /// <summary>
        /// Format : YYYYMMDD
        /// </summary>
        public string Date { get; set; } = string.Empty; //TODO: Make the type of Date to int for easy comparison of latest post
        public Category[] Categorys { get; set; } = null;
        public string LinkOfActualArtical { get; set; } = string.Empty;

        public Artical ToArtical()
        {
            Artical a = new Artical();
            a.Title = Title;
            a.Authors = Authors;
            a.Date = Date;
            a.Authors = Authors;
            return a;
        }

        public Category ToCategory(int index)
        {
            return new Category()
            {
                Name = Title,
                Link = LinkOfActualArtical
            };
        }

        public ArticalOverview() { }
        public ArticalOverview(Bundle bundle)
        {
            Title = bundle.GetString("title");
            SummaryText = bundle.GetString("summaryText");
            Date = bundle.GetString("date");
            LinkOfActualArtical = bundle.GetString("articalLink");
            //TODO: Read Authors[] and Category[] from bundle
        }

        public Bundle ToBundle()
        {
            //TODO: **Complete the full Conversion of ArticalOverview to bundle
            Bundle b = new Bundle();
            b.PutString("title", Title);
            b.PutString("summaryText", SummaryText);
            b.PutString("date", Date);
            b.PutString("articalLink", LinkOfActualArtical);
            //TODO: Convert Authors[] and Category[] of ArticalOverview to bundle
            return b;
        }
    }
}