using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace Threads
{
    public static class CatchForAction
    {
        public static void ExceptionToUIThread(string errorTitle, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                CatchThreadException(errorTitle, ex);
            }
        }

        public static void ExceptionToDoWork(string errorTitle, Action<object, DoWorkEventArgs> action, object sender,
            DoWorkEventArgs args)
        {
            try
            {
                action(sender, args);
            }
            catch (Exception ex)
            {
                CatchThreadException(errorTitle, ex);
            }
        }

        private static void CatchThreadException(string errorTitle, Exception ex)
        {
            var t = Thread.CurrentThread;
            UIThread.Invoke(() =>
            {
                t.Abort();
                throw ex;
            }, DispatcherPriority.Send);
        }
    }
}