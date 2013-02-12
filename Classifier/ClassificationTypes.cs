using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace MarkdownMode
{
    static class MarkdownClassificationTypes
    {
        // Base definition for all other classification types

        [Export]
        [Name(ClassificationTypeNames.Markdown)]
        internal static ClassificationTypeDefinition MarkdownClassificationDefinition = null;

        // Bold/italics

        [Export]
        [Name(ClassificationTypeNames.Italics)]
        [BaseDefinition(ClassificationTypeNames.Markdown)]
        internal static ClassificationTypeDefinition MarkdownItalicsDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.Bold)]
        [BaseDefinition(ClassificationTypeNames.Markdown)]
        internal static ClassificationTypeDefinition MarkdownBoldDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.BoldItalics)]
        [BaseDefinition(ClassificationTypeNames.Bold)]
        [BaseDefinition(ClassificationTypeNames.Italics)]
        internal static ClassificationTypeDefinition MarkdownBoldItalicsDefinition = null;

        // Headers

        [Export]
        [Name(ClassificationTypeNames.Header)]
        [BaseDefinition(ClassificationTypeNames.Markdown)]
        internal static ClassificationTypeDefinition MarkdownHeaderDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.HeaderH1)]
        [BaseDefinition(ClassificationTypeNames.Header)]
        internal static ClassificationTypeDefinition MarkdownH1Definition = null;

        [Export]
        [Name(ClassificationTypeNames.HeaderH2)]
        [BaseDefinition(ClassificationTypeNames.Header)]
        internal static ClassificationTypeDefinition MarkdownH2Definition = null;

        [Export]
        [Name(ClassificationTypeNames.HeaderH3)]
        [BaseDefinition(ClassificationTypeNames.Header)]
        internal static ClassificationTypeDefinition MarkdownH3Definition = null;

        [Export]
        [Name(ClassificationTypeNames.HeaderH4)]
        [BaseDefinition(ClassificationTypeNames.Header)]
        internal static ClassificationTypeDefinition MarkdownH4Definition = null;

        [Export]
        [Name(ClassificationTypeNames.HeaderH5)]
        [BaseDefinition(ClassificationTypeNames.Header)]
        internal static ClassificationTypeDefinition MarkdownH5Definition = null;

        [Export]
        [Name(ClassificationTypeNames.HeaderH6)]
        [BaseDefinition(ClassificationTypeNames.Header)]
        internal static ClassificationTypeDefinition MarkdownH6Definition = null;

        // Lists

        [Export]
        [Name(ClassificationTypeNames.List)]
        [BaseDefinition(ClassificationTypeNames.Markdown)]
        internal static ClassificationTypeDefinition MarkdownListDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.ListUnordered)]
        [BaseDefinition(ClassificationTypeNames.List)]
        internal static ClassificationTypeDefinition MarkdownUnorderedListDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.ListOrdered)]
        [BaseDefinition(ClassificationTypeNames.List)]
        internal static ClassificationTypeDefinition MarkdownOrderedListDefinition = null;

        // Code/pre

        [Export]
        [Name(ClassificationTypeNames.Preformatted)]
        [BaseDefinition(ClassificationTypeNames.Markdown)]
        internal static ClassificationTypeDefinition MarkdownPreDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.Block)]
        [BaseDefinition(ClassificationTypeNames.Preformatted)]
        internal static ClassificationTypeDefinition MarkdownCodeDefinition = null;

        // Quotes

        [Export]
        [Name(ClassificationTypeNames.BlockQuote)]
        [BaseDefinition(ClassificationTypeNames.Markdown)]
        internal static ClassificationTypeDefinition MarkdownBlockquoteDefinition = null;

        // Links

        [Export]
        [Name(ClassificationTypeNames.Label)]
        [BaseDefinition(ClassificationTypeNames.Markdown)]
        internal static ClassificationTypeDefinition MarkdownLabel = null;

        [Export]
        [Name(ClassificationTypeNames.Link)]
        [BaseDefinition(ClassificationTypeNames.Markdown)]
        internal static ClassificationTypeDefinition MarkdownLinkDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.LinkText)]
        [BaseDefinition(ClassificationTypeNames.Link)]
        internal static ClassificationTypeDefinition MarkdownLinkTextDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.LinkTitle)]
        [BaseDefinition(ClassificationTypeNames.Link)]
        internal static ClassificationTypeDefinition MarkdownLinkTitleDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.LinkLabel)]
        [BaseDefinition(ClassificationTypeNames.Label)]
        [BaseDefinition(ClassificationTypeNames.Link)]
        internal static ClassificationTypeDefinition MarkdownLinkLabelDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.LinkPunctuation)]
        [BaseDefinition(ClassificationTypeNames.Link)]
        internal static ClassificationTypeDefinition MarkdownLinkPunctuationDefinition = null;

        // Link URLs
        
        [Export]
        [Name(ClassificationTypeNames.Url)]
        [BaseDefinition(ClassificationTypeNames.Markdown)]
        internal static ClassificationTypeDefinition MarkdownLinkUrlDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.UrlInline)]
        [BaseDefinition(ClassificationTypeNames.Url)]
        internal static ClassificationTypeDefinition MarkdownInlineLinkDefinition = null;
        
        [Export]
        [Name(ClassificationTypeNames.UrlDefinition)]
        [BaseDefinition(ClassificationTypeNames.Url)]
        internal static ClassificationTypeDefinition MarkdownDefinitionLinkDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.UrlAutomatic)]
        [BaseDefinition(ClassificationTypeNames.Url)]
        internal static ClassificationTypeDefinition MarkdownAutomaticLinkDefinition = null;

        // Images

        [Export]
        [Name(ClassificationTypeNames.Image)]
        [BaseDefinition(ClassificationTypeNames.Markdown)]
        internal static ClassificationTypeDefinition MarkdownImageDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.ImageAlt)]
        [BaseDefinition(ClassificationTypeNames.Image)]
        internal static ClassificationTypeDefinition MarkdownImageAltDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.ImageLabel)]
        [BaseDefinition(ClassificationTypeNames.Label)]
        [BaseDefinition(ClassificationTypeNames.Image)]
        internal static ClassificationTypeDefinition MarkdownImageLabelDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.ImageTitle)]
        [BaseDefinition(ClassificationTypeNames.Image)]
        internal static ClassificationTypeDefinition MarkdownImageTitleDefinition = null;

        [Export]
        [Name(ClassificationTypeNames.UrlImage)]
        [BaseDefinition(ClassificationTypeNames.Image)]
        [BaseDefinition(ClassificationTypeNames.Url)]
        internal static ClassificationTypeDefinition MarkdownImageUrlDefinition = null;

        // Miscellaneous

        [Export]
        [Name(ClassificationTypeNames.HorizontalRule)]
        [BaseDefinition(ClassificationTypeNames.Markdown)]
        internal static ClassificationTypeDefinition MarkdownHorizontalRuleDefinition = null;

    }
}
