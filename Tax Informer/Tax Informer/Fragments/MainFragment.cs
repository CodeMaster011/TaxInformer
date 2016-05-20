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
using Android.Graphics;

namespace Tax_Informer.Fragments
{
    public class MainFragment : SupportFragment
    {
        private RecyAdapter adapter = null;
        private RecyclerView recyclerView = null;
        private RecyclerView.LayoutManager recyLayoutManager = null;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here

            MyLog.Log(this, "Fragments creating...");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            MyLog.Log(this, "Fragments view creating...");

            recyclerView = inflater.Inflate(Resource.Layout.main_website_list, container, false) as RecyclerView;
            recyclerView.SetLayoutManager(recyLayoutManager = new GridLayoutManager(container.Context, 2, (int)Orientation.Vertical, false));
            recyclerView.AddItemDecoration(new VerticalSpaceItemDecoration(10, 10));
            recyclerView.SetAdapter(adapter = new RecyAdapter());
            adapter.OnItemClick += Adapter_OnItemClick;

            var data = new string[Config.websites.Keys.Count];

            MyLog.Log(this, "Updating adapter data...");
            Config.websites.Keys.CopyTo(data, 0);
            adapter.data = data;
            MyLog.Log(this, $"Updating adapter data...Done(Length:{adapter.data?.Length})");

            MyLog.Log(this, "Fragments view creating...Done");
            return recyclerView;
        }
        private void Adapter_OnItemClick(object sender, string websiteKey)
        {
            MyLog.Log(this, "Adapter OnItemClick - websiteKey : " + websiteKey);
            MyLog.Log(this, $"starting overview activity - websiteKey :  {websiteKey}...");
            MyGlobal.StartActivityOverview(this.Activity, websiteKey);
            MyLog.Log(this, $"starting overview activity - websiteKey :  {websiteKey}...Done");
        }


        private class RecyAdapter : RecyclerView.Adapter
        {
            public string[] data { get; set; } = null;

            public event EventHandler<string> OnItemClick = null;

            public override int ItemCount
            {
                get
                {
                    return data == null ? 0 : data.Length;
                }
            }
            public override void OnBindViewHolder(RecyclerView.ViewHolder _holder, int position)
            {
                try
                {

                    var holder = _holder as ViewHolder;
                    var data = this.data[position];
                    var website = Config.GetWebsite(data);

                    holder.websiteComicTextView.Text = website.ComicText;
                    holder.linearLayout.SetBackgroundColor(Android.Graphics.Color.ParseColor(website.Color));

                    holder.websiteNameTextView.Text = website.Name;
                }
                catch (Exception) { }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var layoutInflator = (LayoutInflater)parent.Context.GetSystemService(Service.LayoutInflaterService);
                var view = layoutInflator.Inflate(Resource.Layout.main_single_item, parent, false);
                return new ViewHolder(view, onItemClickCallback);
            }

            private void onItemClickCallback(object o, int position) => OnItemClick?.Invoke(o, data[position]);

            private class ViewHolder : RecyclerView.ViewHolder
            {
                public TextView websiteComicTextView, websiteNameTextView;
                public LinearLayout linearLayout;

                public ViewHolder(View v, Action<object, int> onClickCallback) : base(v)
                {
                    websiteComicTextView = v.FindViewById<TextView>(Resource.Id.main_single_item_websiteComicText);
                    websiteNameTextView = v.FindViewById<TextView>(Resource.Id.main_single_item_websiteName);
                    linearLayout = v.FindViewById<LinearLayout>(Resource.Id.main_single_item_LinearLayout);

                    v.Click += (sender, e) => onClickCallback(v, base.AdapterPosition);
                }
            }
        }
    }

    public class VerticalSpaceItemDecoration : RecyclerView.ItemDecoration
    {

        private readonly int mVerticalSpaceHeight;
        private readonly int mHorizontalSpaceWidth;

        public VerticalSpaceItemDecoration(int mVerticalSpaceHeight, int mHorizontalSpaceWidth)
        {
            this.mVerticalSpaceHeight = mVerticalSpaceHeight;
            this.mHorizontalSpaceWidth = mHorizontalSpaceWidth;
        }
        
        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            base.GetItemOffsets(outRect, view, parent, state);

            outRect.Bottom = mVerticalSpaceHeight;
            outRect.Right = mHorizontalSpaceWidth / 2;
        }
    }
}