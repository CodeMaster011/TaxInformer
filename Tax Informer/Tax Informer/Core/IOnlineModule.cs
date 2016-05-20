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
                        MyLog.Log(this, "Cancel request" + "...");
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
                        MyLog.Log(this, "Cancel request" + "...Done");
                    }
                    //===================REQUEST PROCESSING==================================
                    if (pendingRequest.Count > 0)
                    {
                        var packet = pendingRequest.Dequeue();
                        var responseHandler = packet.OnlineModuleResponse;
                        var requestedUrl = packet.Url;

                        MyLog.Log(this, $"Processing request url {requestedUrl}" + "...");
                        try
                        {
                            if (isStringFileLink(requestedUrl))
                            {
                                Log.Debug("Online Module:", $"Downloading (string) url {requestedUrl}");

                                MyLog.Log(this, $"Downloading string file url {requestedUrl}" + "...");
                                string result = Helper.DownloadFile(requestedUrl);
                                packet.DataInString = result; 
                                MyLog.Log(this, $"Downloading string file url {requestedUrl}" + "...Done");
                            }
                            else
                            {
                                //not a string file use external links to view the item
                                MyLog.Log(this, $"Downloading non-string file url {requestedUrl}" + "...");
                                if (!System.IO.Directory.Exists(TempDirectory)) System.IO.Directory.CreateDirectory(TempDirectory);
                                string extrnalLink = ($"{TempDirectory}/{Guid.NewGuid().ToString()}.pdf");
                                MyLog.Log(this, $"Download file src=> {requestedUrl} \t des=>{extrnalLink}" + "...");

                                Helper.DownloadFile(requestedUrl, extrnalLink); 
                                MyLog.Log(this, $"Download file src=> {requestedUrl} \t des=>{extrnalLink}" + "...Done");

                                packet.DataInString = null;
                                packet.ExtrnalLink = extrnalLink; 
                                MyLog.Log(this, $"Downloading non-string file url {requestedUrl}" + "...Done");
                            }
                            Log.Debug("Online Module:", $"Making processed callback for url {requestedUrl}");
                            responseHandler.RequestProcessedCallback(packet);
                        }
                        catch (Exception ex)
                        { 
                            MyLog.Log(this, "--Error " + ex.Message);
                            packet.Error = ex.Message;
                            responseHandler.RequestProcessingError(packet);
                        } 
                        MyLog.Log(this, $"Processing request url {requestedUrl}" + "...Done");
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