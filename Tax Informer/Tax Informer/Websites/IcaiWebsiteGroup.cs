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
    [Website("A4A59C21-B237-4E6C-A530-382580F9AD12")]
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
                new BosAnnouncementChild(this),
                new NewsChild(this),
                new AnnouncementChild(this),
                new ExaminationChild(this)
            };
        }

        public override Artical ReadArtical(ArticalOverview overview, HtmlDocument doc)
        {
            var artical = overview.ToArtical();

            var container = doc.GetElementbyId("maincontent");
            if (container == null) return null;

            var contentTable = Helper.AnyChild(container, "td", new Dictionary<string, string>()
            {
                ["align"] = "left",
                ["style"] = "TEXT-ALIGN:justify;line-height:20px"
            });
            if (contentTable == null) return null;

            artical.HtmlText = contentTable.InnerHtml;

            if(string.IsNullOrEmpty(artical.Title) || string.IsNullOrEmpty(artical.Date))
            {
                var titleContainer = Helper.AnyChild(container, "span", "Heading_Inner");
                if (titleContainer != null)
                {
                    string title, date;
                    getTitleAndDate(HtmlEntity.DeEntitize(titleContainer.InnerText), out title, out date);
                    artical.Title = title;
                    artical.Date = date;
                }                    
            }
            return artical;
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

                string title, date;
                getTitleAndDate(HtmlEntity.DeEntitize(aLink.InnerText), out title, out date);

                data.Add(new ArticalOverview()
                {
                    LinkOfActualArtical = Helper.CombindUrl(webDir, aLink.GetAttributeValue("href", "")),
                    Title = title,
                    Date = date
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

        private void getTitleAndDate(string value, out string title, out string date)
        {
            //Ind AS Transition Facilitation Group (ITFG) Clarification Bulletins - (10-05-2016)
            //CA curriculum, exam pattern to be revised soon: Reddy - The Sikh Times - Vadodara - (11-Apr-2016)
            try
            {
                var indexOfStartDate = value.LastIndexOf('(');

                var tempTitle = value.Substring(0, indexOfStartDate);
                title = tempTitle.EndsWith(" - ") ? (tempTitle.Substring(0, tempTitle.Length - 3)) : tempTitle;

                var tempDate = value.Substring(indexOfStartDate).Replace("(", "").Replace(")", "");
                var ss = tempDate.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                int month = -1;
                if (int.TryParse(ss[1], out month))
                    date = ss[2] + ss[1] + ss[0];
                else
                    date = ss[2] + Helper.GetMonthIndex(ss[1]).ToString("00") + ss[0];
            }
            catch (Exception)
            {
                title = value;
                date = null;                
            }            
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

        private sealed class NewsChild : ChildWebsite
        {
            public override string IndexPageLink { get; } = "http://www.icai.org/new_all_news.html";
            public NewsChild(WebsiteGroup parent) : base(parent, "News") { }

            //TODO: Some articals are now properly viewing in news like http://www.icai.org/news.html?news=7111
            public override Artical ReadArtical(ArticalOverview overview, HtmlDocument doc) => Parent.ReadArtical(overview, doc);

            public override ArticalOverview[] ReadIndexPage(string url, HtmlDocument doc, out string nextPageUrl) => Parent.ReadIndexPage(url, doc, out nextPageUrl);

            public override Artical ReadArticalExtrnal(ArticalOverview overview, string extrnalLink) => Parent.ReadArticalExtrnal(overview, extrnalLink);
        }

        private sealed class AnnouncementChild : ChildWebsite
        {
            public override string IndexPageLink { get; } = "http://www.icai.org/new_category.html?c_id=219";
            public AnnouncementChild(WebsiteGroup parent) : base(parent, "Announcements") { }

            public override Artical ReadArtical(ArticalOverview overview, HtmlDocument doc) => Parent.ReadArtical(overview, doc);

            public override ArticalOverview[] ReadIndexPage(string url, HtmlDocument doc, out string nextPageUrl) => Parent.ReadIndexPage(url, doc, out nextPageUrl);

            public override Artical ReadArticalExtrnal(ArticalOverview overview, string extrnalLink) => Parent.ReadArticalExtrnal(overview, extrnalLink);

            protected override void RestoreState(Dictionary<string, object> state) { }
        }

        private sealed class ExaminationChild : ChildWebsite
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
                            Name = "Students",
                            Link ="http://www.icai.org/new_category.html?c_id=410"
                        },
                        new Category()
                        {
                            Name = "Members",
                            Link ="http://www.icai.org/new_category.html?c_id=411"
                        }
                    };
                }
            }
            public ExaminationChild(WebsiteGroup parent) : base(parent, "Examination") { }
            public override Artical ReadArtical(ArticalOverview overview, HtmlDocument doc) => Parent.ReadArtical(overview, doc);

            public override ArticalOverview[] ReadCategory(Category category, HtmlDocument doc, out string nextPageUrl) => Parent.ReadIndexPage(category.Link, doc, out nextPageUrl);
            public override ArticalOverview[] ReadIndexPage(string url, HtmlDocument doc, out string nextPageUrl) => Parent.ReadIndexPage(url, doc, out nextPageUrl);
            public override Artical ReadArticalExtrnal(ArticalOverview overview, string extrnalLink) => Parent.ReadArticalExtrnal(overview, extrnalLink);
            protected override void RestoreState(Dictionary<string, object> state) { }
        }
    }
}