using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace MarkdownMode
{
    internal class PreviewWindowBackgroundParser : BackgroundParser
    {
        readonly MarkdownSharp.Markdown markdownTransform = new MarkdownSharp.Markdown();
        readonly string markdownDocumentPath;

        public PreviewWindowBackgroundParser(ITextBuffer textBuffer, TaskScheduler taskScheduler, ITextDocumentFactoryService textDocumentFactoryService)
            : base(textBuffer, taskScheduler, textDocumentFactoryService)
        {
            ReparseDelay = TimeSpan.FromMilliseconds(1000);

            ITextDocument markdownDocument;
            if (textDocumentFactoryService.TryGetTextDocument(textBuffer, out markdownDocument))
            {
                markdownDocumentPath = markdownDocument.FilePath;
            } 
        }

        public override string Name
        {
            get
            {
                return "Markdown Preview Window";
            }
        }

        string GetHTMLText(string text, bool extraSpace)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body>");
            html.AppendLine(markdownTransform.Transform(text));
            html.AppendLine(markdownTransform.Transform(text, markdownDocumentPath));
            if (extraSpace)
            {
                for (int i = 0; i < 20; i++)
                    html.Append("<br />");
            }
            html.AppendLine("</body></html>");

            return html.ToString();
        }

        protected override void ReParseImpl()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ITextSnapshot snapshot = TextBuffer.CurrentSnapshot;
            string content = GetHTMLText(snapshot.GetText(), true);

            OnParseComplete(new PreviewParseResultEventArgs(content, snapshot, stopwatch.Elapsed));
        }
    }
}
