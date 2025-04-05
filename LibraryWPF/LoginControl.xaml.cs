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
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {
        public LoginControl()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = MainWindow.connectionString;
            string query = @"
        SELECT 
            u.UserID, 
            CASE 
                WHEN a.AdminID IS NOT NULL THEN 'Admin'
                WHEN m.MemberID IS NOT NULL THEN 'Member'
                WHEN p.PersonnelID IS NOT NULL THEN 'Personnel'
                ELSE NULL
            END AS UserRole
        FROM `User` u
        LEFT JOIN Admin a ON u.UserID = a.UserID
        LEFT JOIN Member m ON u.UserID = m.UserID
        LEFT JOIN Personnel p ON u.UserID = p.UserID
        WHERE u.Username = @Username AND u.PasswordHash = @Password";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username_text.Text);
                        command.Parameters.AddWithValue("@Password", password_text.Password);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string role = reader["UserRole"]?.ToString();

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    var mainWindow = Application.Current.MainWindow;

                                    if (mainWindow != null)
                                    {
                                        switch (role)
                                        {
                                            case "Admin":
                                                mainWindow.Content = new AdminPanel();
                                                break;
                                            case "Member":
                                                mainWindow.Content = new MemberPanel();
                                                break;
                                            case "Personnel":
                                                mainWindow.Content = new PersonnelPanel();
                                                break;
                                            default:
                                                MessageBox.Show("User role not recognized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                                break;
                                        }
                                    }
                                });
                            }
                            else
                            {
                                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
