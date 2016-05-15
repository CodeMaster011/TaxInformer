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

namespace Tax_Informer.Core
{
    //TODO: Think a way to pass Website as object or string as type to activities
    internal abstract class Website
    {
        private Stack<Dictionary<string, object>> stateHolder = null;

        public abstract string Name { get; }
        public abstract string ComicText { get; }
        public abstract string IndexPageLink { get; }
        public abstract string Color { get; }
        public virtual Category[] Categories { get; } = null;

        [Obsolete()]
        public virtual bool IsSearchable { get; } = false;

        protected void UpdateState(Dictionary<string, object> state)
        {
            if (stateHolder == null) stateHolder = new Stack<Dictionary<string, object>>();
            stateHolder.Push(state);
        }
        public bool RestoreState()
        {
            if (stateHolder?.Count > 0)
            {
                RestoreState(stateHolder.Pop());
                return true;
            }
            return false;
        }

        [Obsolete()]
        protected virtual string GetSearchUrl(string keyword) => null;
        [Obsolete()]
        public virtual ArticalOverview[] GetSearchResult(string searchUrl, out string nextPageUrl)
        {
            nextPageUrl = null;
            return null;
        }

        protected abstract void RestoreState(Dictionary<string, object> state);        
        public abstract ArticalOverview[] ReadIndexPage(string url, HtmlAgilityPack.HtmlDocument doc, out string nextPageUrl);
        public abstract Artical ReadArtical(ArticalOverview overview, HtmlAgilityPack.HtmlDocument doc);
        public abstract ArticalOverview[] ReadAuthor(Author author, HtmlAgilityPack.HtmlDocument doc, out string nextPageUrl);
        public abstract ArticalOverview[] ReadCategory(Category category, HtmlAgilityPack.HtmlDocument doc, out string nextPageUrl);        
    }
    internal class WebsitePortableInformation
    {
        public string Name { get; set; }
        public string ComicText { get; set; }
        public string IndexPageLink { get; set; }
        public string Color { get; set; }

        public WebsitePortableInformation() { }
        public WebsitePortableInformation(Website website)
        {
            this.Name = website.Name;
            this.ComicText = website.ComicText;
            this.IndexPageLink = website.IndexPageLink;
            this.Color = website.Color;
        }
        public WebsitePortableInformation(Bundle bundle)
        {
            Name = bundle.GetString(nameof(Name));
            ComicText = bundle.GetString(nameof(ComicText));
            IndexPageLink = bundle.GetString(nameof(IndexPageLink));
            Color = bundle.GetString(nameof(Color));
        }
        public Bundle ToBundle()
        {
            var b = new Bundle();
            b.PutString(nameof(Name), Name);
            b.PutString(nameof(ComicText), ComicText);
            b.PutString(nameof(IndexPageLink), IndexPageLink);
            b.PutString(nameof(Color), Color);
            return b;
        }
        public static WebsitePortableInformation FromWebsite(Website website) => new WebsitePortableInformation(website);
        public static WebsitePortableInformation FromBundle(Bundle bundle) => new WebsitePortableInformation(bundle);
    }
}