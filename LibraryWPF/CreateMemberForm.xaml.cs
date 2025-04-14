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
    /// Interaction logic for CreateMemberForm.xaml
    /// </summary>
    public partial class CreateMemberForm : Window
    {
        public CreateMemberForm()
        {
            InitializeComponent();
            ApplyCurrentTheme();
            if (AdminSettings.selectedIndex == 1) languageChange();
        }
        private void languageChange()
        {
            FirstNameBlock.Text = "Ime:";
            LastNameBlock.Text = "Prezime:";
            DateOfBirthBlock.Text = "Datum rođenja:";
            GenderBlock.Text = "Pol:";
            UsernameBlock.Text = "Korisničko ime:";
            PassBlock.Text = "Šifra:";
            StartDateBlock.Text = "Početak članstva:";
            EndDateBlock.Text = "Kraj članstva:";
            ToPayBlock.Text = "Dug:";
            SaveButton.Content = "Sačuvaj";
            CancelButton.Content = "Otkaži";
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
                            case PasswordBox passwordBox:
                                passwordBox.Foreground = textColor;
                                break;
                            case ComboBox comboBox:
                                comboBox.Foreground = textColor;
                                break;
                            case DatePicker datePicker:
                                datePicker.Foreground = textColor;
                                break;
                            case Button button:
                                button.Foreground = textColor;
                                break;
                        }
                    }
                }
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = MainWindow.connectionString;
            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;
            string gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string email = EmailTextBox.Text;
            DateTime? dob = DateOfBirthPicker.SelectedDate;
            DateTime? membershipStart = MembershipStartDatePicker.SelectedDate;
            DateTime? membershipEnd = MembershipEndDatePicker.SelectedDate;
            int toPay = int.TryParse(ToPayTextBox.Text, out var parsedToPay) ? parsedToPay : 0;

            // Validate fields
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || dob == null ||
                membershipStart == null || membershipEnd == null || string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (membershipEnd <= membershipStart)
            {
                MessageBox.Show("Membership end date must be after the membership start date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Invalid email format.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string hashedPassword = password; // TODO: Hash the password here

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Insert into Person table and retrieve PersonID
                    string personQuery = @"
                INSERT INTO Person (FirstName, LastName, DateOfBirth, Gender)
                VALUES (@FirstName, @LastName, @DateOfBirth, @Gender);
                SELECT LAST_INSERT_ID();";

                    int personID;
                    using (var personCommand = new MySqlCommand(personQuery, connection))
                    {
                        personCommand.Parameters.AddWithValue("@FirstName", firstName);
                        personCommand.Parameters.AddWithValue("@LastName", lastName);
                        personCommand.Parameters.AddWithValue("@DateOfBirth", dob);
                        personCommand.Parameters.AddWithValue("@Gender", gender);

                        personID = Convert.ToInt32(personCommand.ExecuteScalar());
                    }

                    // Insert into User table
                    string userQuery = @"
                INSERT INTO User (PersonID, Username, PasswordHash, Email)
                VALUES (@PersonID, @Username, @HashedPassword, @Email);";

                    using (var userCommand = new MySqlCommand(userQuery, connection))
                    {
                        userCommand.Parameters.AddWithValue("@PersonID", personID);
                        userCommand.Parameters.AddWithValue("@Username", username);
                        userCommand.Parameters.AddWithValue("@HashedPassword", hashedPassword);
                        userCommand.Parameters.AddWithValue("@Email", email);

                        userCommand.ExecuteNonQuery();
                    }

                    // Insert into Member table
                    string memberQuery = @"
                INSERT INTO Member (UserID, MembershipStartDate, MembershipEndDate, ToPay)
                VALUES (@UserID, @MembershipStartDate, @MembershipEndDate, @ToPay);";

                    using (var memberCommand = new MySqlCommand(memberQuery, connection))
                    {
                        memberCommand.Parameters.AddWithValue("@UserID", personID); // UserID is the same as PersonID
                        memberCommand.Parameters.AddWithValue("@MembershipStartDate", membershipStart);
                        memberCommand.Parameters.AddWithValue("@MembershipEndDate", membershipEnd);
                        memberCommand.Parameters.AddWithValue("@ToPay", toPay);

                        memberCommand.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Member created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
