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
            requestPacket.OfflineModuleResponse = responseHandler;
            pendingRequest.Enqueue(requestPacket);
        }

        public void RequestProcessedCallback(RequestPacket requestPacket)
        {
            pendingResponse.Enqueue(requestPacket);
        }

        private void processData()
        {
            while (IsRunning)
            {
                if (cancleRequest.Count > 0)
                {
                    onlineModule.CancleRequest(cancleRequest.Dequeue());
                }

                if (pendingRequest.Count > 0)
                {
                    var reqPacket = pendingRequest.Dequeue();
                    var reqURL = reqPacket.Url;

                    var resultStr = diskCache.GetString(reqURL);
                    if (resultStr == string.Empty || reqPacket.OnlyOnline)
                    {
                        //no data in disk download now
                        onlineModule.RequestData(reqPacket, this);
                    }
                    else
                    {
                        //TODO: Create a delayed queue for handling refreshing existing data
                        onlineModule.RequestData(reqPacket.Clone() as RequestPacket, this); //refresh data

                        //data exist in cache
                        reqPacket.DataInString = resultStr;   //add result
                        pendingResponse.Enqueue(reqPacket); //add to response queue                                    
                    }                    
                }

                if (pendingResponse.Count > 0)
                {
                    var responsePacket = pendingResponse.Dequeue();

                    var responseHandler = responsePacket.OfflineModuleResponse;

                    if (responsePacket.requestObjs.ContainsKey(RequestPacket.RequestPacketOnlineModuleResponse))  
                    {
                        //the response package is coming from IOnlineModule

                        var packUrl = responsePacket.Url;

                        if (diskCache.IsKeyExist(packUrl) && !responsePacket.OnlyOnline)
                        {
                            diskCache.Put(packUrl, responsePacket.DataInString, true);
                            responsePacket.Dispose();
                        }
                        else
                        {
                            diskCache.Put(packUrl, responsePacket.DataInString, true);
                            responseHandler.RequestProcessedCallback(responsePacket); //make callback
                        }
                    }
                    else
                    {
                        //data has been retrieved from cache
                        responseHandler.RequestProcessedCallback(responsePacket); //make callback
                    }
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