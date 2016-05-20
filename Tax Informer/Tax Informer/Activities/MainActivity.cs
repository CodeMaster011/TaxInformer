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
using SupportToolBar = Android.Support.V7.Widget.Toolbar;
using SupportFragment = Android.Support.V4.App.Fragment;
using SupportFragmentManager = Android.Support.V4.App.FragmentManager;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.App;
using Java.Lang;
using System.Collections.Generic;

[assembly: Application(Theme = "@style/MyTheme")]//"@android:style/Theme.Material.Light"
namespace Tax_Informer
{

    [Activity(Label = "Tax Informer", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private TabAdapter adapter = null;
        private SupportToolBar toolBar = null;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            MyLog.Log(this, $"OnCreate...");

            MyGlobal.applicationContext = this;

            SetContentView(Resource.Layout.Main);
            //SetContentView(Resource.Layout.artical);
            //return;
            MyGlobal.ChangeStatusBarColor(Window, "#673AB7");
            toolBar = FindViewById<SupportToolBar>(Resource.Id.mainToolbar);
            toolBar.Title = "Tax Informer";

            TabLayout tabs = FindViewById<TabLayout>(Resource.Id.mainTabs);

            ViewPager viewPager = FindViewById<ViewPager>(Resource.Id.mainViewpager);

            MyLog.Log(this, "Implementing Fragments...");
            SetUpViewPager(viewPager);
            MyLog.Log(this, "Implementing Fragments...Done");
            tabs.SetupWithViewPager(viewPager);

            MyLog.Log(this, $"OnCreate...Done");
        }
        

        private void SetUpViewPager(ViewPager viewPager)
        {
            adapter = new TabAdapter(SupportFragmentManager);
            adapter.AddFragment(new Fragments.MainFragment(), "Online");
            adapter.AddFragment(new Fragments.OfflineFragment(), "Offline");

            viewPager.Adapter = adapter;
        }

        public override void OnBackPressed()
        {
            MyGlobal.database.Close();
            base.OnBackPressed();
        }

        public class TabAdapter : FragmentPagerAdapter
        {
            public List<SupportFragment> Fragments { get; set; }
            public List<string> FragmentNames { get; set; }

            public TabAdapter(SupportFragmentManager sfm) : base(sfm)
            {
                Fragments = new List<SupportFragment>();
                FragmentNames = new List<string>();
            }

            public void AddFragment(SupportFragment fragment, string name)
            {
                Fragments.Add(fragment);
                FragmentNames.Add(name);
            }

            public override int Count
            {
                get
                {
                    return Fragments.Count;
                }
            }

            public override SupportFragment GetItem(int position)
            {
                return Fragments[position];
            }

            public override ICharSequence GetPageTitleFormatted(int position)
            {
                return new Java.Lang.String(FragmentNames[position]);
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
                int spanCount = System.Math.Max(1, MeasuredWidth / columnWidth);
                manager.SpanCount = spanCount;
            }

        }
    }
}

