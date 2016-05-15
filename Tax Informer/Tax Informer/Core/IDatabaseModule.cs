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
//using Android.Database.Sqlite;
using SQLite;
using System.Threading;
using Lib;
using Android.Util;
//TODO: Create our own : Android.Database.Sqlite DB system http://www.mysamplecode.com/2011/10/android-sqlite-query-example-selection.html
//TODO: Watch videos on how to write sqlite query in android
namespace Tax_Informer.Core
{
    interface IDatabaseModule
    {
        void IsSeen(string trasactionId, ArticalOverview articalOverview);
        void IsSeen(string trasactionId, ArticalOverview[] articalOverview);
        void UpdateIsSeen(string trasactionId, ArticalOverview articalOverview);
        void Close();
    }
    class DatabaseModule : IDatabaseModule
    {
        public string DatabaseFilePath { get; } = null;
        public Context Context { get; set; } = null;

        private SQLiteConnection db = null;
        private AutoResetEvent handler = new AutoResetEvent(false);
        private DictionaryStackedQueue<string, InstructionSet> pendingRequest = new DictionaryStackedQueue<string, InstructionSet>();
        private bool isDBclosed = false;

        public void IsSeen(string trasactionId, ArticalOverview articalOverview) => IsSeen(trasactionId, new ArticalOverview[] { articalOverview });
        public void IsSeen(string trasactionId, ArticalOverview[] articalOverview)
        {
            pendingRequest.Push(trasactionId, InstructionSet.IsSeenCheck(articalOverview));
            handler.Set();
        }
        public void UpdateIsSeen(string trasactionId, ArticalOverview articalOverview)
        {
            pendingRequest.Push(trasactionId, InstructionSet.UpdateIsSeen(articalOverview));
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
                            operationSwitcher(request.Value);
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

        private void operationSwitcher(InstructionSet instrution)
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
                case ActionToPerformInfo.Update:
                    break;
                default:
                    break;
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
                    var obj = BrowserHistoryTable.FromArticalOverview(overview);
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
        }

        private enum ActionToPerformInfo
        {
            Null,
            IsSeenCheck,
            UpdateIsSeen,
            Update
        }

        [Table("history")]
        private class BrowserHistoryTable
        {
            [PrimaryKey]
            public string URL { get; set; } = null;
            public string LastSeenOn { get; set; } = null;

            public BrowserHistoryTable() { }
            public static BrowserHistoryTable FromArticalOverview(ArticalOverview overview)
            {
                return new BrowserHistoryTable()
                {
                    URL = overview.LinkOfActualArtical,
                    LastSeenOn = overview.SeenOn
                };
            }
            public static string GetPrimaryKey(ArticalOverview overview) => overview.LinkOfActualArtical;
        }

        [Table("offline")]
        private class OfflineTable
        {
            [PrimaryKey]
            public string LinkOfActualArtical { get; set; }
            public string Title { get; set; }
            public string SummaryText { get; set; }
            public string Date { get; set; }
            public string HtmlText { get; set; } = string.Empty;
            public int IsOfflineAvailable { get; set; } = -1;
            public string SeenOn { get; set; } = null;
            //TODO: Implement the mirror of Artical + ArticalOverview to support linear DB
        }
    }
}