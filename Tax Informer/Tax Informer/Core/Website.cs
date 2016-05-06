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

        protected virtual string GetSearchUrl(string keyword) => null;
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
}