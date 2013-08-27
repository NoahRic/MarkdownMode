using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace MarkdownMode
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType(ContentType.Name)]
    sealed class OutliningTaggerProvider : ITaggerProvider
    {
        [Import]
        internal ITextDocumentFactoryService TextDocumentFactoryService = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() => new OutliningTagger(buffer, this)) as ITagger<T>;
        }
    }

    class MarkdownSection
    {
        public MarkdownParser.TokenType TokenType;
        public ITrackingSpan Span;
    }

    sealed class OutliningTagger : ITagger<IOutliningRegionTag>, IDisposable
    {
        readonly Dispatcher _dispatcher;
        readonly ITextBuffer _buffer;
        readonly MarkdownBackgroundParser _backgroundParser;

        List<MarkdownSection> _sections;

        public OutliningTagger(ITextBuffer buffer, OutliningTaggerProvider provider)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _buffer = buffer;
            _backgroundParser = buffer.Properties.GetOrCreateSingletonProperty(typeof(MarkdownBackgroundParser),
                () => new MarkdownBackgroundParser(buffer, TaskScheduler.Default, provider.TextDocumentFactoryService));

            _sections = new List<MarkdownSection>();
            _backgroundParser.ParseComplete += HandleBackgroundParseComplete;
            _backgroundParser.RequestParse(true);
        }

        private void HandleBackgroundParseComplete(object sender, ParseResultEventArgs e)
        {
            MarkdownParseResultEventArgs args = e as MarkdownParseResultEventArgs;
            List<MarkdownSection> newSections = args != null ? args.Sections : new List<MarkdownSection>();

            ITextSnapshot snapshot = e.Snapshot;
            NormalizedSnapshotSpanCollection oldSectionSpans = new NormalizedSnapshotSpanCollection(
                _sections.Select(s => s.Span.GetSpan(snapshot)));
            NormalizedSnapshotSpanCollection newSectionSpans = new NormalizedSnapshotSpanCollection(
                newSections.Select(s => s.Span.GetSpan(snapshot)));

            NormalizedSnapshotSpanCollection difference = SymmetricDifference(oldSectionSpans, newSectionSpans);

            Action updateAction = () =>
            {
                try
                {
                    _sections = newSections;
                    foreach (var span in difference)
                    {
                        var temp = TagsChanged;
                        if (temp != null)
                            temp(this, new SnapshotSpanEventArgs(span));
                    }
                }
                catch (Exception ex)
                {
                    if (ErrorHandler.IsCriticalException(ex))
                        throw;
                }
            };

            _dispatcher.BeginInvoke(updateAction);
        }

        NormalizedSnapshotSpanCollection SymmetricDifference(NormalizedSnapshotSpanCollection first, NormalizedSnapshotSpanCollection second)
        {
            return NormalizedSnapshotSpanCollection.Union(
                NormalizedSnapshotSpanCollection.Difference(first, second),
                NormalizedSnapshotSpanCollection.Difference(second, first));
        }

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (_sections == null || _sections.Count == 0 || spans.Count == 0)
                yield break;

            ITextSnapshot snapshot = spans[0].Snapshot;

            foreach (var section in _sections)
            {
                var sectionSpan = section.Span.GetSpan(snapshot);

                if (spans.IntersectsWith(new NormalizedSnapshotSpanCollection(sectionSpan)))
                {
                    string firstLine = sectionSpan.Start.GetContainingLine().GetText().TrimStart(' ', '\t', '#');

                    string collapsedHintText;
                    if (sectionSpan.Length > 250)
                        collapsedHintText = snapshot.GetText(sectionSpan.Start, 247) + "...";
                    else
                        collapsedHintText = sectionSpan.GetText();

                    var tag = new OutliningRegionTag(firstLine, collapsedHintText);
                    yield return new TagSpan<IOutliningRegionTag>(sectionSpan, tag);
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public void Dispose()
        {
            _backgroundParser.Dispose();
        }
    }
}
