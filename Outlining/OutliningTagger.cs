using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Threading;
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
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() => new OutliningTagger(buffer)) as ITagger<T>;
        }
    }

    class MarkdownSection
    {
        public MarkdownParser.TokenType TokenType;
        public ITrackingSpan Span;
    }

    sealed class OutliningTagger : ITagger<IOutliningRegionTag>, IDisposable
    {
        ITextBuffer _buffer;

        List<MarkdownSection> _sections;
        DispatcherTimer _timer;

        public OutliningTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _sections = new List<MarkdownSection>();

            TriggerReparse();

            _buffer.Changed += (sender, args) => TriggerReparse();
        }

        void TriggerReparse()
        {
            if (_timer == null)
            {
                _timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
                {
                    Interval = TimeSpan.FromMilliseconds(500)
                };

                _timer.Tick += (sender, args) =>
                    {
                        _timer.Stop();
                        ReparseFile();
                    };
            }

            _timer.Stop();
            _timer.Start();
        }

        void ReparseFile()
        {
            ITextSnapshot snapshot = _buffer.CurrentSnapshot;

            string text = snapshot.GetText();

            List<Tuple<int, MarkdownParser.TokenType>> startPoints = 
                new List<Tuple<int, MarkdownParser.TokenType>>(MarkdownParser.ParseMarkdownParagraph(text)
                                                                             .Where(t => IsHeaderToken(t))
                                                                             .Select(t => Tuple.Create(t.Span.Start, t.TokenType)));

            List<MarkdownSection> newSections = new List<MarkdownSection>();
            Stack<Tuple<int, MarkdownParser.TokenType>> regions = new Stack<Tuple<int,MarkdownParser.TokenType>>();

            foreach (var start in startPoints)
            {
                int previousLineNumber = Math.Max(0, snapshot.GetLineNumberFromPosition(start.Item1) - 1);
                int end = snapshot.GetLineFromLineNumber(previousLineNumber).End;

                while(regions.Count > 0 && regions.Peek().Item2 >= start.Item2)
                {
                    var region = regions.Pop();
                    var trackingSpan = snapshot.CreateTrackingSpan(Span.FromBounds(region.Item1, end), SpanTrackingMode.EdgeExclusive);
                    newSections.Add(new MarkdownSection() { TokenType = region.Item2, Span = trackingSpan });
                }

                regions.Push(start);
            }

            while (regions.Count > 0)
            {
                var region = regions.Pop();
                var trackingSpan = snapshot.CreateTrackingSpan(Span.FromBounds(region.Item1, snapshot.Length), SpanTrackingMode.EdgeExclusive);
                newSections.Add(new MarkdownSection() { TokenType = region.Item2, Span = trackingSpan });
            }

            // For now, just dirty the entire file
            _sections = newSections;

            var temp = TagsChanged;
            if (temp != null)
                temp(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
        }

        static bool IsHeaderToken(MarkdownParser.Token token)
        {
            return token.TokenType >= MarkdownParser.TokenType.H1 && token.TokenType <= MarkdownParser.TokenType.H6;
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
            if (_timer != null)
                _timer.Stop();
        }
    }
}
