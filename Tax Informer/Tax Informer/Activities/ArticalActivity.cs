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
            MyLog.Log(this, nameof(updateArtical) + "...");
            currentArtical = artical;//cache the data

            if (string.IsNullOrEmpty(artical.ExternalFileLink))
            {
                MyLog.Log(this, $"Updating artical data text url {artical.MyLink} " + "...");
                articalContentTextview.Gravity = GravityFlags.Left;
                articalContentTextview.TextFormatted = Android.Text.Html.FromHtml(artical.HtmlText);//TODO: Add an image getter for getting images from web. Use Picasso to download image and use custom memory cache.
                articalContentTextview.GetFocusedRect(new Android.Graphics.Rect(0, 0, 1, 1));

                adapter.NotifyDataSetChanged();

                articalContentTextview.Visibility = ViewStates.Visible;
                if (articalContentWebview != null) articalContentWebview.Visibility = ViewStates.Gone; 
                MyLog.Log(this, $"Updating artical data text url {artical.MyLink} " + "...Done");
            }
            else
            {
                MyLog.Log(this, $"Updating artical data extrnal url {artical.MyLink} \t link {artical.ExternalFileLink}" + "...");
                articalContentWebview.LoadUrl("file:///android_asset/pdfviewer/index.html?file=" + System.Net.WebUtility.UrlDecode(artical.ExternalFileLink));
                articalContentWebview.Settings.DefaultFontSize = 20;
                articalContentTextview.Visibility = ViewStates.Gone;
                articalContentWebview.Visibility = ViewStates.Visible; 
                MyLog.Log(this, $"Updating artical data extrnal url {artical.MyLink} \t link {artical.ExternalFileLink}" + "...Done");
            }
            //if (artical.RelatedPosts != null)
            //    gridview.LayoutParameters.Height = artical.RelatedPosts.Length * dpToPx(70);

            Title = artical.Title;
            articalDateTextview.Text = GetHumanReadableDate(artical.Date);
            articalTitleTextview.Text = artical.Title; 
            MyLog.Log(this, nameof(updateArtical) + "...Done");
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            MyLog.Log(this, nameof(OnCreate) + "...");

            SetContentView(Resource.Layout.artical);

            ActionBar?.Hide();

            articalOverview = null;
            Bundle extras = Intent.Extras;
            Website currentWebsite = null;

            MyLog.Log(this, "Loading bundle data" + "...");
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
            MyLog.Log(this, "Loading bundle data" + "...Done");

            if (!isOffline)
            {
                MyLog.Log(this, $"Request artical data online url {articalOverview.LinkOfActualArtical}" + "...");
                analysisModule.ReadArtical(UidGenerator(), currentWebsiteKey, articalOverview, this);  //make the request  
                MyLog.Log(this, $"Request artical data online url {articalOverview.LinkOfActualArtical}" + "...Done");
            }
                
            else
            {
                MyLog.Log(this, $"Requesting artical data offline url{articalOverview.LinkOfActualArtical}" + "...");
                database.GetArtical(UidGenerator(), articalOverview, this); 
                MyLog.Log(this, $"Requesting artical data offline url{articalOverview.LinkOfActualArtical}" + "...Done");
            }

            currentWebsite = Config.GetWebsite(currentWebsiteKey);

            MyLog.Log(this, "Loading webview" + "...");
            articalContentWebview = FindViewById<WebView>(Resource.Id.articalContentWebView);
            articalContentWebview.Settings.DefaultFontSize = 20;
            articalContentWebview.Settings.BuiltInZoomControls = true;
            articalContentWebview.Settings.JavaScriptEnabled = true;
            articalContentWebview.Settings.AllowFileAccessFromFileURLs = true;
            articalContentWebview.Settings.AllowUniversalAccessFromFileURLs = true;
            articalContentWebview.Visibility = ViewStates.Gone; 
            MyLog.Log(this, "Loading webview" + "...Done");

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
            MyLog.Log(this, nameof(OnCreate) + "...Done");
        }

        private void FloatingButton_Click(object sender, EventArgs e)
        {
            MyLog.Log(this, nameof(FloatingButton_Click) + "...");
            MyLog.Log(this, "Making artical offline" + "...");
            database.MakeOffline(UidGenerator(), currentWebsiteKey, currentArtical, articalOverview);   //request to make data offline  
            MyLog.Log(this, "Making artical offline" + "...Done");
            Snackbar.Make(sender as View, "Offline is now available", (int)ToastLength.Short).Show(); 
            MyLog.Log(this, nameof(FloatingButton_Click) + "...Done");
        }

        private void OptionOpenInBrowser_Click(object sender, EventArgs e)
        {
            MyLog.Log(this, nameof(OptionOpenInBrowser_Click) + "...");
            navDrawerLayout.CloseDrawer((int)GravityFlags.Right);
            Intent browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(currentArtical.MyLink));
            StartActivity(browserIntent); 
            MyLog.Log(this, nameof(OptionOpenInBrowser_Click) + "...Done");
        }

        private void Gridview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            MyLog.Log(this, nameof(Gridview_ItemClick) + "...");
            navDrawerLayout.CloseDrawer((int)GravityFlags.Right);
            MyLog.Log(this, $"Starting activity artical url {currentArtical?.RelatedPosts?[e.Position]?.LinkOfActualArtical}" + "...");
            StartActivityArtical(this, currentArtical.RelatedPosts[e.Position], currentWebsiteKey);  
            MyLog.Log(this, $"Starting activity artical url {currentArtical?.RelatedPosts?[e.Position]?.LinkOfActualArtical}" + "...Done");
            MyLog.Log(this, nameof(Gridview_ItemClick) + "...Done");
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