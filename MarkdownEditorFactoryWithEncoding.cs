namespace MarkdownMode
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("12FC9F48-AA66-4ACB-9C93-9D9715B7DCD1")]
    public class MarkdownEditorFactoryWithEncoding : MarkdownEditorFactory
    {
        public MarkdownEditorFactoryWithEncoding(MarkdownPackage package)
            : base(package, true)
        {
        }
    }
}
