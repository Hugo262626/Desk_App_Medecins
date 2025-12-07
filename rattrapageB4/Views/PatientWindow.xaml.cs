using System.Linq;
using System.Windows;
using rattrapageB4.Models;

namespace rattrapageB4.Views
{
    public partial class PatientWindow : Window
    {
        Patient selected;

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

            if (PatientExists(TxtLastName.Text.Trim(), TxtFirstName.Text.Trim(), TxtPhone.Text.Trim()))
            {
                MessageBox.Show("Un patient avec le même nom, prénom et numéro de téléphone existe déjà.");
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
                LastName = TxtLastName.Text,
                FirstName = TxtFirstName.Text,
                Phone = TxtPhone.Text,
                Email = TxtEmail.Text
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

            if (PatientExists(TxtLastName.Text.Trim(), TxtFirstName.Text.Trim(), TxtPhone.Text.Trim()))
            {
                MessageBox.Show("Un patient avec le même nom, prénom et numéro de téléphone existe déjà.");
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

            p.LastName = TxtLastName.Text;
            p.FirstName = TxtFirstName.Text;
            p.Phone = TxtPhone.Text;
            p.Email = TxtEmail.Text;

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
                (excludeId == null || p.Id != excludeId)
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

