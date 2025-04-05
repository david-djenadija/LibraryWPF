using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Media;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace LibraryWPF
{
    public static class ThemeManager
    {
        private static readonly PaletteHelper _paletteHelper = new PaletteHelper();

        public static void ApplyTheme(string themeName)
        {
            Theme theme = _paletteHelper.GetTheme();

            switch (themeName)
            {
                case "Light":
                    theme.SetBaseTheme(BaseTheme.Light);
                    theme.SetPrimaryColor(Colors.Blue);
                    theme.SetSecondaryColor(Colors.Lime);
                    break;

                case "Dark":
                    theme.SetBaseTheme(BaseTheme.Dark);
                    theme.SetPrimaryColor(Colors.Teal);
                    theme.SetSecondaryColor(Colors.Orange);
                    break;

                case "Deep Purple":
                    theme.SetBaseTheme(BaseTheme.Dark);
                    theme.SetPrimaryColor(Color.FromRgb(103, 58, 183)); // Deep Purple
                    theme.SetSecondaryColor(Color.FromRgb(255, 64, 129)); // Pink accent
                    break;

                default:
                    theme.SetBaseTheme(BaseTheme.Light); // Fallback
                    break;
            }

            _paletteHelper.SetTheme(theme); // Apply the theme globally
        }

        public static string GetCurrentTheme()
        {
            Theme theme = _paletteHelper.GetTheme();
            if (theme.GetBaseTheme() == BaseTheme.Light)
                return "Light";
            else if (theme.PrimaryMid.Color == Color.FromRgb(103, 58, 183))
                return "Deep Purple";
            else
                return "Dark";
        }
    }
}