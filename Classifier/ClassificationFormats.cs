using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.Windows.Media;

namespace MarkdownMode
{
    static class ClassificationFormats
    {
        // Bold/italics

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.italics")]
        [Name("markdown.italics")]
        sealed class MarkdownItalicsFormat : ClassificationFormatDefinition
        {
            public MarkdownItalicsFormat() { this.IsItalic = true; }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.bold")]
        [Name("markdown.bold")]
        sealed class MarkdownBoldFormat : ClassificationFormatDefinition
        {
            public MarkdownBoldFormat() { this.IsBold = true; }
        }

        // Headers

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header")]
        [Name("markdown.header")]
        [DisplayName("Markdown header")]
        [UserVisible(true)]
        sealed class MarkdownHeaderFormat : ClassificationFormatDefinition
        {
            public MarkdownHeaderFormat() { this.ForegroundColor = Colors.MediumPurple; }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header.h1")]
        [Name("markdown.header.h1")]
        sealed class MarkdownH1Format : ClassificationFormatDefinition
        {
            public MarkdownH1Format() { this.FontRenderingSize = 22; }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header.h2")]
        [Name("markdown.header.h2")]
        sealed class MarkdownH2Format : ClassificationFormatDefinition
        {
            public MarkdownH2Format() { this.FontRenderingSize = 20; }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header.h3")]
        [Name("markdown.header.h3")]
        sealed class MarkdownH3Format : ClassificationFormatDefinition
        {
            public MarkdownH3Format() { this.FontRenderingSize = 18; }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header.h4")]
        [Name("markdown.header.h4")]
        sealed class MarkdownH4Format : ClassificationFormatDefinition
        {
            public MarkdownH4Format() { this.FontRenderingSize = 16; }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header.h5")]
        [Name("markdown.header.h5")]
        sealed class MarkdownH5Format : ClassificationFormatDefinition
        {
            public MarkdownH5Format() { this.FontRenderingSize = 14; }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header.h6")]
        [Name("markdown.header.h6")]
        sealed class MarkdownH6Format : ClassificationFormatDefinition
        {
            public MarkdownH6Format() { this.FontRenderingSize = 12; }
        }

        // Lists

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.list")]
        [Name("markdown.list")]
        sealed class MarkdownListFormat : ClassificationFormatDefinition
        {
            public MarkdownListFormat() { this.IsBold = true; this.ForegroundColor = Colors.Teal; }
        }

        // Code/pre

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.block")]
        [Name("markdown.block")]
        [DisplayName("Markdown block block")]
        [UserVisible(true)]
        [Order(Before = Priority.Default, After = "markdown.blockquote")] // Low priority
        sealed class MarkdownCodeFormat : ClassificationFormatDefinition
        {
            public MarkdownCodeFormat() 
            { 
                this.ForegroundColor = Colors.LimeGreen;
                this.FontTypeface = new Typeface("Courier New"); 
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.pre")]
        [Name("markdown.pre")]
        [Order(Before = Priority.Default, After = "markdown.blockquote")] // Low priority
        sealed class MarkdownPreFormat : ClassificationFormatDefinition
        {
            public MarkdownPreFormat() { this.FontTypeface = new Typeface("Courier New"); }
        }

        // Quotes

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.blockquote")]
        [Name("markdown.blockquote")]
        [DisplayName("Markdown Blockquote")]
        [UserVisible(true)]
        [Order(Before = Priority.Default)] // Low priority
        sealed class MarkdownBlockquoteFormat : ClassificationFormatDefinition
        {
            public MarkdownBlockquoteFormat() { this.ForegroundColor = Colors.IndianRed; }
        }

        // Links

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.link")]
        [Name("markdown.link")]
        [Order(Before = Priority.Default, After = "markdown.blockquote")] // Low priority
        sealed class MarkdownLink : ClassificationFormatDefinition
        {
            public MarkdownLink()
            {
                this.ForegroundColor = Colors.Crimson;
                this.IsBold = true;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.link.text")]
        [Name("markdown.link.text")]
        [DisplayName("Markdown link text")]
        [UserVisible(true)]
        sealed class MarkdownLinkText : ClassificationFormatDefinition
        {
            public MarkdownLinkText() 
            {
                this.IsBold = false;
                this.ForegroundColor = Colors.DeepPink;
                this.TextDecorations = System.Windows.TextDecorations.Underline;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.link.title")]
        [Name("markdown.link.title")]
        [DisplayName("Markdown link title")]
        [UserVisible(true)]
        sealed class MarkdownLinkTitle : ClassificationFormatDefinition
        {
            public MarkdownLinkTitle()
            {
                this.IsBold = true;
                this.ForegroundColor = Colors.CadetBlue;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.link.label")]
        [Name("markdown.link.label")]
        [DisplayName("Markdown link label")]
        [UserVisible(true)]
        sealed class MarkdownLinkLabel : ClassificationFormatDefinition
        {
            public MarkdownLinkLabel()
            {
                this.ForegroundColor = Colors.DeepSkyBlue;
                this.IsBold = false;
            }
        }

        // Images

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.image")]
        [Name("markdown.image")]
        [Order(Before = Priority.Default, After = "markdown.blockquote")] // Low priority
        sealed class MarkdownImage : ClassificationFormatDefinition
        {
            public MarkdownImage()
            {
                this.ForegroundColor = Colors.Crimson;
                this.IsBold = true;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.image.alt")]
        [Name("markdown.image.alt")]
        [DisplayName("Markdown image alt text")]
        [UserVisible(true)]
        sealed class MarkdownImageAlt : ClassificationFormatDefinition
        {
            public MarkdownImageAlt() 
            {
                this.IsBold = false;
                this.IsItalic = true;
                this.ForegroundColor = Colors.DeepPink;
            }
        }
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.image.title")]
        [Name("markdown.image.title")]
        [DisplayName("Markdown image title")]
        [UserVisible(true)]
        sealed class MarkdownImageTitle : ClassificationFormatDefinition
        {
            public MarkdownImageTitle()
            {
                this.IsBold = true;
                this.ForegroundColor = Colors.CadetBlue;
            }
        }

        // Miscellaneous

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.horizontalrule")]
        [Name("markdown.horizontalrule")]
        [DisplayName("Markdown horizontal rule")]
        [UserVisible(true)]
        sealed class MarkdownHorizontalRule : ClassificationFormatDefinition
        {
            public MarkdownHorizontalRule()
            {
                this.TextDecorations = System.Windows.TextDecorations.Strikethrough;
                this.IsBold = true;
            }
        }
    }
}
