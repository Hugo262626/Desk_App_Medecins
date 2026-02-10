using System.Linq;
using System.Windows;
using rattrapageB4.Models;

namespace rattrapageB4.Views
{
    public partial class SpecialitiesWindow : Window
    {
        public SpecialitiesWindow()
        {
            InitializeComponent();
            LoadSpecialities();
        }

        private void LoadSpecialities()
        {
            using var db = new ClinicContext();
            dgSpecialities.ItemsSource = db.Specialities.ToList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var form = new SpecialityFormWindow();
            form.Owner = this;
            if (form.ShowDialog() == true) LoadSpecialities();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgSpecialities.SelectedItem is Speciality s)
            {
                var form = new SpecialityFormWindow(s.Id);
                form.Owner = this;
                if (form.ShowDialog() == true) LoadSpecialities();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgSpecialities.SelectedItem is Speciality s)
            {
                using var db = new ClinicContext();

                // Vérifie si des médecins utilisent cette spécialité
                bool hasDoctors = db.Doctors.Any(d => d.SpecialityId == s.Id);
                if (hasDoctors)
                {
                    MessageBox.Show("Impossible de supprimer cette spécialité : au moins un médecin l'utilise.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show($"Supprimer la spécialité {s.Name} ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var sp = db.Specialities.Find(s.Id);
                    if (sp != null)
                    {
                        db.Specialities.Remove(sp);
                        db.SaveChanges();
                        LoadSpecialities();
                    }
                }
            }
        }
    }
}
