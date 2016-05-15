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
    //TODO: Website type name is not stored into ArticalOverview
    internal sealed class ArticalOverview
    {
        public string Title { get; set; } = string.Empty;
        public string SummaryText { get; set; } = string.Empty;
        public Author[] Authors { get; set; } = null;
        /// <summary>
        /// Format : YYYYMMDD
        /// </summary>
        public string Date { get; set; } = null; //TODO: Make the type of Date to int for easy comparison of latest post
        public Category[] Categorys { get; set; } = null;
        public string LinkOfActualArtical { get; set; } = string.Empty;

        public bool IsOfflineAvailable { get; set; } = false;
        public string SeenOn { get; set; } = null;
        public bool IsDatabaseConfirmed_SeenOn { get; set; } = false;
        public bool IsDatabaseConfirmed_Offline { get; set; } = false;

        public Artical ToArtical()
        {
            Artical a = new Artical();
            a.Title = Title;
            a.Authors = Authors;
            a.Date = Date;
            a.Authors = Authors;
            a.MyLink = LinkOfActualArtical;
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
            IsOfflineAvailable = bundle.GetBoolean("IsOfflineAvailable");
            SeenOn = bundle.GetString("SeenOn");
            IsDatabaseConfirmed_Offline = bundle.GetBoolean("IsDatabaseConfirmed_Offline");
            IsDatabaseConfirmed_SeenOn = bundle.GetBoolean("IsDatabaseConfirmed_SeenOn");
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
            b.PutBoolean("IsOfflineAvailable", IsOfflineAvailable);
            b.PutBoolean("IsDatabaseConfirmed_Offline", IsDatabaseConfirmed_Offline);
            b.PutBoolean("IsDatabaseConfirmed_SeenOn", IsDatabaseConfirmed_SeenOn);
            b.PutString("SeenOn", SeenOn);
            //TODO: Convert Authors[] and Category[] of ArticalOverview to bundle
            return b;
        }
    }
}