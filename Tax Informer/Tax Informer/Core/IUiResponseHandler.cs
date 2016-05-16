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
    //TODO: Allow the UI to handle errors
    interface IUiArticalOverviewResponseHandler
    {
        void ArticalOverviewProcessedCallback(string uid, string url, ArticalOverview[] articalOverviews, OverviewType overviewType, string nextPageUrl);
    }
    interface IUiArticalResponseHandler
    {
        void ArticalProcessedCallback(string uid, string url, Artical artical);
    }
    interface IUiOfflineArticalOverviewResponseHandler
    {
        void OfflineArticalOverviewProcessedCallback(string transactionId, ArticalOverviewOffline[] articalOverviews);
    }
}