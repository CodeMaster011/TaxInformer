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
using Android.Support.V7.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using Tax_Informer.Core;
using Android.Support.V4.Widget;

namespace Tax_Informer.Fragments
{
    internal class OfflineFragment : SupportFragment, IUiOfflineArticalOverviewResponseHandler
    {
        private RecyAdapter adapter = null;
        private RecyclerView recyclerView = null;
        private RecyclerView.LayoutManager recyLayoutManager = null;
        private SwipeRefreshLayout swipeRefreshLayout = null;

        public void OfflineArticalOverviewProcessedCallback(string transactionId, ArticalOverviewOffline[] articalOverviews)
        {
            adapter.data = articalOverviews;
            Activity.RunOnUiThread(notify);
        }
        private void notify()
        {
            adapter?.NotifyDataSetChanged();
            if (swipeRefreshLayout != null) swipeRefreshLayout.Refreshing = false;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            swipeRefreshLayout = inflater.Inflate(Resource.Layout.main_offline_list, container, false) as SwipeRefreshLayout;
            swipeRefreshLayout.Refresh += SwipeRefreshLayout_Refresh;
            swipeRefreshLayout.SetColorSchemeColors(new int[] {
                Android.Graphics.Color.ParseColor("#673AB7").ToArgb()});

            recyclerView = swipeRefreshLayout.FindViewById<RecyclerView>(Resource.Id.offlineRecyclerView);
            recyclerView.SetLayoutManager(recyLayoutManager = new LinearLayoutManager(this.Context, (int)Orientation.Vertical, false));
            recyclerView.SetAdapter(adapter = new RecyAdapter());
            adapter.OnItemClick += Adapter_OnItemClick;

            MyGlobal.database.GetAllOfflineArticalList(MyGlobal.UidGenerator(), this);

            return swipeRefreshLayout;
        }

        private void SwipeRefreshLayout_Refresh(object sender, EventArgs e)
        {
            swipeRefreshLayout.Refreshing = true;
            MyGlobal.database.GetAllOfflineArticalList(MyGlobal.UidGenerator(), this);
        }

        private void Adapter_OnItemClick(object sender, ArticalOverviewOffline item)
        {
            MyGlobal.StartActivityArtical(this.Context, item, item.WebsiteKey, true);
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