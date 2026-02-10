using System.Windows;

namespace rattrapageB4.Views
{
    public partial class AdminPasswordWindow : Window
    {
        public string Password => PwdBox.Password;

        public AdminPasswordWindow()
        {
            InitializeComponent();
            Loaded += (_, __) => PwdBox.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e) => DialogResult = true;
        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
