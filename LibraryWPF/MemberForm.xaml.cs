using System;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using System.Windows.Media;
using MySql.Data.MySqlClient;

namespace LibraryWPF
{
    public partial class MemberForm : Window
    {
        private readonly bool _isEditMode;
        private readonly int? _memberId; // Null for Create, Value for Update
        private string ConnectionString = MainWindow.connectionString;

        public MemberForm(bool isEditMode, int? memberId = null, string firstName = null, string lastName = null, DateTime? dob = null, DateTime? membershipStart = null, int? toPay = null)
        {
            InitializeComponent();
            _isEditMode = isEditMode;
            _memberId = memberId;

            if (_isEditMode)
            {
                // Populate fields for Update
                FirstNameTextBox.Text = firstName;
                LastNameTextBox.Text = lastName;
                DateOfBirthPicker.SelectedDate = dob;
                MembershipStartDatePicker.SelectedDate = membershipStart;
                ToPayTextBox.Text = toPay?.ToString();
            }
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
            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;
            DateTime? dob = DateOfBirthPicker.SelectedDate;
            DateTime? membershipStart = MembershipStartDatePicker.SelectedDate;
            int toPay = int.TryParse(ToPayTextBox.Text, out var parsedToPay) ? parsedToPay : 0;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || dob == null || membershipStart == null)
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query;

                    if (_isEditMode)
                    {
                        query = @"
                            UPDATE Member
                            INNER JOIN Person ON Member.UserID = Person.PersonID
                            SET 
                                Person.FirstName = @FirstName, 
                                Person.LastName = @LastName, 
                                Person.DateOfBirth = @DateOfBirth,
                                Member.MembershipStartDate = @MembershipStartDate,
                                Member.ToPay = @ToPay
                            WHERE Member.MemberID = @MemberID;";
                    }
                    else
                    {
                        query = @"
                            INSERT INTO Person (FirstName, LastName, DateOfBirth)
                            VALUES (@FirstName, @LastName, @DateOfBirth);
                            SET @PersonID = LAST_INSERT_ID();
                            INSERT INTO Member (UserID, MembershipStartDate, ToPay)
                            VALUES (@PersonID, @MembershipStartDate, @ToPay);";
                    }

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@DateOfBirth", dob);
                        command.Parameters.AddWithValue("@MembershipStartDate", membershipStart);
                        command.Parameters.AddWithValue("@ToPay", toPay);
                        if (_isEditMode) command.Parameters.AddWithValue("@MemberID", _memberId);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(_isEditMode ? "Member updated successfully!" : "Member created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}
