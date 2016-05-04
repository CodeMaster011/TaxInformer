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

namespace Tax_Informer
{
    public static class UiRunner
    {
        public delegate void Action();

        private static M_Handler mHandler = new M_Handler(Looper.MainLooper);

        //public UiRunner() { mHandler = new M_Handler(Looper.MainLooper); }

        public static void RunOnUi(Action action)
        {
            mHandler.pendingAction.Add(action);
            mHandler.ObtainMessage(M_Handler.RUN_ON_UI).SendToTarget();
        }

        private class M_Handler:Handler
        {
            public const int RUN_ON_UI = 62582;
            public List<Action> pendingAction = new List<Action>();

            public M_Handler(Looper looper) : base(looper) { }
            public override void HandleMessage(Message msg)
            {
                if(msg.What == RUN_ON_UI)
                {
                    try
                    {
                        var action = pendingAction[0];
                        pendingAction.RemoveAt(0);
                        action();   //execute
                    }
                    catch (Exception) { }
                }
                base.HandleMessage(msg);
            }
        }
    }
}