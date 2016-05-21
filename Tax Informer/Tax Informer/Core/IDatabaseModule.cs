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
using SQLite;
using System.Threading;
using Lib;
using Android.Util;
namespace Tax_Informer.Core
{
    interface IDatabaseModule
    {
        void IsSeen(string transactionId, ArticalOverview articalOverview);
        void IsSeen(string transactionId, ArticalOverview[] articalOverview);
        void UpdateIsSeen(string transactionId, ArticalOverview articalOverview);
        void MakeOffline(string transactionId, string websiteKey, Artical artical, ArticalOverview articalOverview);
        void GetAllOfflineArticalList(string transactionId, IUiOfflineArticalOverviewResponseHandler responseHandler);
        void GetArtical(string transactionId, ArticalOverview articalOverview, IUiArticalResponseHandler responseHandler);
        void Close();
    }
    internal class DatabaseModule : IDatabaseModule
    {
        public string DatabaseFilePath { get; } = null;
        public Context Context { get; set; } = null;

        private SQLiteConnection db = null;
        private AutoResetEvent handler = new AutoResetEvent(false);
        private DictionaryStackedQueue<string, InstructionSet> pendingRequest = new DictionaryStackedQueue<string, InstructionSet>();
        private bool isDBclosed = false;

        public void IsSeen(string transactionId, ArticalOverview articalOverview) => IsSeen(transactionId, new ArticalOverview[] { articalOverview });
        public void IsSeen(string transactionId, ArticalOverview[] articalOverview)
        {
            pendingRequest.Push(transactionId, InstructionSet.IsSeenCheck(articalOverview));
            handler.Set();
        }
        public void UpdateIsSeen(string transactionId, ArticalOverview articalOverview)
        {
            pendingRequest.Push(transactionId, InstructionSet.UpdateIsSeen(articalOverview));
            handler.Set();
        }
        public void MakeOffline(string transactionId, string websiteKey, Artical artical, ArticalOverview articalOverview)
        {
            pendingRequest.Push(transactionId, InstructionSet.MakeOffline(websiteKey, artical, articalOverview));
            handler.Set();
        }
        public void GetAllOfflineArticalList(string transactionId, IUiOfflineArticalOverviewResponseHandler responseHandler)
        {
            pendingRequest.Push(transactionId, InstructionSet.GetAllOfflineArticalList(responseHandler));
            handler.Set();
        }
        public void GetArtical(string transactionId, ArticalOverview articalOverview, IUiArticalResponseHandler responseHandler)
        {
            pendingRequest.Push(transactionId, InstructionSet.GetArtical(articalOverview, responseHandler));
            handler.Set();
        }
        private void initilizeDB()
        {
            db = new SQLiteConnection(DatabaseFilePath);

            db?.CreateTable<BrowserHistoryTable>();
            db?.CreateTable<OfflineTable>();
        }

        private void requestProcessing()
        {
            try
            {
                initilizeDB();
                
                while (MyGlobal.IsRunning || !isDBclosed)
                {
                    try
                    {

                        if (pendingRequest.Count == 0) handler.WaitOne();   //wait until a request is submitted
                        var request = pendingRequest.Pop(); //get the request for processing
                        lock (request.Value)
                        {
                            operationSwitcher(request.Key, request.Value);
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception ex)
            {
                Log.Error("requestProcessing", ex.Message);
            }
        }

        private void operationSwitcher(string transactionId, InstructionSet instrution)
        {
            switch (instrution.action)
            {
                case ActionToPerformInfo.Null:
                    break;
                case ActionToPerformInfo.IsSeenCheck:
                    operationIsSeenCheck(instrution.articalOverview);
                    break;
                case ActionToPerformInfo.UpdateIsSeen:
                    operationUpdateIsSeen(instrution.articalOverview);
                    break;
                case ActionToPerformInfo.MakeOffline:
                    operationMakeOffline(instrution.tags[0] as string, instrution.artical, instrution.articalOverview[0]);
                    break;
                case ActionToPerformInfo.GetAllOfflineList:
                    operationGetAllOfflineList(transactionId, instrution.tags[0] as IUiOfflineArticalOverviewResponseHandler);
                    break;
                case ActionToPerformInfo.GetArtical:
                    operationGetArtical(transactionId, instrution.articalOverview[0], instrution.tags[0] as IUiArticalResponseHandler);
                    break;
                default:
                    break;
            }            
        }
        private void operationGetArtical(string transactionId, ArticalOverview overview, IUiArticalResponseHandler responseHandler)
        {
            if (db == null) throw new InvalidOperationException("Database is not created yet.");
            if (responseHandler == null) return;

            var row = db.Get<OfflineTable>(OfflineTable.GetPrimaryKey(overview));
            if (row == null) return;

            responseHandler?.ArticalProcessedCallback(transactionId, row.LinkOfActualArtical, row.ToArtical());
        }
        private void operationGetAllOfflineList(string transactionId, IUiOfflineArticalOverviewResponseHandler responseHandler)
        {
            if (db == null) throw new InvalidOperationException("Database is not created yet.");
            if (responseHandler == null) return;

            var _list = new TableQuery<OfflineTable>(db).OrderByDescending(e => e.OfflineAvailableOn).ToList();
            var result = new List<ArticalOverviewOffline>(_list.Count);
            foreach (var item in _list)            
                result.Add(item.ToArticalOverviewOffline());
            responseHandler?.OfflineArticalOverviewProcessedCallback(transactionId, result.ToArray());
        }
        private void operationMakeOffline(string websiteKey, Artical artical, ArticalOverview overview)
        {
            //TODO: Find a way to save the pdf data in a different folder for future use as folder
            if (db == null) throw new InvalidOperationException("Database is not created yet.");

            overview.IsDatabaseConfirmed_Offline = true;
            overview.OfflineAvailableOn = DateTime.Today.ToString("yyyyMMdd");
            var obj = OfflineTable.New(websiteKey, artical, overview);
            try
            {
                db.Insert(obj);
            }
            catch (Exception)
            {
                try
                {
                    db.Update(obj);
                }
                catch (Exception) { }
            }
        }

        private void operationUpdateIsSeen(ArticalOverview[] articalOverviews)
        {
            if (db == null) throw new InvalidOperationException("Database is not created yet.");

            lock (articalOverviews)
            {
                foreach (var overview in articalOverviews)
                {
                    overview.SeenOn = DateTime.Today.ToString("yyyyMMdd");
                    overview.IsDatabaseConfirmed_SeenOn = true;
                    var obj = BrowserHistoryTable.New(overview);
                    try
                    {
                        if (db.Insert(obj) != 0)                        
                            db.Update(obj);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            db.Update(obj);
                        }
                        catch (Exception ex1)
                        {
                            Log.Error("operationUpdateIsSeen -- update", ex1.Message);
                        }
                        Log.Error("operationUpdateIsSeen -- edit", ex.Message);
                    }
                }
            }
        }

        private void operationIsSeenCheck(ArticalOverview[] articalOverviews)
        {
            if (db == null) throw new InvalidOperationException("Database is not created yet.");

            lock (articalOverviews)
            {
                foreach (var overview in articalOverviews)
                {
                    if (!overview.IsDatabaseConfirmed_SeenOn)
                    {
                        try
                        {
                            var data = db.Get<BrowserHistoryTable>(BrowserHistoryTable.GetPrimaryKey(overview));
                            overview.IsDatabaseConfirmed_SeenOn = true;
                            if (data != null)
                                overview.SeenOn = data.LastSeenOn;
                            else
                                overview.SeenOn = null;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("operationIsSeenCheck", ex.Message);
                        }                        
                    }
                }
            }
        }

        public void Close() => db?.Close();

        public DatabaseModule(string DatabaseFilePath)
        {
            this.DatabaseFilePath = DatabaseFilePath;
            new Thread(requestProcessing).Start();
        }

        private class InstructionSet
        {
            public ArticalOverview[] articalOverview { get; set; } = null;
            public ActionToPerformInfo action { get; set; } = ActionToPerformInfo.Null;
            public Artical artical { get; set; } = null;
            public object[] tags { get; set; } = null;

            public static InstructionSet IsSeenCheck(ArticalOverview[] overview)
            {
                return new InstructionSet()
                {
                    articalOverview = overview,
                    action = ActionToPerformInfo.IsSeenCheck,
                    artical = null
                };
            }
            public static InstructionSet UpdateIsSeen(ArticalOverview overview)
            {
                return new InstructionSet()
                {
                    articalOverview = new ArticalOverview[] { overview },
                    action = ActionToPerformInfo.UpdateIsSeen,
                    artical = null
                };
            }
            public static InstructionSet MakeOffline(string wesiteKey, Artical artical, ArticalOverview overview)
            {
                return new InstructionSet()
                {
                    articalOverview = new ArticalOverview[] { overview },
                    action = ActionToPerformInfo.MakeOffline,
                    artical = artical,
                    tags = new object[] { wesiteKey }
                };
            }
            public static InstructionSet GetAllOfflineArticalList(IUiOfflineArticalOverviewResponseHandler responseHandler)
            {
                return new InstructionSet()
                {
                    action = ActionToPerformInfo.GetAllOfflineList,
                    tags = new object[] { responseHandler }
                };
            }
            public static InstructionSet GetArtical(ArticalOverview overview, IUiArticalResponseHandler responseHandler)
            {
                return new InstructionSet()
                {
                    articalOverview = new ArticalOverview[] { overview },
                    action = ActionToPerformInfo.GetArtical,
                    tags = new object[] { responseHandler }
                };
            }
        }

        private enum ActionToPerformInfo
        {
            Null,
            IsSeenCheck,
            UpdateIsSeen,
            MakeOffline,
            GetAllOfflineList,
            GetArtical
        }

        [Table("history")]
        private class BrowserHistoryTable
        {
            [PrimaryKey]
            public string URL { get; set; } = null;
            public string LastSeenOn { get; set; } = null;

            public BrowserHistoryTable() { }
            public static BrowserHistoryTable New(ArticalOverview overview)
            {
                return new BrowserHistoryTable()
                {
                    URL = overview.LinkOfActualArtical,
                    LastSeenOn = DateTime.Today.ToString("yyyyMMdd")
                };
            }
            public static string GetPrimaryKey(ArticalOverview overview) => overview.LinkOfActualArtical;
        }

        [Table("offline")]
        private class OfflineTable
        {
            [PrimaryKey]
            public string LinkOfActualArtical { get; set; } = null;
            public string Title { get; set; } = null;
            public string SummaryText { get; set; } = null;
            public string Date { get; set; } = null;
            public string HtmlText { get; set; } = string.Empty;
            [Indexed]
            public string OfflineAvailableOn { get; set; } = string.Empty;
            public string SeenOn { get; set; } = null;
            public string ExtrnalLinks { get; set; } = null;
            public string WebsiteKey { get; set; } = null;

            public ArticalOverviewOffline ToArticalOverviewOffline()
            {
                return new ArticalOverviewOffline()
                {
                    LinkOfActualArtical = LinkOfActualArtical,
                    Title = Title,
                    SummaryText = SummaryText,
                    Date = Date,
                    IsDatabaseConfirmed_Offline = true,
                    OfflineAvailableOn = OfflineAvailableOn,
                    SeenOn = SeenOn,
                    IsDatabaseConfirmed_SeenOn = true,
                    WebsiteKey = WebsiteKey
                };
            }
            public Artical ToArtical()
            {
                return new Artical()
                {
                    MyLink = LinkOfActualArtical,
                    Title = Title,
                    HtmlText  = HtmlText,
                    Date = Date,
                    ExternalFileLink = ExtrnalLinks
                };
            }
            public static OfflineTable New(string websiteKey, Artical artical, ArticalOverview overview)
            {
                return new OfflineTable()
                {
                    LinkOfActualArtical = overview.LinkOfActualArtical,
                    Title = overview.Title,
                    SummaryText = overview.SummaryText,
                    Date =  overview.Date,
                    SeenOn = overview.SeenOn,
                    HtmlText = artical.HtmlText,
                    ExtrnalLinks = artical.ExternalFileLink,
                    OfflineAvailableOn = DateTime.Today.ToString("yyyyMMdd"),
                    WebsiteKey = websiteKey
                };
            }
            public static string GetPrimaryKey(ArticalOverview overview) => overview.LinkOfActualArtical;
            public static string GetPrimaryKey(Artical artical) => artical.MyLink;
        }
    }
}