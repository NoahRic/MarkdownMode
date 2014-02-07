namespace MarkdownMode
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("9BBB2E49-A2F3-4ED3-A1FB-40EBC986C83B")]
    public class MarkdownEditorFactoryWithoutEncoding : MarkdownEditorFactory
    {
        public MarkdownEditorFactoryWithoutEncoding(MarkdownPackage package)
            : base(package, false)
        {
        }
    }
}
