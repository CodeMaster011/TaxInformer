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
using static Tax_Informer.MyGlobal;
using Android.Webkit;
using Java.Lang;
using Android.Util;

namespace Tax_Informer.Activities
{
    [Activity(Label = "ArticalActivity")]
    internal class ArticalActivity : Activity, IUiArticalResponseHandler
    {
        public const string PassArticalOverviewObj = "articalOverviewObj";

        private WebView webview = null;
        private GridView gridview = null;
        private GridviewAdapter adapter = null;

        public Artical currentArtical = null;

        private int dpToPx(int dp)
        {
            DisplayMetrics displayMetrics = BaseContext.Resources.DisplayMetrics;
            int px = (int)System.Math.Round((double)dp * (double)((float)displayMetrics.Xdpi / (int)DisplayMetrics.DensityDefault));
            return px;
        }

        public void ArticalProcessedCallback(string uid, string url, Artical artical)
        {
            //TODO: Handle the callback response of artical
            currentArtical = artical;//cache the data

            RunOnUiThread(new Action(() =>
            {
                webview.LoadData(artical.HtmlText, "text/html; charset=utf-8", null);
                webview.Settings.DefaultFontSize = 20;

                adapter.NotifyDataSetChanged();

                if (artical.RelatedPosts != null)
                    gridview.LayoutParameters.Height = artical.RelatedPosts.Length * dpToPx(70);
            }));            
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.artical);

            ArticalOverview articalOverview = null;

            Bundle extras = Intent.Extras;

            if (extras != null && extras.ContainsKey(PassArticalOverviewObj))
                articalOverview = new ArticalOverview(extras.GetBundle(PassArticalOverviewObj));
            else
            {
                Finish();
                return;
            }

            analysisModule.ReadArtical(UidGenerator(), articalOverview, this);  //make the request

            webview = FindViewById<WebView>(Resource.Id.contentWebView);
            webview.Settings.DefaultFontSize = 20;
            webview.Settings.BuiltInZoomControls = true;

            gridview = FindViewById<GridView>(Resource.Id.relatedPostGridView);
            adapter = new GridviewAdapter() { parent = this };
            gridview.Adapter = adapter;
            gridview.ItemClick += Gridview_ItemClick;
        }

        private void Gridview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Intent intent = new Intent(this, typeof(ArticalActivity));
            intent.PutExtra(ArticalActivity.PassArticalOverviewObj, currentArtical.RelatedPosts[e.Position].ToBundle());
            StartActivity(intent);
        }

        class GridviewAdapter : BaseAdapter
        {
            public ArticalActivity parent { get; set; } = null;
            //public event EventHandler<int> onClick = null;

            public override int Count
            {
                get
                {
                    if (parent?.currentArtical?.RelatedPosts != null)
                        return parent.currentArtical.RelatedPosts.Length;
                    //if (parent!=null && parent.currentArtical!=null && parent.currentArtical.RelatedPosts != null)
                    //    return parent.currentArtical.RelatedPosts.Length;
                    return 0;
                }
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return null;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                if (convertView == null)
                {
                    var layoutInflator = this.parent.LayoutInflater;
                    convertView = layoutInflator.Inflate(Resource.Layout.artical_relatedpost_single_item, parent, false);
                    convertView.Tag = convertView.FindViewById(Resource.Id.relatedPostTextView);
                    //convertView.Click += (sender, e) => onClickCallback(position);
                }
                var textview = convertView.Tag as TextView;
                textview.Text = this.parent.currentArtical.RelatedPosts[position].Title;
                return convertView;
            }

            //private void onClickCallback(int position) => onClick?.Invoke(this, position);
        }
    }
}