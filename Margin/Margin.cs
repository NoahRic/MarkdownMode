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
        IMarkdownPreviewWindowBroker previewWindowBroker;

        MarkdownSharp.Markdown markdownTransform = new MarkdownSharp.Markdown();

        MarkdownPreviewToolWindow GetPreviewWindow(bool create)
        {
            return (previewWindowBroker != null) ? previewWindowBroker.GetMarkdownPreviewToolWindow(create) : null;
        }

        public Margin(IWpfTextView wpfTextView, IMarkdownPreviewWindowBroker previewWindowBroker, ITextDocument document)
        {
            this.textView = wpfTextView;
            this.previewWindowBroker = previewWindowBroker;
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

            Button button = new Button() { Content = "Show preview window" };
            button.Click += (sender, args) =>
                {
                    if (previewWindowBroker != null)
                    {
                        var window = previewWindowBroker.GetMarkdownPreviewToolWindow(true);
                        ((IVsWindowFrame)window.Frame).Show();
                    }
                };

            this.Children.Add(button);
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
                        string content = markdownTransform.Transform(textView.TextBuffer.CurrentSnapshot.GetText());

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
                    previewWindow.SetPreviewContent(this, markdownTransform.Transform(textView.TextBuffer.CurrentSnapshot.GetText()), GetDocumentName());
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
