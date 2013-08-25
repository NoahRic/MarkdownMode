using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using StreamReader = System.IO.StreamReader;
using StreamWriter = System.IO.StreamWriter;

namespace MarkdownMode
{
    internal class PreviewWindowBackgroundParser : BackgroundParser
    {
        readonly MarkdownSharp.Markdown markdownTransform = new MarkdownSharp.Markdown();

        public PreviewWindowBackgroundParser(ITextBuffer textBuffer, TaskScheduler taskScheduler, ITextDocumentFactoryService textDocumentFactoryService)
            : base(textBuffer, taskScheduler, textDocumentFactoryService)
        {
            ReparseDelay = TimeSpan.FromMilliseconds(1000);
        }

        public override string Name
        {
            get
            {
                return "Markdown Preview Window";
            }
        }

        string GetHTMLText(string text, bool extraSpace)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");

            if (MarkdownPackage.Instance.RenderingOptions.RenderUsingGitHub)
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://api.github.com/markdown/raw");
                request.Method = "POST";
                request.ContentType = "text/x-markdown; charset=utf-8";
                request.UserAgent = Resources.GitHubUserAgent;

                string accessToken = MarkdownPackage.Instance.RenderingOptions.AccessToken;
                if (!string.IsNullOrEmpty(accessToken))
                    request.Headers.Add("Authorization", "token " + accessToken);

                var requestStream = request.GetRequestStream();
                using (StreamWriter writer = new StreamWriter(requestStream, Encoding.UTF8))
                {
                    writer.Write(text);
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string body = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(response.CharacterSet)).ReadToEnd();

                html.AppendLine("<html><head><meta charset='utf-8'>");
                html.AppendLine("<style type='text/css'>");
                html.AppendLine(Resources.GitHubCss);
                html.AppendLine("</style>");

                // highlight
                html.AppendLine("<style type='text/css'>");
                html.AppendLine(Resources.HighlightCss);
                html.AppendLine("</style>");

                html.AppendLine("<script type='text/javascript'>");
                html.AppendLine(Resources.HighlightJavaScript);
                html.AppendLine("</script>");

                html.AppendLine("<script type='text/javascript'>hljs.initHighlightingOnLoad();</script>");

                // mathjax
                html.AppendLine(Resources.Mathjax);

                html.AppendLine("</head><body>");

                html.AppendLine(body);
            }
            else
            {
                html.AppendLine("<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body>");
                html.AppendLine(markdownTransform.Transform(text));
            }

            if (extraSpace)
            {
                for (int i = 0; i < 20; i++)
                    html.Append("<br />");
            }
            html.AppendLine("</body></html>");

            return html.ToString();
        }

        protected override void ReParseImpl()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ITextSnapshot snapshot = TextBuffer.CurrentSnapshot;
            string content = GetHTMLText(snapshot.GetText(), true);

            OnParseComplete(new PreviewParseResultEventArgs(content, snapshot, stopwatch.Elapsed));
        }
    }
}
