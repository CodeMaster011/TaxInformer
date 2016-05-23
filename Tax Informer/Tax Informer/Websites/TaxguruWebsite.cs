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
    [Website("7238BEE9-E236-4340-A2BA-D59B95956F06")]
    internal class TaxguruWebsite : Website
    {
        public override string ComicText { get; } = "Tg";

        public override string IndexPageLink { get; } = "http://taxguru.in/category/chartered-accountant/";

        public override string Name { get; } = "Taxguru";

        public override string Color { get; } = "#EC407A";  //400

        public override Category[] Categories
        {
            get
            {
                return new Category[]
                {
                    new Category()
                    {
                        Name ="CA CS ICWA", Link = "http://taxguru.in/category/chartered-accountant/",
                    },
                    new Category()
                    {
                        Name ="Income Tax", Link = "http://taxguru.in/category/income-tax/",
                    },
                    new Category()
                    {
                        Name ="Service Tax", Link = "http://taxguru.in/category/service-tax/",
                    },
                    new Category()
                    {
                        Name ="Company Law", Link = "http://taxguru.in/category/company-law/",
                    },
                    new Category()
                    {
                        Name ="Excise Duty", Link = "http://taxguru.in/category/excise-duty/",
                    },
                    new Category()
                    {
                        Name ="Custom Duty", Link = "http://taxguru.in/category/custom-duty/",
                    },
                    new Category()
                    {
                        Name ="GST", Link = "http://taxguru.in/category/goods-and-service-tax/",
                    },
                    new Category()
                    {
                        Name ="RBI\\FEMA", Link = "http://taxguru.in/category/rbi/",
                    },
                    new Category()
                    {
                        Name ="SEBI", Link = "http://taxguru.in/category/sebi/",
                    },
                    new Category()
                    {
                        Name ="Finance", Link = "http://taxguru.in/category/finance/",
                    },
                    new Category()
                    {
                        Name ="Corporate Law", Link = "http://taxguru.in/category/corporate-law/",
                    }
                };
            }
        }

        public override Artical ReadArtical(ArticalOverview overview, HtmlDocument doc)
        {
            //TODO: Fix the single cote (') bug in http://taxguru.in/chartered-accountant/hey-examinee-thy-duty-result-thy-concern.html
            //TODO: Fix bug http://taxguru.in/company-law/download-annual-fillingxbrleforms.html

            var container = Helper.AnyChild(doc.DocumentNode, "div", "latestPosts main-box latestPostsBg");
            if (container == null) return null;

            var artical = new Artical() { MyLink = overview.LinkOfActualArtical };

            var aLinkTitle = Helper.AnyChild(Helper.AnyChild(container, "div", "homeTitle margint-10"),"a");
            artical.Title = HtmlEntity.DeEntitize(aLinkTitle.GetAttributeValue("title", ""));

            var aLinkAuthor = Helper.AnyChild(Helper.AnyChild(container, "li", "postAuthor"), "a");
            artical.Authors = new Author[] 
            {
                new Author()
                {
                    Name = aLinkAuthor.InnerText,
                    Link = aLinkAuthor.GetAttributeValue("href","")
                }
            };

            var aLinkDate = Helper.AnyChild(Helper.AnyChild(container, "li", "MetapostDate"), "a");
            artical.Date = getFormatedDate(aLinkDate.InnerText);

            var relatedPostContainer = Helper.AnyChild(container, "div", "rp4wp-related-posts rp4wp-related-post");
            var aLinkRelatedPosts = Helper.AllChild(relatedPostContainer, "a");
            if (aLinkRelatedPosts != null)
            {
                var reletedPost = new List<ArticalOverview>();
                foreach (var aNode in aLinkRelatedPosts)
                {
                    reletedPost.Add(
                        new ArticalOverview()
                        {
                            LinkOfActualArtical = aNode.GetAttributeValue("href", ""),
                            Title = aNode.InnerText
                        });
                }
                artical.RelatedPosts = reletedPost.ToArray();
            }
            
            var articalContainer = Helper.AnyChild(container, "div", "fsize16");
            if(relatedPostContainer!=null) articalContainer.RemoveChild(relatedPostContainer);
            //HtmlNode node = HtmlNode.CreateNode("<div></div>");
            //var pNodes = Helper.AllChild(articalContainer, "p");
            //foreach (var pNode in pNodes)
            //    node.AppendChild(pNode);
            artical.HtmlText = $"<html><head><link rel='stylesheet' id='custom-css' href='http://taxguru.in/wp-content/themes/tg4/style.css?ver=4.5.1' type='text/css' media='all' /></head><body>{ articalContainer.InnerHtml}</body></html>";

            return artical;
        }

        public override ArticalOverview[] ReadAuthor(Author author, HtmlDocument doc, out string nextPageUrl)
        {
            return ReadIndexPage(author.Link, doc, out nextPageUrl);
        }

        public override ArticalOverview[] ReadCategory(Category category, HtmlDocument doc, out string nextPageUrl)
        {
            return ReadIndexPage(category.Link, doc, out nextPageUrl);
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
                o.Date = getFormatedDate(aLinkDate.InnerText);

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

        private string getFormatedDate(string rawDate)
        {
            var ss = rawDate.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var month = Helper.GetMonthIndex(ss[1]).ToString("00");
            return ss[2] + month + ss[0];
        }
    }
}