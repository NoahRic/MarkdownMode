using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace MarkdownMode
{
    internal class PreviewWindowBackgroundParser : BackgroundParser
    {
        readonly MarkdownSharp.Markdown markdownTransform = new MarkdownSharp.Markdown();
        readonly string markdownDocumentPath;

        public PreviewWindowBackgroundParser(ITextBuffer textBuffer, TaskScheduler taskScheduler, ITextDocumentFactoryService textDocumentFactoryService)
            : base(textBuffer, taskScheduler, textDocumentFactoryService)
        {
            ReparseDelay = TimeSpan.FromMilliseconds(1000);

            ITextDocument markdownDocument;
            if (textDocumentFactoryService.TryGetTextDocument(textBuffer, out markdownDocument))
            {
                markdownDocumentPath = markdownDocument.FilePath;
            }
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

            // The style sheet could probably be moved into an embedded resource or perhaps made configurable.
            // For now, it's just hard-coded.
            html.AppendLine(@"<!DOCTYPE html>
<html>
<head>
<meta http-equiv=""X-UA-Compatible"" content=""edge"" />
<meta charset=""utf-8"">
<style>
body {
	margin: 15px;
	font-family: ""Helvetica Neue"", Helvetica, ""Segoe UI"", Arial, sans-serif;
	font-size: 16px;
	line-height: 1.6;
}

h1 {
	padding-bottom: 0.3em;
	font-size: 2.25em;
	line-height: 1.2;
}

h2 {
	padding-bottom: 0.3em;
	font-size: 1.75em;
	line-height: 1.225;
}

h3 {
	font-size: 1.5em;
	line-height: 1.43;
}

h4 {
	font-size: 1.25em;
}

h5 {
	font-size: 1em;
}

h6 {
	font-size: 1em;
	color: #777;
}

p, blockquote, ul, ol, dl, table, pre {
	margin-top: 0;
	margin-bottom: 16px;
}

hr {
	height: 4px;
	padding: 0;
	margin: 16px 0;
	background-color: #e7e7e7;
	border: 0 none;
}

ul, ol {
	padding-left: 2em;
}

ul ul, ul ol, ol ol, ol ul {
	margin-top: 0;
	margin-bottom: 0;
}

li > p {
	margin-top: 16px;
}

dl {
	padding: 0;
}

dl dt {
	padding: 0;
	margin-top: 16px;
	font-size: 1em;
	font-style: italic;
	font-weight: bold;
}

dl dd {
	padding: 0 16px;
	margin-bottom: 16px;
}

blockquote {
	padding: 0 15px;
	color: #777;
	border-left: 4px solid #ddd;
}

table {
	border-collapse: collapse;
	border-spacing: 0;
	display: block;
	width: 100%;
	word-break: normal;
	word-break: keep-all;
}

table th {
	font-weight: bold;
}

table th, table td {
	padding: 6px 13px;
	border: 1px solid #ddd;
}

table tr {
	background-color: #fff;
	border-top: 1px solid #ccc;
}

img {
	border: 0;
}

kbd {
	display: inline-block;
	padding: 3px 5px;
	font-size: 11px;
	line-height: 10px;
	color: #555;
	vertical-align: middle;
	background-color: #fcfcfc;
	border: solid 1px #ccc;
	border-bottom-color: #bbb;
	border-radius: 3px;
	box-shadow: inset 0 -1px 0 #bbb;
}

code, tt {
	font-family: Consolas, ""Courier New"", monospace;
}

pre {
	margin-top: 0;
	margin-bottom: 16px;
    padding: 10px;
	font-family: Consolas, ""Courier New"", monospace;
	background-color: #f7f7f7;
	border-radius: 3px;
}

pre code, pre tt {
	font-family: Consolas, ""Courier New"", monospace;
}
</style>
</head>
<body>");

            html.AppendLine(markdownTransform.Transform(text, markdownDocumentPath));

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
