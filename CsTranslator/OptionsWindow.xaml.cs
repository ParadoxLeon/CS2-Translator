using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using CsTranslator.Helpers;

namespace CsTranslator
{
    public partial class OptionsWindow
    {
    
        public string CurrentVersion
        {
            get { return Version.CurrentVersion; }
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
        }

        private void BtnSaveOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsManager.InstallationPath = TbFolderPath.Text;
            OptionsManager.Language = TbLang.Text;
            
            OptionsManager.Save();
            Close();
        }

        private void BtnSetDefault_Click(object sender, RoutedEventArgs e)
        {
            OptionsManager.SetDefault();
            LoadOptions();
        }
        
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }


}
