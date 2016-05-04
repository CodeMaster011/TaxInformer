using System;
using System.Collections.Generic;

namespace Tax_Informer.Core
{    
    internal class RequestPacket : IDisposable, ICloneable
    {
        public const string RequestPacketUid = "uid";
        public const string RequestPacketUrl = "url";
        public const string RequestPacketAnalisisModuleResponse = "aResponse";
        public const string RequestPacketOfflineModuleResponse = "offResponse";
        public const string RequestPacketOnlineModuleResponse = "oResponse";
        public const string RequestPacketData = "data";
        public const string RequestPacketError = "error";
        public const string RequestPacketOwner = "owner";
        public const string RequestPacketTag = "tag";
        public const string RequestPacketOverviewType = "overviewType";

        public Dictionary<string, object> requestObjs = null;

        public RequestPacket()
        {
            requestObjs = new Dictionary<string, object>();
        }

        public RequestPacket Add(string key, object value)
        {
            requestObjs.Add(key, value);
            return this;
        }

        public string Uid
        {
            get
            {
                return Get<string>(RequestPacketUid);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketUid))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketUid, value);
            }
        }

        public string Url
        {
            get
            {
                return Get<string>(RequestPacketUrl);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketUrl))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketUrl, value);
            }
        }

        public OverviewType OverviewType
        {
            get
            {
                return Get<OverviewType>(RequestPacketOverviewType);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketOverviewType))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketOverviewType, value);
            }
        }
        
        public IUiArticalOverviewResponseHandler AnalisisModuleResponseUiArticalOverview
        {
            get
            {
                return Get<IUiArticalOverviewResponseHandler>(RequestPacketAnalisisModuleResponse);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketAnalisisModuleResponse))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketAnalisisModuleResponse, value);
            }
        }

        public IUiArticalResponseHandler AnalisisModuleResponseUiArtical
        {
            get
            {
                return Get<IUiArticalResponseHandler>(RequestPacketAnalisisModuleResponse);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketAnalisisModuleResponse))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketAnalisisModuleResponse, value);
            }
        }

        public IResponseHandler OfflineModuleResponse
        {
            get
            {
                return Get<IResponseHandler>(RequestPacketOfflineModuleResponse);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketOfflineModuleResponse))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketOfflineModuleResponse, value);
            }
        }

        public IResponseHandler OnlineModuleResponse
        {
            get
            {
                return Get<IResponseHandler>(RequestPacketOnlineModuleResponse);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketOnlineModuleResponse))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketOnlineModuleResponse, value);
            }
        }

        public string DataInString
        {
            get
            {
                return Get<string>(RequestPacketData);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketData))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketData, value);
            }
        }

        public List<string> DataInStringList
        {
            get
            {
                return Get<List<string>>(RequestPacketData);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketData))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketData, value);
            }
        }

        public string Error
        {
            get
            {
                return Get<string>(RequestPacketError);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketError))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketError, value);
            }
        }

        public object Tag
        {
            get
            {
                return Get<object>(RequestPacketTag);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketTag))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketTag, value);
            }
        }
        public RequestPacketOwners Owner
        {
            get
            {
                return Get<RequestPacketOwners>(RequestPacketOwner);
            }
            set
            {
                if (requestObjs.ContainsKey(RequestPacketOwner))
                    throw new InvalidOperationException("The data already exist.");
                else
                    requestObjs.Add(RequestPacketOwner, value);
            }
        }

        public T Get<T>(string key) => Get<T>(key, false);

        private T Get<T>(string key, bool isConfirm)
        {
            if (!isConfirm)
                if (!requestObjs.ContainsKey(key))
                    return default(T);
            return (T)requestObjs[key];
        }

        public void Dispose()
        { 

            if (requestObjs != null)
            {
                requestObjs.Clear();
            }
            requestObjs = null;
        }

        public object Clone()
        {
            RequestPacket n = null;
            lock (requestObjs)
            {
                n = new RequestPacket();
                foreach (var item in requestObjs)
                {
                    n.Add(item.Key, item.Value);
                }
                n.requestObjs.Remove(RequestPacketUid);
                n.Uid = MyGlobal.UidGenerator();
            }            
            return n;
        }

        ~RequestPacket()
        {
            Dispose();
            
            //Android.Util.Log.Debug("RequestPacket", $"GC Collected {--MyGlobal.requestPacketCount}");
        }

        public static RequestPacket CreatePacket(string uid, string url, RequestPacketOwners owner, 
            IUiArticalResponseHandler analisisModuleResponseUiArtical = null, IResponseHandler offlineModuleResponse = null, IResponseHandler onlineModuleResponse = null)
        {
            var r = new RequestPacket() { Uid = uid, Url = url, Owner = owner, OverviewType = OverviewType.Null };
            if (analisisModuleResponseUiArtical != null) r.AnalisisModuleResponseUiArtical = analisisModuleResponseUiArtical;
            if (offlineModuleResponse != null) r.OfflineModuleResponse = offlineModuleResponse;
            if (onlineModuleResponse != null) r.OnlineModuleResponse = onlineModuleResponse;

            //Android.Util.Log.Debug("RequestPacket", $"Created {++MyGlobal.requestPacketCount}");

            return r;
        }

        public static RequestPacket CreatePacket(string uid, string url, RequestPacketOwners owner, OverviewType overviewType,
            IUiArticalOverviewResponseHandler analisisModuleResponseUiArticalOverview = null, IResponseHandler offlineModuleResponse = null, IResponseHandler onlineModuleResponse = null)
        {
            var r = new RequestPacket() { Uid = uid, Url = url, Owner = owner, OverviewType = overviewType };
            if (analisisModuleResponseUiArticalOverview != null) r.AnalisisModuleResponseUiArticalOverview = analisisModuleResponseUiArticalOverview;
            if (offlineModuleResponse != null) r.OfflineModuleResponse = offlineModuleResponse;
            if (onlineModuleResponse != null) r.OnlineModuleResponse = onlineModuleResponse;

            //Android.Util.Log.Debug("RequestPacket", $"Created {++MyGlobal.requestPacketCount}");

            return r;
        }

        public static RequestPacket CreatePacket(RequestPacketOwners owner)
        {
            return new RequestPacket() { Owner = owner};
        }
        public RequestPacket AddUid(string uid)
        {
            this.Uid = uid;
            return this;
        }
        public RequestPacket AddUrl(string url)
        {
            this.Url = url;
            return this;
        }
        public RequestPacket AddTag(object tag)
        {
            this.Tag = tag;
            return this;
        }
        public RequestPacket AddAnalisisModuleResponseUiArticalOverview(IUiArticalOverviewResponseHandler responseHandler, OverviewType overviewType)
        {
            this.OverviewType = overviewType;
            this.AnalisisModuleResponseUiArticalOverview = responseHandler;
            return this;
        }
        public RequestPacket AddAnalisisModuleResponseUiArtical(IUiArticalResponseHandler responseHandler, OverviewType overviewType)
        {
            this.OverviewType = overviewType;
            this.AnalisisModuleResponseUiArtical = responseHandler;
            return this;
        }
        public RequestPacket AddOfflineModuleResponse(IResponseHandler responseHandler)
        {
            this.OfflineModuleResponse = responseHandler;
            return this;
        }
        public RequestPacket AddOnlineModuleResponse(IResponseHandler responseHandler)
        {
            this.OnlineModuleResponse = responseHandler;
            return this;
        }
        public RequestPacket AddError(string error)
        {
            this.Error = error;
            return this;
        }
        public RequestPacket AddData(string data)
        {
            this.DataInString = data;
            return this;
        }
    }

    internal enum OverviewType
    {
        Null = -1,
        UNKNOWN = 0,
        IndexPage,
        Author,
        Category
    }
    internal enum RequestPacketOwners
    {
        UI,
        AnalysisModule,
        OfflineModule,
        OnlineModule
    }
}