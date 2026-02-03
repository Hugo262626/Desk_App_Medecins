using System.Collections.Generic;

namespace rattrapageB4.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int SpecialityId { get; set; }
        public Speciality Speciality { get; set; }

        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
        public string FullName => $"{LastName} {FirstName}";

    }
}
