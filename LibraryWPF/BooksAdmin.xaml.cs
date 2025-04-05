using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MySql.Data.MySqlClient;

namespace LibraryWPF
{
    public partial class BooksAdmin : UserControl
    {
        private string _connectionString = "Server=localhost;Database=library_hci;Uid=root;Pwd=root;";

        public BooksAdmin()
        {
            InitializeComponent();
            LoadBooks();
            BooksNav.Background= new SolidColorBrush(Colors.Red);
            if (AdminSettings.selectedIndex == 1)
                changeToSerbian();
        }
        private void changeToSerbian()
        {
            MembersNav.Content = "Članovi";
            BooksNav.Content = "Knjige";
            SettingsNav.Content = "Podešavanja";
            LogoutNav.Content = "Odjava";
            CreateButton.Content = "Dodaj";
            UpdateButton.Content = "Ažuriraj";
            DeleteButton.Content = "Obriši";
        }

        private void LoadBooks()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Book";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
                    {
                        DataTable booksTable = new DataTable();
                        adapter.Fill(booksTable);
                        BooksDataGrid.ItemsSource = booksTable.DefaultView;
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
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT * FROM Book 
                WHERE Title LIKE @Search OR
                      ISBN LIKE @Search OR
                      Genre LIKE @Search OR
                      PublicationYear LIKE @Search";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@Search", "%" + SearchBox.Text + "%");

                        DataTable booksTable = new DataTable();
                        adapter.Fill(booksTable);
                        BooksDataGrid.ItemsSource = booksTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching for books: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateBookWindow createBookWindow = new CreateBookWindow();
                createBookWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is DataRowView rowView) // Get the selected row as DataRowView
            {
                int bookId = Convert.ToInt32(rowView["BookID"]); // Extract BookID
                UpdateBookWindow updateBookWindow = new UpdateBookWindow(bookId);

                if (updateBookWindow.ShowDialog() == true) // If update is successful, reload books
                {
                    LoadBooks();
                }
            }
            else
            {
                MessageBox.Show("Please select a book to edit.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is DataRowView selectedRow)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(_connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM Book WHERE BookID = @BookID";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@BookID", selectedRow["BookID"]);

                            command.ExecuteNonQuery();
                            MessageBox.Show("Book deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadBooks();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a book to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        private void SettingsNav_click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentControl)
            {
                contentControl.Content = new AdminSettings();
            }
        }
    }
}
