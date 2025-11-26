using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using rattrapageB4.Models;

namespace rattrapageB4
{
    public partial class AppointmentDetailWindow : Window
    {
        private readonly int _appointmentId;

        public AppointmentDetailWindow(int appointmentId)
        {
            InitializeComponent();
            _appointmentId = appointmentId;
            LoadDetails();
        }

        private void LoadDetails()
        {
            using var db = new ClinicContext();
            var a = db.Appointments
                      .Include(x => x.Doctor)
                      .Include(x => x.Patient)
                      .FirstOrDefault(x => x.Id == _appointmentId);

            if (a == null)
            {
                MessageBox.Show("Rendez-vous introuvable.");
                Close();
                return;
            }

            txtTitle.Text = $"Rendez-vous #{a.Id}";
            txtDoctor.Text = $"Médecin : {a.Doctor?.LastName} {a.Doctor?.FirstName}";
            txtPatient.Text = $"Patient : {a.Patient?.LastName} {a.Patient?.FirstName}";
            txtTime.Text = $"De {a.StartAt:g} à {a.EndAt:g}";
            txtNotes.Text = string.IsNullOrWhiteSpace(a.Notes) ? "(Aucune information)" : a.Notes;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
