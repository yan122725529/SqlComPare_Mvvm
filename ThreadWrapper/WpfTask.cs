using System;
using System.Threading.Tasks;


namespace Threads
{
    public static class WpfTask
    {
        public static Task FactoryStartNew(Action action)
        {
            return Task.Factory.StartNew(() => CatchForAction.ExceptionToUIThread("Factory Task Error", action));
        }
    }
}