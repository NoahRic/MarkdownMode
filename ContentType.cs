using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace MarkdownMode
{
    static class ContentType
    {
        public const string Name = "markdown";

        [Export]
        [Name(Name)]
        [DisplayName("Markdown")]
        [BaseDefinition("HTML")]
        public static ContentTypeDefinition MarkdownModeContentType = null;

        // Pre-RTM workaround:
        // We need to also explicitly declare HTML, or else our declaration will cause HTML to break
        [Export]
        [Name("HTML")]
        [BaseDefinition("code")]
        public static ContentTypeDefinition HTMLContentType = null;

        [Export]
        [ContentType(Name)]
        [FileExtension(".mkd")]
        public static FileExtensionToContentTypeDefinition MkdFileExtension = null;

        [Export]
        [ContentType(Name)]
        [FileExtension(".md")]
        public static FileExtensionToContentTypeDefinition MdFileExtension = null;

        [Export]
        [ContentType(Name)]
        [FileExtension(".mdown")]
        public static FileExtensionToContentTypeDefinition MdownFileExtension = null;

        [Export]
        [ContentType(Name)]
        [FileExtension(".mkdn")]
        public static FileExtensionToContentTypeDefinition MkdnFileExtension = null;

        [Export]
        [ContentType(Name)]
        [FileExtension(".markdown")]
        public static FileExtensionToContentTypeDefinition MarkdownFileExtension = null;
    }
}
