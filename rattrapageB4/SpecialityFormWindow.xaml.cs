using System.Linq;
using System.Windows;
using rattrapageB4.Models;

namespace rattrapageB4
{
    public partial class SpecialityFormWindow : Window
    {
        private int? specialityId = null;

        public SpecialityFormWindow(int? id = null)
        {
            InitializeComponent();
            specialityId = id;
            if (specialityId.HasValue) LoadSpeciality();
        }

        private void LoadSpeciality()
        {
            using var db = new ClinicContext();
            var sp = db.Specialities.Find(specialityId.Value);
            if (sp != null) txtName.Text = sp.Name;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Veuillez entrer un nom.");
                return;
            }

            using var db = new ClinicContext();
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

            sp.Name = txtName.Text.Trim();
            db.SaveChanges();
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
