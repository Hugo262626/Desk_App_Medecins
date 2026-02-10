using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using rattrapageB4.Models;

namespace rattrapageB4.Views
{
    public partial class SpecialityFormWindow : Window
    {
        private int? specialityId = null;

        // Lettres (accents OK) + espaces + tiret + apostrophe
        private static readonly Regex AllowedChars =
            new Regex(@"^[\p{L}\s\-']+$", RegexOptions.Compiled);

        public SpecialityFormWindow(int? id = null)
        {
            InitializeComponent();
            specialityId = id;

            if (specialityId.HasValue)
                LoadSpeciality();
        }

        private void LoadSpeciality()
        {
            using var db = new ClinicContext();
            var sp = db.Specialities.Find(specialityId.Value);
            if (sp != null) txtName.Text = sp.Name;
        }

        // Bloque les chiffres et caractères interdits à la saisie
        private void TxtName_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !AllowedChars.IsMatch(e.Text);
        }

        // Bloque le collage si le texte collé contient des caractères interdits
        private void TxtName_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            var pasted = (string)e.DataObject.GetData(typeof(string));
            if (string.IsNullOrWhiteSpace(pasted) || !AllowedChars.IsMatch(pasted))
                e.CancelCommand();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            var name = (txtName.Text ?? "").Trim();

            // Obligatoire
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Veuillez entrer un nom.");
                return;
            }

            // Max 15 caractères
            if (name.Length > 15)
            {
                MessageBox.Show("Le nom doit contenir au maximum 15 caractères.");
                return;
            }

            // Lettres uniquement (espaces/tiret/' autorisés) => pas de chiffres
            if (!AllowedChars.IsMatch(name))
            {
                MessageBox.Show("Nom invalide : lettres uniquement (espaces, tirets et apostrophes autorisés).");
                return;
            }

            using var db = new ClinicContext();

            // Unicité (insensible à la casse), en excluant l'élément modifié
            bool exists = db.Specialities.Any(s =>
                s.Name.ToLower() == name.ToLower() &&
                (!specialityId.HasValue || s.Id != specialityId.Value)
            );

            if (exists)
            {
                MessageBox.Show("Une spécialité avec ce nom existe déjà.");
                return;
            }

            Speciality sp;

            if (specialityId.HasValue)
            {
                sp = db.Specialities.Find(specialityId.Value);
                if (sp == null) return;
            }
            else
            {
                sp = new Speciality();
                db.Specialities.Add(sp);
            }

            sp.Name = name;

            try
            {
                db.SaveChanges();
            }
            catch
            {
                // Si index unique DB existe, il peut lever une exception
                MessageBox.Show("Impossible d'enregistrer : ce nom de spécialité existe déjà.");
                return;
            }

            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
