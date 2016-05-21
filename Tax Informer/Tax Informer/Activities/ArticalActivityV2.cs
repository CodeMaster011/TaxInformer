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
using Android.Support.V7.App;
using Tax_Informer.Core;
using Android.Webkit;
using Android.Support.Design.Widget;
using SupportToolBar = Android.Support.V7.Widget.Toolbar;
using static Tax_Informer.MyGlobal;
namespace Tax_Informer.Activities
{
    [Activity(Label = "Artical")]
    internal class ArticalActivityV2 : AppCompatActivity, IUiArticalResponseHandler
    {
        public const string PassArticalOverviewObj = nameof(PassArticalOverviewObj);
        public const string PassWebsiteKey = nameof(PassWebsiteKey);
        public const string PassIsOffline = nameof(PassIsOffline);

        private ArticalOverview articalOverview = null;
        private Artical currentArtical = null;
        private bool isOffline = false;
        private string currentWebsiteKey = null;
        private AppBarLayout appBarLayout = null;
        private SupportToolBar toolBar = null;
        private TextView articalTitleTextview = null, articalDateTextview = null, articalWebsiteComicTextview = null;
        private TextView loadingTextView = null;
        private WebView articalContentWebview = null;
        private FloatingActionButton fabOfflineButton = null;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            MyLog.Log(this, nameof(OnCreate) + "...");

            SetContentView(Resource.Layout.artical_layout_v2);
            ActionBar?.Hide();

            #region Grabbing data from bundles
            articalOverview = null;
            Bundle extras = Intent.Extras;
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
            #endregion

            #region Making request for artical
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
            #endregion



            //=================Getting current website===============
            var currentWebsite = Config.GetWebsite(currentWebsiteKey);
            ChangeStatusBarColor(Window, currentWebsite.Color);
            //===============Variable Findings==================
            appBarLayout = FindViewById<AppBarLayout>(Resource.Id.mainAppbar);
            toolBar = FindViewById<SupportToolBar>(Resource.Id.mainToolbar);
            loadingTextView = FindViewById<TextView>(Resource.Id.articalLoadingTextView);
            fabOfflineButton = FindViewById<FloatingActionButton>(Resource.Id.articalFab);
            articalContentWebview = FindViewById<WebView>(Resource.Id.articalContentWebView);            
            articalTitleTextview = FindViewById<TextView>(Resource.Id.articalTitleTextView);
            articalDateTextview = FindViewById<TextView>(Resource.Id.articalDateTextView);
            articalWebsiteComicTextview = FindViewById<TextView>(Resource.Id.articalWebsiteComicTextView);
            //===============Setup WebView==================
            MyLog.Log(this, "Loading web view" + "...");            
            articalContentWebview.Settings.DefaultFontSize = 20;
            articalContentWebview.Settings.BuiltInZoomControls = true;
            articalContentWebview.Settings.JavaScriptEnabled = true;
            articalContentWebview.Settings.AllowFileAccessFromFileURLs = true;
            articalContentWebview.Settings.AllowUniversalAccessFromFileURLs = true;
            articalContentWebview.Visibility = ViewStates.Gone;
            MyLog.Log(this, "Loading web view" + "...Done");
            //===============Setup WebView==================
            
            appBarLayout.SetBackgroundColor(Android.Graphics.Color.ParseColor(currentWebsite.Color));
            //Loading data from artical overview
            toolBar.Title = articalOverview.Title ?? "Artical";
            articalTitleTextview.Text = articalOverview.Title ?? "";
            articalDateTextview.Text = GetHumanReadableDate(articalOverview.Date) ?? "";
            articalWebsiteComicTextview.Text = currentWebsite.ComicText ?? "";
            //Setting the text for loading
            loadingTextView.Text = "Loading...";
            loadingTextView.Gravity = GravityFlags.CenterHorizontal;

            fabOfflineButton.Visibility = !isOffline ? ViewStates.Visible : ViewStates.Gone;//determining the visibility of fab as online or offline

            //TODO: Add a support to open the artical in web browser
            fabOfflineButton.Click += FloatingButton_Click;
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

        //private void OptionOpenInBrowser_Click(object sender, EventArgs e)
        //{
        //    MyLog.Log(this, nameof(OptionOpenInBrowser_Click) + "...");
        //    navDrawerLayout.CloseDrawer((int)GravityFlags.Right);
        //    Intent browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(currentArtical.MyLink));
        //    StartActivity(browserIntent);
        //    MyLog.Log(this, nameof(OptionOpenInBrowser_Click) + "...Done");
        //}

        public void ArticalProcessedCallback(string uid, string url, Artical artical)
        {
            RunOnUiThread(new Action(() => { updateArtical(artical); }));
        }
        private void updateArtical(Artical artical)
        {
            MyLog.Log(this, nameof(updateArtical) + "...");
            currentArtical = artical;//cache the data

            if (string.IsNullOrEmpty(artical.ExternalFileLink))
            {
                MyLog.Log(this, $"Updating artical data text url {artical.MyLink} " + "...");
                articalContentWebview.LoadData(artical.HtmlText, "text/html", "utf-8");
                MyLog.Log(this, $"Updating artical data text url {artical.MyLink} " + "...Done");                
            }
            else
            {
                MyLog.Log(this, $"Updating artical data extrnal url {artical.MyLink} \t link {artical.ExternalFileLink}" + "...");
                articalContentWebview.LoadUrl("file:///android_asset/pdfviewer/index.html?file=" + System.Net.WebUtility.UrlDecode(artical.ExternalFileLink));                
                MyLog.Log(this, $"Updating artical data extrnal url {artical.MyLink} \t link {artical.ExternalFileLink}" + "...Done");
            }
            
            articalContentWebview.Settings.DefaultFontSize = 20;
            loadingTextView.Visibility = ViewStates.Gone;
            articalContentWebview.Visibility = ViewStates.Visible;
            Title = artical.Title;
            articalDateTextview.Text = GetHumanReadableDate(artical.Date);
            articalTitleTextview.Text = artical.Title;
            MyLog.Log(this, nameof(updateArtical) + "...Done");
        }
    }
}