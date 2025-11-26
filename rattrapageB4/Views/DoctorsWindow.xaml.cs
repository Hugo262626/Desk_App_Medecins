using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using rattrapageB4.Models;

namespace rattrapageB4.Views
{
    public partial class DoctorsWindow : Window
    {
        public DoctorsWindow()
        {
            InitializeComponent();
            LoadDoctors();
        }

        private void LoadDoctors()
        {
            using var db = new ClinicContext();
            dgDoctors.ItemsSource = db.Doctors.Include(d => d.Speciality).ToList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var form = new DoctorFormWindow();
            form.Owner = this;
            if (form.ShowDialog() == true) LoadDoctors();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgDoctors.SelectedItem is Doctor doc)
            {
                var form = new DoctorFormWindow(doc.Id);
                form.Owner = this;
                if (form.ShowDialog() == true) LoadDoctors();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgDoctors.SelectedItem is Doctor doc)
            {
                if (MessageBox.Show($"Supprimer le médecin {doc.LastName} {doc.FirstName} ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using var db = new ClinicContext();
                    var doctor = db.Doctors.Find(doc.Id);
                    if (doctor != null)
                    {
                        db.Doctors.Remove(doctor);
                        db.SaveChanges();
                        LoadDoctors();
                    }
                }
            }
        }
    }
}
