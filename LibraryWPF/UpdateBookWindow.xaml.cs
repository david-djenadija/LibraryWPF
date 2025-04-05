using LibraryWPF;
using MaterialDesignThemes.Wpf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LibraryWPF
{
    /// <summary>
    /// Interaction logic for UpdateBookWindow.xaml
    /// </summary>
    public partial class UpdateBookWindow : Window
    {
        private int bookId;
        private string connectionString = MainWindow.connectionString;
        public UpdateBookWindow(int bookId)
        {
            InitializeComponent();
            this.bookId = bookId;
            LoadBookData();
            ApplyCurrentTheme();
        }
        private void ApplyCurrentTheme()
        {
            string currentTheme = ThemeManager.GetCurrentTheme();
            ThemeManager.ApplyTheme(currentTheme); // Sync with global theme

            // Get the current theme
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            // Set window background
            Background = new SolidColorBrush(theme.Background);

            // Set foreground for all text-related controls
            var textColor = new SolidColorBrush(theme.Foreground);
            foreach (var child in LogicalTreeHelper.GetChildren(this))
            {
                if (child is Grid grid)
                {
                    foreach (var gridChild in grid.Children)
                    {
                        switch (gridChild)
                        {
                            case TextBlock textBlock:
                                textBlock.Foreground = textColor;
                                break;
                            case TextBox textBox:
                                textBox.Foreground = textColor;
                                break;
                            case Button button:
                                button.Foreground = textColor;
                                break;
                        }
                    }
                }
            }
        }
        private void LoadBookData()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT b.Title, b.ISBN, b.PublicationYear, b.Genre, b.CopiesAvailable, 
                               p.FirstName, p.LastName
                        FROM Book b
                        LEFT JOIN BookAuthor ba ON b.BookID = ba.BookID
                        LEFT JOIN Author a ON ba.AuthorID = a.AuthorID
                        LEFT JOIN Person p ON a.PersonID = p.PersonID
                        WHERE b.BookID = @BookID";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BookID", bookId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                TitleTextBox.Text = reader.GetString("Title");
                                ISBNTextBox.Text = reader.GetString("ISBN");
                                PublicationYearTextBox.Text = reader["PublicationYear"].ToString();
                                GenreTextBox.Text = reader.GetString("Genre");
                                CopiesAvailableTextBox.Text = reader["CopiesAvailable"].ToString();
                                AuthorFirstNameTextBox.Text = reader.GetString("FirstName");
                                AuthorLastNameTextBox.Text = reader.GetString("LastName");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading book data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateBook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string updateQuery = @"
                        UPDATE Book 
                        SET Title = @Title, ISBN = @ISBN, PublicationYear = @PublicationYear, 
                            Genre = @Genre, CopiesAvailable = @CopiesAvailable
                        WHERE BookID = @BookID";

                    using (var command = new MySqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Title", TitleTextBox.Text);
                        command.Parameters.AddWithValue("@ISBN", ISBNTextBox.Text);
                        command.Parameters.AddWithValue("@PublicationYear", int.Parse(PublicationYearTextBox.Text));
                        command.Parameters.AddWithValue("@Genre", GenreTextBox.Text);
                        command.Parameters.AddWithValue("@CopiesAvailable", int.Parse(CopiesAvailableTextBox.Text));
                        command.Parameters.AddWithValue("@BookID", bookId);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Book updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}



