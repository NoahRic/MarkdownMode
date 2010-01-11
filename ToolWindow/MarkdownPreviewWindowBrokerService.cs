using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace MarkdownMode
{
    interface IMarkdownPreviewWindowBrokerService
    {
        void RegisterPreviewWindowBroker(IMarkdownPreviewWindowBroker broker);
        IMarkdownPreviewWindowBroker GetPreviewWindowBroker();
    }

    interface IMarkdownPreviewWindowBroker
    {
        MarkdownPreviewToolWindow GetMarkdownPreviewToolWindow(bool create);
    }

    [Export(typeof(IMarkdownPreviewWindowBrokerService))]
    sealed class MarkdownPreviewWindowBrokerService : IMarkdownPreviewWindowBrokerService
    {
        IMarkdownPreviewWindowBroker broker;

        public void RegisterPreviewWindowBroker(IMarkdownPreviewWindowBroker broker)
        {
            this.broker = broker;
        }

        public IMarkdownPreviewWindowBroker GetPreviewWindowBroker()
        {
            return broker;
        }
    }
}
