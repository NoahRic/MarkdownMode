using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio;

namespace MarkdownMode
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0")]
    [ProvideMenuResource(1000, 1)]
    [ProvideToolWindow(typeof(MarkdownPreviewToolWindow))]
    [Guid(GuidList.guidMarkdownPackagePkgString)]
    public sealed class MarkdownPackage : Package, IMarkdownPreviewWindowBroker
    {
        public static bool ForceLoadPackage(IVsShell shell)
        {
            Guid packageGuid = new Guid(GuidList.guidMarkdownPackagePkgString);
            IVsPackage package;

            int hr = shell.IsPackageLoaded(ref packageGuid, out package);

            if (hr == VSConstants.S_OK)
                return true;
            else
                return ErrorHandler.Succeeded(shell.LoadPackage(ref packageGuid, out package));
        }

        public MarkdownPackage()
        {
            Trace.WriteLine("Loaded MarkdownPackage.");
        }

        private void ShowToolWindow(object sender, EventArgs e)
        {
            var window = GetToolWindow(true);

            if (window != null)
            {
                IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
        }

        internal MarkdownPreviewToolWindow GetToolWindow(bool create)
        {
            var window = this.FindToolWindow(typeof(MarkdownPreviewToolWindow), 0, create);
            if ((null == window) || (null == window.Frame))
            {
                return null;
            }

            return window as MarkdownPreviewToolWindow;
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            IMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (mcs != null)
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidMarkdownPackageCmdSet, PkgCmdId.cmdidMarkdownPreviewWindow);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );
            }

            IComponentModel componentModel = GetService(typeof(SComponentModel)) as IComponentModel;
            if (componentModel != null)
            {
                var brokerService = componentModel.GetService<IMarkdownPreviewWindowBrokerService>();
                if (brokerService != null)
                    brokerService.RegisterPreviewWindowBroker(this);
            }
        }
        #endregion

        public MarkdownPreviewToolWindow GetMarkdownPreviewToolWindow(bool create)
        {
            return GetToolWindow(create);
        }
    }
}
