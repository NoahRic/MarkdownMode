using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;

namespace MarkdownMode
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [MarginContainer(PredefinedMarginNames.Top)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [ContentType(ContentType.Name)]
    sealed class PreviewWindowViewCreationListener : IWpfTextViewCreationListener
    {
        [Import]
        SVsServiceProvider GlobalServiceProvider = null;

        public void  TextViewCreated(IWpfTextView textView)
        {
            ITextDocument document;
            if (!textView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument), out document))
            {
                document = null;
            }

            MarkdownPackage package = null;

            // If there is a shell service (which there should be, in VS), force the markdown package to load
            IVsShell shell = GlobalServiceProvider.GetService(typeof(SVsShell)) as IVsShell;
            if (shell != null)
                package = MarkdownPackage.ForceLoadPackage(shell);

            textView.Properties.GetOrCreateSingletonProperty(() => new PreviewWindowUpdateListener(textView, package, document));
        }
    }

    sealed class PreviewWindowUpdateListener
    {
        public const string Name = "MarkdownMargin";
        
        IWpfTextView textView;
        ITextDocument document;
        MarkdownPackage package;

        EventHandler updateHandler;

        MarkdownSharp.Markdown markdownTransform = new MarkdownSharp.Markdown();

        MarkdownPreviewToolWindow GetPreviewWindow(bool create)
        {
            return (package != null) ? package.GetMarkdownPreviewToolWindow(create) : null;
        }

        string GetHTMLText(bool extraSpace = false)
        {
            StringBuilder html = new StringBuilder(markdownTransform.Transform(textView.TextBuffer.CurrentSnapshot.GetText()));
            if (extraSpace)
            {
                for (int i = 0; i < 20; i++)
                    html.Append("<br />");
            }

            return html.ToString();
        }

        public PreviewWindowUpdateListener(IWpfTextView wpfTextView, MarkdownPackage package, ITextDocument document)
        {
            this.textView = wpfTextView;
            this.package = package;
            this.document = document;

            if (textView.HasAggregateFocus)
                UpdatePreviewWindow(false);

            updateHandler = (sender, args) =>
                {
                    UpdatePreviewWindow(async: true);
                };

            BufferIdleEventUtil.AddBufferIdleEventListener(wpfTextView.TextBuffer, updateHandler);

            textView.Closed += (sender, args) =>
                {
                    ClearPreviewWindow();
                    BufferIdleEventUtil.RemoveBufferIdleEventListener(wpfTextView.TextBuffer, updateHandler);
                };

            textView.GotAggregateFocus += (sender, args) => 
                {
                    var window = GetPreviewWindow(false);
                    if (window != null)
                    {
                        if (window.CurrentSource == null || window.CurrentSource != this)
                            UpdatePreviewWindow(false);
                    }
                };
        }

        string GetDocumentName()
        {
            if (document == null)
                return "(no name)";
            else
                return Path.GetFileName(document.FilePath);
        }

        void UpdatePreviewWindow(bool async)
        {
            if (async)
            {
                ThreadPool.QueueUserWorkItem(state =>
                    {
                        string content = GetHTMLText(extraSpace: true);

                        textView.VisualElement.Dispatcher.Invoke(new Action(() =>
                            {
                                var previewWindow = GetPreviewWindow(create: true);

                                if (previewWindow.CurrentSource == this || previewWindow.CurrentSource == null)
                                    previewWindow.SetPreviewContent(this, content, GetDocumentName());
                            }), DispatcherPriority.ApplicationIdle);
                    });
            }
            else
            {
                var previewWindow = GetPreviewWindow(create: true);
                if (previewWindow != null)
                    previewWindow.SetPreviewContent(this, GetHTMLText(extraSpace: true), GetDocumentName());
            }
        }

        void ClearPreviewWindow()
        {
            var window = GetPreviewWindow(create: false);
            if (window != null && window.CurrentSource == this)
                window.ClearPreviewContent();
        }
    }
}
