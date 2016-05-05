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
        void ReadIndexPage(string uid, string indexPageLink, IUiArticalOverviewResponseHandler responseHandler, bool isForced = false);
        void ReadArtical(string uid, ArticalOverview overview, IUiArticalResponseHandler responseHandler, bool isForced = false);
        void ReadAuthor(string uid, Author author, IUiArticalOverviewResponseHandler responseHandler, bool isForced = false);
        void ReadCategory(string uid, Category category, IUiArticalOverviewResponseHandler responseHandler, bool isForced = false);
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
            var data = packet.DataInString;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(data);

            var overviewType = packet.OverviewType;
            if(overviewType == OverviewType.Null)
            {
                //Artical
                packet.AnalisisModuleResponseUiArtical
                    .ArticalProcessedCallback
                        (packet.Uid, packet.Url, currentWebsite.ReadArtical(packet.Tag as ArticalOverview, doc));
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
                        packet.AnalisisModuleResponseUiArticalOverview
                            .ArticalOverviewProcessedCallback
                                (packet.Uid, packet.Url, currentWebsite.ReadIndexPage(packet.Url, doc, out nextPage), overviewType, nextPage);
                        break;
                    case OverviewType.Author:
                        packet.AnalisisModuleResponseUiArticalOverview
                            .ArticalOverviewProcessedCallback
                                (packet.Uid, packet.Url, currentWebsite.ReadAuthor(packet.Tag as Author, doc, out nextPage), overviewType, nextPage);
                        break;
                    case OverviewType.Category:
                        packet.AnalisisModuleResponseUiArticalOverview
                            .ArticalOverviewProcessedCallback
                                (packet.Uid, packet.Url, currentWebsite.ReadCategory(packet.Tag as Category, doc, out nextPage), overviewType, nextPage);
                        break;
                    default:
                        break;
                }
                if (nextPage != null && nextPage != string.Empty)
                {
                    //TODO: Think how to hold the next page info
                }
            }
            packet.Dispose();
        }

        private void processData()
        {
            while (IsRunning)
            {
                if (cancleRequest.Count > 0)
                {
                    offlineModule.CancleRequest(cancleRequest.Dequeue());
                }

                if (pendingRequest.Count > 0)
                {
                    var reqObj = pendingRequest.Dequeue();
                    offlineModule.RequestData(reqObj, this);
                }
                if (pendingResponse.Count > 0)
                {
                    var responsePacket = pendingResponse.Dequeue();
                    originalResponse(responsePacket);
                    responsePacket.Dispose();
                }
                Thread.Sleep(1);
            }
        }

        public void ReadIndexPage(string uid, string indexPageLink, IUiArticalOverviewResponseHandler responseHandler, bool isForced = false)
        {
            pendingRequest.Enqueue(
                RequestPacket.CreatePacket(uid, indexPageLink, isForced, RequestPacketOwners.AnalysisModule, OverviewType.IndexPage, responseHandler));
        }

        public void ReadArtical(string uid, ArticalOverview overview, IUiArticalResponseHandler responseHandler, bool isForced = false)
        {
            pendingRequest.Enqueue(
                RequestPacket.CreatePacket(uid, overview.LinkOfActualArtical, isForced, RequestPacketOwners.AnalysisModule, responseHandler).AddTag(overview));
        }

        public void ReadAuthor(string uid, Author author, IUiArticalOverviewResponseHandler responseHandler, bool isForced = false)
        {
            pendingRequest.Enqueue(
                RequestPacket.CreatePacket(uid, author.Link, isForced, RequestPacketOwners.AnalysisModule, OverviewType.Author, responseHandler).AddTag(author));
        }

        public void ReadCategory(string uid, Category category, IUiArticalOverviewResponseHandler responseHandler, bool isForced = false)
        {
            pendingRequest.Enqueue(
                RequestPacket.CreatePacket(uid, category.Link, isForced, RequestPacketOwners.AnalysisModule, OverviewType.Category, responseHandler).AddTag(category));
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