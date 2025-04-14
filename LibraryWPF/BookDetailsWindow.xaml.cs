using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;

namespace LibraryWPF
{
    public partial class BookDetailsWindow : Window
    {
        public static string connectionString = MainWindow.connectionString;
        private int bookId;

        public BookDetailsWindow(int bookId)
        {
            InitializeComponent();
            this.bookId = bookId;
            LoadUsers();
            if(PersonnelSettings.selectedIndex == 1) languageChange();
        }
        private void languageChange()
        {
            ReturnedButton.Content = "Označi kao vraćeno";
        }

        private void LoadUsers()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            r.RentalID, 
                            m.MemberID, 
                            CONCAT(p.FirstName, ' ', p.LastName) AS FullName,
                            'Renting' AS Status
                        FROM Rental r
                        JOIN Member m ON r.MemberID = m.MemberID
                        JOIN User u ON m.UserID = u.UserID
                        JOIN Person p ON u.PersonID = p.PersonID
                        WHERE r.BookID = @BookID

                        UNION

                        SELECT 
                            NULL AS RentalID, 
                            m.MemberID, 
                            CONCAT(p.FirstName, ' ', p.LastName) AS FullName,
                            'Reserving' AS Status
                        FROM Reservation res
                        JOIN Member m ON res.MemberID = m.MemberID
                        JOIN User u ON m.UserID = u.UserID
                        JOIN Person p ON u.PersonID = p.PersonID
                        WHERE res.BookID = @BookID";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BookID", bookId);

                        MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        UsersDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MarkAsReturnedButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)UsersDataGrid.SelectedItem;

                // Ensure the selected user is renting the book
                if (selectedRow["Status"].ToString() == "Renting")
                {
                    int rentalId = Convert.ToInt32(selectedRow["RentalID"]);

                    try
                    {
                        using (MySqlConnection connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();

                            // Start a transaction to ensure data consistency
                            using (MySqlTransaction transaction = connection.BeginTransaction())
                            {
                                try
                                {
                                    // Delete the rental record
                                    string deleteRentalQuery = "DELETE FROM Rental WHERE RentalID = @RentalID";
                                    using (MySqlCommand deleteCommand = new MySqlCommand(deleteRentalQuery, connection, transaction))
                                    {
                                        deleteCommand.Parameters.AddWithValue("@RentalID", rentalId);
                                        deleteCommand.ExecuteNonQuery();
                                    }

                                    // Update the book's CopiesAvailable
                                    string updateBookQuery = "UPDATE Book SET CopiesAvailable = CopiesAvailable + 1 WHERE BookID = @BookID";
                                    using (MySqlCommand updateCommand = new MySqlCommand(updateBookQuery, connection, transaction))
                                    {
                                        updateCommand.Parameters.AddWithValue("@BookID", bookId);
                                        updateCommand.ExecuteNonQuery();
                                    }
                                    // Update the book's CopiesRented
                                    updateBookQuery = "UPDATE Book SET CopiesRented = CopiesRented - 1 WHERE BookID = @BookID";
                                    using (MySqlCommand updateCommand = new MySqlCommand(updateBookQuery, connection, transaction))
                                    {
                                        updateCommand.Parameters.AddWithValue("@BookID", bookId);
                                        updateCommand.ExecuteNonQuery();
                                    }

                                    // Commit the transaction
                                    transaction.Commit();

                                    MessageBox.Show("Book marked as returned successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                    // Reload the users data to reflect changes
                                    LoadUsers();
                                }
                                catch
                                {
                                    transaction.Rollback();
                                    throw;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("The selected user is not currently renting this book.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please select a user renting the book.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
