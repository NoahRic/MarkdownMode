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
        sealed class MarkdownHeaderFormat : ClassificationFormatDefinition
        {
            public MarkdownHeaderFormat() { this.ForegroundColor = Colors.LightPink; }
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
        [ClassificationType(ClassificationTypeNames = "markdown.code")]
        [Name("markdown.code")]
        sealed class MarkdownCodeFormat : ClassificationFormatDefinition
        {
            public MarkdownCodeFormat() { this.FontTypeface = new Typeface("Consolas"); }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.pre")]
        [Name("markdown.pre")]
        sealed class MarkdownPreFormat : ClassificationFormatDefinition
        {
            public MarkdownPreFormat() { this.FontTypeface = new Typeface("Courier New"); }
        }

        // Quotes

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "markdown.blockquote")]
        [Name("markdown.blockquote")]
        sealed class MarkdownBlockquoteFormat : ClassificationFormatDefinition
        {
            public MarkdownBlockquoteFormat() { this.ForegroundColor = Colors.DarkRed; }
        }

        // Links
	//
	// TODO: Finish writing these formats

        [Export]
        [Name("markdown.link")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownLinkDefinition;

        [Export]
        [Name("markdown.link.text")]
        [BaseDefinition("markdown.link")]
        internal static ClassificationTypeDefinition MarkdownLinkTextDefinition;

        [Export]
        [Name("markdown.link.title")]
        [BaseDefinition("markdown.link")]
        internal static ClassificationTypeDefinition MarkdownLinkTitleDefinition;

        [Export]
        [Name("markdown.link.label")]
        [BaseDefinition("markdown.link")]
        internal static ClassificationTypeDefinition MarkdownLinkLabelDefinition;

        [Export]
        [Name("markdown.link.definition")]
        [BaseDefinition("markdown.link")]
        internal static ClassificationTypeDefinition MarkdownLinkDefinitionDefinition;

        // Link URLs

        [Export]
        [Name("markdown.url")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownLinkUrlDefinition;

        [Export]
        [Name("markdown.url.inline")]
        [BaseDefinition("markdown.link.url")]
        internal static ClassificationTypeDefinition MarkdownInlineLinkDefinition;

        [Export]
        [Name("markdown.url.reference")]
        [BaseDefinition("markdown.link.url")]
        internal static ClassificationTypeDefinition MarkdownReferenceLinkDefinition;

        [Export]
        [Name("markdown.url.automatic")]
        [BaseDefinition("markdown.link.url")]
        internal static ClassificationTypeDefinition MarkdownAutomaticLinkDefinition;

        // Images

        [Export]
        [Name("markdown.image")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownImageDefinition;

        [Export]
        [Name("markdown.image.alt")]
        [BaseDefinition("markdown.image")]
        internal static ClassificationTypeDefinition MarkdownImageAltDefinition;

        [Export]
        [Name("markdown.image.alt")]
        [BaseDefinition("markdown.image")]
        internal static ClassificationTypeDefinition MarkdownImageAltDefinition;

        [Export]
        [Name("markdown.image.title")]
        [BaseDefinition("markdown.image")]
        internal static ClassificationTypeDefinition MarkdownImageTitleDefinition;

        [Export]
        [Name("markdown.url.image")]
        [BaseDefinition("markdown.image")]
        [BaseDefinition("markdown.url")]
        internal static ClassificationTypeDefinition MarkdownImageAltDefinition;

        // Miscellaneous

        [Export]
        [Name("markdown.horizontalrule")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownHorizontalRuleDefinition;
    }
}
