using System;
using Microsoft.VisualStudio.Text;

namespace MarkdownMode
{
    internal class PreviewParseResultEventArgs : ParseResultEventArgs
    {
        readonly string _html;

        public PreviewParseResultEventArgs(string html, ITextSnapshot snapshot, TimeSpan elapsedTime)
            : base(snapshot, elapsedTime)
        {
            this._html = html;
        }

        public string Html
        {
            get
            {
                return _html;
            }
        }
    }
}
