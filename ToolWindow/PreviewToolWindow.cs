using System;
using System.IO;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
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
        int? scrollBackTo;

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
            browser.Navigating += (sender, args) =>
                {
                    if (args.Uri == null || args.Uri.HostNameType != UriHostNameType.Unknown || string.IsNullOrEmpty(args.Uri.LocalPath))
                    {
                        return; // doesn't look like a relative uri, preserve default behavior
                    }

                    var srvice = (IVsUIShellOpenDocument)this.GetService(typeof(IVsUIShellOpenDocument));
                    if (srvice == null)
                    {
                        return; // error give up, preserver default behavior
                    }

                    string documentName = Path.GetFileName(args.Uri.LocalPath);
                    string[] documentInfo = new string[2];
                    if (srvice.SearchProjectsForRelativePath(0 /* RPS_UseAllSearchStrategies */, documentName, documentInfo) == VSConstants.S_OK)
                    {
                        VsShellUtilities.OpenDocument(this, documentInfo[0]);
                        args.Cancel = true; // open matching solution document, prevent default behavior
                    }
                };
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
