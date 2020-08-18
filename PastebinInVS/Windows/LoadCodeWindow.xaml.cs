using System;
using System.Windows;

using Microsoft.VisualStudio.Shell.Interop;

using PastebinApiWrapper;
using PastebinApiWrapper.Models;
using PastebinApiWrapper.Models.Enums;

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
            var pastebin = new Pastebin(Settings.Default.DeveloperApiKey);

            var r = pastebin.CreateNewPasteAsync(
                new PasteInfo
                {
                    Name = PasteName,
                    Code = PasteCode,
                    ExpireDate = PasteExpireDate.TenMinutes,
                    Language = PasteLanguage.csharp,
                    Private = PastePrivate.Public
                }).Result;

            new TextCopy.Clipboard().SetText(r.Link);  // copy link to system clipbaord 
        }
    }
}
