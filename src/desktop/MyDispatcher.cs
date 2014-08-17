using System;
using System.Windows.Threading;
using deduper.core;

namespace deduper.wpf
{
    internal class MyDispatcher : IDispatcher
    {
        private readonly Dispatcher dispatcher;

        public MyDispatcher(Dispatcher dispatcher)
        {
            // TODO: Complete member initialization
            this.dispatcher = dispatcher;
        }

        public void Execute(Action method)
        {
            dispatcher.BeginInvoke(method);
        }
    }
}