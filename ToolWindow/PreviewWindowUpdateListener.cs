using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

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

        [Import]
        ITextDocumentFactoryService TextDocumentFactoryService = null;

        public void  TextViewCreated(IWpfTextView textView)
        {
            MarkdownPackage package = null;

            // If there is a shell service (which there should be, in VS), force the markdown package to load
            IVsShell shell = GlobalServiceProvider.GetService(typeof(SVsShell)) as IVsShell;
            if (shell != null)
                package = MarkdownPackage.ForceLoadPackage(shell);

            if (package == null)
                return;

            textView.Properties.GetOrCreateSingletonProperty(() => new PreviewWindowUpdateListener(textView, package, TextDocumentFactoryService));
        }
    }

    sealed class PreviewWindowUpdateListener
    {
        public const string Name = "MarkdownMargin";
        
        readonly IWpfTextView textView;
        readonly ITextDocument document;
        readonly MarkdownPackage package;
        readonly PreviewWindowBackgroundParser backgroundParser;

        string previousHtml;

        MarkdownPreviewToolWindow GetPreviewWindow(bool create)
        {
            return (package != null) ? package.GetMarkdownPreviewToolWindow(create) : null;
        }

        public PreviewWindowUpdateListener(IWpfTextView wpfTextView, MarkdownPackage package, ITextDocumentFactoryService textDocumentFactoryService)
        {
            this.textView = wpfTextView;
            this.package = package;
            this.backgroundParser = new PreviewWindowBackgroundParser(wpfTextView.TextBuffer, TaskScheduler.Default, textDocumentFactoryService);
            this.backgroundParser.ParseComplete += HandleBackgroundParseComplete;

            if (!textDocumentFactoryService.TryGetTextDocument(wpfTextView.TextDataModel.DocumentBuffer, out document))
                document = null;

            if (textView.HasAggregateFocus)
                UpdatePreviewWindow(string.Empty);

            backgroundParser.RequestParse(true);

            textView.Closed += HandleTextViewClosed;
            textView.GotAggregateFocus += HandleTextViewGotAggregateFocus;
        }

        private void HandleBackgroundParseComplete(object sender, ParseResultEventArgs e)
        {
            PreviewParseResultEventArgs args = e as PreviewParseResultEventArgs;
            Action updateAction = () =>
            {
                try
                {
                    UpdatePreviewWindow(args != null ? args.Html : string.Empty);
                }
                catch (Exception ex)
                {
                    if (ErrorHandler.IsCriticalException(ex))
                        throw;
                }
            };

            textView.VisualElement.Dispatcher.BeginInvoke(updateAction);
        }

        void HandleTextViewClosed(object sender, EventArgs e)
        {
            ClearPreviewWindow();
            backgroundParser.Dispose();
        }

        void HandleTextViewGotAggregateFocus(object sender, EventArgs e)
        {
            var window = GetPreviewWindow(false);
            if (window != null)
            {
                if (window.CurrentSource == null || window.CurrentSource != this)
                    UpdatePreviewWindow(previousHtml);
            }
        }

        string GetDocumentName()
        {
            if (document == null)
                return "(no name)";
            else
                return Path.GetFileName(document.FilePath);
        }

        private void UpdatePreviewWindow(string htmlText)
        {
            previousHtml = htmlText;
            var previewWindow = GetPreviewWindow(create: false);
            if (previewWindow != null)
                previewWindow.SetPreviewContent(this, htmlText, GetDocumentName());
        }

        void ClearPreviewWindow()
        {
            var window = GetPreviewWindow(create: false);
            if (window != null && window.CurrentSource == this)
                window.ClearPreviewContent();
        }
    }
}
