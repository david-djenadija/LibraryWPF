using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for AdminPanel.xaml
    /// </summary>
    public partial class AdminPanel : UserControl
    {
        public AdminPanel()
        {
            InitializeComponent();
            LoadMembers();
            MembersNav.Background = new SolidColorBrush(Colors.Red);
            if (AdminSettings.selectedIndex==1)
            changeToSerbian();
        }

        private void changeToSerbian()
        {
            MembersNav.Content = "Članovi";
            BooksNav.Content = "Knjige";
            SettingsNav.Content = "Podešavanja";
            LogoutNav.Content = "Odjava";
            SearchButton.Content = "Pretraži";
            DetailsButton.Content = "Detalji";
            CreateButton.Content = "Dodaj";
            UpdateButton.Content = "Ažuriraj";
            DeleteButton.Content = "Obriši";
        }
       
        private void CreateButton_Click(object sender, RoutedEventArgs e)
{
    var memberForm = new CreateMemberForm();
    if (memberForm.ShowDialog() == true)
    {
        LoadMembers(); // Reload the members data in the DataGrid
    }
}

private void UpdateButton_Click(object sender, RoutedEventArgs e)
{
    if (DataGridTable.SelectedItem is DataRowView row)
    {
        int memberId = Convert.ToInt32(row["MemberID"]);
        string firstName = row["Name"].ToString().Split(" ")[0];
        string lastName = row["Name"].ToString().Split(" ")[1];
        DateTime dob = Convert.ToDateTime(row["DateOfBirth"]);
        DateTime membershipStart = Convert.ToDateTime(row["MembershipStartDate"]);
        int toPay = Convert.ToInt32(row["ToPay"]);

        var memberForm = new MemberForm(isEditMode: true, memberId, firstName, lastName, dob, membershipStart, toPay);
        if (memberForm.ShowDialog() == true)
        {
            LoadMembers(); // Reload the members data in the DataGrid
        }
    }
    else
    {
        MessageBox.Show("Please select a member to update.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTable.SelectedItem == null)
            {
                MessageBox.Show("Please select a member to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Retrieve the selected member from the DataGrid
            var selectedMember = DataGridTable.SelectedItem as DataRowView;
            if (selectedMember == null)
            {
                MessageBox.Show("Error retrieving selected member.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Extract the MemberID of the selected member
            int memberId = Convert.ToInt32(selectedMember["MemberID"]);

            // Confirm deletion
            var result = MessageBox.Show("Are you sure you want to delete this member?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                string connectionString = MainWindow.connectionString;
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Delete member from the database
                    string query = "DELETE FROM Member WHERE MemberID = @MemberID";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MemberID", memberId);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Member deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadMembers(); // Refresh the DataGrid
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete member. Ensure the selected member exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMembers()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(BookDetailsWindow.connectionString))
                {
                    connection.Open();
                    string query = "SELECT MemberID, MembershipStartDate, MembershipEndDate, CONCAT(FirstName, ' ' , LastName) as Name, Username, Email, DateOfBirth, Gender, ToPay FROM Member m INNER JOIN USER u on m.UserID=u.UserID INNER JOIN PERSON p on u.PersonID=p.PersonID";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    DataGridTable.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading members: {ex.Message}");
            }
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchLastNameTextBox.Text == "")
            {
                LoadMembers();
                return;
            }
            try
            {
                using (MySqlConnection connection = new MySqlConnection(BookDetailsWindow.connectionString))
                {
                    connection.Open();
                    string query = @"
                       SELECT MemberID, MembershipStartDate, MembershipEndDate, CONCAT(FirstName, ' ' , LastName) as Name, Username, Email, DateOfBirth, Gender, ToPay FROM Member m INNER JOIN USER u on m.UserID=u.UserID INNER JOIN PERSON p on u.PersonID=p.PersonID
                       WHERE concat(FirstName, ' ', LastName) LIKE @LastName"
                       ;
                    MySqlCommand command = new MySqlCommand(query, connection);
           
                    command.Parameters.AddWithValue("@LastName", $"%{SearchLastNameTextBox.Text}%");

                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    DataGridTable.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching members: {ex.Message}");
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
        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTable.SelectedItem is DataRowView row)
            {
                try
                {
                    string memberId = row["MemberID"].ToString();
                    DetailsView.UserID=int.Parse(memberId);
                    
                    var mainWindow = Application.Current.MainWindow;

                    if (mainWindow != null)
                    {
                        mainWindow.Content = new DetailsView();
                    }
                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching details: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please select a member from the table to view details.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        

        private void BooksNav_click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentControl)
            {
                contentControl.Content = new BooksAdmin();
            }
        }
        private void SettingsNav_click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentControl)
            {
                contentControl.Content = new AdminSettings();
            }
        }
        private void MembersNav_click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentControl)
            {
                contentControl.Content = new AdminPanel();
            }
        }
    }
}
