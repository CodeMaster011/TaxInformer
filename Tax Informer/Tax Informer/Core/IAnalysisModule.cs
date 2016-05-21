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
using System.Threading;
using static Tax_Informer.MyGlobal;

namespace Tax_Informer.Core
{
    //TODO: Find a way to localize the currentWebsite into IAnalysisModule
    interface IAnalysisModule
    {
        void ReadIndexPage(string uid, string websiteKey, string indexPageLink, IUiArticalOverviewResponseHandler responseHandler, bool isForced = false);
        void ReadArtical(string uid, string websiteKey, ArticalOverview overview, IUiArticalResponseHandler responseHandler, bool isForced = false);
        void ReadAuthor(string uid, string websiteKey, Author author, IUiArticalOverviewResponseHandler responseHandler, bool isForced = false);
        void ReadCategory(string uid, string websiteKey, Category category, IUiArticalOverviewResponseHandler responseHandler, bool isForced = false);
        void CancleRequest(List<string> UId);
    }
    class AnalysisModule : IAnalysisModule, IResponseHandler
    {
        private Queue<RequestPacket> pendingRequest = new Queue<RequestPacket>();
        private Queue<RequestPacket> pendingResponse = new Queue<RequestPacket>();
        private Queue<RequestPacket> cancleRequest = new Queue<RequestPacket>();

        public void RequestProcessingError(RequestPacket requestPacket)
        {
            throw new NotImplementedException();
        }
        
        public void CancleRequest(List<string> UId)
        {
            RequestPacket packet = new RequestPacket();
            packet.DataInStringList = UId;

            cancleRequest.Enqueue(packet);
        }
        

        public void RequestProcessedCallback(RequestPacket requestPacket)
        {
            pendingResponse.Enqueue(requestPacket);
        }
        
        private void originalResponse(RequestPacket packet)
        {
            MyLog.Log(this, nameof(originalResponse) + packet.Url + "...");
            var data = packet.DataInString;
            HtmlAgilityPack.HtmlDocument doc = null;

            if (!string.IsNullOrEmpty(data))
            {
                MyLog.Log(this, "Loading HTML data" + "...");
                doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(data); 
                MyLog.Log(this, "Loading HTML data" + "...Done");
            }

            var currentWebsite = Config.GetWebsite(packet.WebsiteKey);

            var overviewType = packet.OverviewType;
            if (overviewType == OverviewType.Null)
            {
                //Artical
                Artical artical = null;
                MyLog.Log(this, "Reading artical" + "...");
                if (doc != null)
                    artical = currentWebsite.ReadArtical(packet.Tag as ArticalOverview, doc);
                else
                    artical = currentWebsite.ReadArticalExtrnal(packet.Tag as ArticalOverview, packet.ExtrnalLink); 
                MyLog.Log(this, "Reading artical" + "...Done");

                MyLog.Log(this, "Making callback on artical" + "...");

                packet.AnalisisModuleResponseUiArtical
                    .ArticalProcessedCallback
                        (packet.Uid, packet.Url, artical); 
                MyLog.Log(this, "Making callback on artical" + "...Done");
            }
            else
            {
                string nextPage = string.Empty;

                switch (overviewType)
                {
                    case OverviewType.Null:
                        break;
                    case OverviewType.UNKNOWN:
                        break;
                    case OverviewType.IndexPage:
                        MyLog.Log(this, "Reading index page data" + "...");
                        var iData = currentWebsite.ReadIndexPage(packet.Url, doc, out nextPage); 
                        MyLog.Log(this, "Reading index page data" + "...Done");

                        MyLog.Log(this, "Making index page callback" + "...");
                        packet.AnalisisModuleResponseUiArticalOverview
                                            .ArticalOverviewProcessedCallback
                                                (packet.Uid, packet.Url, iData, overviewType, nextPage); 
                        MyLog.Log(this, "Making index page callback" + "...Done");
                        break;
                    case OverviewType.Author:
                        MyLog.Log(this, "Reading author page data" + "...");
                        var aData = currentWebsite.ReadAuthor(packet.Tag as Author, doc, out nextPage); 
                        MyLog.Log(this, "Reading author page data" + "...Done");
                        MyLog.Log(this, "Making author page callback" + "...");
                        packet.AnalisisModuleResponseUiArticalOverview
                                            .ArticalOverviewProcessedCallback
                                                (packet.Uid, packet.Url, aData, overviewType, nextPage); 
                        MyLog.Log(this, "Making author page callback" + "...Done");
                        break;
                    case OverviewType.Category:
                        MyLog.Log(this, "Reading category page data" + "...");
                        var cData = currentWebsite.ReadCategory(packet.Tag as Category, doc, out nextPage); 
                        MyLog.Log(this, "Reading category page data" + "...Done");
                        MyLog.Log(this, "Making category page callback" + "...");
                        packet.AnalisisModuleResponseUiArticalOverview
                                            .ArticalOverviewProcessedCallback
                                                (packet.Uid, packet.Url, cData, overviewType, nextPage); 
                        MyLog.Log(this, "Making category page callback" + "...Done");
                        break;
                    default:
                        break;
                }
                if (nextPage != null && nextPage != string.Empty)
                {
                    //TODO: Think how to hold the next page info
                }
            }
            MyLog.Log(this, nameof(originalResponse) + packet.Url + "...Done");
        }

        private void processData()
        {
            while (IsRunning)
            {
                try
                {

                    if (cancleRequest.Count > 0)
                    {
                        offlineModule.CancleRequest(cancleRequest.Dequeue());
                    }

                    if (pendingRequest.Count > 0)
                    {
                        var reqObj = pendingRequest.Dequeue();
                        MyLog.Log(this, $"Passing request packet {reqObj.Url}" + "...");
                        offlineModule.RequestData(reqObj, this); 
                        MyLog.Log(this, $"Passing request packet {reqObj.Url}" + "...Done");
                    }
                    if (pendingResponse.Count > 0)
                    {
                        var responsePacket = pendingResponse.Dequeue();
                        MyLog.Log(this, $"Response received on packet {responsePacket.Url}" + "...");
                        originalResponse(responsePacket); 
                        MyLog.Log(this, $"Response received on packet {responsePacket.Url}" + "...Done");
                        responsePacket.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    MyLog.Log(this, "--Error " + ex.Message);
                }
                
                Thread.Sleep(1);
            }
        }

        public void ReadIndexPage(string uid, string websiteKey, string indexPageLink, IUiArticalOverviewResponseHandler responseHandler, bool isForced = false)
        {
            MyLog.Log(this, nameof(ReadIndexPage) + "...");
            pendingRequest.Enqueue(
                    RequestPacket.CreatePacket(uid, websiteKey, indexPageLink, isForced, RequestPacketOwners.AnalysisModule, OverviewType.IndexPage, responseHandler)); 
            MyLog.Log(this, nameof(ReadIndexPage) + "...Done");
        }

        public void ReadArtical(string uid, string websiteKey, ArticalOverview overview, IUiArticalResponseHandler responseHandler, bool isForced = false)
        {
            MyLog.Log(this, nameof(ReadArtical) + "...");
            pendingRequest.Enqueue(
                    RequestPacket.CreatePacket(uid, websiteKey, overview.LinkOfActualArtical, isForced, RequestPacketOwners.AnalysisModule, responseHandler).AddTag(overview)); 
            MyLog.Log(this, nameof(ReadArtical) + "...Done");
        }

        public void ReadAuthor(string uid, string websiteKey, Author author, IUiArticalOverviewResponseHandler responseHandler, bool isForced = false)
        {
            MyLog.Log(this, nameof(ReadAuthor) + "...");
            pendingRequest.Enqueue(
                    RequestPacket.CreatePacket(uid, websiteKey, author.Link, isForced, RequestPacketOwners.AnalysisModule, OverviewType.Author, responseHandler).AddTag(author)); 
            MyLog.Log(this, nameof(ReadAuthor) + "...Done");
        }

        public void ReadCategory(string uid, string websiteKey, Category category, IUiArticalOverviewResponseHandler responseHandler, bool isForced = false)
        {
            MyLog.Log(this, nameof(ReadCategory) + "...");
            pendingRequest.Enqueue(
                    RequestPacket.CreatePacket(uid, websiteKey, category.Link, isForced, RequestPacketOwners.AnalysisModule, OverviewType.Category, responseHandler).AddTag(category)); 
            MyLog.Log(this, nameof(ReadCategory) + "...Done");
        }

        public AnalysisModule()
        {
            Thread th = new Thread(processData);
            th.Name = "Analysis Processing Thread";
            th.Priority = System.Threading.ThreadPriority.Highest;
            th.Start();
        }
    }
}