using System;
using Windows.UI.Core;
using deduper.core;

namespace deduper.win8store
{
    internal class MyDispatcher : IDispatcher
    {
        private readonly CoreDispatcher coreDispatcher;

        public MyDispatcher(CoreDispatcher coreDispatcher)
        {
            // TODO: Complete member initialization
            this.coreDispatcher = coreDispatcher;
        }

        public void Execute(Action action)
        {
            coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }
    }
}