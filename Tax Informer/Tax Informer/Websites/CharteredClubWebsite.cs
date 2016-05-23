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
    [Website("E0479F9D-6C52-42F3-83FB-B08614452DE4")]
    internal class CharteredClubWebsite : Website
    {
        public override string Color { get; } = "#AB47BC";//400

        public override string ComicText { get; } = "CC";

        public override string IndexPageLink { get; } = "http://www.charteredclub.com/category/tax/";

        public override string Name { get; } = "Chartered Club";

        public override Category[] Categories
        {
            get
            {
                return new Category[] 
                {
                    new Category()
                    {
                        Name = "Income Tax",
                        Link = "http://www.charteredclub.com/category/tax/"
                    },
                    new Category()
                    {
                        Name = "Personal Finance",
                        Link = "http://www.charteredclub.com/category/money/"
                    }
                };
            }
        }

        public override Artical ReadArtical(ArticalOverview overview, HtmlDocument doc)
        {
            var container = Helper.AnyChild(doc.DocumentNode, "div", "content");
            if (container == null) return null;

            var artical = new Artical() { MyLink = overview.LinkOfActualArtical };

            var titleNode = Helper.AnyChild(Helper.AnyChild(container, "div", "posthead"), "h1", "headline");
            artical.Title = HtmlEntity.DeEntitize(titleNode.InnerText);

            var conNode = Helper.AnyChild(container, "div", "post_content");
            artical.HtmlText = $"<html><head></head><body>{conNode.InnerHtml}</body></html>";

            var authorNode = Helper.AnyChild(Helper.AnyChild(container, "div", "author-bio"), "a");
            artical.Authors = new Author[]
            {
                new Author()
                {
                    Name = HtmlEntity.DeEntitize(authorNode.InnerText),
                    Link = authorNode.GetAttributeValue("href","")
                }
            };

            var relatedPostNode = Helper.AnyChild(container, "div", "yarpp-related");
            if (relatedPostNode != null)
            {
                var re = new List<ArticalOverview>();
                var aLinks = Helper.AllChild(relatedPostNode, "a", new SearchCritriaBuilder().AddNotHasChild(new ChildNode() { Name = "img" }).Build());
                foreach (var aL in aLinks)
                {
                    re.Add(new ArticalOverview()
                    {
                        LinkOfActualArtical = aL.GetAttributeValue("href",""),
                        Title = HtmlEntity.DeEntitize(aL.InnerText)
                    });
                }
                artical.RelatedPosts = re.ToArray();
            }
            return artical;
        }

        public override ArticalOverview[] ReadAuthor(Author author, HtmlDocument doc, out string nextPageUrl)
        {
            throw new NotImplementedException();
        }

        public override ArticalOverview[] ReadCategory(Category category, HtmlDocument doc, out string nextPageUrl)
            => ReadIndexPage(category.Link, doc, out nextPageUrl);

        public override ArticalOverview[] ReadIndexPage(string url, HtmlDocument doc, out string nextPageUrl)
        {
            nextPageUrl = null;

            var allPosts = Helper.AllChild(doc.DocumentNode, "div", "postcontentbox");
            if (allPosts == null) return null;

            List<ArticalOverview> overview = new List<ArticalOverview>();
            foreach (var postNode in allPosts)
            {
                var o = new ArticalOverview();
                var titleNode = Helper.AnyChild(Helper.AnyChild(postNode, "h2", "headline"), "a");
                o.LinkOfActualArtical = titleNode.GetAttributeValue("href", "");
                o.Title = HtmlEntity.DeEntitize(titleNode.InnerText);

                o.SummaryText = HtmlEntity.DeEntitize(Helper.AnyChild(postNode, "div", new Dictionary<string, string>()
                {
                    ["itemprop"] = "description"
                }).InnerText);

                overview.Add(o);
            }

            var divPage = Helper.AnyChild(doc.DocumentNode, "div", "wp-pagenavi");
            var currentPageIndex = int.Parse(Helper.AnyChild(divPage, "span", "current").InnerText);
            var aPageLinks = Helper.AllChild(divPage, "a", "page larger");
            foreach (var aPage in aPageLinks)
            {
                if (aPage.InnerText == (currentPageIndex + 1).ToString())
                {
                    nextPageUrl = aPage.GetAttributeValue("href", "");
                    break;
                }
            }
            return overview.Count == 0 ? null : overview.ToArray();
        }

        protected override void RestoreState(Dictionary<string, object> state)
        {
            throw new NotImplementedException();
        }
    }
}