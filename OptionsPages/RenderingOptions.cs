namespace MarkdownMode.OptionsPages
{
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using DialogPage = Microsoft.VisualStudio.Shell.DialogPage;
    using IWin32Window = System.Windows.Forms.IWin32Window;
    using Point = System.Drawing.Point;

    [Guid("58C664D6-B068-41FD-9787-D55E408027D1")]
    public class RenderingOptions : DialogPage
    {
        public RenderingOptions()
        {
            // initialize to default values
            RenderUsingGitHub = false;
        }

        [DefaultValue(false)]
        public bool RenderUsingGitHub
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public string AccessToken
        {
            get;
            set;
        }

        RenderingOptionsControl OptionsControl
        {
            get;
            set;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get
            {
                OptionsControl = new RenderingOptionsControl(this);
                OptionsControl.Location = new Point(0, 0);
                return OptionsControl;
            }
        }

        public override void SaveSettingsToStorage()
        {
            if (OptionsControl != null)
                OptionsControl.ApplyChanges();

            base.SaveSettingsToStorage();
        }
    }
}
