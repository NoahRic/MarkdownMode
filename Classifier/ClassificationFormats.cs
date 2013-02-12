using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace MarkdownMode
{
    static class ClassificationFormats
    {
        // Bold/italics

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.italics")]
        [Name("markdown.italics")]
        [UserVisible(true)]
        sealed class MarkdownItalicsFormat : ClassificationFormatDefinition
        {
            public MarkdownItalicsFormat()
            {
                this.DisplayName = Resources.FormatItalics;
                this.IsItalic = true;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.bold")]
        [Name("markdown.bold")]
        [UserVisible(true)]
        sealed class MarkdownBoldFormat : ClassificationFormatDefinition
        {
            public MarkdownBoldFormat()
            {
                this.DisplayName = Resources.FormatBold;
                this.IsBold = true;
            }
        }

        // Headers

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header")]
        [Name("markdown.header")]
        [UserVisible(true)]
        sealed class MarkdownHeaderFormat : ClassificationFormatDefinition
        {
            public MarkdownHeaderFormat()
            {
                this.DisplayName = Resources.FormatHeader;
                this.ForegroundColor = Colors.MediumPurple;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header.h1")]
        [Name("markdown.header.h1")]
        [UserVisible(true)]
        sealed class MarkdownH1Format : ClassificationFormatDefinition
        {
            public MarkdownH1Format()
            {
                this.DisplayName = Resources.FormatHeaderH1;
                this.ForegroundColor = Colors.MediumPurple;
                this.FontRenderingSize = 22;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header.h2")]
        [Name("markdown.header.h2")]
        [UserVisible(true)]
        sealed class MarkdownH2Format : ClassificationFormatDefinition
        {
            public MarkdownH2Format()
            {
                this.DisplayName = Resources.FormatHeaderH2;
                this.ForegroundColor = Colors.MediumPurple;
                this.FontRenderingSize = 20;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header.h3")]
        [Name("markdown.header.h3")]
        [UserVisible(true)]
        sealed class MarkdownH3Format : ClassificationFormatDefinition
        {
            public MarkdownH3Format()
            {
                this.DisplayName = Resources.FormatHeaderH3;
                this.ForegroundColor = Colors.MediumPurple;
                this.FontRenderingSize = 18;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header.h4")]
        [Name("markdown.header.h4")]
        [UserVisible(true)]
        sealed class MarkdownH4Format : ClassificationFormatDefinition
        {
            public MarkdownH4Format()
            {
                this.DisplayName = Resources.FormatHeaderH4;
                this.ForegroundColor = Colors.MediumPurple;
                this.FontRenderingSize = 16;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header.h5")]
        [Name("markdown.header.h5")]
        [UserVisible(true)]
        sealed class MarkdownH5Format : ClassificationFormatDefinition
        {
            public MarkdownH5Format()
            {
                this.DisplayName = Resources.FormatHeaderH5;
                this.ForegroundColor = Colors.MediumPurple;
                this.FontRenderingSize = 14;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.header.h6")]
        [Name("markdown.header.h6")]
        [UserVisible(true)]
        sealed class MarkdownH6Format : ClassificationFormatDefinition
        {
            public MarkdownH6Format()
            {
                this.DisplayName = Resources.FormatHeaderH6;
                this.ForegroundColor = Colors.MediumPurple;
                this.FontRenderingSize = 12;
            }
        }

        // Lists

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.list")]
        [Name("markdown.list")]
        [UserVisible(true)]
        sealed class MarkdownListFormat : ClassificationFormatDefinition
        {
            public MarkdownListFormat()
            {
                this.DisplayName = Resources.FormatList;
                this.IsBold = true;
                this.ForegroundColor = Colors.Teal;
            }
        }

        // Code/pre

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.block")]
        [Name("markdown.block")]
        [UserVisible(true)]
        [Order(Before = Priority.Default, After = "markdown.blockquote")] // Low priority
        sealed class MarkdownCodeFormat : ClassificationFormatDefinition
        {
            public MarkdownCodeFormat()
            {
                this.DisplayName = Resources.FormatBlock;
                this.ForegroundColor = Colors.LimeGreen;
                this.FontTypeface = new Typeface("Courier New");
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.pre")]
        [Name("markdown.pre")]
        [UserVisible(true)]
        [Order(Before = Priority.Default, After = "markdown.blockquote")] // Low priority
        sealed class MarkdownPreFormat : ClassificationFormatDefinition
        {
            public MarkdownPreFormat()
            {
                this.DisplayName = Resources.FormatPre;
                this.FontTypeface = new Typeface("Courier New");
            }
        }

        // Quotes

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.blockquote")]
        [Name("markdown.blockquote")]
        [UserVisible(true)]
        [Order(Before = Priority.Default)] // Low priority
        sealed class MarkdownBlockquoteFormat : ClassificationFormatDefinition
        {
            public MarkdownBlockquoteFormat()
            {
                this.DisplayName = Resources.FormatBlockQuote;
                this.ForegroundColor = Colors.IndianRed;
            }
        }

        // Links

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.link")]
        [Name("markdown.link")]
        [Order(Before = Priority.Default, After = "markdown.blockquote")] // Low priority
        [UserVisible(true)]
        sealed class MarkdownLink : ClassificationFormatDefinition
        {
            public MarkdownLink()
            {
                this.DisplayName = Resources.FormatLink;
                this.ForegroundColor = Colors.Crimson;
                this.IsBold = true;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.link.text")]
        [Name("markdown.link.text")]
        [UserVisible(true)]
        sealed class MarkdownLinkText : ClassificationFormatDefinition
        {
            public MarkdownLinkText()
            {
                this.DisplayName = Resources.FormatLinkText;
                this.IsBold = false;
                this.ForegroundColor = Colors.DeepPink;
                this.TextDecorations = System.Windows.TextDecorations.Underline;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.link.title")]
        [Name("markdown.link.title")]
        [UserVisible(true)]
        sealed class MarkdownLinkTitle : ClassificationFormatDefinition
        {
            public MarkdownLinkTitle()
            {
                this.DisplayName = Resources.FormatLinkTitle;
                this.IsBold = true;
                this.ForegroundColor = Colors.CadetBlue;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.link.label")]
        [Name("markdown.link.label")]
        [UserVisible(true)]
        [Order(After = "markdown.link")]
        sealed class MarkdownLinkLabel : ClassificationFormatDefinition
        {
            public MarkdownLinkLabel()
            {
                this.DisplayName = Resources.FormatLinkLabel;
                this.ForegroundColor = Colors.DeepSkyBlue;
                this.IsBold = false;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.url.inline")]
        [Name("markdown.url.inline")]
        [UserVisible(true)]
        [Order(After = "markdown.link")]
        sealed class MarkdownUrl : ClassificationFormatDefinition
        {
            public MarkdownUrl()
            {
                this.DisplayName = Resources.FormatUrl;
                this.ForegroundColor = Colors.Blue;
                this.IsBold = false;
            }
        }

        // Images

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.image")]
        [Name("markdown.image")]
        [UserVisible(true)]
        [Order(Before = Priority.Default, After = "markdown.blockquote")] // Low priority
        sealed class MarkdownImage : ClassificationFormatDefinition
        {
            public MarkdownImage()
            {
                this.DisplayName = Resources.FormatImage;
                this.ForegroundColor = Colors.Crimson;
                this.IsBold = true;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.image.alt")]
        [Name("markdown.image.alt")]
        [UserVisible(true)]
        sealed class MarkdownImageAlt : ClassificationFormatDefinition
        {
            public MarkdownImageAlt()
            {
                this.DisplayName = Resources.FormatImageAlt;
                this.IsBold = false;
                this.IsItalic = true;
                this.ForegroundColor = Colors.DeepPink;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.image.label")]
        [Name("markdown.image.label")]
        [UserVisible(true)]
        [Order(After = "markdown.image")]
        sealed class MarkdownImageLabel : ClassificationFormatDefinition
        {
            public MarkdownImageLabel()
            {
                this.DisplayName = Resources.FormatImageLabel;
                this.ForegroundColor = Colors.DeepSkyBlue;
                this.IsBold = false;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.image.title")]
        [Name("markdown.image.title")]
        [UserVisible(true)]
        sealed class MarkdownImageTitle : ClassificationFormatDefinition
        {
            public MarkdownImageTitle()
            {
                this.DisplayName = Resources.FormatImageTitle;
                this.IsBold = true;
                this.ForegroundColor = Colors.CadetBlue;
            }
        }

        // Miscellaneous

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.horizontalrule")]
        [Name("markdown.horizontalrule")]
        [UserVisible(true)]
        sealed class MarkdownHorizontalRule : ClassificationFormatDefinition
        {
            public MarkdownHorizontalRule()
            {
                this.DisplayName = Resources.FormatHorizontalRule;
                this.TextDecorations = System.Windows.TextDecorations.Strikethrough;
                this.IsBold = true;
            }
        }
    }
}
