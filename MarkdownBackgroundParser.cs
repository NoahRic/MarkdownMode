using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace MarkdownMode
{
    internal class MarkdownBackgroundParser : BackgroundParser
    {
        public MarkdownBackgroundParser(ITextBuffer textBuffer, TaskScheduler taskScheduler, ITextDocumentFactoryService textDocumentFactoryService)
            : base(textBuffer, taskScheduler, textDocumentFactoryService)
        {
            ReparseDelay = TimeSpan.FromMilliseconds(300);
        }

        protected override void ReParseImpl()
        {
            ITextSnapshot snapshot = TextBuffer.CurrentSnapshot;
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<MarkdownSection> sections = new List<MarkdownSection>(
                MarkdownParser.ParseMarkdownSections(snapshot)
                              .Select(t => new MarkdownSection()
                              {
                                  TokenType = t.TokenType,
                                  Span = snapshot.CreateTrackingSpan(t.Span, SpanTrackingMode.EdgeExclusive)
                              }));

            OnParseComplete(new MarkdownParseResultEventArgs(sections, snapshot, stopwatch.Elapsed));
        }
    }
}
