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
using SupportToolBar = Android.Support.V7.Widget.Toolbar;
using SupportFragment = Android.Support.V4.App.Fragment;
using SupportFragmentManager = Android.Support.V4.App.FragmentManager;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.App;
using Java.Lang;

namespace Tax_Informer.Activities
{
    [Activity(Label = "Overview")]
    public class OverviewActivityWithFragments : AppCompatActivity
    {
        public const string PassWebsiteKey = nameof(PassWebsiteKey);

        private string currentWebsiteKey = null;
        private TabAdapter adapter = null;
        private SupportToolBar toolBar = null;
        private TabLayout tabs = null;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            MyLog.Log(this, $"OnCreate...");

            SetContentView(Resource.Layout.Main);
            
            if (Intent.Extras != null && Intent.Extras.ContainsKey(PassWebsiteKey))
                currentWebsiteKey = Intent.Extras.GetString(PassWebsiteKey);
            else { Finish(); return; }

            var website = Config.GetWebsite(currentWebsiteKey);

            MyGlobal.ChangeStatusBarColor(Window, website.Color);
            toolBar = FindViewById<SupportToolBar>(Resource.Id.mainToolbar);
            toolBar.Title = website.Name;
            toolBar.SetBackgroundColor(Android.Graphics.Color.ParseColor(website.Color));

            tabs = FindViewById<TabLayout>(Resource.Id.mainTabs);
            tabs.SetBackgroundColor(Android.Graphics.Color.ParseColor(website.Color));

            ViewPager viewPager = FindViewById<ViewPager>(Resource.Id.mainViewpager);

            SetUpViewPager(viewPager);

            tabs.SetupWithViewPager(viewPager);

            MyLog.Log(this, $"OnCreate...Done");
        }

        private void SetUpViewPager(ViewPager viewPager)
        {
            adapter = new TabAdapter(SupportFragmentManager);
            try
            {
                var websiteGroup = Config.GetWebsite(currentWebsiteKey) as WebsiteGroup;
                foreach (var child in websiteGroup.ChildWebsites)
                {
                    MyLog.Log(this, $"Adding OverviewFragment {child.ChildName}...");
                    adapter.AddFragment(new Fragments.OverviewFragment(child.HiddenKey), child.ChildName);
                    MyLog.Log(this, $"Adding OverviewFragment {child.ChildName}...Done");
                }               
                    
            }
            catch (System.Exception)
            {
                MyLog.Log(this, $"Adding OverviewFragment as single child --website key {currentWebsiteKey}...");
                adapter.AddFragment(new Fragments.OverviewFragment(currentWebsiteKey), Config.GetWebsite(currentWebsiteKey).Name);
                tabs.Visibility = ViewStates.Gone;
                MyLog.Log(this, $"Adding OverviewFragment as single child --website key {currentWebsiteKey}...Done");
            }            

            viewPager.Adapter = adapter;
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
}