using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace PastebinInVS.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoadCodeWindow.xaml
    /// </summary>
    public partial class LoadCodeWindow : Window
    {
        #region Properties
        /// <summary>
        /// Name of the future paste
        /// </summary>
        public string PasteName { get; set; }
        /// <summary>
        /// Code of the future paste
        /// </summary>
        public string PasteCode { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Load window constructor
        /// </summary>
        /// <param name="pasteName">Paste name</param>
        /// <param name="pasteCode">Paste code</param>
        public LoadCodeWindow(string pasteName, string pasteCode)
        {
            PasteName = pasteName;
            PasteCode = pasteCode;
            InitializeComponent();
        }
        /// <summary>
        /// Load window constructor
        /// </summary>
        public LoadCodeWindow() : this("", "") { }
        #endregion


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CodeTextBox.Text = PasteCode;
            PasteNameTextBox.Text = PasteName;
            PasteSendDateTextBox.Text = DateTime.Now.ToString();
            PasteExpireTextBox.Text = Settings.Default.PasteExpireTime;
            PasteLanguageTextBox.Text = Settings.Default.PasteLanguage;
            PastePrivateTextBox.Text = Settings.Default.PastePrivate.ToString();
        }

        private void SendCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            _ = PastebinPasteRequestAsync(
                    Settings.Default.DeveloperApiKey,
                    PastePrivateTextBox.Text,
                    PasteNameTextBox.Text,
                    PasteExpireTextBox.Text,
                    PasteLanguageTextBox.Text,
                    CodeTextBox.Text);
        }

        /// <summary>
        /// Paste selected code to pastebin.com
        /// </summary>
        /// <param name="name">Paste name</param>
        /// <param name="code">Paste code</param>
        private async Task PastebinPasteRequestAsync(string devKey, string pastePrivate, string pasteName, string pasteExpireDate, string pasteLanguage, string pasteCode, string userKey = "")
        {
            var baseAddress = new Uri("https://pastebin.com/api/api_post.php");
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("api_option", "paste"),
                    new KeyValuePair<string, string>("api_user_key", userKey),
                    new KeyValuePair<string, string>("api_paste_private", pastePrivate),
                    new KeyValuePair<string, string>("api_paste_name", pasteName),
                    new KeyValuePair<string, string>("api_paste_expire_date", pasteExpireDate),
                    new KeyValuePair<string, string>("api_paste_format", pasteLanguage),
                    new KeyValuePair<string, string>("api_dev_key", devKey),
                    new KeyValuePair<string, string>("api_paste_code", pasteCode)
                });

                cookieContainer.Add(baseAddress, new Cookie("CookieName", "cookie_value"));
                var result = await client.PostAsync(baseAddress, content).ConfigureAwait(false);

                if (result.IsSuccessStatusCode)
                {
                    var clipboard = new TextCopy.Clipboard();
                    clipboard.SetText(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync().ConfigureAwait(false));

                    MessageBox.Show(await clipboard.GetTextAsync().ConfigureAwait(false), "Request result");
                }
            }
        }
    }
}
