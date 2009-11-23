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
        internal static ClassificationTypeDefinition MarkdownClassificationDefinition;

        // Bold/italics

        [Export]
        [Name("markdown.italics")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownItalicsDefinition;

        [Export]
        [Name("markdown.bold")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownBoldDefinition;

        [Export]
        [Name("markdown.bolditalics")]
        [BaseDefinition("markdown.bold")]
        [BaseDefinition("markdown.itlalics")]
        internal static ClassificationTypeDefinition MarkdownBoldItalicsDefinition;

        // Headers

        [Export]
        [Name("markdown.header")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownHeaderDefinition;

        [Export]
        [Name("markdown.header.h1")]
        [BaseDefinition("markdown.header")]
        internal static ClassificationTypeDefinition MarkdownH1Definition;

        [Export]
        [Name("markdown.header.h2")]
        [BaseDefinition("markdown.header")]
        internal static ClassificationTypeDefinition MarkdownH2Definition;

        [Export]
        [Name("markdown.header.h3")]
        [BaseDefinition("markdown.header")]
        internal static ClassificationTypeDefinition MarkdownH3Definition;

        [Export]
        [Name("markdown.header.h4")]
        [BaseDefinition("markdown.header")]
        internal static ClassificationTypeDefinition MarkdownH4Definition;

        [Export]
        [Name("markdown.header.h5")]
        [BaseDefinition("markdown.header")]
        internal static ClassificationTypeDefinition MarkdownH5Definition;

        [Export]
        [Name("markdown.header.h6")]
        [BaseDefinition("markdown.header")]
        internal static ClassificationTypeDefinition MarkdownH6Definition;

        // Lists

        [Export]
        [Name("markdown.list")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownListDefinition;

        [Export]
        [Name("markdown.list.unordered")]
        [BaseDefinition("markdown.list")]
        internal static ClassificationTypeDefinition MarkdownUnorderedListDefinition;

        [Export]
        [Name("markdown.list.ordered")]
        [BaseDefinition("markdown.list")]
        internal static ClassificationTypeDefinition MarkdownOrderedListDefinition;

        // Code/pre

        [Export]
        [Name("markdown.pre")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownPreDefinition;

        [Export]
        [Name("markdown.code")]
        [BaseDefinition("markdown.pre")]
        internal static ClassificationTypeDefinition MarkdownCodeDefinition;

        // Quotes

        [Export]
        [Name("markdown.blockquote")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdowBlockquoteDefinition;

        // Links

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
        [Name("markdown.link.punctuation")]
        [BaseDefinition("markdown.link")]
        internal static ClassificationTypeDefinition MarkdownLinkPunctuationDefinition;

        // Link URLs
        
        [Export]
        [Name("markdown.url")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownLinkUrlDefinition;

        [Export]
        [Name("markdown.url.inline")]
        [BaseDefinition("markdown.url")]
        internal static ClassificationTypeDefinition MarkdownInlineLinkDefinition;
        
        [Export]
        [Name("markdown.url.definition")]
        [BaseDefinition("markdown.url")]
        internal static ClassificationTypeDefinition MarkdownDefinitionLinkDefinition;

        [Export]
        [Name("markdown.url.automatic")]
        [BaseDefinition("markdown.url")]
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
        [Name("markdown.image.title")]
        [BaseDefinition("markdown.image")]
        internal static ClassificationTypeDefinition MarkdownImageTitleDefinition;

        [Export]
        [Name("markdown.url.image")]
        [BaseDefinition("markdown.image")]
        [BaseDefinition("markdown.url")]
        internal static ClassificationTypeDefinition MarkdownImageUrlDefinition;

        // Miscellaneous

        [Export]
        [Name("markdown.horizontalrule")]
        [BaseDefinition("markdown")]
        internal static ClassificationTypeDefinition MarkdownHorizontalRuleDefinition;

    }
}
