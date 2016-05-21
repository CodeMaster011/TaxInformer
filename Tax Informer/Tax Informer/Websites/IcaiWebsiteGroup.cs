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
using Tax_Informer.Core;

namespace Tax_Informer.Websites
{
    class IcaiWebsiteGroup : WebsiteGroup
    {
        private const string webDir = "http://www.icai.org/";

        public override string Color { get; } = "#EF5350";
        public override string ComicText { get; } = "ICAI";

        public override string Name { get; } = "ICAI HO";

        protected override List<ChildWebsite> GetChildWebsiteList()
        {
            return new List<ChildWebsite>()
            {
                new WhatsNewChild(this),
                new BosAnnouncementChild(this)
            };
        }

        public override Artical ReadArtical(ArticalOverview overview, HtmlDocument doc)
        {
            var container = doc.GetElementbyId("maincontent");
            if (container == null) return null;

            var contentTable = Helper.AnyChild(container, "td", new Dictionary<string, string>()
            {
                ["align"] = "left",
                ["style"] = "TEXT-ALIGN:justify;line-height:20px"
            });
            if (contentTable == null) return null;

            return new Artical()
            {
                Title = overview.Title,
                MyLink = overview.LinkOfActualArtical,
                HtmlText = contentTable.InnerHtml
            };
        }

        public override ArticalOverview[] ReadIndexPage(string url, HtmlDocument doc, out string nextPageUrl)
        {
            nextPageUrl = null;

            var container = doc.GetElementbyId("maincontent");
            if (container == null) return null;

            var liTags = Helper.AllChild(container, "li");
            if (liTags == null) return null;

            List<ArticalOverview> data = new List<ArticalOverview>();
            foreach (var li in liTags)
            {
                var aLink = Helper.AnyChild(li, "a");
                data.Add(new ArticalOverview()
                {
                    LinkOfActualArtical = Helper.CombindUrl(webDir, aLink.GetAttributeValue("href", "")),
                    Title = HtmlEntity.DeEntitize(aLink.InnerText)
                });
            }

            var pageContainer = Helper.AnyChild(container, "td", new Dictionary<string, string>() { ["align"] = "center" });
            if (pageContainer != null)
            {
                try
                {

                    var currentPageIndex = int.Parse(Helper.AnyChild(pageContainer, "font").InnerText);
                    var allLinks = Helper.AllChild(pageContainer, "a");

                    foreach (var link in allLinks)
                    {
                        if (link.InnerText == (currentPageIndex + 1).ToString())
                            nextPageUrl = Helper.CombindUrl(webDir, link.GetAttributeValue("href", ""));
                    }
                }
                catch (Exception) { }
            }

            return data.Count > 0 ? data.ToArray() : null;
        }

        public override Artical ReadArticalExtrnal(ArticalOverview overview, string extrnalLink)
        {
            var artical = overview.ToArtical();
            artical.ExternalFileLink = extrnalLink;
            return artical;
        }

        private sealed class WhatsNewChild : ChildWebsite
        {
            public override string IndexPageLink { get; } = "http://www.icai.org/new_category.html?c_id=240";
            public WhatsNewChild(WebsiteGroup parent) : base(parent, "What's New") { }

            public override Artical ReadArtical(ArticalOverview overview, HtmlDocument doc) => Parent.ReadArtical(overview, doc);

            public override ArticalOverview[] ReadIndexPage(string url, HtmlDocument doc, out string nextPageUrl) => Parent.ReadIndexPage(url, doc, out nextPageUrl);

            public override Artical ReadArticalExtrnal(ArticalOverview overview, string extrnalLink) => Parent.ReadArticalExtrnal(overview, extrnalLink);

            protected override void RestoreState(Dictionary<string, object> state) { }
        }

        private sealed class BosAnnouncementChild : ChildWebsite
        {             
            public override string IndexPageLink { get; } = "http://www.icai.org/new_category.html?c_id=342";
            public override Category[] Categories
            {
                get
                {
                    return new Category[]
                    {
                        new Category()
                        {
                            Name = "IPC/ATC",
                            Link ="http://www.icai.org/new_category.html?c_id=342"
                        },
                        new Category()
                        {
                            Name = "CPT",
                            Link ="http://www.icai.org/new_category.html?c_id=340"
                        },
                        new Category()
                        {
                            Name = "Final",
                            Link ="http://www.icai.org/new_category.html?c_id=343"
                        },
                        new Category()
                        {
                            Name = "e-Learning",
                            Link ="http://www.icai.org/new_category.html?c_id=344"
                        },
                        new Category()
                        {
                            Name = "Events",
                            Link ="http://www.icai.org/new_category.html?c_id=348"
                        },
                        new Category()
                        {
                            Name = "Important Announcements",
                            Link ="http://www.icai.org/new_category.html?c_id=347"
                        }
                    };
                }
            }
            public BosAnnouncementChild(WebsiteGroup parent) : base(parent, "Bos") { }
            public override Artical ReadArtical(ArticalOverview overview, HtmlDocument doc) => Parent.ReadArtical(overview, doc);

            public override ArticalOverview[] ReadCategory(Category category, HtmlDocument doc, out string nextPageUrl) => Parent.ReadIndexPage(category.Link, doc, out nextPageUrl);
            public override ArticalOverview[] ReadIndexPage(string url, HtmlDocument doc, out string nextPageUrl) => Parent.ReadIndexPage(url, doc, out nextPageUrl);
            public override Artical ReadArticalExtrnal(ArticalOverview overview, string extrnalLink) => Parent.ReadArticalExtrnal(overview, extrnalLink);
            protected override void RestoreState(Dictionary<string, object> state) { }
        }
    }
}