using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using static MaterialDesignThemes.Wpf.Theme;

namespace LibraryWPF
{
    public partial class MemberPanel : UserControl
    {
        private string connectionString = MainWindow.connectionString;

        public MemberPanel()
        {
            InitializeComponent();
            LoadBooksData();
            LoadMemberBooks();
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
            }
        }

        private void LoadBooksData(string searchQuery = "")
        {
            try
            {
                var books = new List<BookModel>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT b.BookID, b.Title, b.PublicationYear, b.CopiesAvailable, 
       CONCAT(p.FirstName, ' ', p.LastName) AS AuthorName
FROM Book b
LEFT JOIN BookAuthor ba ON b.BookID = ba.BookID
LEFT JOIN Author a ON ba.AuthorID = a.AuthorID
LEFT JOIN Person p ON a.PersonID=p.PersonID"
                        ;

                    if (!string.IsNullOrWhiteSpace(searchQuery))
                    {
                        query += " WHERE b.Title LIKE @Search OR CONCAT(p.FirstName, ' ', p.LastName) LIKE @Search OR PublicationYear LIKE @Search";
                    }

                    using (var command = new MySqlCommand(query, connection))
                    {
                        if (!string.IsNullOrWhiteSpace(searchQuery))
                        {
                            command.Parameters.AddWithValue("@Search", "%" + searchQuery + "%");
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                books.Add(new BookModel
                                {
                                    BookID = reader.GetInt32("BookID"),
                                    Title = reader.GetString("Title"),
                                    PublicationYear = reader["PublicationYear"].ToString(), // Converts the YEAR to string
                                    AuthorName = reader.GetString("AuthorName"),
                                    CopiesAvailable = reader.GetInt32("CopiesAvailable")
                                });
                            }
                        }
                    }
                }

                BooksDataGrid.ItemsSource = books;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading books: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBooksData(SearchTextBox.Text);
        }

        private void RentBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is BookModel selectedBook)
            {
                if (selectedBook.CopiesAvailable > 0)
                {
                    try
                    {
                        using (var connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();

                            // Check if the member is already renting this book
                            string checkQuery = "SELECT COUNT(*) FROM Rental WHERE MemberID = @MemberID AND BookID = @BookID AND ReturnDate IS NULL;";
                            using (var checkCommand = new MySqlCommand(checkQuery, connection))
                            {
                                checkCommand.Parameters.AddWithValue("@MemberID", Session.MemberID);
                                checkCommand.Parameters.AddWithValue("@BookID", selectedBook.BookID);

                                int rentalCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                                if (rentalCount > 0)
                                {
                                    MessageBox.Show("You have already rented this book and haven't returned it yet.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }
                            }

                            // If not already rented, proceed with rental
                            string rentQuery = "INSERT INTO Rental (MemberID, BookID, RentalDate, DueDate) VALUES (@MemberID, @BookID, CURDATE(), DATE_ADD(CURDATE(), INTERVAL 14 DAY));";
                            using (var rentCommand = new MySqlCommand(rentQuery, connection))
                            {
                                rentCommand.Parameters.AddWithValue("@MemberID", Session.MemberID);
                                rentCommand.Parameters.AddWithValue("@BookID", selectedBook.BookID);
                                rentCommand.ExecuteNonQuery();
                            }

                            // Update CopiesAvailable
                            string updateQuery = "UPDATE Book SET CopiesAvailable = CopiesAvailable - 1 WHERE BookID = @BookID;";
                            using (var updateCommand = new MySqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@BookID", selectedBook.BookID);
                                updateCommand.ExecuteNonQuery();
                            }

                            MessageBox.Show("Book rented successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadBooksData();
                            LoadMemberBooks();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while renting the book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("This book is not currently available.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please select a book to rent.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void LoadMemberBooks()
        {
            try
            {
                var books = new List<BookModel>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Query for rented books
                    string rentedQuery = @"
                SELECT b.BookID, b.Title, b.PublicationYear, CONCAT(p.FirstName, ' ', p.LastName) AS AuthorName 
                FROM Rental r
                JOIN Book b ON r.BookID = b.BookID
                LEFT JOIN BookAuthor ba ON b.BookID = ba.BookID
                LEFT JOIN Author a ON ba.AuthorID = a.AuthorID
                LEFT JOIN Person p ON a.PersonID = p.PersonID
                WHERE r.MemberID = @MemberID AND r.ReturnDate IS NULL;";

                    using (var command = new MySqlCommand(rentedQuery, connection))
                    {
                        command.Parameters.AddWithValue("@MemberID", Session.MemberID);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                books.Add(new BookModel
                                {
                                    BookID = reader.GetInt32("BookID"),
                                    Title = reader.GetString("Title"),
                                    PublicationYear = reader["PublicationYear"].ToString(), // YEAR type needs conversion
                                    AuthorName = reader.GetString("AuthorName")
                                });
                            }
                        }
                    }

                    // Query for reserved books
                    string reservedQuery = @"
                SELECT b.BookID, b.Title, b.PublicationYear, CONCAT(p.FirstName, ' ', p.LastName) AS AuthorName 
                FROM Reservation res
                JOIN Book b ON res.BookID = b.BookID
                LEFT JOIN BookAuthor ba ON b.BookID = ba.BookID
                LEFT JOIN Author a ON ba.AuthorID = a.AuthorID
                LEFT JOIN Person p ON a.PersonID = p.PersonID
                WHERE res.MemberID = @MemberID;";

                    using (var command = new MySqlCommand(reservedQuery, connection))
                    {
                        command.Parameters.AddWithValue("@MemberID", Session.MemberID);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                books.Add(new BookModel
                                {
                                    BookID = reader.GetInt32("BookID"),
                                    Title = reader.GetString("Title"),
                                    PublicationYear = reader["PublicationYear"].ToString(),
                                    AuthorName = reader.GetString("AuthorName")
                                });
                            }
                        }
                    }
                }

                BooksData1Grid.ItemsSource = books;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading member books: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LanBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (LanBox.SelectedIndex == 0) {
                    SearchButton.Content = "Search";
                    LogoutButton.Content = "Logout";
                    RentButton.Content = "Rent Book";
                    ReserveButton.Content = "Reserve Book";
                }
                else
                {
                    SearchButton.Content = "Pretraži";
                    LogoutButton.Content = "Odjavi se";
                    RentButton.Content = "Digni knjigu";
                    ReserveButton.Content = "Rezerviši knjigu";
                }
            }
        }
        private void ReserveBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is BookModel selectedBook)
            {
                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "INSERT INTO Reservation (MemberID, BookID, ReservationDate) VALUES (@MemberID, @BookID, CURDATE());";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@MemberID", Session.MemberID); // Replace with the actual member ID
                            command.Parameters.AddWithValue("@BookID", selectedBook.BookID);
                            command.ExecuteNonQuery();
                        }

                        MessageBox.Show("Book reserved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadBooksData();
                        LoadMemberBooks();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while reserving the book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a book to reserve.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
     

    public class BookModel
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string PublicationYear { get; set; }
        public string AuthorName { get; set; }
        public int CopiesAvailable { get; set; }
    }
}
