using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using rattrapageB4.Models;

namespace rattrapageB4.Views
{
    public partial class MainWindow : Window
    {
        private List<AppointmentListItem> currentList = new List<AppointmentListItem>();

        private bool _isAdmin = false;

        // mot de passe hash (ex: "admin")
        private const string AdminPasswordSha256 = "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918";

        public MainWindow()
        {
            InitializeComponent();

            var workArea = SystemParameters.WorkArea;

            Width = workArea.Width * 0.95;
            Height = workArea.Height * 0.95;

            Left = workArea.Left + (workArea.Width - Width) / 2;
            Top = workArea.Top + (workArea.Height - Height) / 2;

            SetAdminMode(false);

            LoadFiltersAndAppointments();
        }

        // ===== ADMIN =====

        private void SetAdminMode(bool enabled)
        {
            _isAdmin = enabled;

            // boutons admin only (définis dans XAML)
            btnDoctors.IsEnabled = enabled;
            btnSpecialities.IsEnabled = enabled;

            // bouton déconnexion admin (défini dans XAML)
            // Visible seulement en admin
            if (btnLogoutAdmin != null)
                btnLogoutAdmin.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;
        }

        private static bool IsAdminComboPressed(KeyEventArgs e)
        {
            return (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift))
                   == (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift)
                   && e.Key == Key.A;
        }

        private static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        // Déclenché par PreviewKeyDown dans XAML
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Login admin via combo
            if (!IsAdminComboPressed(e))
                return;

            e.Handled = true;

            var dlg = new AdminPasswordWindow { Owner = this };
            if (dlg.ShowDialog() != true) return;

            var enteredHash = Sha256(dlg.Password);

            if (enteredHash == AdminPasswordSha256)
            {
                SetAdminMode(true);
                MessageBox.Show("Mode administrateur activé.");
            }
            else
            {
                MessageBox.Show("Mot de passe incorrect.");
            }
        }

        // Bouton "Déconnexion admin" (remplace Refresh)
        private void BtnLogoutAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin) return;

            if (MessageBox.Show("Se déconnecter du mode administrateur ?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SetAdminMode(false);
            }
        }

        // Recharge filtres + RDV (auto après fermeture des fenêtres)
        private void ReloadData()
        {
            using var db = new ClinicContext();

            cboDoctor.ItemsSource = db.Doctors
                .OrderBy(d => d.LastName)
                .Select(d => new { d.Id, FullName = (d.LastName ?? "") + " " + (d.FirstName ?? "") })
                .ToList();

            cboPatient.ItemsSource = db.Patients
                .OrderBy(p => p.LastName)
                .Select(p => new { p.Id, FullName = (p.LastName ?? "") + " " + (p.FirstName ?? "") })
                .ToList();

            cboDoctor.SelectedIndex = -1;
            cboPatient.SelectedIndex = -1;

            RefreshAppointments();
        }

        // ===== DATA =====

        private void LoadFiltersAndAppointments()
        {
            ReloadData();
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

        // ===== FILTRAGE =====

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            var filtered = currentList.AsEnumerable();

            if (cboDoctor.SelectedItem != null)
            {
                var doctorId = (int)cboDoctor.SelectedItem.GetType().GetProperty("Id")!.GetValue(cboDoctor.SelectedItem);
                filtered = filtered.Where(x => x.DoctorId == doctorId);
            }

            if (cboPatient.SelectedItem != null)
            {
                var patientId = (int)cboPatient.SelectedItem.GetType().GetProperty("Id")!.GetValue(cboPatient.SelectedItem);
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

        private void DgAppointments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgAppointments.SelectedItem is AppointmentListItem item)
            {
                var detail = new AppointmentDetailWindow(item.Id);
                detail.Owner = this;
                detail.ShowDialog();
            }
        }

        // ===== BOUTONS =====

        private void BtnPatients_Click(object sender, RoutedEventArgs e)
        {
            var win = new PatientWindow();
            win.Owner = this;
            win.ShowDialog();
            ReloadData();
        }

        private void BtnDoctors_Click(object sender, RoutedEventArgs e)
        {
            var win = new DoctorsWindow();
            win.Owner = this;
            win.ShowDialog();
            ReloadData();
        }

        private void BtnSpecialities_Click(object sender, RoutedEventArgs e)
        {
            var win = new SpecialitiesWindow();
            win.Owner = this;
            win.ShowDialog();
            ReloadData();
        }

        private void BtnManageAppointments_Click(object sender, RoutedEventArgs e)
        {
            var win = new AppointmentWindow();
            win.Owner = this;
            win.ShowDialog();
            ReloadData();
        }

        private void BtnDeleteAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (dgAppointments.SelectedItem is AppointmentListItem item)
            {
                if (MessageBox.Show($"Supprimer RDV {item.PatientName} - {item.StartAt:g} ?",
                        "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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

        // ===== CLASSE LISTE =====

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
