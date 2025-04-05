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
using System.Windows.Shapes;

namespace LibraryWPF
{
    /// <summary>
    /// Interaction logic for CreateBookWindow.xaml
    /// </summary>
    public partial class CreateBookWindow : Window
    {
        String connectionString = MainWindow.connectionString;
        public CreateBookWindow()
        {
            InitializeComponent();
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
        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string title = txtTitle.Text;
                string isbn = txtISBN.Text;
                string publicationYear = txtPublicationYear.Text;
                string genre = txtGenre.Text;
                int copiesAvailable = int.TryParse(txtCopiesAvailable.Text, out int copies) ? copies : 0;
                string authorFirstName = txtAuthorFirstName.Text;
                string authorLastName = txtAuthorLastName.Text;

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            long personId, authorId, bookId;

                            // Insert person (author)
                            string insertPerson = "INSERT INTO Person (FirstName, LastName) VALUES (@FirstName, @LastName); SELECT LAST_INSERT_ID();";
                            using (var cmd = new MySqlCommand(insertPerson, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@FirstName", authorFirstName);
                                cmd.Parameters.AddWithValue("@LastName", authorLastName);
                                personId = Convert.ToInt64(cmd.ExecuteScalar());
                            }

                            // Insert author
                            string insertAuthor = "INSERT INTO Author (PersonID) VALUES (@PersonID); SELECT LAST_INSERT_ID();";
                            using (var cmd = new MySqlCommand(insertAuthor, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PersonID", personId);
                                authorId = Convert.ToInt64(cmd.ExecuteScalar());
                            }

                            // Insert book
                            string insertBook = @"
                                INSERT INTO Book (Title, ISBN, PublicationYear, Genre, CopiesAvailable)
                                VALUES (@Title, @ISBN, @PublicationYear, @Genre, @CopiesAvailable); 
                                SELECT LAST_INSERT_ID();";
                            using (var cmd = new MySqlCommand(insertBook, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Title", title);
                                cmd.Parameters.AddWithValue("@ISBN", isbn);
                                cmd.Parameters.AddWithValue("@PublicationYear", publicationYear);
                                cmd.Parameters.AddWithValue("@Genre", genre);
                                cmd.Parameters.AddWithValue("@CopiesAvailable", copiesAvailable);
                                bookId = Convert.ToInt64(cmd.ExecuteScalar());
                            }

                            // Insert BookAuthor relation
                            string insertBookAuthor = "INSERT INTO BookAuthor (BookID, AuthorID) VALUES (@BookID, @AuthorID)";
                            using (var cmd = new MySqlCommand(insertBookAuthor, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@BookID", bookId);
                                cmd.Parameters.AddWithValue("@AuthorID", authorId);
                                cmd.ExecuteNonQuery();
                            }

                            // Commit transaction
                            transaction.Commit();
                            MessageBox.Show("Book added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close();
                        }
                        catch
                        {
                            transaction.Rollback();
                            MessageBox.Show("Failed to add book.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
