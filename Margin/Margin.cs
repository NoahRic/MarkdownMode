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

namespace MarkdownMode
{
    sealed class Margin : StackPanel, IWpfTextViewMargin
    {
        public const string Name = "MarkdownMargin";
        
        IWpfTextView textView;
        ITextDocument document;
        DispatcherTimer timer;
        MarkdownPackage package;

        MarkdownSharp.Markdown markdownTransform = new MarkdownSharp.Markdown();

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

        public Margin(IWpfTextView wpfTextView, MarkdownPackage package, ITextDocument document)
        {
            this.textView = wpfTextView;
            this.package = package;
            this.timer = null;
            this.document = document;

            textView.GotAggregateFocus += (sender, args) => 
                {
                    var window = GetPreviewWindow(false);
                    if (window != null)
                    {
                        if (window.CurrentSource == null || window.CurrentSource != this)
                            UpdatePreviewWindow(false);
                    }
                };

            textView.Closed += (sender, args) => ClearPreviewWindow();

            if (textView.HasAggregateFocus)
                UpdatePreviewWindow(false);

            textView.TextBuffer.Changed += BufferChanged;

            this.Orientation = System.Windows.Controls.Orientation.Horizontal;
            this.Background = Brushes.SlateGray;
            this.Height = 25;

            Button showPreview = new Button() { Content = "Show preview window" };
            showPreview.Click += (sender, args) =>
                {
                    if (package != null)
                    {
                        var window = package.GetMarkdownPreviewToolWindow(true);
                        ((IVsWindowFrame)window.Frame).Show();
                    }
                };

            this.Children.Add(showPreview);

            Button copyHtml = new Button() { Content = "Copy HTML to clipboard" };
            copyHtml.Click += (sender, args) =>
                {
                    Clipboard.SetText(GetHTMLText());
                };

            this.Children.Add(copyHtml);
        }

        string GetDocumentName()
        {
            if (document == null)
                return "(no name)";
            else
                return Path.GetFileName(document.FilePath);
        }

        void BufferChanged(object sender, TextContentChangedEventArgs args)
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }

            timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            timer.Tick += (s, e) =>
                {
                    if (timer != null)
                        timer.Stop();
                    timer = null;

                    UpdatePreviewWindow(async: true);
                };

            timer.Start();
        }

        void UpdatePreviewWindow(bool async)
        {
            if (async)
            {
                ThreadPool.QueueUserWorkItem(state =>
                    {
                        string content = GetHTMLText(extraSpace: true);

                        this.Dispatcher.Invoke(new Action(() =>
                            {
                                var previewWindow = GetPreviewWindow(create: true);

                                if (previewWindow.CurrentSource == this || previewWindow.CurrentSource == null)
                                    previewWindow.SetPreviewContent(this, content, GetDocumentName());
                            }), DispatcherPriority.ApplicationIdle);
                    });
            }
            else
            {
                var previewWindow = GetPreviewWindow(create: true);
                if (previewWindow != null)
                    previewWindow.SetPreviewContent(this, GetHTMLText(extraSpace: true), GetDocumentName());
            }
        }

        void ClearPreviewWindow()
        {
            var window = GetPreviewWindow(create: false);
            if (window != null && window.CurrentSource == this)
                window.ClearPreviewContent();
        }

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
            return (marginName == Name) ? this : null;
        }

        public double MarginSize
        {
            get { return this.Height; }
        }

        public void Dispose()
        {
            // Nothing to see here.  Move along.
        }
    }
}
