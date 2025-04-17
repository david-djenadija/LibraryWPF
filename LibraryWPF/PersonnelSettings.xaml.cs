using MaterialDesignThemes.Wpf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LibraryWPF
{
    /// <summary>
    /// Interaction logic for PersonnelSettings.xaml
    /// </summary>
    public partial class PersonnelSettings : UserControl
    {
        public static int selectedIndex = 0;
        public PersonnelSettings()
        {
            InitializeComponent();
            SettingsNav.Background = new SolidColorBrush(Colors.LightGreen);
            if (selectedIndex == 1)LanguageChange();
            string currentTheme = ThemeManager.GetCurrentTheme();
            ThemeChangeBox.SelectedItem = ThemeChangeBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == currentTheme);
        }
        private void ThemeChangeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeChangeBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string themeName = selectedItem.Content.ToString();
                ThemeManager.ApplyTheme(themeName); // Apply theme globally
                UpdateUserTheme(Session.UserID,themeName);
            }
        }
        private void UpdateUserTheme(int userId, string theme)
        {
            string connectionString = MainWindow.connectionString;
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE User SET Theme = @Theme WHERE UserID = @UserID";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Theme", theme);
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.ExecuteNonQuery();
                }
            }
        }
        private void LanguageChange_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex = LanguageChangeBox.SelectedIndex;
            if (selectedIndex == 1) LanguageChange();
            else
            {
                if (this.Parent is ContentControl contentControl)
                {
                    contentControl.Content = new PersonnelSettings();
                }
            }
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
          "Are you sure you want to log out?",
          "Confirm Logout",
          MessageBoxButton.YesNo,
          MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Close the current MainWindow
                // Application.Current.MainWindow.Close();
                if (this.Parent is ContentControl contentControl)
                {
                    contentControl.Content = new LoginControl();
                }
            }
        }
        private void LanguageChange()
        {
            BooksNav.Content = "Knjige";
            SettingsNav.Content = "Podešavanja";
            LogoutNav.Content = "Odjava";
            HintAssist.SetHint(LanguageChangeBox, "Izaberite jezik");
            LanButton.Content = "Promjeni";
            
        }
        private void BooksNav_click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentControl)
            {
                contentControl.Content = new PersonnelPanel();
            }
        }
    }
}
