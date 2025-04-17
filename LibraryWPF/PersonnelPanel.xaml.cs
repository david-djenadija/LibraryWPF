using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows.Media;

namespace LibraryWPF
{
    public partial class PersonnelPanel : UserControl
    {
        private string connectionString = MainWindow.connectionString;
        int activeMembersCount = 0;
        int rentedBooksCount = 0;

        public PersonnelPanel()
        {
            InitializeComponent();
            LoadBooks();
            UpdateLabels();
            BooksNav.Background = new SolidColorBrush(Colors.LightGreen);
            if (PersonnelSettings.selectedIndex == 1) changeLanguage();
        }

        private void changeLanguage()
        {
            BooksNav.Content = "Knjige";
            SettingsNav.Content = "Podešavanja";
            LogoutNav.Content = "Odjava";
            SearchButton.Content = "Pretraži";
            NumOfActiveMembersLabel.Text = "Broj aktivnih članova: " + activeMembersCount;
            NumOfRentedBooksLabel.Text="Broj podignutih knjiga: " + rentedBooksCount;
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentControl)
            {
                contentControl.Content = new PersonnelSettings();
            }
        }
        private void UpdateLabels()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Query to get the number of active members
                    string activeMembersQuery = "SELECT COUNT(*) FROM Member WHERE MembershipEndDate IS NULL OR MembershipEndDate >= CURDATE();";

                    // Query to get the number of rented books
                    string rentedBooksQuery = "SELECT COUNT(*) FROM Rental WHERE ReturnDate IS NULL;";

                    

                    using (MySqlCommand command = new MySqlCommand(activeMembersQuery, connection))
                    {
                        activeMembersCount = Convert.ToInt32(command.ExecuteScalar());
                    }

                    using (MySqlCommand command = new MySqlCommand(rentedBooksQuery, connection))
                    {
                        rentedBooksCount = Convert.ToInt32(command.ExecuteScalar());
                    }

                    // Update the labels
                    NumOfActiveMembersLabel.Text = $"Number of active members: {activeMembersCount}";
                    NumOfRentedBooksLabel.Text = $"Number of rented books: {rentedBooksCount}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating the labels: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadBooks(string searchQuery = null)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Book";
                    if (!string.IsNullOrEmpty(searchQuery))
                    {
                        query += " WHERE Title LIKE @SearchQuery OR PublicationYear LIKE @SearchQuery";
                    }

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(searchQuery))
                        {
                            command.Parameters.AddWithValue("@SearchQuery", "%" + searchQuery + "%");
                        }

                        MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        BooksDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading books: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = SearchBox.Text;
            LoadBooks(searchQuery);
        }

        private void BooksDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (BooksDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)BooksDataGrid.SelectedItem;
                int bookId = Convert.ToInt32(selectedRow["BookID"]);

                BookDetailsWindow bookDetailsWindow = new BookDetailsWindow(bookId);
                bookDetailsWindow.Show();
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
    }
}
