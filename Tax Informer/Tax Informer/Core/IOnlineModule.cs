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
using Android.Util;

namespace Tax_Informer.Core
{
    interface IOnlineModule
    {
        void RequestData(RequestPacket requestPacket, IResponseHandler responseHandler);
        void CancleRequest(RequestPacket requestPacket);
    }
    class OnlineModule : IOnlineModule
    {
        private Queue<RequestPacket> pendingRequest = new Queue<RequestPacket>();
        private Queue<RequestPacket> cancleRequest = new Queue<RequestPacket>();

        public void CancleRequest(RequestPacket requestPacket)
        {
            cancleRequest.Enqueue(requestPacket);
        }

        public void RequestData(RequestPacket requestPacket, IResponseHandler responseHandler)
        {
            string uid = requestPacket.Uid;

            requestPacket.OnlineModuleResponse = responseHandler;

            pendingRequest.Enqueue(requestPacket);         
        }        

        private void processRequest()
        {
            while (IsRunning)
            {
                try
                {
                    //===================REQUEST CANCELING==================================
                    if (cancleRequest.Count > 0)
                    {
                        var cancleReqPacket = cancleRequest.Dequeue();

                        var cUids = cancleReqPacket.DataInStringList;
                        if (cUids != null)
                        {
                            Queue<RequestPacket> tempRequest = new Queue<RequestPacket>();
                            for (int i = 0; i < pendingRequest.Count; i++)
                            {
                                var tPacket = pendingRequest.Dequeue();
                                var tUid = tPacket.Uid;

                                if (cUids.Contains(tUid)) cUids.Remove(tUid);
                                else tempRequest.Enqueue(tPacket);
                            }
                            pendingRequest = tempRequest;
                        }
                    }
                    //===================REQUEST PROCESSING==================================
                    var packet = pendingRequest.Dequeue();
                    var responseHandler = packet.OnlineModuleResponse;
                    var requestedUrl = packet.Url;

                    try
                    {
                        if (isStringFileLink(requestedUrl))
                        {
                            Log.Debug("Online Module:", $"Downloading (string) url {requestedUrl}");

                            string result = Helper.DownloadFile(requestedUrl);
                            packet.DataInString = result;
                        }
                        else
                        {
                            //not a string file use external links to view the item
                            if (!System.IO.Directory.Exists(TempDirectory)) System.IO.Directory.CreateDirectory(TempDirectory);
                            string extrnalLink = null;
                            Helper.DownloadFile(requestedUrl, extrnalLink = $"{TempDirectory}/{Guid.NewGuid().ToString()}.pdf");

                            packet.DataInString = null;
                            packet.ExtrnalLink = extrnalLink;                            
                        }
                        Log.Debug("Online Module:", $"Making processed callback for url {requestedUrl}");
                        responseHandler.RequestProcessedCallback(packet);
                    }
                    catch (Exception ex)
                    {
                        packet.Error = ex.Message;
                        responseHandler.RequestProcessingError(packet);
                    }
                }
                catch (Exception) { }
                Thread.Sleep(1);
            }
        }
        private string[] stringFileExtentions = new string[] { ".html", ".php", ".asp", ".aspx" };
        private bool isStringFileLink(string url)
        {
            foreach (var s in stringFileExtentions)            
                if (url.Contains(s)) return true;
            if (url.EndsWith("/")) return true;    //Links like http://www.charteredclub.com/category/tax/
            return false;
        }
        public OnlineModule()
        {
            Thread th = new Thread(processRequest);
            th.Name = "Online Request Processing Thread";
            th.Priority = System.Threading.ThreadPriority.Highest;
            th.Start();
        }
        
    }
}