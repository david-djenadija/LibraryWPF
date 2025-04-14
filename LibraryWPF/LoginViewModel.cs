using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace LibraryWPF
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _password;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }

        private void ExecuteLogin(object parameter)
        {
            string connectionString = MainWindow.connectionString;
            string query = @"
            SELECT 
                u.UserID, 
                m.MemberID,
                u.Theme,
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
                        command.Parameters.AddWithValue("@Username", Username);
                        command.Parameters.AddWithValue("@Password", Password);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Extract UserID, MemberID, and role
                                int userId = reader.GetInt32("UserID");
                                string role = reader["UserRole"]?.ToString();
                                int? memberId = reader["MemberID"] != DBNull.Value ? reader.GetInt32("MemberID") : (int?)null;
                                string theme = reader["Theme"].ToString();
                                ThemeManager.ApplyTheme(theme); // Apply the theme
                                // Store UserID and MemberID in session
                                Session.UserID = userId;
                                Session.UserRole = role;

                                if (role == "Member")
                                {
                                    Session.MemberID = memberId;
                                }

                                // Navigate to the appropriate panel
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

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Static Session class to store session-related data
    public static class Session
    {
        public static int UserID { get; set; }
        public static int? MemberID { get; set; } // Nullable for non-member users
        public static string UserRole { get; set; }
    }
}
