using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using rattrapageB4.Models;

namespace rattrapageB4.Views
{
    public partial class PatientWindow : Window
    {
        Patient selected;

        // Lettres (accents OK) + espace + tiret + apostrophe (=> pas de chiffres)
        private static readonly Regex AllowedNameChars =
            new Regex(@"^[\p{L}\s\-']+$", RegexOptions.Compiled);

        public PatientWindow()
        {
            InitializeComponent();
            LoadPatients();
        }

        private void LoadPatients()
        {
            using var db = new ClinicContext();
            PatientGrid.ItemsSource = db.Patients.OrderBy(p => p.LastName).ToList();
        }

        private void PatientGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selected = PatientGrid.SelectedItem as Patient;

            if (selected == null) return;

            TxtLastName.Text = selected.LastName;
            TxtFirstName.Text = selected.FirstName;
            TxtPhone.Text = selected.Phone;
            TxtEmail.Text = selected.Email;
        }

        // Bloque chiffres et caractères interdits à la saisie (Nom/Prénom)
        private void Name_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !AllowedNameChars.IsMatch(e.Text);
        }

        // Bloque le collage si texte collé contient des caractères interdits
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

        private void AddPatient_Click(object sender, RoutedEventArgs e)
        {
            if (!AreFieldsFilled())
            {
                MessageBox.Show("Tous les champs sont obligatoires.");
                return;
            }

            if (TxtLastName.Text.Length > 20 || TxtFirstName.Text.Length > 20)
            {
                MessageBox.Show("Nom et Prénom doivent contenir maximum 20 caractères.");
                return;
            }

            // Validation lettres uniquement (pas de chiffres)
            if (!AllowedNameChars.IsMatch(TxtLastName.Text.Trim()) || !AllowedNameChars.IsMatch(TxtFirstName.Text.Trim()))
            {
                MessageBox.Show("Nom et Prénom invalides : lettres uniquement (espaces, tirets et apostrophes autorisés).");
                return;
            }

            if (PatientExists(TxtLastName.Text.Trim(), TxtFirstName.Text.Trim(), TxtPhone.Text.Trim()))
            {
                MessageBox.Show("Un patient avec le même nom, prénom et numéro de téléphone existe déjà.");
                return;
            }

            if (TxtEmail.Text.Length > 35)
            {
                MessageBox.Show("L'email ne doit pas dépasser 35 caractères.");
                return;
            }

            if (!IsValidEmail(TxtEmail.Text))
            {
                MessageBox.Show("Email invalide.");
                return;
            }

            if (!IsValidPhone(TxtPhone.Text))
            {
                MessageBox.Show("Numéro de téléphone invalide (10 chiffres et doit commencer par 0).");
                return;
            }

            using var db = new ClinicContext();

            var p = new Patient
            {
                LastName = TxtLastName.Text.Trim(),
                FirstName = TxtFirstName.Text.Trim(),
                Phone = TxtPhone.Text.Trim(),
                Email = TxtEmail.Text.Trim()
            };

            db.Patients.Add(p);
            db.SaveChanges();
            LoadPatients();
        }

        private void UpdatePatient_Click(object sender, RoutedEventArgs e)
        {
            if (selected == null) return;

            if (!AreFieldsFilled())
            {
                MessageBox.Show("Tous les champs sont obligatoires.");
                return;
            }

            if (TxtLastName.Text.Length > 20 || TxtFirstName.Text.Length > 20)
            {
                MessageBox.Show("Nom et Prénom doivent contenir maximum 20 caractères.");
                return;
            }

            // Validation lettres uniquement (pas de chiffres)
            if (!AllowedNameChars.IsMatch(TxtLastName.Text.Trim()) || !AllowedNameChars.IsMatch(TxtFirstName.Text.Trim()))
            {
                MessageBox.Show("Nom et Prénom invalides : lettres uniquement (espaces, tirets et apostrophes autorisés).");
                return;
            }

            // IMPORTANT : exclure l'ID du patient modifié pour ne pas se bloquer soi-même
            if (PatientExists(TxtLastName.Text.Trim(), TxtFirstName.Text.Trim(), TxtPhone.Text.Trim(), excludeId: selected.Id))
            {
                MessageBox.Show("Un patient avec le même nom, prénom et numéro de téléphone existe déjà.");
                return;
            }

            if (TxtEmail.Text.Length > 35)
            {
                MessageBox.Show("L'email ne doit pas dépasser 35 caractères.");
                return;
            }

            if (!IsValidEmail(TxtEmail.Text))
            {
                MessageBox.Show("Email invalide.");
                return;
            }

            if (!IsValidPhone(TxtPhone.Text))
            {
                MessageBox.Show("Numéro de téléphone invalide (10 chiffres et doit commencer par 0).");
                return;
            }

            using var db = new ClinicContext();
            var p = db.Patients.Find(selected.Id);
            if (p == null) return;

            p.LastName = TxtLastName.Text.Trim();
            p.FirstName = TxtFirstName.Text.Trim();
            p.Phone = TxtPhone.Text.Trim();
            p.Email = TxtEmail.Text.Trim();

            db.SaveChanges();
            LoadPatients();
        }

        private bool PatientExists(string lastName, string firstName, string phone, int? excludeId = null)
        {
            using var db = new ClinicContext();

            return db.Patients.Any(p =>
                p.LastName.ToLower() == lastName.ToLower() &&
                p.FirstName.ToLower() == firstName.ToLower() &&
                p.Phone == phone &&
                (excludeId == null || p.Id != excludeId.Value)
            );
        }

        private void TxtPhone_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Annule si le caractère n'est pas un chiffre
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(email)
                    && email.Contains("@")
                    && email.Contains(".")
                    && email.IndexOf("@") < email.LastIndexOf(".");
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            // Doit être exactement 10 chiffres
            if (phone.Length != 10)
                return false;

            // Doit commencer par 0
            if (!phone.StartsWith("0"))
                return false;

            // Doit être composé uniquement de chiffres
            return phone.All(char.IsDigit);
        }

        private void DeletePatient_Click(object sender, RoutedEventArgs e)
        {
            if (selected == null) return;

            using var db = new ClinicContext();
            var p = db.Patients.Find(selected.Id);
            if (p == null) return;

            try
            {
                db.Patients.Remove(p);
                db.SaveChanges();
            }
            catch
            {
                MessageBox.Show("Impossible de supprimer : ce patient a déjà un rendez-vous !");
            }

            LoadPatients();
        }

        private bool AreFieldsFilled()
        {
            return !string.IsNullOrWhiteSpace(TxtLastName.Text)
                && !string.IsNullOrWhiteSpace(TxtFirstName.Text)
                && !string.IsNullOrWhiteSpace(TxtPhone.Text)
                && !string.IsNullOrWhiteSpace(TxtEmail.Text);
        }
    }
}
