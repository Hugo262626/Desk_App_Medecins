using System.Linq;
using System.Windows;
using rattrapageB4.Models;

namespace rattrapageB4.Views
{
    public partial class DoctorFormWindow : Window
    {
        private int? doctorId = null;

        public DoctorFormWindow(int? id = null)
        {
            InitializeComponent();
            doctorId = id;
            LoadSpecialities();
            if (doctorId.HasValue) LoadDoctor();
        }

        private void LoadSpecialities()
        {
            using var db = new ClinicContext();
            cboSpeciality.ItemsSource = db.Specialities.ToList();
            if (cboSpeciality.Items.Count > 0) cboSpeciality.SelectedIndex = 0;
        }

        private void LoadDoctor()
        {
            using var db = new ClinicContext();
            var doctor = db.Doctors.Find(doctorId.Value);
            if (doctor != null)
            {
                txtLastName.Text = doctor.LastName;
                txtFirstName.Text = doctor.FirstName;
                cboSpeciality.SelectedItem = db.Specialities.Find(doctor.SpecialityId);
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text) || cboSpeciality.SelectedItem == null)
            {
                MessageBox.Show("Veuillez remplir tous les champs.");
                return;
            }

            using var db = new ClinicContext();

            Doctor doctor;
            if (doctorId.HasValue)
            {
                doctor = db.Doctors.Find(doctorId.Value);
                if (doctor == null) return;
            }
            else
            {
                doctor = new Doctor();
                db.Doctors.Add(doctor);
            }

            doctor.LastName = txtLastName.Text.Trim();
            doctor.FirstName = txtFirstName.Text.Trim();
            doctor.SpecialityId = ((Speciality)cboSpeciality.SelectedItem).Id;

            db.SaveChanges();
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
