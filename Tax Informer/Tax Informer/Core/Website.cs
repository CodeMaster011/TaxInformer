using System;
using System.Collections.Generic;

namespace Tax_Informer.Core
{
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

        protected virtual void RestoreState(Dictionary<string, object> state)
        {
        }

        public abstract ArticalOverview[] ReadIndexPage(string url, HtmlAgilityPack.HtmlDocument doc, out string nextPageUrl);

        public abstract Artical ReadArtical(ArticalOverview overview, HtmlAgilityPack.HtmlDocument doc);
        public virtual Artical ReadArticalExtrnal(ArticalOverview overview, string extrnalLink) => null;

        public virtual ArticalOverview[] ReadAuthor(Author author, HtmlAgilityPack.HtmlDocument doc, out string nextPageUrl)
        {
            nextPageUrl = null; return null;
        }

        public virtual ArticalOverview[] ReadCategory(Category category, HtmlAgilityPack.HtmlDocument doc, out string nextPageUrl)
        {
            nextPageUrl = null; return null;
        }
    }

    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class WebsiteAttribute : Attribute
    {
        readonly string websiteKey;

        public WebsiteAttribute(string websiteKey)
        {
            this.websiteKey = websiteKey;
        }

        public string WebsiteKey
        {
            get
            {
                return websiteKey;
            }
        }
    }
}