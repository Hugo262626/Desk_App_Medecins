using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using rattrapageB4.Models; // <- tes modèles Doctor, Patient, Appointment, Speciality

namespace rattrapageB4.Views
{
    public partial class MainWindow : Window
    {
        private List<AppointmentListItem> currentList = new List<AppointmentListItem>();

        public MainWindow()
        {
            InitializeComponent();
            LoadFiltersAndAppointments();
        }

        private void LoadFiltersAndAppointments()
        {
            using var db = new ClinicContext();

            // Médecins : projection FullName
            var doctors = db.Doctors
                            .OrderBy(d => d.LastName)
                            .Select(d => new { d.Id, FullName = (d.LastName ?? "") + " " + (d.FirstName ?? "") })
                            .ToList();
            cboDoctor.ItemsSource = doctors;
            cboDoctor.SelectedIndex = -1;

            // Patients : projection FullName
            var patients = db.Patients
                             .OrderBy(p => p.LastName)
                             .Select(p => new { p.Id, FullName = (p.LastName ?? "") + " " + (p.FirstName ?? "") })
                             .ToList();
            cboPatient.ItemsSource = patients;
            cboPatient.SelectedIndex = -1;

            RefreshAppointments();
        }

        private void RefreshAppointments()
        {
            DateTime now = DateTime.Now;
            using var db = new ClinicContext();

            var list = db.Appointments
                         .Include(a => a.Doctor)
                         .Include(a => a.Patient)
                         .Where(a => a.StartAt >= now)
                         .OrderBy(a => a.StartAt)
                         .ToList()
                         .Select(a => new AppointmentListItem
                         {
                             Id = a.Id,
                             StartAt = a.StartAt,
                             EndAt = a.EndAt,
                             DoctorId = a.DoctorId,
                             DoctorName = a.Doctor != null ? $"{a.Doctor.LastName} {a.Doctor.FirstName}" : "",
                             PatientId = a.PatientId,
                             PatientName = a.Patient != null ? $"{a.Patient.LastName} {a.Patient.FirstName}" : "",
                             Notes = a.Notes
                         }).ToList();

            currentList = list;
            dgAppointments.ItemsSource = currentList;
        }

        // FILTRAGE
        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            var filtered = currentList.AsEnumerable();

            if (cboDoctor.SelectedItem != null)
            {
                var doctorId = (int)cboDoctor.SelectedItem.GetType().GetProperty("Id").GetValue(cboDoctor.SelectedItem);
                filtered = filtered.Where(x => x.DoctorId == doctorId);
            }

            if (cboPatient.SelectedItem != null)
            {
                var patientId = (int)cboPatient.SelectedItem.GetType().GetProperty("Id").GetValue(cboPatient.SelectedItem);
                filtered = filtered.Where(x => x.PatientId == patientId);
            }

            if (dpFrom.SelectedDate.HasValue)
                filtered = filtered.Where(x => x.StartAt.Date >= dpFrom.SelectedDate.Value);

            if (dpTo.SelectedDate.HasValue)
                filtered = filtered.Where(x => x.StartAt.Date <= dpTo.SelectedDate.Value);

            dgAppointments.ItemsSource = filtered.ToList();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            cboDoctor.SelectedIndex = -1;
            cboPatient.SelectedIndex = -1;
            dpFrom.SelectedDate = null;
            dpTo.SelectedDate = null;
            dgAppointments.ItemsSource = currentList;
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            using var db = new ClinicContext();
            var past = db.Appointments
                         .Include(a => a.Doctor)
                         .Include(a => a.Patient)
                         .Where(a => a.EndAt < DateTime.Now)
                         .OrderByDescending(a => a.StartAt)
                         .ToList()
                         .Select(a => new AppointmentListItem
                         {
                             Id = a.Id,
                             StartAt = a.StartAt,
                             EndAt = a.EndAt,
                             DoctorName = a.Doctor != null ? $"{a.Doctor.LastName} {a.Doctor.FirstName}" : "",
                             PatientName = a.Patient != null ? $"{a.Patient.LastName} {a.Patient.FirstName}" : "",
                             Notes = a.Notes
                         }).ToList();

            dgAppointments.ItemsSource = past;
        }

        private void DgAppointments_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgAppointments.SelectedItem is AppointmentListItem item)
            {
                var detail = new AppointmentDetailWindow(item.Id);
                detail.Owner = this;
                detail.ShowDialog();
            }
        }

        // BOUTONS CRUD (placeholders)
        private void BtnPatients_Click(object sender, RoutedEventArgs e)
        {
            var win = new Views.PatientWindow();
            win.ShowDialog();
        }
        private void BtnDoctors_Click(object sender, RoutedEventArgs e)
        {
            var doctorsWindow = new DoctorsWindow();
            doctorsWindow.Owner = this; // optionnel, pour que la fenêtre soit “attachée” à la MainWindow
            doctorsWindow.ShowDialog(); // ShowDialog() bloque la MainWindow tant que DoctorsWindow est ouverte
        }
        private void BtnSpecialities_Click(object sender, RoutedEventArgs e)
        {
            var spWindow = new SpecialitiesWindow();
            spWindow.Owner = this;
            spWindow.ShowDialog();
        }
        private void BtnAddAppointment_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Ouvrir formulaire RDV (ajout)...");
        private void BtnEditAppointment_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Ouvrir formulaire RDV (édition)...");
        private void BtnDeleteAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (dgAppointments.SelectedItem is AppointmentListItem item)
            {
                if (MessageBox.Show($"Supprimer RDV {item.PatientName} - {item.StartAt:g} ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using var db = new ClinicContext();
                    var ap = db.Appointments.Find(item.Id);
                    if (ap != null)
                    {
                        db.Appointments.Remove(ap);
                        db.SaveChanges();
                        RefreshAppointments();
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadFiltersAndAppointments();

        // Classe interne pour le DataGrid
        private class AppointmentListItem
        {
            public int Id { get; set; }
            public DateTime StartAt { get; set; }
            public DateTime EndAt { get; set; }
            public int DoctorId { get; set; }
            public string DoctorName { get; set; }
            public int PatientId { get; set; }
            public string PatientName { get; set; }
            public string Notes { get; set; }
            public string ShortNotes => string.IsNullOrWhiteSpace(Notes) ? "" : (Notes.Length > 80 ? Notes.Substring(0, 77) + "..." : Notes);
        }
    }
}
