using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace LibraryWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static String connectionString = "";
        public MainWindow()
        {
            InitializeComponent();
            string configPath = "C:\\Users\\Hp\\source\\repos\\LibraryWPF\\LibraryWPF\\config.properties"; // Adjust path if necessary
            ConfigReader config = new ConfigReader(configPath);

            string server = config.GetValue("server");
            string database = config.GetValue("database");
            string user = config.GetValue("user");
            string password = config.GetValue("password");
            string port = config.GetValue("port");

            connectionString = $"Server={server};Database={database};User={user};Password={password};Port={port};";

            // Start with the login view
            MainContent.Content = new LoginControl();
            //MainGrid.Children.Add(MainContent);
        }
       
      
    }
}