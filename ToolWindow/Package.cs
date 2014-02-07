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
using RenderingOptions = MarkdownMode.OptionsPages.RenderingOptions;

namespace MarkdownMode
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0")]
    [ProvideMenuResource(1000, 1)]
    [ProvideToolWindow(typeof(MarkdownPreviewToolWindow))]

    [ProvideLanguageService(typeof(MarkdownLanguageInfo), MarkdownLanguageInfo.LanguageName, MarkdownLanguageInfo.LanguageResourceId,
        DefaultToInsertSpaces = true,
        EnableLineNumbers = true,
        RequestStockColors = true)]
    [ProvideLanguageExtension(typeof(MarkdownLanguageInfo), ".mkd")]
    [ProvideLanguageExtension(typeof(MarkdownLanguageInfo), ".md")]
    [ProvideLanguageExtension(typeof(MarkdownLanguageInfo), ".mdown")]
    [ProvideLanguageExtension(typeof(MarkdownLanguageInfo), ".mkdn")]
    [ProvideLanguageExtension(typeof(MarkdownLanguageInfo), ".markdown")]
    [ProvideLanguageEditorOptionPage(typeof(RenderingOptions), MarkdownLanguageInfo.LanguageName, "", "Rendering", "#410")]

    [Guid(GuidList.guidMarkdownPackagePkgString)]
    public sealed class MarkdownPackage : Package
    {
        static MarkdownPackage _instance;
        MarkdownLanguageInfo _languageInfo;

        public static MarkdownPackage ForceLoadPackage(IVsShell shell)
        {
            Guid packageGuid = new Guid(GuidList.guidMarkdownPackagePkgString);
            IVsPackage package;

            if (VSConstants.S_OK == shell.IsPackageLoaded(ref packageGuid, out package))
                return package as MarkdownPackage;
            else if (ErrorHandler.Succeeded(shell.LoadPackage(ref packageGuid, out package)))
                return package as MarkdownPackage;

            return null;
        }

        public MarkdownPackage()
        {
            Trace.WriteLine("Loaded MarkdownPackage.");
            _instance = this;
        }

        public static MarkdownPackage Instance
        {
            get
            {
                return _instance;
            }
        }

        public RenderingOptions RenderingOptions
        {
            get
            {
                return GetDialogPage<RenderingOptions>();
            }
        }

        T GetDialogPage<T>()
            where T : DialogPage
        {
            return (T)base.GetDialogPage(typeof(T));
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

            // register the language service
            _languageInfo = new MarkdownLanguageInfo(new VsServiceProviderWrapper(this));
            ((IServiceContainer)this).AddService(typeof(MarkdownLanguageInfo), _languageInfo, true);

            // Add our command handlers for menu (commands must exist in the .vsct file)
            IMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (mcs != null)
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidMarkdownPackageCmdSet, PkgCmdId.cmdidMarkdownPreviewWindow);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_languageInfo != null)
                {
                    _languageInfo.Dispose();
                    _languageInfo = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        public MarkdownPreviewToolWindow GetMarkdownPreviewToolWindow(bool create)
        {
            return GetToolWindow(create);
        }
    }
}
