using System.Windows;

namespace rattrapageB4
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnPatients_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ouverture fenêtre Patients...");
        }

        private void BtnDoctors_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ouverture fenêtre Médecins...");
        }

        private void BtnSpecialities_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ouverture fenêtre Spécialités...");
        }

        private void BtnAppointments_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ouverture fenêtre Rendez-vous...");
        }
    }
}
