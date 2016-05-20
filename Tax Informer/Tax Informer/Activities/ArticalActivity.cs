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
using Android.Graphics.Drawables;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using other = com.refractored.fab;
using Android.Support.V7.App;

namespace Tax_Informer.Activities
{
    [Activity(Label = "ArticalActivity")]
    internal class ArticalActivity : AppCompatActivity, IUiArticalResponseHandler
    {
        public const string PassArticalOverviewObj = nameof(PassArticalOverviewObj);
        public const string PassWebsiteKey = nameof(PassWebsiteKey);
        public const string PassIsOffline = nameof(PassIsOffline);

        private ArticalOverview articalOverview = null;
        private bool isOffline = false;
        private string currentWebsiteKey = null;
        private DrawerLayout navDrawerLayout = null;
        private LinearLayout headerLayout = null;
        private TextView articalTitleTextview = null, articalDateTextview = null, articalWebsiteComicTextview = null;
        private WebView articalContentWebview = null;
        private TextView articalContentTextview = null;
        private other.ObservableScrollView scrollView = null;
        private other.FloatingActionButton floatingButton = null;
        private GridView gridview = null;
        private GridviewAdapter adapter = null;

        private TextView optionOpenInBrowser = null;

        public Artical currentArtical = null;

        private int dpToPx(int dp)
        {
            DisplayMetrics displayMetrics = BaseContext.Resources.DisplayMetrics;
            int px = (int)System.Math.Round((double)dp * (double)((float)displayMetrics.Xdpi / (int)DisplayMetrics.DensityDefault));
            return px;
        }

        public void ArticalProcessedCallback(string uid, string url, Artical artical)
        {
            RunOnUiThread(new Action(() =>{ updateArtical(artical); }));
        }
        private void updateArtical(Artical artical)
        {
            currentArtical = artical;//cache the data

            if (string.IsNullOrEmpty(artical.ExternalFileLink))
            {
                articalContentTextview.Gravity = GravityFlags.Left;
                articalContentTextview.TextFormatted = Android.Text.Html.FromHtml(artical.HtmlText);//TODO: Add an image getter for getting images from web. Use Picasso to download image and use custom memory cache.
                articalContentTextview.GetFocusedRect(new Android.Graphics.Rect(0, 0, 1, 1));

                adapter.NotifyDataSetChanged();

                articalContentTextview.Visibility = ViewStates.Visible;
                articalContentWebview.Visibility = ViewStates.Gone;
            }
            else
            {
                articalContentWebview.LoadUrl("file:///android_asset/PDFViewer/index.html?file=" + System.Net.WebUtility.UrlDecode(artical.ExternalFileLink));
                articalContentWebview.Settings.DefaultFontSize = 20;
                articalContentTextview.Visibility = ViewStates.Gone;
                articalContentWebview.Visibility = ViewStates.Visible;
            }
            //if (artical.RelatedPosts != null)
            //    gridview.LayoutParameters.Height = artical.RelatedPosts.Length * dpToPx(70);

            Title = artical.Title;
            articalDateTextview.Text = GetHumanReadableDate(artical.Date);
            articalTitleTextview.Text = artical.Title;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.artical);

            ActionBar?.Hide();

            articalOverview = null;
            Bundle extras = Intent.Extras;
            Website currentWebsite = null;

            if (extras != null && extras.ContainsKey(PassArticalOverviewObj))
                articalOverview = new ArticalOverview(extras.GetBundle(PassArticalOverviewObj));
            else
            {
                Finish();
                return;
            }
            if (extras != null || extras.ContainsKey(PassWebsiteKey))
                currentWebsiteKey = extras.GetString(PassWebsiteKey);
            else
            {
                Finish();
                return;
            }
            if (extras != null || extras.ContainsKey(PassIsOffline))
                isOffline = extras.GetBoolean(PassIsOffline);
            else
            {
                Finish();
                return;
            }

            if (!isOffline)
                analysisModule.ReadArtical(UidGenerator(), currentWebsiteKey, articalOverview, this);  //make the request
            else
                database.GetArtical(UidGenerator(), articalOverview, this);

            currentWebsite = Config.GetWebsite(currentWebsiteKey);

            articalContentWebview = FindViewById<WebView>(Resource.Id.articalContentWebView);
            articalContentWebview.Settings.DefaultFontSize = 20;
            articalContentWebview.Settings.BuiltInZoomControls = true;
            articalContentWebview.Settings.JavaScriptEnabled = true;
            articalContentWebview.Settings.AllowFileAccessFromFileURLs = true;
            articalContentWebview.Settings.AllowUniversalAccessFromFileURLs = true;
            articalContentWebview.Visibility = ViewStates.Gone;

            ChangeStatusBarColor(Window, currentWebsite.Color);

            gridview = FindViewById<GridView>(Resource.Id.relatedPostGridView);
            adapter = new GridviewAdapter() { parent = this };
            gridview.Adapter = adapter;
            gridview.ItemClick += Gridview_ItemClick;

            articalContentTextview = FindViewById<TextView>(Resource.Id.articalContentTextView);
            headerLayout = FindViewById<LinearLayout>(Resource.Id.articalHeaderLinearLayout);
            articalTitleTextview = FindViewById<TextView>(Resource.Id.articalTitleTextView);
            articalDateTextview = FindViewById<TextView>(Resource.Id.articalDateTextView);
            articalWebsiteComicTextview = FindViewById<TextView>(Resource.Id.articalWebsiteComicTextView);
            scrollView = FindViewById<other.ObservableScrollView>(Resource.Id.articalScrollView);
            floatingButton = FindViewById<other.FloatingActionButton>(Resource.Id.articalFab);
            navDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.articalDrawerLayout);


            headerLayout.SetBackgroundColor(Android.Graphics.Color.ParseColor(currentWebsite.Color));

            articalTitleTextview.Text = articalOverview.Title ?? "";
            articalDateTextview.Text = GetHumanReadableDate(articalOverview.Date) ?? "";
            articalWebsiteComicTextview.Text = currentWebsite.ComicText ?? "";
            articalContentTextview.Text = "Loading...";
            articalContentTextview.Gravity = GravityFlags.CenterHorizontal;

            floatingButton.AttachToScrollView(scrollView);
            floatingButton.Visibility = !isOffline ? ViewStates.Visible : ViewStates.Gone;

            optionOpenInBrowser = FindViewById<TextView>(Resource.Id.articalOptionOpenInBrowserTextView);
            optionOpenInBrowser.Click += OptionOpenInBrowser_Click;
            floatingButton.Click += FloatingButton_Click;
        }

        private void FloatingButton_Click(object sender, EventArgs e)
        {
            database.MakeOffline(UidGenerator(), currentWebsiteKey, currentArtical, articalOverview);   //request to make data offline
            Snackbar.Make(sender as View, "Offline is now available", (int)ToastLength.Short).Show();
        }

        private void OptionOpenInBrowser_Click(object sender, EventArgs e)
        {
            navDrawerLayout.CloseDrawer((int)GravityFlags.Right);
            Intent browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(currentArtical.MyLink));
            StartActivity(browserIntent);
        }

        private void Gridview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            navDrawerLayout.CloseDrawer((int)GravityFlags.Right);            
            StartActivityArtical(this, currentArtical.RelatedPosts[e.Position], currentWebsiteKey);            
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
                }
                var textview = convertView.Tag as TextView;
                textview.Text = this.parent.currentArtical.RelatedPosts[position].Title;
                return convertView;
            }
        }
    }
}