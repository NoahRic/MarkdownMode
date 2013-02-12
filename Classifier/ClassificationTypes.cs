using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace MarkdownMode
{
    static class MarkdownClassificationTypes
    {
        // Base definition for all other classification types

        [Export]
        [Name("markdown")]
        internal static ClassificationTypeDefinition MarkdownClassificationDefinition = null;

        // Bold/italics

        [Export]
        [Name("markdown.italics")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownItalicsDefinition = null;

        [Export]
        [Name("markdown.bold")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownBoldDefinition = null;

        [Export]
        [Name("markdown.bolditalics")]
        [BaseDefinition("markdown.bold")]
        [BaseDefinition("markdown.itlalics")]
        internal static ClassificationTypeDefinition MarkdownBoldItalicsDefinition = null;

        // Headers

        [Export]
        [Name("markdown.header")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownHeaderDefinition = null;

        [Export]
        [Name("markdown.header.h1")]
        [BaseDefinition("markdown.header")]
        internal static ClassificationTypeDefinition MarkdownH1Definition = null;

        [Export]
        [Name("markdown.header.h2")]
        [BaseDefinition("markdown.header")]
        internal static ClassificationTypeDefinition MarkdownH2Definition = null;

        [Export]
        [Name("markdown.header.h3")]
        [BaseDefinition("markdown.header")]
        internal static ClassificationTypeDefinition MarkdownH3Definition = null;

        [Export]
        [Name("markdown.header.h4")]
        [BaseDefinition("markdown.header")]
        internal static ClassificationTypeDefinition MarkdownH4Definition = null;

        [Export]
        [Name("markdown.header.h5")]
        [BaseDefinition("markdown.header")]
        internal static ClassificationTypeDefinition MarkdownH5Definition = null;

        [Export]
        [Name("markdown.header.h6")]
        [BaseDefinition("markdown.header")]
        internal static ClassificationTypeDefinition MarkdownH6Definition = null;

        // Lists

        [Export]
        [Name("markdown.list")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownListDefinition = null;

        [Export]
        [Name("markdown.list.unordered")]
        [BaseDefinition("markdown.list")]
        internal static ClassificationTypeDefinition MarkdownUnorderedListDefinition = null;

        [Export]
        [Name("markdown.list.ordered")]
        [BaseDefinition("markdown.list")]
        internal static ClassificationTypeDefinition MarkdownOrderedListDefinition = null;

        // Code/pre

        [Export]
        [Name("markdown.pre")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownPreDefinition = null;

        [Export]
        [Name("markdown.block")]
        [BaseDefinition("markdown.pre")]
        internal static ClassificationTypeDefinition MarkdownCodeDefinition = null;

        // Quotes

        [Export]
        [Name("markdown.blockquote")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownBlockquoteDefinition = null;

        // Links

        [Export]
        [Name("markdown.label")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownLabel = null;

        [Export]
        [Name("markdown.link")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownLinkDefinition = null;

        [Export]
        [Name("markdown.link.text")]
        [BaseDefinition("markdown.link")]
        internal static ClassificationTypeDefinition MarkdownLinkTextDefinition = null;

        [Export]
        [Name("markdown.link.title")]
        [BaseDefinition("markdown.link")]
        internal static ClassificationTypeDefinition MarkdownLinkTitleDefinition = null;

        [Export]
        [Name("markdown.link.label")]
        [BaseDefinition("markdown.label")]
        [BaseDefinition("markdown.link")]
        internal static ClassificationTypeDefinition MarkdownLinkLabelDefinition = null;

        [Export]
        [Name("markdown.link.punctuation")]
        [BaseDefinition("markdown.link")]
        internal static ClassificationTypeDefinition MarkdownLinkPunctuationDefinition = null;

        // Link URLs
        
        [Export]
        [Name("markdown.url")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownLinkUrlDefinition = null;

        [Export]
        [Name("markdown.url.inline")]
        [BaseDefinition("markdown.url")]
        internal static ClassificationTypeDefinition MarkdownInlineLinkDefinition = null;
        
        [Export]
        [Name("markdown.url.definition")]
        [BaseDefinition("markdown.url")]
        internal static ClassificationTypeDefinition MarkdownDefinitionLinkDefinition = null;

        [Export]
        [Name("markdown.url.automatic")]
        [BaseDefinition("markdown.url")]
        internal static ClassificationTypeDefinition MarkdownAutomaticLinkDefinition = null;

        // Images

        [Export]
        [Name("markdown.image")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownImageDefinition = null;

        [Export]
        [Name("markdown.image.alt")]
        [BaseDefinition("markdown.image")]
        internal static ClassificationTypeDefinition MarkdownImageAltDefinition = null;

        [Export]
        [Name("markdown.image.label")]
        [BaseDefinition("markdown.label")]
        [BaseDefinition("markdown.image")]
        internal static ClassificationTypeDefinition MarkdownImageLabelDefinition = null;

        [Export]
        [Name("markdown.image.title")]
        [BaseDefinition("markdown.image")]
        internal static ClassificationTypeDefinition MarkdownImageTitleDefinition = null;

        [Export]
        [Name("markdown.url.image")]
        [BaseDefinition("markdown.image")]
        [BaseDefinition("markdown.url")]
        internal static ClassificationTypeDefinition MarkdownImageUrlDefinition = null;

        // Miscellaneous

        [Export]
        [Name("markdown.horizontalrule")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownHorizontalRuleDefinition = null;

    }
}
