using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using Android.Support.V7.Widget;
using Tax_Informer.Core;
using Android.Support.V4.Widget;
using static Tax_Informer.MyGlobal;
using Android.Support.Design.Widget;

namespace Tax_Informer.Fragments
{
    internal class OverviewFragment : SupportFragment, IUiArticalOverviewResponseHandler
    {
        private string websiteKey = null;
        private Website website = null;
        private SwipeRefreshLayout swipeRefLayout = null;
        private string refreshingLink = string.Empty;
        public string refreshingRequestUid = string.Empty;

        private RecyclerView recyView = null;
        private RecyclerViewAdpater adapter = null;
        private RecyclerView.LayoutManager recyLayoutManager = null;
        private NextPageContext nextPageContext = null;
        private string NextPageRequestUids = string.Empty;
        private OverviewType browsingContext = OverviewType.UNKNOWN;

        public OverviewFragment(string websiteKey)
        {
            MyLog.Log(this, $"OnCreate...");
            this.websiteKey = websiteKey;
            website = Config.GetWebsite(websiteKey);

            refreshingLink = website.IndexPageLink;
            MyLog.Log(this, $"OnCreate...Done");
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            MyLog.Log(this, $"OnCreateView...");

            swipeRefLayout = inflater.Inflate(Resource.Layout.main_offline_list, container, false) as SwipeRefreshLayout;
            swipeRefLayout.Refreshing = true;
            swipeRefLayout.Refresh += SwipeRefLayout_Refresh;
            swipeRefLayout.SetColorSchemeColors(new int[] {
                Android.Graphics.Color.ParseColor(website.Color).ToArgb()});

            recyView = swipeRefLayout.FindViewById<RecyclerView>(Resource.Id.offlineRecyclerView);
            recyView.SetLayoutManager(recyLayoutManager = new LinearLayoutManager(this.Context, (int)Orientation.Vertical, false));
            recyView.SetAdapter(adapter = new RecyclerViewAdpater(website));
            adapter.OnItemClick += RecyclerView_OnItemClick;
            adapter.LoadNextPage += (sender, e) => LoadNextPage();
            adapter.OnCategorySelected += OnCategorySelected;

            if (website.Categories == null)
            {
                MyLog.Log(this, $"Requesting index page data from analysisModule url {website.IndexPageLink}...");
                browsingContext = OverviewType.IndexPage;
                analysisModule.ReadIndexPage(refreshingRequestUid = UidGenerator(), websiteKey, website.IndexPageLink, this);
                MyLog.Log(this, $"Requesting index page data from analysisModule url {website.IndexPageLink}...Done");
            }
            else
            {
                MyLog.Log(this, $"Requesting index page data from analysisModule category {website?.Categories[0]?.Link}...");
                browsingContext = OverviewType.Category;
                refreshingLink = website.Categories[0].Link;
                analysisModule.ReadCategory(refreshingRequestUid = UidGenerator(), websiteKey, website.Categories[0], this);
                MyLog.Log(this, $"Requesting index page data from analysisModule category {website?.Categories[0]?.Link}...Done");
            }

            MyLog.Log(this, $"OnCreateView...Done");
            return swipeRefLayout;
        }

        private void OnCategorySelected(object sender, Category category)
        {
            MyLog.Log(this, nameof(OnCategorySelected));
            
            if (browsingContext == OverviewType.Category && refreshingLink == category.Link) return;
            swipeRefLayout.Refreshing = true;
            browsingContext = OverviewType.Category;
            refreshingLink = category.Link;
            MyLog.Log(this, $"Read category url {refreshingLink}" + "...");
            analysisModule.ReadCategory(refreshingRequestUid = UidGenerator(), websiteKey, category, this); 
            MyLog.Log(this, $"Read category url {refreshingLink}" + "...Done");
            adapter.data = null;
            adapter.NotifyDataSetChanged();

            MyLog.Log(this, nameof(OnCategorySelected) + "...Done");
        }

        private void RecyclerView_OnItemClick(object sender, ArticalOverview articalOverview)
        {
            MyLog.Log(this, $"Starting activity artical" + "...");
            StartActivityArtical(applicationContext, articalOverview, websiteKey);
            MyLog.Log(this, $"Starting activity artical" + "...Done");
        }

        public override void OnResume()
        {
            adapter?.NotifyDataSetChanged();

            base.OnResume();
        }
        private void SwipeRefLayout_Refresh(object sender, EventArgs e)
        {
            if (refreshingRequestUid != string.Empty) return;

            MyLog.Log(this, nameof(SwipeRefLayout_Refresh));
            swipeRefLayout.Refreshing = true;
            switch (browsingContext)
            {
                case OverviewType.Null:
                    break;
                case OverviewType.UNKNOWN:
                    break;
                case OverviewType.IndexPage:
                    MyLog.Log(this, $"Requesting index page data url {refreshingLink}" + "...");
                    analysisModule.ReadIndexPage(refreshingRequestUid = UidGenerator(), websiteKey, refreshingLink, this, true); 
                    MyLog.Log(this, $"Requesting index page data url {refreshingLink}" + "...Done");
                    break;
                case OverviewType.Author:
                    break;
                case OverviewType.Category:
                    MyLog.Log(this, $"Requesting category data {refreshingLink}" + "...");
                    analysisModule.ReadCategory(refreshingRequestUid = UidGenerator(), websiteKey, new Category() { Link = refreshingLink }, this); 
                    MyLog.Log(this, $"Requesting category data {refreshingLink}" + "...Done");
                    break;
                default:
                    break;
            } 
            MyLog.Log(this, nameof(SwipeRefLayout_Refresh) + "...Done");
        }

        public void ArticalOverviewProcessedCallback(string uid, string url, ArticalOverview[] articalOverviews, OverviewType overviewType, string nextPageUrl)
        {
            MyLog.Log(this, nameof(ArticalOverviewProcessedCallback));
            ArticalOverview[] newData = null;
            lock (NextPageRequestUids)
            {

                if (NextPageRequestUids == uid)
                {
                    MyLog.Log(this, $"next page data received" + "...");
                    newData = new ArticalOverview[adapter.data.Length + articalOverviews.Length];
                    adapter.data.CopyTo(newData, 0);
                    articalOverviews.CopyTo(newData, adapter.data.Length);

                    NextPageRequestUids = string.Empty; 
                    MyLog.Log(this, $"next page data received" + "...Done");
                }
            }
            if (nextPageUrl != null && nextPageUrl != string.Empty)
            {
                nextPageContext = new NextPageContext() { overviewType = overviewType, url = nextPageUrl }; //save the next page state
            }

            if (uid == refreshingRequestUid)
            {
                MyLog.Log(this, "refreshing content received" + "...");
                newData = null;
                refreshingRequestUid = string.Empty;
                Activity.RunOnUiThread(new Action(() => { swipeRefLayout.Refreshing = false; })); 
                MyLog.Log(this, "refreshing content received" + "...Done");
            }

            MyLog.Log(this, "Updating adapter data" + "...");
            adapter.data = newData == null ? articalOverviews : newData;
            database.IsSeen(UidGenerator(), adapter.data);  //request a filter from DB
            Activity.RunOnUiThread(responseUpdate);  
            MyLog.Log(this, "Updating adapter data" + "...Done");
            MyLog.Log(this, nameof(ArticalOverviewProcessedCallback) + "...Done");
        }

        private void responseUpdate()
        {
            adapter.NotifyDataSetChanged();
        }

        public void LoadNextPage()
        {
            
            if (NextPageRequestUids != string.Empty) return;    //their is a pending request for next page
            if (nextPageContext == null) return;

            MyLog.Log(this, nameof(LoadNextPage) + "...");
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
                    MyLog.Log(this, $"Requesting index page data {nextPageContext.url}" + "...");
                    analysisModule.ReadIndexPage(uid, websiteKey, nextPageContext.url, this); 
                    MyLog.Log(this, $"Requesting index page data {nextPageContext.url}" + "...Done");
                    break;
                case OverviewType.Author:
                    MyLog.Log(this, $"Requesting author data {nextPageContext.url}" + "...");
                    analysisModule.ReadAuthor(uid, websiteKey, obj as Author, this); 
                    MyLog.Log(this, $"Requesting author data {nextPageContext.url}" + "...Done");
                    break;
                case OverviewType.Category:
                    MyLog.Log(this, $"Requesting Category data {nextPageContext.url}" + "...");
                    analysisModule.ReadCategory(uid, websiteKey, obj as Category, this); 
                    MyLog.Log(this, $"Requesting Category data {nextPageContext.url}" + "...Done");
                    break;
                default:
                    break;
            } 
            MyLog.Log(this, nameof(LoadNextPage) + "...Done");
        }

        private class RecyclerViewAdpater : RecyclerView.Adapter
        {
            public event EventHandler<ArticalOverview> OnItemClick = null;
            public event EventHandler LoadNextPage = null;
            public event EventHandler<Category> OnCategorySelected = null;
            public Website currentWebsite { get; set; } = null;
            public ArticalOverview[] data { get; set; } = null;

            private int oldCategory = 0;
            private int currentCategory = 0;

            public RecyclerViewAdpater(Website currentWebsite) : base()
            {
                this.currentWebsite = currentWebsite;
            }
            public override int ItemCount
            {
                get
                {
                    return data != null ? data.Length : 0;
                }
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var item = data[position];
                var vHolder = holder as ViewHolder;

                vHolder.hScrollView.Visibility = position == 0 ? ViewStates.Visible : ViewStates.Gone;
                if (position == 0) vHolder.SetCurrentCategorySelection(currentWebsite.Color, currentCategory, oldCategory);

                vHolder.websiteComicTextView.Text = currentWebsite.ComicText;
                vHolder.websiteComicTextView.SetBackgroundColor(Android.Graphics.Color.ParseColor(currentWebsite.Color));
                vHolder.titleTextView.Text = item.Title;

                vHolder.summaryTextView.Text = item.SummaryText;
                vHolder.summaryTextView.Visibility = string.IsNullOrEmpty(item.SummaryText) ? ViewStates.Invisible : ViewStates.Visible;

                vHolder.dateTextView.Text = MyGlobal.GetHumanReadableDate(item.Date);
                if (vHolder.dateTextView.Text == null) vHolder.dateTextView.Visibility = ViewStates.Gone;
                else vHolder.dateTextView.Visibility = ViewStates.Visible;

                if (item?.Authors != null)
                {
                    vHolder.authorTextView.Text = "By, " + item.Authors[0].Name;//TODO: Extend support for more author and allow individual author to have their options with onClick listener
                    vHolder.authorTextView.Visibility = ViewStates.Visible;
                }
                else vHolder.authorTextView.Visibility = ViewStates.Gone;

                if (item.IsDatabaseConfirmed_SeenOn && !string.IsNullOrEmpty(item.SeenOn))
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


                if (data.Length - position <= MyGlobal.NextPageContentNumber) LoadNextPage?.Invoke(this, null); //this.parent.LoadNextPage();    //sent a request to load next page

            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var layoutInflator = (LayoutInflater)parent.Context.GetSystemService(Service.LayoutInflaterService);
                var view = layoutInflator.Inflate(Resource.Layout.overview_single_item_category, parent, false);
                return new ViewHolder(layoutInflator, currentWebsite, view, onItemClick, onCatSelected);
            }
            private void onItemClick(int position) => OnItemClick?.Invoke(this, data[position]);
            private void onCatSelected(View view, int index)
            {
                oldCategory = currentCategory;
                currentCategory = index;
                
                NotifyDataSetChanged();

                

                OnCategorySelected?.Invoke(view, currentWebsite.Categories[(int)view.Tag]);
            }

            class ViewHolder : RecyclerView.ViewHolder
            {
                public TextView websiteComicTextView, titleTextView, summaryTextView, authorTextView, dateTextView;
                public TextView[] tag = null;
                public LinearLayout hScrollView = null;

                public CardView[] cat_cardView = null;
                public TextView[] cat_textView = null;

                public void SetCurrentCategorySelection(string defaultColor, int currentIndex, int oldIndex = -1)
                {
                    if (oldIndex == -1) for (int i = 0; i < cat_cardView.Length; i++) setColorTo(i, defaultColor);
                    else setColorTo(oldIndex, defaultColor);

                    cat_cardView?[currentIndex].SetCardBackgroundColor(Android.Graphics.Color.ParseColor("#CFD8DC"));
                    cat_textView?[currentIndex].SetTextColor(Android.Graphics.Color.ParseColor("#000000"));

                    cat_cardView?[currentIndex].Parent.RequestChildFocus(cat_cardView?[currentIndex], cat_cardView?[currentIndex]);
                }
                private void setColorTo(int index, string defaultColor)
                {
                    cat_cardView?[index].SetCardBackgroundColor(Android.Graphics.Color.ParseColor(defaultColor));
                    cat_textView?[index].SetTextColor(Android.Graphics.Color.ParseColor("#ffffff"));
                }

                public ViewHolder(LayoutInflater inflater, Website website, View v, Action<int> onClick, Action<View, int> onCategorySelected) : base(v)
                {
                    hScrollView = v.FindViewById<LinearLayout>(Resource.Id.overview_cat_horizontalScrollView);
                    setupCategories(inflater, website, hScrollView, onCategorySelected);

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

                    v.Click += (sender, e) => onClick(AdapterPosition);
                }

                private void setupCategories(LayoutInflater inflater, Website website, LinearLayout hScrollView, Action<View, int> onCategorySelected)
                {
                    if (website == null || website.Categories == null) return;
                    var index = 0;

                    cat_cardView = new CardView[website.Categories.Length];
                    cat_textView = new TextView[website.Categories.Length];

                    foreach (var cat in website.Categories)
                    {
                        var cardView = inflater.Inflate(Resource.Layout.overview_cat_single_item, hScrollView, false) as CardView;
                        cardView.SetCardBackgroundColor(Android.Graphics.Color.ParseColor(website.Color).ToArgb());

                        var textView = cardView.FindViewById<TextView>(Resource.Id.overviewCatNameTextView);
                        textView.Text = cat.Name;
                        cardView.Tag = index;

                        cat_cardView[index] = cardView;
                        cat_textView[index] = textView;

                        hScrollView.AddView(cardView);

                        cardView.Click += (sender, e) => onCategorySelected(cardView, (int)cardView.Tag);

                        index++;
                    }
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