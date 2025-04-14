using MaterialDesignThemes.Wpf;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
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
    /// Interaction logic for AdminSettings.xaml
    /// </summary>
    public partial class AdminSettings : UserControl
    {
        public static int selectedIndex = 0;
        public static int LateFee = 0;
        public AdminSettings()
        {
            InitializeComponent();
            SettingsNav.Background = new SolidColorBrush(Colors.Red);
            if (selectedIndex == 1) LanguageChange();
            string currentTheme = ThemeManager.GetCurrentTheme();
            ThemeChangeBox.SelectedItem = ThemeChangeBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == currentTheme);
        }
        // Theme change event handler
        private void ThemeChangeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeChangeBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string themeName = selectedItem.Content.ToString();
                ThemeManager.ApplyTheme(themeName); // Apply theme globally
                UpdateUserTheme(Session.UserID, themeName);
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
            MembersNav.Content = "Članovi";
            BooksNav.Content = "Knjige";
            SettingsNav.Content = "Podešavanja";
            LogoutNav.Content = "Odjava";
            LanLabel.Text = "Jezik:";
            LanButton.Content = "Promjeni";
            FeeButton.Content = "Promjeni";
            HintAssist.SetHint(FeeBox, "Unesi taksu");
        }

        private void LanguageChange_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex=LanguageChangeBox.SelectedIndex;
            if(selectedIndex==1)LanguageChange();
            else
            {
                if (this.Parent is ContentControl contentControl)
                {
                    contentControl.Content = new AdminSettings();
                }
            }
        }

        private void BooksNav_click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentControl)
            {
                contentControl.Content = new BooksAdmin();
            }
        }
        private void MembersNav_click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentControl)
            {
                contentControl.Content = new AdminPanel();
            }
        }
        private void UpdateFineButton_Click(object sender, RoutedEventArgs e)
        {
            LateFee = int.Parse(FeeBox.Text);
            if (int.TryParse(FeeBox.Text, out int newFineAmount) && newFineAmount >= 0)
            {
                try
                {
                    string connectionString = MainWindow.connectionString;
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "UPDATE Settings SET FineAmount = @FineAmount LIMIT 1";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@FineAmount", newFineAmount);
                            command.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Fine amount updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating fine amount: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid fine amount.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        
    }

}
