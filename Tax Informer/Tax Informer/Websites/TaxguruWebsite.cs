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
    internal class TaxguruWebsite : Website
    {
        public override string ComicText { get; } = "Tg";

        public override string IndexPageLink { get; } = "http://taxguru.in/category/chartered-accountant/";

        public override string Name { get; } = "Taxguru";

        public override string Color { get; } = "#e43f4d";

        public override Artical ReadArtical(ArticalOverview overview, HtmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public override ArticalOverview[] ReadAuthor(Author author, HtmlDocument doc, out string nextPageUrl)
        {
            throw new NotImplementedException();
        }

        public override ArticalOverview[] ReadCategory(Category category, HtmlDocument doc, out string nextPageUrl)
        {
            throw new NotImplementedException();
        }

        public override ArticalOverview[] ReadIndexPage(string url, HtmlDocument doc, out string nextPageUrl)
        {
            nextPageUrl = string.Empty;
            HtmlNode contentBox = null;

            foreach (var item in Helper.AllChild(doc.DocumentNode, "div", "contentBox"))
            {
                if(Helper.AnyChild(item, "div", "seprate-col-post") != null)
                {
                    contentBox = item;
                    break;
                }
            }

            var oNodes = Helper.AllChild(contentBox, "div", "newsBoxPost margint-10");
            if (oNodes == null) return null;

            List<ArticalOverview> overview = new List<ArticalOverview>();
            foreach (var oNode in oNodes)
            {
                var o = new ArticalOverview();

                var aLinkTitleNode = Helper.AnyChild(oNode, "a");
                o.Title = HtmlEntity.DeEntitize(aLinkTitleNode.GetAttributeValue("title", ""));
                o.LinkOfActualArtical = aLinkTitleNode.GetAttributeValue("href", "");

                var aLinkAuthors = Helper.AnyChild(Helper.AnyChild(oNode, "li"), "a");
                Author author = new Author()
                {
                    Name = aLinkAuthors.InnerText,
                    Link = aLinkAuthors.GetAttributeValue("href", "")
                };
                o.Authors = new Author[] { author };

                var aLinkDate = Helper.AnyChild(Helper.AnyChild(oNode, "li", "MetapostDate"), "a", "linkblack");
                o.Date = aLinkDate.InnerText;

                var divSummaryNode = Helper.AnyChild(oNode, "div", "fsize16");
                o.SummaryText = divSummaryNode.InnerText;

                var allGroups = Helper.AllChild(oNode, "div", "margint10");
                var tagNodeContainer = allGroups[allGroups.Count - 1];
                var tagNodes = Helper.AllChild(tagNodeContainer, "a");
                var catList = new List<Category>();
                foreach (var tagNode in tagNodes)
                {
                    catList.Add(new Category() { Name = tagNode.InnerText, Link = tagNode.GetAttributeValue("href", "") });
                }
                o.Categorys = catList.ToArray();

                overview.Add(o);
            }

            var divPage = Helper.AnyChild(contentBox, "div", "wp-pagenavi");
            var currentPageIndex = int.Parse(Helper.AnyChild(divPage, "span", "current").InnerText);
            var aPageLinks = Helper.AllChild(divPage, "a");
            foreach (var aPage in aPageLinks)
            {
                if(aPage.InnerText == (currentPageIndex + 1).ToString())
                {
                    nextPageUrl = aPage.GetAttributeValue("href", "");
                    break;
                }
            }
            
            return overview.ToArray();
        }

        protected override void RestoreState(Dictionary<string, object> state)
        {
            throw new NotImplementedException();
        }
    }
}