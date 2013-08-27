using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;

namespace MarkdownMode
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(Margin.MarginName)]
    [MarginContainer(PredefinedMarginNames.Top)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [ContentType(ContentType.Name)]
    sealed class MarginProvider : IWpfTextViewMarginProvider
    {
        [Import]
        SVsServiceProvider GlobalServiceProvider = null;

        [Import]
        ITextDocumentFactoryService TextDocumentFactoryService = null;

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            MarkdownPackage package = null;

            // If there is a shell service (which there should be, in VS), force the markdown package to load
            IVsShell shell = GlobalServiceProvider.GetService(typeof(SVsShell)) as IVsShell;
            if (shell != null)
                package = MarkdownPackage.ForceLoadPackage(shell);

            return new Margin(wpfTextViewHost.TextView, package, TextDocumentFactoryService);
        }
    }
}
