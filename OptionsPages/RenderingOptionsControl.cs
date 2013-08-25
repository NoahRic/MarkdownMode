namespace MarkdownMode.OptionsPages
{
    using EventArgs = System.EventArgs;
    using UserControl = System.Windows.Forms.UserControl;

    public partial class RenderingOptionsControl : UserControl
    {
        string _accessToken;

        public RenderingOptionsControl(RenderingOptions optionsPage)
        {
            InitializeComponent();
            OptionsPage = optionsPage;
            ReloadOptions();
        }

        RenderingOptions OptionsPage
        {
            get;
            set;
        }

        public string AccessToken
        {
            get
            {
                return _accessToken;
            }

            set
            {
                _accessToken = value;
                if (string.IsNullOrEmpty(value))
                    btnAuthorize.Text = "Authorize...";
                else
                    btnAuthorize.Text = "Authorized";
            }
        }

        public void ReloadOptions()
        {
            btnMarkdownSharp.Checked = !OptionsPage.RenderUsingGitHub;
            btnGitHub.Checked = OptionsPage.RenderUsingGitHub;
            AccessToken = OptionsPage.AccessToken;
        }

        public void ApplyChanges()
        {
            OptionsPage.RenderUsingGitHub = btnGitHub.Checked;
            OptionsPage.AccessToken = AccessToken;
        }

        void HandleAuthorize_Click(object sender, EventArgs e)
        {
            using (var form = new GitHubAuthorizationForm())
            {
                form.ShowDialog();
                if (!string.IsNullOrEmpty(form.Token))
                {
                    AccessToken = form.Token;
                }
            }
        }
    }
}
