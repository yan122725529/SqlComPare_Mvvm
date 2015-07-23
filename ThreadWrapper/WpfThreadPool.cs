using System.Threading;

namespace Threads
{
    public static class WpfThreadPool
    {
        public static void QueueUserWorkItem(WaitCallback callback)
        {
            ThreadPool.QueueUserWorkItem(x => CatchForAction.ExceptionToUIThread("WpfThreadPool Error", () => callback(x)));
        }

        public static void QueueUserWorkItem(WaitCallback callback, object state)
        {
            ThreadPool.QueueUserWorkItem(x => CatchForAction.ExceptionToUIThread("WpfThreadPool Error", () => callback(state)));
        }

        public static void UnsafeQueueUserWorkItem(WaitCallback callback, object state)
        {
            ThreadPool.UnsafeQueueUserWorkItem(x => CatchForAction.ExceptionToUIThread("WpfThreadPool Error", () => callback(state)), null);
        }
    }
}