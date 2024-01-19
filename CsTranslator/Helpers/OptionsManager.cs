using CsTranslator.Enums;

namespace CsTranslator.Helpers
{
    internal static class OptionsManager
    {
        public static string InstallationPath
        {
            get => Properties.Settings.Default.Path;
            set => Properties.Settings.Default.Path = value;
        }
        public static string Language
        {
            get => Properties.Settings.Default.Lang;
            set => Properties.Settings.Default.Lang = value;
        }
        public static string OwnUsername
        {
            get => Properties.Settings.Default.OwnUsername;
            set => Properties.Settings.Default.OwnUsername = value;
        }
        public static bool IgnoreOwnMessages
        {
            get => Properties.Settings.Default.IgnoreOwnMessages;
            set => Properties.Settings.Default.IgnoreOwnMessages = value;
        }

        public static void ValidateSettings()
        {
            if (InstallationPath.Length == 0 || Language.Length == 0)
                SetDefault();
        }

        public static void SetDefault()
        {
            Language = "en";
            InstallationPath = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive";
            OwnUsername = "";
            IgnoreOwnMessages = false;
           
            Save();
        }

        public static void Save()
        {
            Properties.Settings.Default.Save();
        }
    }
}
