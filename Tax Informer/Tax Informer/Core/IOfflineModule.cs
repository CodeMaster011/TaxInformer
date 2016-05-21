using System;
using System.Collections.Generic;
using System.Threading;
using static Tax_Informer.MyGlobal;

namespace Tax_Informer.Core
{
    interface IOfflineModule
    {
        void RequestData(RequestPacket requestPacket, IResponseHandler responseHandler);
        void CancleRequest(RequestPacket requestPacket);
    }

    class OfflineModule : IOfflineModule, IResponseHandler
    {
        private Queue<RequestPacket> pendingRequest = new Queue<RequestPacket>();
        private Queue<RequestPacket> pendingResponse = new Queue<RequestPacket>();
        private Queue<RequestPacket> cancleRequest = new Queue<RequestPacket>();

        public void RequestProcessingError(RequestPacket requestPacket)
        {
            throw new NotImplementedException();
        }

        public void CancleRequest(RequestPacket requestPacket)
        {
            cancleRequest.Enqueue(requestPacket);
        }

        public void RequestData(RequestPacket requestPacket, IResponseHandler responseHandler)
        {
            MyLog.Log(this, nameof(RequestData) + requestPacket.Url + "...");
            requestPacket.OfflineModuleResponse = responseHandler;
            pendingRequest.Enqueue(requestPacket); 
            MyLog.Log(this, nameof(RequestData) + requestPacket.Url + "...Done");
        }

        public void RequestProcessedCallback(RequestPacket requestPacket)
        {
            pendingResponse.Enqueue(requestPacket);
        }

        private void processData()
        {
            while (IsRunning)
            {
                try
                {
                    if (cancleRequest.Count > 0)
                    {
                        onlineModule.CancleRequest(cancleRequest.Dequeue());
                    }

                    if (pendingRequest.Count > 0)
                    {
                        var reqPacket = pendingRequest.Dequeue();
                        var reqURL = reqPacket.Url;

                        MyLog.Log(this, $"Request processing {reqPacket.Url}" + "...");
                        var resultStr = diskCache.GetString(reqURL);
                        if (resultStr == string.Empty || reqPacket.OnlyOnline)
                        {
                            //no data in disk download now
                            MyLog.Log(this, "no data on disk requesting to download" + "...");
                            onlineModule.RequestData(reqPacket, this); 
                            MyLog.Log(this, "no data on disk requesting to download" + "...Done");
                        }
                        else
                        {
                            //TODO: Create a delayed queue for handling refreshing existing data
                            MyLog.Log(this, "data is on the disk. A refreshing request is sent." + "...");
                            onlineModule.RequestData(reqPacket.Clone() as RequestPacket, this); //refresh data  
                            MyLog.Log(this, "data is on the disk. A refreshing request is sent." + "...Done");

                            //data exist in cache
                            reqPacket.DataInString = resultStr;   //add result
                            pendingResponse.Enqueue(reqPacket); //add to response queue            
                        } 
                        MyLog.Log(this, $"Request processing {reqPacket.Url}" + "...Done");
                    }

                    if (pendingResponse.Count > 0)
                    {
                        var responsePacket = pendingResponse.Dequeue();

                        MyLog.Log(this, $"Response processing url {responsePacket.Url}" + "...");
                        var responseHandler = responsePacket.OfflineModuleResponse;

                        if (responsePacket.requestObjs.ContainsKey(RequestPacket.RequestPacketOnlineModuleResponse))
                        {
                            //the response package is coming from IOnlineModule

                            var packUrl = responsePacket.Url;

                            MyLog.Log(this, "response is coming from online" + "...");
                            
                            if (!string.IsNullOrEmpty(responsePacket.DataInString) && diskCache.IsKeyExist(packUrl) && !responsePacket.OnlyOnline)
                            {
                                MyLog.Log(this, "Data was exist. Refreshing disk data" + "...");
                                diskCache.Put(packUrl, responsePacket.DataInString, true);
                                responsePacket.Dispose(); 
                                MyLog.Log(this, "Data was exist. Refreshing disk data" + "...Done");
                            }
                            else if (string.IsNullOrEmpty(responsePacket.DataInString))
                            {
                                MyLog.Log(this, "Response for non-string data" + "...");
                                responseHandler.RequestProcessedCallback(responsePacket); //make callback  
                                MyLog.Log(this, "Response for non-string data" + "...Done");
                            }
                            else
                            {
                                MyLog.Log(this, "No Data on disk. Dumping data to disk and callback is made" + "...");
                                diskCache.Put(packUrl, responsePacket.DataInString, true);
                                responseHandler.RequestProcessedCallback(responsePacket); //make callback  
                                MyLog.Log(this, "No Data on disk. Dumping data to disk and callback is made" + "...Done");
                            } 
                            MyLog.Log(this, "response is coming from online" + "...Done");
                        }
                        else
                        {
                            //data has been retrieved from cache
                            responseHandler.RequestProcessedCallback(responsePacket); //make callback
                        } 
                        MyLog.Log(this, $"Response processing url {responsePacket.Url}" + "...Done");
                    }
                }
                catch (Exception ex)
                {
                    MyLog.Log(this, "--Error " + ex.Message);
                }
                
                Thread.Sleep(1);
            }
        }
        public OfflineModule()
        {
            Thread th = new Thread(processData);
            th.Name = "Offline Processing Thread";
            th.Priority = System.Threading.ThreadPriority.Normal;
            th.Start();
        }
    }
}