using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace MarkdownMode
{
    static class ContentType
    {
        [Export]
        [Name("markdown")]
        [DisplayName("Markdown")]
        [BaseDefinition("text")]
        public static ContentTypeDefinition MarkdownModeContentType = null;

        [Export]
        [ContentType("markdown")]
        [FileExtension(".mkd")]
        public static FileExtensionToContentTypeDefinition MkdFileExtension = null;

        [Export]
        [ContentType("markdown")]
        [FileExtension(".markdown")]
        public static FileExtensionToContentTypeDefinition MarkdownFileExtension = null;
    }
}
