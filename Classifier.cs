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
            return buffer.Properties.GetOrCreateSingletonProperty(() => new MarkdownClassifier(ClassificationRegistry));
        }
    }

    class MarkdownClassifier : IClassifier
    {
        /// <summary>
        /// A list of regular expression groups (list of regular expressions) to use.
        /// When the first match is found in each group, the rest of that group will be skipped.
        /// </summary>
        static List<List<MarkupRule>> Rules;

        static MarkdownClassifier()
        {
            Rules = new List<List<MarkupRule>>();
            string p = "markdown.link.punctuation";

            // Headers
            List<MarkupRule> headers = new List<MarkupRule>();
            headers.Add(new MarkupRule(@"^######.*$", "markdown.header.h6"));
            headers.Add(new MarkupRule(@"^#####.*$", "markdown.header.h5"));
            headers.Add(new MarkupRule(@"^####.*$", "markdown.header.h4"));
            headers.Add(new MarkupRule(@"^###.*$", "markdown.header.h3"));
            headers.Add(new MarkupRule(@"^##.*$", "markdown.header.h2"));
            headers.Add(new MarkupRule(@"^#.*$", "markdown.header.h1"));

            Rules.Add(headers);

            // Blocks
            List<MarkupRule> blocks = new List<MarkupRule>();
            blocks.Add(new MarkupRule(@"^\s*>.*$", "markdown.blockquote"));
            blocks.Add(new MarkupRule(@"    |\t", "markdown.code"));

            // Link definition
            blocks.Add(new MarkupRule(@"^ {0,3}(\[)([^\]]+)(\]:)\s*(\S*)",
                                              new string[] { p, "markdown.link.label", p, "markdown.url.definition" }));

            Rules.Add(blocks);

            // Links

            // By label
            AddRule(Rules, new MarkupRule(@"(\[)([^\]]+)(\]\s*\[)([^\]]*)(\])",
                                          new string[] { p, "markdown.link.text", p, "markdown.link.label", p }));

            // By inline url
            AddRule(Rules, new MarkupRule(@"(\[)([^\]]+)(\]\s*\()([^\)]*)(\))",
                                          new string[] { p, "markdown.link.text", p, "markdown.url.inline", p }));

            // Automatic link
            AddRule(Rules, new MarkupRule(@"(\<)([^\>]+)(\>)",
                                          new string[] { p, "markdown.url.automatic", p }));

            // Bold/italics
            AddRule(Rules, new MarkupRule(@"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)", "markdown.italics"));
            AddRule(Rules, new MarkupRule(@"(?<!\*)\*\*(?!\*)(.+?)(?<!\*)\*\*(?!\*)", "markdown.bold"));
            // TODO: What is bold + italics?

            // Lists
            AddRule(Rules, new MarkupRule(@"\s*([-*+])\s+", "markdown.list.unordered"));
            AddRule(Rules, new MarkupRule(@"\s*(\d+.)\s+", "markdown.list.ordered"));
        }

        static void AddRule(List<List<MarkupRule>> rules, MarkupRule rule)
        {
            rules.Add(new List<MarkupRule>() { rule });
        }

        IClassificationTypeRegistryService _classificationRegistry;

        public MarkdownClassifier(IClassificationTypeRegistryService classificationRegistry)
        {
            _classificationRegistry = classificationRegistry;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            ITextSnapshot snapshot = span.Snapshot;

            ITextSnapshotLine startLine = span.Start.GetContainingLine();
            int startLineNumber = startLine.LineNumber;
            int endLineNumber = (span.End <= startLine.EndIncludingLineBreak) ? startLineNumber : snapshot.GetLineNumberFromPosition(span.End);

            List<ClassificationSpan> spans = new List<ClassificationSpan>();

            for (int lineNumber = startLineNumber; lineNumber <= endLineNumber; lineNumber++)
            {
                var line = snapshot.GetLineFromLineNumber(lineNumber);
                string text = line.GetText();

                foreach (var group in Rules)
                {
                    foreach (var rule in group)
                    {
                        bool matched = false;

                        foreach (Match match in rule.Regex.Matches(text))
                        {
                            matched = true;

                            //Try and add the capture groups, if there are any
                            if (match.Groups.Count > 1)
                            {
                                for (int j = 1; j < match.Groups.Count; j++)
                                {
                                    IClassificationType classification = _classificationRegistry.GetClassificationType(rule.Names[j - 1]);
                            
                                    spans.Add(new ClassificationSpan(CreateSpan(line.Start, match.Groups[j].Index, match.Groups[j].Length),
                                              classification));
                                }
                            }
                            else
                            {
                                IClassificationType classification = _classificationRegistry.GetClassificationType(rule.Names[0]);
                            
                                spans.Add(new ClassificationSpan(CreateSpan(line.Start, match.Groups[0].Index, match.Groups[0].Length),
                                          classification));
                            }
                        }

                        if (matched)
                            break;
                    }
                }
            }

            return spans;
        }

        private static SnapshotSpan CreateSpan(SnapshotPoint start, int offset, int length)
        {
            return new SnapshotSpan(start + offset, start + offset + length);
        }
    }

    /// <summary>
    /// A regex/classification type pair.
    /// </summary>
    class MarkupRule
    {
        public Regex Regex { get; private set; }
        public string[] Names { get; private set; }

        public MarkupRule(string regex, string classificationTypeName) : this(regex, new string[] { classificationTypeName }) { }

        public MarkupRule(string regex, string[] classificationTypeNames)
        {
            Regex = new Regex(regex, RegexOptions.Compiled | RegexOptions.Singleline);
            Names = classificationTypeNames;
        }
    }
}
