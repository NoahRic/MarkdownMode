using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace MarkdownMode
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(MarkdownClassifier.ContentType)]
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
        public const string ContentType = "markdown";

        private static readonly string[] _tokenToClassificationTypeName;

        private readonly IClassificationType[] _tokenToClassificationType = new IClassificationType[_tokenToClassificationTypeName.Length];
        private readonly IClassificationTypeRegistryService _classificationRegistry;
        private readonly ITextBuffer _buffer;

        static MarkdownClassifier()
        {
            MarkdownParser.TokenType[] values = (MarkdownParser.TokenType[])Enum.GetValues(typeof(MarkdownParser.TokenType));
            MarkdownParser.TokenType max = values.Max();
            _tokenToClassificationTypeName = new string[(int)max + 1];

            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.AutomaticUrl] = ClassificationTypeNames.UrlAutomatic;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.Blockquote] = ClassificationTypeNames.BlockQuote;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.Bold] = ClassificationTypeNames.Bold;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.CodeBlock] = ClassificationTypeNames.Block;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.H1] = ClassificationTypeNames.HeaderH1;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.H2] = ClassificationTypeNames.HeaderH2;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.H3] = ClassificationTypeNames.HeaderH3;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.H4] = ClassificationTypeNames.HeaderH4;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.H5] = ClassificationTypeNames.HeaderH5;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.H6] = ClassificationTypeNames.HeaderH6;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.HorizontalRule] = ClassificationTypeNames.HorizontalRule;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.ImageAltText] = ClassificationTypeNames.ImageAlt;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.ImageExpression] = ClassificationTypeNames.Image;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.ImageLabel] = ClassificationTypeNames.ImageLabel;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.ImageTitle] = ClassificationTypeNames.ImageTitle;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.InlineUrl] = ClassificationTypeNames.UrlInline;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.Italics] = ClassificationTypeNames.Italics;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.LinkExpression] = ClassificationTypeNames.Link;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.LinkLabel] = ClassificationTypeNames.LinkLabel;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.LinkText] = ClassificationTypeNames.LinkText;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.LinkTitle] = ClassificationTypeNames.LinkTitle;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.OrderedListElement] = ClassificationTypeNames.ListOrdered;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.PreBlock] = ClassificationTypeNames.Preformatted;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.UnorderedListElement] = ClassificationTypeNames.ListUnordered;
            _tokenToClassificationTypeName[(int)MarkdownParser.TokenType.UrlDefinition] = ClassificationTypeNames.UrlDefinition;
        }

        public MarkdownClassifier(ITextBuffer buffer, IClassificationTypeRegistryService classificationRegistry)
        {
            _classificationRegistry = classificationRegistry;
            _buffer = buffer;

            _buffer.Changed += BufferChanged;
        }

        /// <summary>
        /// When the buffer changes, check to see if any of the edits were in a paragraph with multi-line tokens.
        /// If so, we need to send out a classification changed event for those paragraphs.
        /// </summary>
        void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            int eventsSent = 0;

            foreach (var change in e.Changes)
            {
                SnapshotSpan paragraph = GetEnclosingParagraph(new SnapshotSpan(e.After, change.NewSpan));

                if (MarkdownParser.ParagraphContainsMultilineTokens(paragraph.GetText()))
                {
                    eventsSent++;

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

        private IClassificationType GetClassificationTypeForMarkdownToken(MarkdownParser.TokenType tokenType)
        {
            if ((int)tokenType < 0 || (int)tokenType >= _tokenToClassificationType.Length)
                return null;

            IClassificationType result = _tokenToClassificationType[(int)tokenType];
            if (result == null)
            {
                if (string.IsNullOrEmpty(_tokenToClassificationTypeName[(int)tokenType]))
                    throw new ArgumentException("Unable to find classification type for " + tokenType.ToString(), "tokenType");

                result = _classificationRegistry.GetClassificationType(_tokenToClassificationTypeName[(int)tokenType]);
                _tokenToClassificationType[(int)tokenType] = result;
            }

            return result;
        }
    }
}
