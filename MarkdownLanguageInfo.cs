namespace MarkdownMode
{
    using System;
    using System.Runtime.InteropServices;
    using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
    using IConnectionPointContainer = Microsoft.VisualStudio.OLE.Interop.IConnectionPointContainer;
    using IVsCodeWindow = Microsoft.VisualStudio.TextManager.Interop.IVsCodeWindow;
    using IVsCodeWindowManager = Microsoft.VisualStudio.TextManager.Interop.IVsCodeWindowManager;
    using IVsColorizer = Microsoft.VisualStudio.TextManager.Interop.IVsColorizer;
    using IVsEnumBSTR = Microsoft.VisualStudio.TextManager.Interop.IVsEnumBSTR;
    using IVsEnumDebugName = Microsoft.VisualStudio.TextManager.Interop.IVsEnumDebugName;
    using IVsLanguageDebugInfo = Microsoft.VisualStudio.TextManager.Interop.IVsLanguageDebugInfo;
    using IVsLanguageInfo = Microsoft.VisualStudio.TextManager.Interop.IVsLanguageInfo;
    using IVsTextBuffer = Microsoft.VisualStudio.TextManager.Interop.IVsTextBuffer;
    using IVsTextLines = Microsoft.VisualStudio.TextManager.Interop.IVsTextLines;
    using IVsTextManager2 = Microsoft.VisualStudio.TextManager.Interop.IVsTextManager2;
    using IVsTextManagerEvents2 = Microsoft.VisualStudio.TextManager.Interop.IVsTextManagerEvents2;
    using LANGPREFERENCES2 = Microsoft.VisualStudio.TextManager.Interop.LANGPREFERENCES2;
    using SVsServiceProvider = Microsoft.VisualStudio.Shell.SVsServiceProvider;
    using SVsTextManager = Microsoft.VisualStudio.TextManager.Interop.SVsTextManager;
    using TextSpan = Microsoft.VisualStudio.TextManager.Interop.TextSpan;
    using VSConstants = Microsoft.VisualStudio.VSConstants;

    [Guid("986C3927-0E8B-4E7B-B26D-DAF18F78F2C0")]
    class MarkdownLanguageInfo : IVsLanguageInfo, IVsLanguageDebugInfo, IDisposable
    {
        /* The language name (used for the language service) and content type must be the same
         * due to the way Visual Studio internally registers file extensions and content types.
         */
        internal const string LanguageName = ContentType.Name;
        internal const int LanguageResourceId = 100;
        static readonly string[] FileExtensions = { ".mkd", ".md", ".mdown", ".mkdn", ".markdown" };

        readonly SVsServiceProvider _serviceProvider;
        LanguagePreferences _languagePreferences;
        IDisposable _languagePreferencesCookie;

        public MarkdownLanguageInfo(SVsServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            IVsTextManager2 textManager = (IVsTextManager2)serviceProvider.GetService(typeof(SVsTextManager));
            LANGPREFERENCES2[] preferences = new LANGPREFERENCES2[1];
            preferences[0].guidLang = typeof(MarkdownLanguageInfo).GUID;
            ErrorHandler.ThrowOnFailure(textManager.GetUserPreferences2(null, null, preferences, null));
            _languagePreferences = CreateLanguagePreferences(preferences[0]);
            _languagePreferencesCookie = ((IConnectionPointContainer)textManager).Advise<LanguagePreferences, IVsTextManagerEvents2>(_languagePreferences);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_languagePreferencesCookie != null)
                {
                    _languagePreferencesCookie.Dispose();
                    _languagePreferencesCookie = null;
                }
            }
        }

        public int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr)
        {
            // will need to implement this method to support type and member dropdown bars
            ppCodeWinMgr = null;
            return VSConstants.E_FAIL;
        }

        public int GetColorizer(IVsTextLines pBuffer, out IVsColorizer ppColorizer)
        {
            ppColorizer = null;
            return VSConstants.E_FAIL;
        }

        public int GetFileExtensions(out string pbstrExtensions)
        {
            pbstrExtensions = string.Join(";", FileExtensions);
            return VSConstants.S_OK;
        }

        public int GetLanguageName(out string bstrName)
        {
            bstrName = LanguageName;
            return VSConstants.S_OK;
        }

        protected virtual LanguagePreferences CreateLanguagePreferences(LANGPREFERENCES2 preferences)
        {
            return new LanguagePreferences(preferences);
        }

        public int GetLanguageID(IVsTextBuffer pBuffer, int iLine, int iCol, out Guid pguidLanguageID)
        {
            pguidLanguageID = typeof(MarkdownLanguageInfo).GUID;
            return VSConstants.S_OK;
        }

        public int GetLocationOfName(string pszName, out string pbstrMkDoc, TextSpan[] pspanLocation)
        {
            pbstrMkDoc = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetNameOfLocation(IVsTextBuffer pBuffer, int iLine, int iCol, out string pbstrName, out int piLineOffset)
        {
            pbstrName = null;
            piLineOffset = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int GetProximityExpressions(IVsTextBuffer pBuffer, int iLine, int iCol, int cLines, out IVsEnumBSTR ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int IsMappedLocation(IVsTextBuffer pBuffer, int iLine, int iCol)
        {
            return VSConstants.S_FALSE;
        }

        public int ResolveName(string pszName, uint dwFlags, out IVsEnumDebugName ppNames)
        {
            ppNames = null;
            return VSConstants.E_NOTIMPL;
        }

        public int ValidateBreakpointLocation(IVsTextBuffer pBuffer, int iLine, int iCol, TextSpan[] pCodeSpan)
        {
            return VSConstants.E_NOTIMPL;
        }
    }
}
