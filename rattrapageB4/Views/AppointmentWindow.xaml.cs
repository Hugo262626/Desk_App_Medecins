using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using rattrapageB4.Models;

namespace rattrapageB4.Views
{
    public partial class AppointmentWindow : Window
    {
        private Appointment selected;

        public AppointmentWindow()
        {
            InitializeComponent();
            LoadCombos();
            LoadAppointments();
            DatePicker.SelectedDate = DateTime.Today;
        }

        private void LoadCombos()
        {
            using var db = new ClinicContext();

            DoctorCombo.ItemsSource = db.Doctors
                .OrderBy(d => d.LastName).ThenBy(d => d.FirstName)
                .ToList();

            PatientCombo.ItemsSource = db.Patients
                .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
                .ToList();
        }

        private void LoadAppointments()
        {
            using var db = new ClinicContext();

            AppointmentGrid.ItemsSource = db.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .OrderBy(a => a.StartAt)
                .ToList();
        }

        private void AppointmentGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selected = AppointmentGrid.SelectedItem as Appointment;
            if (selected == null) return;

            DoctorCombo.SelectedValue = selected.DoctorId;
            PatientCombo.SelectedValue = selected.PatientId;

            DatePicker.SelectedDate = selected.StartAt.Date;
            TxtTime.Text = selected.StartAt.ToString("HH:mm");

            var duration = (int)(selected.EndAt - selected.StartAt).TotalMinutes;
            foreach (var item in DurationCombo.Items)
            {
                if (item is System.Windows.Controls.ComboBoxItem cbi &&
                    int.TryParse(cbi.Content?.ToString(), out int v) && v == duration)
                {
                    DurationCombo.SelectedItem = cbi;
                    break;
                }
            }

            TxtNotes.Text = selected.Notes;
        }

        private void AddAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetFormValues(out int doctorId, out int patientId, out DateTime start, out DateTime end, out string notes))
                return;

            if (HasDoctorConflict(doctorId, start, end, excludeAppointmentId: null))
            {
                MessageBox.Show("Conflit : ce médecin a déjà un rendez-vous sur ce créneau.");
                return;
            }

            using var db = new ClinicContext();
            db.Appointments.Add(new Appointment
            {
                DoctorId = doctorId,
                PatientId = patientId,
                StartAt = start,
                EndAt = end,
                Notes = notes
            });

            db.SaveChanges();
            LoadAppointments();
            ClearForm();
        }

        private void UpdateAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (selected == null) return;

            if (!TryGetFormValues(out int doctorId, out int patientId, out DateTime start, out DateTime end, out string notes))
                return;

            if (HasDoctorConflict(doctorId, start, end, excludeAppointmentId: selected.Id))
            {
                MessageBox.Show("Conflit : ce médecin a déjà un rendez-vous sur ce créneau.");
                return;
            }

            using var db = new ClinicContext();
            var appt = db.Appointments.Find(selected.Id);
            if (appt == null) return;

            appt.DoctorId = doctorId;
            appt.PatientId = patientId;
            appt.StartAt = start;
            appt.EndAt = end;
            appt.Notes = notes;

            db.SaveChanges();
            LoadAppointments();
        }

        private void DeleteAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (selected == null) return;

            using var db = new ClinicContext();
            var appt = db.Appointments.Find(selected.Id);
            if (appt == null) return;

            db.Appointments.Remove(appt);
            db.SaveChanges();

            LoadAppointments();
            ClearForm();
        }

        // Autorise uniquement chiffres et ":" à la saisie
        private void TxtTime_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9:]$");
        }

        private bool TryGetFormValues(out int doctorId, out int patientId, out DateTime start, out DateTime end, out string notes)
        {
            doctorId = 0;
            patientId = 0;
            start = default;
            end = default;

            notes = TxtNotes.Text?.Trim() ?? "";

            // Champs obligatoires
            if (DoctorCombo.SelectedValue == null || PatientCombo.SelectedValue == null)
            {
                MessageBox.Show("Choisir un médecin et un patient.");
                return false;
            }

            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Choisir une date.");
                return false;
            }

            if (DurationCombo.SelectedItem == null)
            {
                MessageBox.Show("Choisir une durée.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(notes))
            {
                MessageBox.Show("Le champ Notes est obligatoire.");
                return false;
            }

            // Heure 24h HH:mm
            var timeText = TxtTime.Text.Trim();
            if (!DateTime.TryParseExact(timeText, "HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTime))
            {
                MessageBox.Show("Heure invalide. Format attendu : HH:mm en 24h (ex: 09:30, 14:05).");
                return false;
            }

            // Durée
            var cbi = (System.Windows.Controls.ComboBoxItem)DurationCombo.SelectedItem;
            if (!int.TryParse(cbi.Content?.ToString(), out int durationMin) || durationMin <= 0)
            {
                MessageBox.Show("Durée invalide.");
                return false;
            }

            doctorId = (int)DoctorCombo.SelectedValue;
            patientId = (int)PatientCombo.SelectedValue;

            start = DatePicker.SelectedDate.Value.Date + parsedTime.TimeOfDay;
            end = start.AddMinutes(durationMin);

            if (end <= start)
            {
                MessageBox.Show("La fin doit être après le début.");
                return false;
            }

            return true;
        }

        private bool HasDoctorConflict(int doctorId, DateTime start, DateTime end, int? excludeAppointmentId)
        {
            using var db = new ClinicContext();

            return db.Appointments.Any(a =>
                a.DoctorId == doctorId &&
                (excludeAppointmentId == null || a.Id != excludeAppointmentId.Value) &&
                a.StartAt < end &&
                start < a.EndAt
            );
        }

        private void ClearForm()
        {
            AppointmentGrid.SelectedItem = null;
            selected = null;

            DoctorCombo.SelectedIndex = -1;
            PatientCombo.SelectedIndex = -1;

            DatePicker.SelectedDate = DateTime.Today;
            TxtTime.Text = "09:00";
            DurationCombo.SelectedIndex = 2; // 30 min (si ordre inchangé)
            TxtNotes.Text = "";
        }
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }
        private void DatePicker_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true; // bloque toute saisie clavier
        }

    }
}
