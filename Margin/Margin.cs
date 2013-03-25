using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MarkdownMode
{
    sealed class Margin : StackPanel, IWpfTextViewMargin
    {
        public const string MarginName = "MarkdownMargin";
        
        readonly IWpfTextView textView;
        readonly MarkdownPackage package;
        readonly MarkdownBackgroundParser backgroundParser;

        List<MarkdownSection> sections;

        MarkdownSharp.Markdown markdownTransform = new MarkdownSharp.Markdown();

        ComboBox sectionCombo;
        bool ignoreComboChange = false;

        public Margin(IWpfTextView wpfTextView, MarkdownPackage package, ITextDocumentFactoryService textDocumentFactoryService)
        {
            this.textView = wpfTextView;
            this.package = package;

            this.Orientation = System.Windows.Controls.Orientation.Horizontal;
            this.Background = Brushes.SlateGray;
            this.Height = 25;

            Button showPreview = new Button() { Content = "Show preview window" };
            showPreview.Click += HandleShowPreviewClick;

            this.Children.Add(showPreview);

            Button copyHtml = new Button() { Content = "Copy HTML to clipboard" };
            copyHtml.Click += HandleCopyHtmlClick;

            this.Children.Add(copyHtml);

            sectionCombo = new ComboBox();
            sectionCombo.SelectionChanged += HandleSectionComboSelectionChanged;

            this.Children.Add(sectionCombo);

            backgroundParser = textView.TextBuffer.Properties.GetOrCreateSingletonProperty(typeof(MarkdownBackgroundParser),
                () => new MarkdownBackgroundParser(textView.TextBuffer, TaskScheduler.Default, textDocumentFactoryService));

            sections = new List<MarkdownSection>();
            backgroundParser.ParseComplete += HandleBackgroundParseComplete;
            backgroundParser.RequestParse(true);

            textView.Closed += HandleTextViewClosed;
            textView.Caret.PositionChanged += HandleTextViewCaretPositionChanged;
        }

        void HandleShowPreviewClick(object sender, EventArgs e)
        {
            if (package != null)
            {
                var window = package.GetMarkdownPreviewToolWindow(true);
                ((IVsWindowFrame)window.Frame).ShowNoActivate();
                backgroundParser.RequestParse(true);
            }
        }

        void HandleCopyHtmlClick(object sender, EventArgs e)
        {
            Clipboard.SetText(GetHTMLText());
        }

        void HandleSectionComboSelectionChanged(object sender, EventArgs e)
        {
            if (ignoreComboChange)
                return;

            ITextSnapshot snapshot = textView.TextSnapshot;

            int selectedIndex = sectionCombo.SelectedIndex;

            if (selectedIndex == 0)
            {
                NavigateTo(new SnapshotPoint(snapshot, 0));
            }
            else
            {
                selectedIndex--;

                if (selectedIndex >= sections.Count)
                {
                    Debug.Fail("An item in the combo was selected that isn't a valid section.");
                    return;
                }

                NavigateTo(sections[selectedIndex].Span.GetStartPoint(snapshot));
            }
        }

        void HandleTextViewClosed(object sender, EventArgs e)
        {
            backgroundParser.Dispose();
        }

        void HandleTextViewCaretPositionChanged(object sender, EventArgs e)
        {
            RefreshComboItems(textView.TextBuffer.CurrentSnapshot, sections);
        }

        void NavigateTo(SnapshotPoint point)
        {
            textView.Selection.Clear();
            textView.Caret.MoveTo(point);

            textView.ViewScroller.EnsureSpanVisible(textView.Selection.StreamSelectionSpan.SnapshotSpan, EnsureSpanVisibleOptions.AlwaysCenter);

            textView.VisualElement.Focus();
        }

        void HandleBackgroundParseComplete(object sender, ParseResultEventArgs e)
        {
            MarkdownParseResultEventArgs args = e as MarkdownParseResultEventArgs;
            List<MarkdownSection> newSections = args != null ? args.Sections : new List<MarkdownSection>();
            Action updateAction = () => RefreshComboItems(textView.TextBuffer.CurrentSnapshot, args.Sections);
            Dispatcher.BeginInvoke(updateAction);
        }

        void RefreshComboItems(ITextSnapshot snapshot, List<MarkdownSection> sections)
        {
            try
            {
                ignoreComboChange = true;

                if (sections != this.sections)
                {
                    this.sections = sections;
                    sectionCombo.Items.Clear();

                    // First, add an item for "Top of the file"
                    sectionCombo.Items.Add(" (Top of file)");

                    foreach (var section in sections)
                    {
                        var lineText = section.Span.GetStartPoint(snapshot).GetContainingLine().GetText().TrimStart(' ', '\t', '#');
                        string spaces = new string('-', section.TokenType - MarkdownParser.TokenType.H1);
                        string comboText = string.Format("{0}{1}", spaces, lineText);

                        sectionCombo.Items.Add(comboText);
                    }
                }

                int currentItem = 0;
                for (int i = sections.Count - 1; i >= 0; i--)
                {
                    var span = sections[i].Span.GetSpan(textView.TextSnapshot);
                    if (span.Contains(textView.Selection.Start.Position))
                    {
                        currentItem = i + 1;
                        break;
                    }
                }

                sectionCombo.SelectedIndex = currentItem;
            }
            finally
            {
                ignoreComboChange = false;
            }
        }

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

        #region IWpfTextViewMargin members

        public FrameworkElement VisualElement
        {
            get { return this; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return (marginName == MarginName) ? this : null;
        }

        public double MarginSize
        {
            get { return this.Height; }
        }

        public void Dispose()
        {
            // Nothing to see here.  Move along.
        }

        #endregion
    }
}
