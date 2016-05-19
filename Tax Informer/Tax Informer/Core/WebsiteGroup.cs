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
using HtmlAgilityPack;

namespace Tax_Informer.Core
{
    internal abstract class WebsiteGroup : Website
    {
        public override string IndexPageLink { get; } = null;
        public List<ChildWebsite> ChildWebsites { get; } = null;
        public WebsiteGroup()
        {
            this.ChildWebsites = GetChildWebsiteList();

            foreach (var child in ChildWebsites)
                Config.AddHiddenWebsite(child.HiddenKey, child);

        }
        protected abstract List<ChildWebsite> GetChildWebsiteList();
        public override Artical ReadArtical(ArticalOverview overview, HtmlDocument doc) => null;

        public override ArticalOverview[] ReadAuthor(Author author, HtmlDocument doc, out string nextPageUrl) { nextPageUrl = null; return  null; }

        public override ArticalOverview[] ReadCategory(Category category, HtmlDocument doc, out string nextPageUrl) { nextPageUrl = null; return null; }

        public override ArticalOverview[] ReadIndexPage(string url, HtmlDocument doc, out string nextPageUrl) { nextPageUrl = null; return null; }
        protected override void RestoreState(Dictionary<string, object> state) { }

        internal abstract class ChildWebsite : Website
        {
            public WebsiteGroup Parent { get; } = null;
            public string HiddenKey { get; } = null;
            public  string ChildName { get; }
            public override string Name { get; }
            public override string ComicText { get; }
            public override string Color { get; }
            
            public ChildWebsite(WebsiteGroup parent, string childName) : base()
            {
                this.Name = parent.Name;
                this.ComicText = parent.ComicText;
                this.Color = parent.Color;
                this.Parent = parent;
                this.ChildName = childName;
                this.HiddenKey = $"{parent.Name}-{childName}";
            }
        }
    }
}