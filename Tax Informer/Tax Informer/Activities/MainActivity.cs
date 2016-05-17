using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Tax_Informer.Core;
using Android.Util;

[assembly: Application(Theme = "@style/MyTheme")]//"@android:style/Theme.Material.Light"
namespace Tax_Informer
{

    [Activity(Label = "Tax Informer", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : ActionBarActivity
    {
        private RecyAdapter adapter = null;
        private RecyclerView recyclerView = null;
        private RecyclerView.LayoutManager recyLayoutManager = null;
        private Android.Support.V7.Widget.Toolbar toolbar = null;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            MyGlobal.ChangeStatusBarColor(Window, "#673AB7");

            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.mainToolbar);
            toolbar.Title = "Tax Informer";

            recyclerView = FindViewById<RecyclerView>(Resource.Id.mainRecyclerView);
            recyclerView.SetLayoutManager(recyLayoutManager = new GridLayoutManager(this, 2, (int)Orientation.Vertical, false));
            recyclerView.SetAdapter(adapter = new RecyAdapter());
            adapter.OnItemClick += Adapter_OnItemClick;

            var data = new string[Config.websites.Keys.Count];
            Config.websites.Keys.CopyTo(data, 0);
            adapter.data = data;
        }

        private void Adapter_OnItemClick(object sender, string websiteKey)
        {
            MyGlobal.StartActivityOverview(this, websiteKey);
        }

        public override void OnBackPressed()
        {
            MyGlobal.database.Close();
            base.OnBackPressed();
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
    public class AutofitRecyclerView : RecyclerView
    {
        private GridLayoutManager manager;
        private int columnWidth = -1;

        public AutofitRecyclerView(Context context) : base(context)
        {

            init(context, null);
        }

        public AutofitRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {

            init(context, attrs);
        }

        public AutofitRecyclerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {

            init(context, attrs);
        }

        private void init(Context context, IAttributeSet attrs)
        {

            manager = new GridLayoutManager(Context, 1);
            SetLayoutManager(manager);
        }


        protected void onMeasure(int widthSpec, int heightSpec)
        {
            base.OnMeasure(widthSpec, heightSpec);
            if (columnWidth > 0)
            {
                int spanCount = Math.Max(1, MeasuredWidth / columnWidth);
                manager.SpanCount = spanCount;
            }

        }
    }
}

