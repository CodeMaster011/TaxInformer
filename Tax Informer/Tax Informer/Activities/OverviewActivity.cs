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
using Java.Lang;
using Tax_Informer.Core;
using static Tax_Informer.MyGlobal;
using Android.Support.V4.Widget;
using Android.Graphics.Drawables;

namespace Tax_Informer.Activities
{
    [Activity(Label = "OverviewActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    internal class OverviewActivity : Activity, IUiArticalOverviewResponseHandler
    {
        public const string PassWebsiteKey = nameof(PassWebsiteKey);

        private Website currentWebsite = null;
        private string currentWebsiteKey = null;
        private DrawerLayout drawerLayout = null;
        private ArrayAdapter navAdapter = null;
        private ListView navListview = null;
        private string[] navData = null;

        private SwipeRefreshLayout swipeRefLayout = null;
        private string refreshingLink = string.Empty;
        public string refreshingRequestUid = string.Empty;

        private ListView listview = null;
        private ListviewAdpater adapter = null;
        private NextPageContext nextPageContext = null;
        private string NextPageRequestUids = string.Empty;
        private OverviewType browsingContext = OverviewType.UNKNOWN;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.overview);

            //ActionBar.Hide();

            if (Intent.Extras != null && Intent.Extras.ContainsKey(PassWebsiteKey))
                currentWebsite = Config.GetWebsite(currentWebsiteKey = Intent.Extras.GetString(PassWebsiteKey));
            

            browsingContext = OverviewType.IndexPage;
            analysisModule.ReadIndexPage(UidGenerator(), currentWebsiteKey, refreshingLink = currentWebsite.IndexPageLink, this);   //make the request

            ActionBar.SetBackgroundDrawable(new ColorDrawable(Android.Graphics.Color.ParseColor(currentWebsite.Color)));
            Title = currentWebsite.Name;

            Window window = Window;
            // clear FLAG_TRANSLUCENT_STATUS flag:
            window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            // add FLAG_DRAWS_SYSTEM_BAR_BACKGROUNDS flag to the window
            window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            // finally change the color
            window.SetStatusBarColor(Android.Graphics.Color.ParseColor(currentWebsite.Color));


            swipeRefLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swiperefresh);
            swipeRefLayout.Refresh += SwipeRefLayout_Refresh;
            swipeRefLayout.SetColorSchemeColors(new int[] {
                Android.Graphics.Color.ParseColor(currentWebsite.Color).ToArgb()});
            
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.overviewDrawerLayout);
            navListview = FindViewById<ListView>(Resource.Id.navigationDrawerListView);
            navData = new string[currentWebsite.Categories.Length];
            int index = 0;
            foreach (var item in currentWebsite.Categories) navData[index++] = item.Name;
            navAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, navData);
            navListview.Adapter = navAdapter;
            navListview.ItemClick += NavListview_ItemClick;

            listview = FindViewById<ListView>(Resource.Id.contentListView);
            adapter = new ListviewAdpater() { parent = this, currentWebsite = currentWebsite };
            listview.Adapter = adapter;
            listview.ItemClick += Listview_ItemClick;
        }
        protected override void OnResume()
        {            
            adapter.NotifyDataSetChanged();

            base.OnResume();
        }

        private void SwipeRefLayout_Refresh(object sender, EventArgs e)
        {
            if (refreshingRequestUid != string.Empty) return;

            swipeRefLayout.Refreshing = true;
            switch (browsingContext)
            {
                case OverviewType.Null:
                    break;
                case OverviewType.UNKNOWN:
                    break;
                case OverviewType.IndexPage:
                    analysisModule.ReadIndexPage(refreshingRequestUid = UidGenerator(), currentWebsiteKey, refreshingLink, this, true);
                    break;
                case OverviewType.Author:
                    break;
                case OverviewType.Category:
                    analysisModule.ReadCategory(refreshingRequestUid = UidGenerator(), currentWebsiteKey, new Category() { Link = refreshingLink }, this);
                    break;
                default:
                    break;
            }
        }

        private void NavListview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            browsingContext = OverviewType.Category;
            var cat = currentWebsite.Categories[e.Position];
            refreshingLink = cat.Link;
            Title = $"{cat.Name} - {currentWebsite.Name}";
            analysisModule.ReadCategory(UidGenerator(), currentWebsiteKey, cat, this);
            adapter.data = null;
            adapter.NotifyDataSetChanged();
            drawerLayout.CloseDrawer((int)GravityFlags.Left);
        }

        private void Listview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var articalOverview = adapter.data[e.Position];
            database.UpdateIsSeen(UidGenerator(), articalOverview);//add to seen list
            Intent intent = new Intent(this, typeof(ArticalActivity));
            intent.PutExtra(ArticalActivity.PassArticalOverviewObj, articalOverview.ToBundle());
            intent.PutExtra(ArticalActivity.PassWebsiteKey, currentWebsiteKey);
            StartActivity(intent);
        }

        public void ArticalOverviewProcessedCallback(string uid, string url, ArticalOverview[] articalOverviews, OverviewType overviewType, string nextPageUrl)
        {
            ArticalOverview[] newData = null;
            lock (NextPageRequestUids)
            {

                if (NextPageRequestUids == uid)
                {
                    newData = new ArticalOverview[adapter.data.Length + articalOverviews.Length];
                    adapter.data.CopyTo(newData, 0);
                    articalOverviews.CopyTo(newData, adapter.data.Length);

                    NextPageRequestUids = string.Empty;
                }
            }            
            if(nextPageUrl!=null && nextPageUrl != string.Empty)
            {
                nextPageContext = new NextPageContext() { overviewType = overviewType, url = nextPageUrl }; //save the next page state
            }

            if(uid == refreshingRequestUid)
            {
                newData = null;
                refreshingRequestUid = string.Empty;
                RunOnUiThread(new Action(() => { swipeRefLayout.Refreshing = false; }));
            }

            adapter.data = newData == null ? articalOverviews : newData;
            database.IsSeen(UidGenerator(), adapter.data);  //request a filter from DB
            RunOnUiThread(responseUpdate);
        }
        private void responseUpdate()
        {
            adapter.NotifyDataSetChanged();
        }

        public void LoadNextPage()
        {
            if (NextPageRequestUids != string.Empty) return;    //their is a pending request for next page
            if (nextPageContext == null) return;
            string uid = UidGenerator();
            var obj = nextPageContext.createObj();
            NextPageRequestUids = uid;   //hold the uid of request 
            switch (nextPageContext.overviewType)
            {
                case OverviewType.Null:
                    break;
                case OverviewType.UNKNOWN:
                    break;
                case OverviewType.IndexPage:
                    analysisModule.ReadIndexPage(uid, currentWebsiteKey, nextPageContext.url, this);
                    break;
                case OverviewType.Author:
                    analysisModule.ReadAuthor(uid, currentWebsiteKey, obj as Author, this);
                    break;
                case OverviewType.Category:
                    analysisModule.ReadCategory(uid, currentWebsiteKey, obj as Category, this);
                    break;
                default:
                    break;
            }
        }

        class ListviewAdpater : BaseAdapter
        {
            public ArticalOverview[] data { get; set; } = null;
            public OverviewActivity parent { get; set; } = null;
            public Website currentWebsite { get; set; } = null;

            public override int Count
            {
                get
                {
                    return data != null ? data.Length : 0;
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
                    var layoutInflator = (LayoutInflater)this.parent.BaseContext.GetSystemService(Service.LayoutInflaterService);
                    convertView = layoutInflator.Inflate(Resource.Layout.overview_single_item, parent, false);
                    convertView.Tag = new ViewHolder(convertView);  //TODO: Add the onClick listeners tags and author
                }
                var vHolder = convertView.Tag as ViewHolder;
                var item = data[position];

                vHolder.websiteComicTextView.Text = currentWebsite.ComicText;
                vHolder.websiteComicTextView.SetBackgroundColor(Android.Graphics.Color.ParseColor(currentWebsite.Color));
                vHolder.titleTextView.Text = item.Title;
                vHolder.summaryTextView.Text = item.SummaryText;

                vHolder.dateTextView.Text = GetHumanReadableDate(item.Date);
                if (vHolder.dateTextView.Text == null) vHolder.dateTextView.Visibility = ViewStates.Gone;
                else vHolder.dateTextView.Visibility = ViewStates.Visible;

                if (item?.Authors != null)
                {
                    vHolder.authorTextView.Text = "By, " + item.Authors[0].Name;//TODO: Extend support for more author and allow individual author to have their options with onClick listener
                    vHolder.authorTextView.Visibility = ViewStates.Visible;
                }
                else vHolder.authorTextView.Visibility = ViewStates.Gone;

                if(item.IsDatabaseConfirmed_SeenOn && !string.IsNullOrEmpty(item.SeenOn))
                    vHolder.titleTextView.Alpha = 0.2f;
                else
                    vHolder.titleTextView.Alpha = 1f;

                //for (int i = 0; i < vHolder.tag.Length; i++)
                //{
                //    if (i >= item.Categorys.Length)
                //    {
                //        vHolder.tag[i].Visibility = ViewStates.Gone;
                //    }
                //    else
                //    {
                //        var category = item.Categorys[i];
                //        vHolder.tag[i].Visibility = ViewStates.Visible;
                //        vHolder.tag[i].Text = item.Categorys[i].Name;
                //        //TODO:Find a way to clear the old callbacks and then add the new one
                //        vHolder.tag[i].Click += new EventHandler((sender, e) =>
                //        {
                //            Toast.MakeText(this.parent, $"Tag: {category.Name}", ToastLength.Short).Show();
                //        });
                //    }                    
                //}


                if (data.Length - position <= NextPageContentNumber) this.parent.LoadNextPage();    //sent a request to load next page

                return convertView;
            }

            class ViewHolder: Java.Lang.Object
            {
                public TextView websiteComicTextView, titleTextView, summaryTextView, authorTextView, dateTextView;
                public TextView[] tag = null;

                public ViewHolder(View v)
                {
                    websiteComicTextView = v.FindViewById<TextView>(Resource.Id.websiteComicTextView);
                    titleTextView = v.FindViewById<TextView>(Resource.Id.titleTextView);
                    summaryTextView = v.FindViewById<TextView>(Resource.Id.summaryTextView);
                    authorTextView = v.FindViewById<TextView>(Resource.Id.authorTextView);
                    dateTextView = v.FindViewById<TextView>(Resource.Id.dateTextView);

                    tag = new TextView[3];
                    tag[0] = v.FindViewById<TextView>(Resource.Id.tagTextView1);
                    tag[1] = v.FindViewById<TextView>(Resource.Id.tagTextView2);
                    tag[2] = v.FindViewById<TextView>(Resource.Id.tagTextView3);

                    //TODO: Pass the onClick listeners on tags and author
                }
            }
        }

        class NextPageContext
        {
            public OverviewType overviewType { get; set; } = OverviewType.UNKNOWN;
            public string url { get; set; } = string.Empty;

            public object createObj()
            {
                switch (overviewType)
                {
                    case OverviewType.Null:
                        break;
                    case OverviewType.UNKNOWN:
                        break;
                    case OverviewType.IndexPage:
                        break;
                    case OverviewType.Author:
                        return new Author() { Link = url };
                    case OverviewType.Category:
                        return new Category() { Link = url };
                    default:
                        break;
                }
                return null;
            }
        }
    }
}