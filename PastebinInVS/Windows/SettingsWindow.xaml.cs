using System.Windows;

namespace PastebinInVS.Windows
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.DeveloperApiKey = DevApiKey.Text;
            Settings.Default.UserApiKey = UserApiKey.Text;
            Settings.Default.PasteExpireTime = PasteExpireTime.Text;
            Settings.Default.PasteLanguage = PasteLanguage.Text;
            Settings.Default.PastePrivate = int.Parse(PastePrivate.Text);

            Settings.Default.Save();
            Settings.Default.Reload();

            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DevApiKey.Text = Settings.Default.DeveloperApiKey;
            UserApiKey.Text = Settings.Default.UserApiKey;
            PasteExpireTime.Text = Settings.Default.PasteExpireTime;
            PasteLanguage.Text = Settings.Default.PasteLanguage;
            PastePrivate.Text = Settings.Default.PastePrivate.ToString();
        }
    }
}
