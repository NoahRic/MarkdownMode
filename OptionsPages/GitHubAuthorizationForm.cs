namespace MarkdownMode.OptionsPages
{
    using EventArgs = System.EventArgs;
    using Form = System.Windows.Forms.Form;
    using HttpUtility = System.Web.HttpUtility;
    using HttpWebRequest = System.Net.HttpWebRequest;
    using NameValueCollection = System.Collections.Specialized.NameValueCollection;
    using StreamReader = System.IO.StreamReader;
    using Uri = System.Uri;
    using UriTemplate = System.UriTemplate;
    using WebBrowserNavigatedEventArgs = System.Windows.Forms.WebBrowserNavigatedEventArgs;
    using WebBrowserNavigatingEventArgs = System.Windows.Forms.WebBrowserNavigatingEventArgs;

    public partial class GitHubAuthorizationForm : Form
    {
        static readonly string ClientId = "21646e88c7006f9bbdbb";
        static readonly string ClientSecret = "29c29fb3d1442e93a9a6b79496269b08d6c046e2";
        static readonly string[] Scopes = { };

        static readonly Uri BaseAddress = new Uri("https://github.com");
        static readonly UriTemplate AuthorizationUriTemplate = new UriTemplate("login/oauth/authorize?client_id={client_id}&scope={scope}");
        static readonly UriTemplate RequestTokenUriTemplate = new UriTemplate("/login/oauth/access_token?client_id={client_id}&client_secret={client_secret}&code={code}");

        public GitHubAuthorizationForm()
        {
            InitializeComponent();
        }

        internal string Token
        {
            get;
            private set;
        }

        protected override void OnLoad(EventArgs e)
        {
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.CausesValidation = false;

            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("client_id", ClientId);
            parameters.Add("scope", string.Join(",", Scopes));
            Uri uri = AuthorizationUriTemplate.BindByName(BaseAddress, parameters);

            webBrowser1.Navigate(uri);
        }

        void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            CheckAuthorization(e.Url);
        }

        void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            CheckAuthorization(e.Url);
        }

        void CheckAuthorization(Uri uri)
        {
            NameValueCollection parameters = HttpUtility.ParseQueryString(uri.Query);
            string code = parameters["code"];
            if (string.IsNullOrEmpty(code))
                return;

            Hide();
            Close();

            NameValueCollection requestTokenParameters = new NameValueCollection();
            requestTokenParameters.Add("client_id", ClientId);
            requestTokenParameters.Add("client_secret", ClientSecret);
            requestTokenParameters.Add("code", code);
            Uri requestTokenUri = RequestTokenUriTemplate.BindByName(BaseAddress, requestTokenParameters);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestTokenUri);
            request.UserAgent = Resources.GitHubUserAgent;
            request.Method = "POST";
            using (var response = request.GetResponse())
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string accessParameters = reader.ReadToEnd();
                NameValueCollection accessTokenResponseParameters = HttpUtility.ParseQueryString(accessParameters);
                Token = accessTokenResponseParameters["access_token"];
            }
        }
    }
}
