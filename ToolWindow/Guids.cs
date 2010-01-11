using System;

namespace MarkdownMode
{
    static class GuidList
    {
        public const string guidMarkdownPackagePkgString = "06fa6fbc-681e-4fdc-bd58-81e8401c5e00";
        public const string guidMarkdownPackageCmdSetString = "36c5243f-48c6-4666-bc21-8c398b312f0f";
        public const string guidToolWindowPersistanceString = "acd82a5f-9c35-400b-b9d0-f97925f3b312";

        public static readonly Guid guidMarkdownPackageCmdSet = new Guid(guidMarkdownPackageCmdSetString);
    };

    static class PkgCmdId
    {
        public const int cmdidMarkdownPreviewWindow = 0x0101;
    }
}