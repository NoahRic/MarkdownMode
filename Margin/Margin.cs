using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;

namespace MarkdownMode
{
    sealed class Margin : StackPanel, IWpfTextViewMargin
    {
        public const string MarginName = "MarkdownMargin";
        
        IWpfTextView textView;
        MarkdownPackage package;

        List<MarkdownSection> sections;

        MarkdownSharp.Markdown markdownTransform = new MarkdownSharp.Markdown();

        ComboBox sectionCombo;
        bool ignoreComboChange = false;

        public Margin(IWpfTextView wpfTextView, MarkdownPackage package)
        {
            this.textView = wpfTextView;
            this.package = package;

            this.Orientation = System.Windows.Controls.Orientation.Horizontal;
            this.Background = Brushes.SlateGray;
            this.Height = 25;

            Button showPreview = new Button() { Content = "Show preview window" };
            showPreview.Click += (sender, args) =>
                {
                    if (package != null)
                    {
                        var window = package.GetMarkdownPreviewToolWindow(true);
                        ((IVsWindowFrame)window.Frame).ShowNoActivate();
                    }
                };

            this.Children.Add(showPreview);

            Button copyHtml = new Button() { Content = "Copy HTML to clipboard" };
            copyHtml.Click += (sender, args) =>
                {
                    Clipboard.SetText(GetHTMLText());
                };

            this.Children.Add(copyHtml);

            sectionCombo = new ComboBox();
            sectionCombo.SelectionChanged += (sender, args) =>
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
                };
            RefreshComboItems(null, EventArgs.Empty);

            this.Children.Add(sectionCombo);

            BufferIdleEventUtil.AddBufferIdleEventListener(textView.TextBuffer, RefreshComboItems);
            textView.Closed += (sender, args) => BufferIdleEventUtil.RemoveBufferIdleEventListener(textView.TextBuffer, RefreshComboItems);

            textView.Caret.PositionChanged += (sender, args) => SetSectionComboToCaretPosition();
        }

        void NavigateTo(SnapshotPoint point)
        {
            textView.Selection.Clear();
            textView.Caret.MoveTo(point);

            textView.ViewScroller.EnsureSpanVisible(textView.Selection.StreamSelectionSpan.SnapshotSpan, EnsureSpanVisibleOptions.AlwaysCenter);

            textView.VisualElement.Focus();
        }

        void SetSectionComboToCaretPosition()
        {
            try
            {
                ignoreComboChange = true;

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

        void RefreshComboItems(object sender, EventArgs args)
        {
            try
            {
                ignoreComboChange = true;

                ITextSnapshot snapshot = textView.TextBuffer.CurrentSnapshot;

                sections = new List<MarkdownSection>(
                    MarkdownParser.ParseMarkdownSections(snapshot)
                                  .Select(t => new MarkdownSection()
                                  {
                                      TokenType = t.TokenType,
                                      Span = snapshot.CreateTrackingSpan(t.Span, SpanTrackingMode.EdgeExclusive)
                                  }));

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

                SetSectionComboToCaretPosition();
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
