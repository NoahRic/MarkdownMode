using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.Text.RegularExpressions;

namespace MarkdownMode
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("markdown")]
    class MarkdownClassifierProvider : IClassifierProvider
    {
        [Import]
        IClassificationTypeRegistryService ClassificationRegistry = null;

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() => new MarkdownClassifier(buffer, ClassificationRegistry));
        }
    }

    class MarkdownClassifier : IClassifier
    {
        IClassificationTypeRegistryService _classificationRegistry;
        ITextBuffer _buffer;

        public MarkdownClassifier(ITextBuffer buffer, IClassificationTypeRegistryService classificationRegistry)
        {
            _classificationRegistry = classificationRegistry;
            _buffer = buffer;

            _buffer.Changed += BufferChanged;
        }

        void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            if (e.After != _buffer.CurrentSnapshot)
                return;

            ITextSnapshot snapshot = e.After;

            foreach (var change in e.Changes)
            {
                SnapshotSpan span = new SnapshotSpan(snapshot, change.NewSpan);
                SnapshotSpan paragraph = GetEnclosingParagraph(span);

                if (MarkdownParser.ParagraphContainsMultilineTokens(paragraph.GetText()))
                {
                    var temp = this.ClassificationChanged;
                    if (temp != null)
                        temp(this, new ClassificationChangedEventArgs(paragraph));
                }
            }
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        SnapshotSpan GetEnclosingParagraph(SnapshotSpan span)
        {
            ITextSnapshot snapshot = span.Snapshot;

            ITextSnapshotLine startLine = span.Start.GetContainingLine();
            int startLineNumber = startLine.LineNumber;
            int endLineNumber = (span.End <= startLine.EndIncludingLineBreak) ? startLineNumber : snapshot.GetLineNumberFromPosition(span.End);

            // Find the first/last empty line
            bool foundEmpty = false;
            while (startLineNumber > 0)
            {
                bool lineEmpty = snapshot.GetLineFromLineNumber(startLineNumber).GetText().Trim().Length == 0;

                if (lineEmpty)
                {
                    foundEmpty = true;
                }
                else if (foundEmpty)
                {
                    startLineNumber++;
                    break;
                }

                startLineNumber--;
            }

            foundEmpty = false;
            while (endLineNumber < snapshot.LineCount - 1)
            {
                bool lineEmpty = snapshot.GetLineFromLineNumber(endLineNumber).GetText().Trim().Length == 0;

                if (lineEmpty)
                {
                    foundEmpty = true;
                }
                else if (foundEmpty)
                {
                    endLineNumber--;
                    break;
                }

                endLineNumber++;
            }

            // Generate a string for this paragraph chunk
            SnapshotPoint startPoint = snapshot.GetLineFromLineNumber(startLineNumber).Start;
            SnapshotPoint endPoint = snapshot.GetLineFromLineNumber(endLineNumber).End;

            return new SnapshotSpan(startPoint, endPoint);
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            ITextSnapshot snapshot = span.Snapshot;

            SnapshotSpan paragraph = GetEnclosingParagraph(span);

            string paragraphText = snapshot.GetText(paragraph);

            // And now parse the given paragraph and return classification spans for everything

            List<ClassificationSpan> spans = new List<ClassificationSpan>();

            foreach (var token in MarkdownParser.ParseMarkdownParagraph(paragraphText))
            {
                IClassificationType type = GetClassificationTypeForMarkdownToken(token.TokenType);

                spans.Add(new ClassificationSpan(new SnapshotSpan(paragraph.Start + token.Span.Start, token.Span.Length), type));
            }

            return spans;
        }

        static Dictionary<MarkdownParser.TokenType, string> _tokenToClassificationType = new Dictionary<MarkdownParser.TokenType, string>()
        {
            { MarkdownParser.TokenType.AutomaticUrl, "markdown.url.automatic" },
            { MarkdownParser.TokenType.Blockquote, "markdown.blockquote" },
            { MarkdownParser.TokenType.Bold, "markdown.bold" },
            { MarkdownParser.TokenType.CodeBlock, "markdown.code" },
            { MarkdownParser.TokenType.H1, "markdown.header.h1" },
            { MarkdownParser.TokenType.H2, "markdown.header.h2" },
            { MarkdownParser.TokenType.H3, "markdown.header.h3" },
            { MarkdownParser.TokenType.H4, "markdown.header.h4" },
            { MarkdownParser.TokenType.H5, "markdown.header.h5" },
            { MarkdownParser.TokenType.H6, "markdown.header.h6" },
            { MarkdownParser.TokenType.HorizontalRule, "markdown.horizontalrule" },
            { MarkdownParser.TokenType.ImageAltText, "markdown.image.alt" },
            { MarkdownParser.TokenType.ImageExpression, "markdown.image" },
            { MarkdownParser.TokenType.ImageLabel, "markdown.image.label" },
            { MarkdownParser.TokenType.ImageTitle, "markdown.image.title" },
            { MarkdownParser.TokenType.InlineUrl, "markdown.url.inline" },
            { MarkdownParser.TokenType.Italics, "markdown.italics" },
            { MarkdownParser.TokenType.LinkExpression, "markdown.link" },
            { MarkdownParser.TokenType.LinkLabel, "markdown.link.label" },
            { MarkdownParser.TokenType.LinkText, "markdown.link.text" },
            { MarkdownParser.TokenType.LinkTitle, "markdown.link.title" },
            { MarkdownParser.TokenType.OrderedListElement, "markdown.list.ordered" },
            { MarkdownParser.TokenType.PreBlock, "markdown.pre" },
            { MarkdownParser.TokenType.UnorderedListElement, "markdown.list.unordered" },
            { MarkdownParser.TokenType.UrlDefinition, "markdown.url.definition" },
        };

        IClassificationType GetClassificationTypeForMarkdownToken(MarkdownParser.TokenType tokenType)
        {
            string classificationType;
            if (!_tokenToClassificationType.TryGetValue(tokenType, out classificationType))
                throw new ArgumentException("Unable to find classification type for " + tokenType.ToString(), "tokenType");

            return _classificationRegistry.GetClassificationType(classificationType);
        }
    }
}
