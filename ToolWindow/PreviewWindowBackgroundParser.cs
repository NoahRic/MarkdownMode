using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace MarkdownMode
{
    internal class PreviewWindowBackgroundParser : BackgroundParser
    {
        readonly MarkdownSharp.Markdown markdownTransform = new MarkdownSharp.Markdown();
        readonly ITextDocument document;

        public PreviewWindowBackgroundParser(ITextBuffer textBuffer, TaskScheduler taskScheduler, ITextDocumentFactoryService textDocumentFactoryService)
            : base(textBuffer, taskScheduler, textDocumentFactoryService)
        {
            ReparseDelay = TimeSpan.FromMilliseconds(1000);
            if (!textDocumentFactoryService.TryGetTextDocument(textBuffer, out document))
            {
                document = null;
            }
            markdownTransform.DocumentToTransformPath = document == null ? null : document.FilePath;
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
