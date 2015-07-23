using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Threads
{
    public class UIThread
    {
        public static bool IsUIThread()
        {
            return
                Application.Current.Dispatcher.Thread.ManagedThreadId ==
                Thread.CurrentThread.ManagedThreadId;
        }

        public static void Invoke(Action action)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, action);
        }

        public static void Invoke(Action action, DispatcherPriority priority)
        {
            Application.Current.Dispatcher.Invoke(priority, action);
        }

        public static void BeginInvoke(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, action);
        }

        public static void BeginInvoke(Action action, DispatcherPriority priority)
        {
            Application.Current.Dispatcher.BeginInvoke(priority, action);
        }
    }
}