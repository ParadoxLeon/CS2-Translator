using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using CsTranslator.Helpers;

namespace CsTranslator
{
    /**
     * <summary>
     *Interaction logic for OptionsWindow.xaml
     * </summary>
     * commented strings related to telnet
     */
    public partial class OptionsWindow
    {
    
        public string CurrentVersion
        {
            get { return Version.CurrentVersion; }
        }

        private bool IgnoreOwnMessages
        {
            get => CbIgnoreOwnMessages.IsChecked != null && (bool) CbIgnoreOwnMessages.IsChecked;
            set => CbIgnoreOwnMessages.IsChecked = value;
        }
        private string OwnUsername
        {
            get => TbOwnUsername.Text.Trim();
            set
            {
                TbOwnUsername.Text = value;

                if (!string.IsNullOrEmpty(value))
                {
                    CbIgnoreOwnMessages.IsEnabled = true;
                }
                else
                {
                    CbIgnoreOwnMessages.IsEnabled = false;
                    CbIgnoreOwnMessages.IsChecked = false;
                }
            }
        }
        
        public OptionsWindow()
        {
            InitializeComponent();
            LoadOptions();
            DataContext = this;
        }

        private void LoadOptions()
        {
            TbFolderPath.Text = OptionsManager.InstallationPath;
            TbLang.Text = OptionsManager.Language;
            OwnUsername = OptionsManager.OwnUsername;
            IgnoreOwnMessages = OptionsManager.IgnoreOwnMessages;
        }

        private void BtnSaveOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsManager.InstallationPath = TbFolderPath.Text;
            OptionsManager.Language = TbLang.Text;
            OptionsManager.OwnUsername = OwnUsername;
            OptionsManager.IgnoreOwnMessages = IgnoreOwnMessages; 
            
            OptionsManager.Save();
            Close();
        }

        private void BtnSetDefault_Click(object sender, RoutedEventArgs e)
        {
            OptionsManager.SetDefault();
            LoadOptions();
        }
        
        private void TbTelnetPort_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("^[0-9]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void TbOwnUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox) sender).Text.Length > 0)
            {
                CbIgnoreOwnMessages.IsEnabled = true;
            }
            else
            {
                CbIgnoreOwnMessages.IsEnabled = false;
                CbIgnoreOwnMessages.IsChecked = false;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

    }
}
