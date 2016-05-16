using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using widget = Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using Tax_Informer.Core;

namespace Tax_Informer.Activities
{
    [Activity(Label = "OfflineActivity")]
    internal class OfflineActivity : ActionBarActivity, IUiOfflineArticalOverviewResponseHandler
    {
        private RecyAdapter adapter = null;
        private RecyclerView recyclerView = null;
        private RecyclerView.LayoutManager recyLayoutManager = null;
        private widget.Toolbar toolbar = null;

        public void OfflineArticalOverviewProcessedCallback(string transactionId, ArticalOverviewOffline[] articalOverviews)
        {
            adapter.data = articalOverviews;
            RunOnUiThread(notify);
        }
        private void notify() => adapter.NotifyDataSetChanged();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.offline);

            recyclerView = FindViewById<RecyclerView>(Resource.Id.offlineRecyclerView);
            recyclerView.SetLayoutManager(recyLayoutManager = new LinearLayoutManager(this, (int)Orientation.Vertical, false));
            recyclerView.SetAdapter(adapter = new RecyAdapter());
            adapter.OnItemClick += Adapter_OnItemClick;

            toolbar = FindViewById<widget.Toolbar>(Resource.Id.offlineToolbar);
            toolbar.Title = "Offline";
            toolbar.SetBackgroundColor(Android.Graphics.Color.ParseColor("#4CAF50"));
            toolbar.SetTitleTextColor(Android.Graphics.Color.ParseColor("#ffffff"));

            MyGlobal.database.GetAllOfflineArticalList(MyGlobal.UidGenerator(), this);
        }

        private void Adapter_OnItemClick(object sender, ArticalOverviewOffline item)
        {
            MyGlobal.StartActivityArtical(this, item, item.WebsiteKey, true);
        }

        private class RecyAdapter : RecyclerView.Adapter
        {
            public ArticalOverviewOffline[] data { get; set; } = null;

            public event EventHandler<ArticalOverviewOffline> OnItemClick = null;

            public override int ItemCount
            {
                get
                {
                    return data == null ? 0 : data.Length;
                }
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder _holder, int position)
            {
                var holder = _holder as ViewHolder;
                var data = this.data[position];
                var website = Config.GetWebsite(data.WebsiteKey);

                holder.websiteComicTextView.Text = website.ComicText;
                holder.websiteComicTextView.Visibility = ViewStates.Visible;
                holder.websiteComicTextView.SetBackgroundColor(Android.Graphics.Color.ParseColor(website.Color));

                holder.titleTextView.Text = data.Title;

                holder.summaryTextView.Text = data.SummaryText;

                holder.authorTextView.Text = data.Authors?[0].Name;
                holder.authorTextView.Visibility = data.Authors == null ? ViewStates.Invisible : ViewStates.Visible;

                holder.dateTextView.Text = MyGlobal.GetHumanReadableDate(data.Date);
                holder.dateTextView.Visibility = data.Date == null ? ViewStates.Invisible : ViewStates.Visible;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var layoutInflator = (LayoutInflater)parent.Context.GetSystemService(Service.LayoutInflaterService);
                var view = layoutInflator.Inflate(Resource.Layout.overview_single_item, parent, false);
                return new ViewHolder(view, onItemClickCallback);
            }

            private void onItemClickCallback(object o, int position) => OnItemClick?.Invoke(o, data[position]);

            private class ViewHolder : RecyclerView.ViewHolder
            {
                public TextView websiteComicTextView, titleTextView, summaryTextView, authorTextView, dateTextView;

                public ViewHolder(View v, Action<object, int> onClickCallback) : base(v)
                {
                    websiteComicTextView = v.FindViewById<TextView>(Resource.Id.websiteComicTextView);
                    titleTextView = v.FindViewById<TextView>(Resource.Id.titleTextView);
                    summaryTextView = v.FindViewById<TextView>(Resource.Id.summaryTextView);
                    authorTextView = v.FindViewById<TextView>(Resource.Id.authorTextView);
                    dateTextView = v.FindViewById<TextView>(Resource.Id.dateTextView);

                    v.Click += (sender, e) => onClickCallback(v, base.AdapterPosition);
                }
            }
        }
    }
}