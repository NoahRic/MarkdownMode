using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace MarkdownMode
{
    internal class MarkdownParseResultEventArgs : ParseResultEventArgs
    {
        readonly List<MarkdownSection> _sections;

        public MarkdownParseResultEventArgs(List<MarkdownSection> sections, ITextSnapshot snapshot, TimeSpan elapsedTime)
            : base(snapshot, elapsedTime)
        {
            if (sections == null)
                throw new ArgumentNullException("sections");

            _sections = sections;
        }

        public List<MarkdownSection> Sections
        {
            get
            {
                return _sections;
            }
        }
    }
}
