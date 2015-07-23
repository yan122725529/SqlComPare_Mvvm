using System.Threading;


namespace Threads
{
    /// <summary>
    /// Default Thread is background
    /// </summary>
    public class WpfThread
    {
        public Thread BackgroundThread { get; set; }

        public WpfThread(ThreadStart threadStart)
        {
            BackgroundThread = new Thread(() => CatchForAction.ExceptionToUIThread("WpfThread Error", threadStart.Invoke)) { IsBackground = true };
        }

        public WpfThread(ParameterizedThreadStart parameterizedThreadStart)
        {
            BackgroundThread = new Thread(x => CatchForAction.ExceptionToUIThread("WpfThreadPool Error", () => parameterizedThreadStart.Invoke(x))) { IsBackground = true };
        }
    }
}