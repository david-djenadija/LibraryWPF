using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using MySql.Data.MySqlClient;

namespace LibraryWPF
{
    public partial class DetailsView : UserControl
    {
        public static int UserID; // Set this value before navigating to DetailsView
        private string _connectionString = MainWindow.connectionString;
        private bool showingRentals = true; // Tracks whether rentals or reservations are displayed
        public DetailsView()
        {
            InitializeComponent();
            LoadUserDetails();
            LoadUserRentals();
            if (AdminSettings.selectedIndex == 1) LanguageChange();
        }
        private void LanguageChange()
        {
            MembersNav.Content = "Članovi";
            BooksNav.Content = "Knjige";
            SettingsNav.Content = "Podešavanja";
            LogoutNav.Content = "Odjava";
            ReservationButton.Content = "Prikaži rezervacije";
        }

        private void LoadUserRentals()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    string rentalsQuery = @"
                        SELECT 
                            b.Title AS 'Book Title',
                            b.ISBN AS 'ISBN',
                            b.Genre AS 'Genre',
                            r.RentalDate AS 'Rental Date',
                            r.DueDate AS 'Due Date',
                            r.ReturnDate AS 'Return Date'
                        FROM Rental r
                        JOIN Member m ON r.MemberID = m.MemberID
                        JOIN User u ON m.UserID = u.UserID
                        JOIN Book b ON r.BookID = b.BookID
                        WHERE m.MemberID = @UserID";

                    using (MySqlCommand command = new MySqlCommand(rentalsQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", UserID);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable rentalTable = new DataTable();
                            adapter.Fill(rentalTable);

                            DataGridTable.ItemsSource = rentalTable.DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading rental data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ToggleDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (showingRentals)
            {
                LoadUserReservations();
                if(AdminSettings.selectedIndex == 0)
                ((Button)sender).Content = "Show Rentals";
                else
                    ((Button)sender).Content = "Prikaži iznajmljivanje";
            }
            else
            {
                LoadUserRentals();
                if (AdminSettings.selectedIndex == 0)
                    ((Button)sender).Content = "Show Reservations";
                else
                    ((Button)sender).Content = "Prikaži rezervacije";
               
            }

            showingRentals = !showingRentals; // Toggle the state
        }
        private void LoadUserReservations()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    string reservationsQuery = @"
                        SELECT 
                            b.Title AS 'Book Title',
                            b.ISBN AS 'ISBN',
                            b.Genre AS 'Genre',
                            r.ReservationDate AS 'Reservation Date',
                            r.ReservationStatus AS 'Status'
                        FROM Reservation r
                        JOIN Member m ON r.MemberID = m.MemberID
                        JOIN User u ON m.UserID = u.UserID
                        JOIN Book b ON r.BookID = b.BookID
                        WHERE u.UserID = @UserID";

                    using (MySqlCommand command = new MySqlCommand(reservationsQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", UserID);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable reservationTable = new DataTable();
                            adapter.Fill(reservationTable);

                            DataGridTable.ItemsSource = reservationTable.DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading reservation data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUserDetails()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // Determine the role of the user (Admin, Member, or Personnel)
                    string roleQuery = @"
                        SELECT 
                            CASE 
                                WHEN EXISTS (SELECT 1 FROM Admin WHERE UserID = @UserID) THEN 'Admin'
                                WHEN EXISTS (SELECT 1 FROM Member WHERE UserID = @UserID) THEN 'Member'
                                WHEN EXISTS (SELECT 1 FROM Personnel WHERE UserID = @UserID) THEN 'Personnel'
                                ELSE 'Unknown'
                            END AS UserRole
                        ";

                    using (MySqlCommand roleCommand = new MySqlCommand(roleQuery, connection))
                    {
                        roleCommand.Parameters.AddWithValue("@UserID", UserID);
                        string userRole = "Member";
                            //roleCommand.ExecuteScalar()?.ToString();

                        if (userRole == "Admin")
                        {
                            LoadAdminDetails(connection);
                        }
                        else if (userRole == "Member")
                        {
                            LoadMemberDetails(connection);
                        }
                        else if (userRole == "Personnel")
                        {
                            LoadPersonnelDetails(connection);
                        }
                        else
                        {
                            MessageBox.Show("User role not found or invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading user details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAdminDetails(MySqlConnection connection)
        {
            string adminQuery = @"
                SELECT 
                    CONCAT(p.FirstName, ' ', p.LastName) AS FullName,
                    p.DateOfBirth,
                    p.Gender,
                    u.Email
                FROM Admin a
                JOIN User u ON a.UserID = u.UserID
                JOIN Person p ON u.PersonID = p.PersonID
                WHERE a.UserID = @UserID";

            using (MySqlCommand command = new MySqlCommand(adminQuery, connection))
            {
                command.Parameters.AddWithValue("@UserID", UserID);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        NameLabel.Content = reader["FullName"];
                        BirthDateLabel.Content = Convert.ToDateTime(reader["DateOfBirth"]).ToString("yyyy-MM-dd");
                        GenderLabel.Content = reader["Gender"].ToString();
                        EmailLabel.Content = reader["Email"].ToString();
                        MembershipStartLabel.Content = "N/A";
                        MembershipEndLabel.Content = "N/A";
                        AmountDueLabel.Content = "N/A";
                    }
                }
            }
        }

        private void LoadMemberDetails(MySqlConnection connection)
        {
            string memberQuery = @"
                SELECT 
                    CONCAT(p.FirstName, ' ', p.LastName) AS FullName,
                    p.DateOfBirth,
                    p.Gender,
                    u.Email,
                    m.MembershipStartDate,
                    m.MembershipEndDate,
                    m.ToPay
                FROM Member m
                JOIN User u ON m.UserID = u.UserID
                JOIN Person p ON u.PersonID = p.PersonID
                WHERE m.MemberID = @UserID";

            using (MySqlCommand command = new MySqlCommand(memberQuery, connection))
            {
                command.Parameters.AddWithValue("@UserID", UserID);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        NameLabel.Content = reader["FullName"];
                        BirthDateLabel.Content = Convert.ToDateTime(reader["DateOfBirth"]).ToString("yyyy-MM-dd");
                        GenderLabel.Content = reader["Gender"].ToString();
                        EmailLabel.Content = reader["Email"].ToString();
                        MembershipStartLabel.Content = Convert.ToDateTime(reader["MembershipStartDate"]).ToString("yyyy-MM-dd");
                        MembershipEndLabel.Content = reader["MembershipEndDate"] != DBNull.Value
                            ? Convert.ToDateTime(reader["MembershipEndDate"]).ToString("yyyy-MM-dd")
                            : "N/A";
                        AmountDueLabel.Content = $"{reader["ToPay"]} USD";
                    }
                }
            }
        }

        private void LoadPersonnelDetails(MySqlConnection connection)
        {
            string personnelQuery = @"
                SELECT 
                    CONCAT(p.FirstName, ' ', p.LastName) AS FullName,
                    p.DateOfBirth,
                    p.Gender,
                    u.Email,
                    pr.JobTitle
                FROM Personnel pr
                JOIN User u ON pr.UserID = u.UserID
                JOIN Person p ON u.PersonID = p.PersonID
                WHERE pr.UserID = @UserID";

            using (MySqlCommand command = new MySqlCommand(personnelQuery, connection))
            {
                command.Parameters.AddWithValue("@UserID", UserID);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        NameLabel.Content = reader["FullName"];
                        BirthDateLabel.Content = Convert.ToDateTime(reader["DateOfBirth"]).ToString("yyyy-MM-dd");
                        GenderLabel.Content = reader["Gender"].ToString();
                        EmailLabel.Content = reader["Email"].ToString();
                        MembershipStartLabel.Content = reader["JobTitle"].ToString();
                        MembershipEndLabel.Content = "N/A";
                        AmountDueLabel.Content = "N/A";
                    }
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
