using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace rattrapageB4.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        [Required]
        [MaxLength(15)]
        public string LastName { get; set; }
        [Required]
        [MaxLength(15)]

        public int SpecialityId { get; set; }
        public Speciality Speciality { get; set; }

        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
        public string FullName => $"{LastName} {FirstName}";

    }
}
