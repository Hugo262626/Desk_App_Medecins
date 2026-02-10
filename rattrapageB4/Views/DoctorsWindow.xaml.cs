using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using rattrapageB4.Models;

namespace rattrapageB4.Views
{
    public partial class DoctorsWindow : Window
    {
        private Doctor selected;

        // Lettres (accents OK) + espace + tiret + apostrophe
        private static readonly Regex AllowedNameChars =
            new Regex(@"^[\p{L}\s\-']+$", RegexOptions.Compiled);

        public DoctorsWindow()
        {
            InitializeComponent();
            LoadSpecialities();
            LoadDoctors();
        }

        private void LoadDoctors()
        {
            using var db = new ClinicContext();
            dgDoctors.ItemsSource = db.Doctors
                                     .Include(d => d.Speciality)
                                     .OrderBy(d => d.LastName)
                                     .ThenBy(d => d.FirstName)
                                     .ToList();
        }

        private void LoadSpecialities()
        {
            using var db = new ClinicContext();
            cboSpeciality.ItemsSource = db.Specialities.OrderBy(s => s.Name).ToList();
            if (cboSpeciality.Items.Count > 0) cboSpeciality.SelectedIndex = 0;
        }

        private void DgDoctors_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selected = dgDoctors.SelectedItem as Doctor;
            if (selected == null) return;

            txtLastName.Text = selected.LastName;
            txtFirstName.Text = selected.FirstName;

            var specId = selected.SpecialityId;
            cboSpeciality.SelectedItem = ((System.Collections.Generic.IEnumerable<Speciality>)cboSpeciality.ItemsSource)
                .FirstOrDefault(s => s.Id == specId);
        }

        // Bloque chiffres/caractères interdits à la saisie
        private void Name_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !AllowedNameChars.IsMatch(e.Text);
        }

        // Bloque collage si le texte collé contient des caractères interdits
        private void Name_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            var pasted = (string)e.DataObject.GetData(typeof(string));
            if (string.IsNullOrWhiteSpace(pasted) || !AllowedNameChars.IsMatch(pasted))
                e.CancelCommand();
        }

        private bool ValidateForm()
        {
            var ln = (txtLastName.Text ?? "").Trim();
            var fn = (txtFirstName.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(ln) || string.IsNullOrWhiteSpace(fn) || cboSpeciality.SelectedItem == null)
            {
                MessageBox.Show("Veuillez remplir tous les champs.");
                return false;
            }

            if (!AllowedNameChars.IsMatch(ln) || !AllowedNameChars.IsMatch(fn))
            {
                MessageBox.Show("Nom/Prénom invalides : lettres uniquement (espaces, tirets et apostrophes autorisés).");
                return false;
            }

            return true;
        }

        // Unicité : Nom + Prénom + Spécialité
        private bool DoctorExists(string lastName, string firstName, int? excludeId = null)
        {
            using var db = new ClinicContext();

            var ln = lastName.Trim().ToLower();
            var fn = firstName.Trim().ToLower();

            return db.Doctors.Any(d =>
                d.LastName.ToLower() == ln &&
                d.FirstName.ToLower() == fn &&
                (excludeId == null || d.Id != excludeId.Value)
            );
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            var ln = txtLastName.Text.Trim();
            var fn = txtFirstName.Text.Trim();
            var specId = ((Speciality)cboSpeciality.SelectedItem).Id;

            if (DoctorExists(ln, fn))
            {
                MessageBox.Show("Ce medecin est déjà enregistré.");
                return;
            }

            using var db = new ClinicContext();
            db.Doctors.Add(new Doctor
            {
                LastName = ln,
                FirstName = fn,
                SpecialityId = specId
            });

            try
            {
                db.SaveChanges();
            }
            catch
            {
                MessageBox.Show("Ce medecin est déjà enregistré.");
                return;
            }

            LoadDoctors();
            ClearForm();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (selected == null)
            {
                MessageBox.Show("Sélectionner un médecin à modifier.");
                return;
            }

            if (!ValidateForm()) return;

            var ln = txtLastName.Text.Trim();
            var fn = txtFirstName.Text.Trim();
            var specId = ((Speciality)cboSpeciality.SelectedItem).Id;

            if (DoctorExists(ln, fn, excludeId: selected.Id))
            {
                MessageBox.Show("Ce medecin est déjà enregistré.");
                return;
            }

            using var db = new ClinicContext();
            var doctor = db.Doctors.Find(selected.Id);
            if (doctor == null) return;

            doctor.LastName = ln;
            doctor.FirstName = fn;
            doctor.SpecialityId = specId;

            try
            {
                db.SaveChanges();
            }
            catch
            {
                MessageBox.Show("Ce medecin est déjà enregistré.");
                return;
            }

            LoadDoctors();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (selected == null) return;

            if (MessageBox.Show($"Supprimer le médecin {selected.LastName} {selected.FirstName} ?",
                    "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            using var db = new ClinicContext();
            var doctor = db.Doctors.Find(selected.Id);
            if (doctor == null) return;

            try
            {
                db.Doctors.Remove(doctor);
                db.SaveChanges();
            }
            catch
            {
                MessageBox.Show("Impossible de supprimer : ce médecin a déjà des rendez-vous !");
            }

            LoadDoctors();
            ClearForm();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            dgDoctors.SelectedItem = null;
            selected = null;

            txtLastName.Text = "";
            txtFirstName.Text = "";

            if (cboSpeciality.Items.Count > 0) cboSpeciality.SelectedIndex = 0;
        }
    }
}
