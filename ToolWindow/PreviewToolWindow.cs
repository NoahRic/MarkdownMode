using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.Windows.Controls;

namespace MarkdownMode
{
    [Guid("acd82a5f-9c35-400b-b9d0-f97925f3b312")]
    public class MarkdownPreviewToolWindow : ToolWindowPane
    {
        private readonly WebBrowser browser;

        object source;
        string html;
        string title;

        const string EmptyWindowHtml = "Open a markdown file to see a preview.";
        int? scrollBackTo = null;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public MarkdownPreviewToolWindow() : base(null)
        {
            this.Caption = "Markdown Preview";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            browser = new WebBrowser();
            browser.NavigateToString(EmptyWindowHtml);
            browser.LoadCompleted += (sender, args) =>
            {
                if (scrollBackTo.HasValue)
                {
                    var document = browser.Document as mshtml.IHTMLDocument2;

                    if (document != null)
                    {
                        var element = document.body as mshtml.IHTMLElement2;
                        if (element != null)
                        {
                            element.scrollTop = scrollBackTo.Value;
                        }
                    }
                }

                scrollBackTo = null;
            };
            browser.IsVisibleChanged += HandleBrowserIsVisibleChanged;
        }

        public override object Content
        {
            get { return browser; }
        }

        public bool IsVisible
        {
            get { return browser != null && browser.IsVisible; }
        }

        public object CurrentSource
        {
            get
            {
                return source;
            }
        }

        void HandleBrowserIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool visible = (bool)e.NewValue;
            if (visible)
            {
                object source = this.source;
                this.source = null;
                SetPreviewContent(source, html, title);
            }
        }

        public void SetPreviewContent(object source, string html, string title)
        {
            if (string.IsNullOrEmpty(html) && string.IsNullOrEmpty(title))
            {
                ClearPreviewContent();
                return;
            }

            bool sameSource = source == this.source;
            this.source = source;
            this.html = html;
            this.title = title;

            if (!IsVisible)
                return;

            this.Caption = "Markdown Preview - " + title;

            if (sameSource)
            {
                // If the scroll back to already has a value, it means the current content hasn't finished loading yet,
                // so the current scroll position isn't ready for us to use.  Just use the existing scroll position.
                if (!scrollBackTo.HasValue)
                {
                    var document = browser.Document as mshtml.IHTMLDocument2;
                    if (document != null)
                    {
                        var element = document.body as mshtml.IHTMLElement2;
                        if (element != null)
                        {
                            scrollBackTo = element.scrollTop;
                        }
                    }
                }
            }
            else
            {
                scrollBackTo = null;
            }

            browser.NavigateToString(html);
        }

        public void ClearPreviewContent()
        {
            this.Caption = "Markdown Preview";
            scrollBackTo = null;
            source = null;
            html = string.Empty;
            title = string.Empty;

            browser.NavigateToString(EmptyWindowHtml);
        }
    }
}
