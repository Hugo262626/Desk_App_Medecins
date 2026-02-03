using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace rattrapageB4.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string Phone { get; set; }

        [Required]
        [MaxLength(35)]
        public string Email { get; set; }

        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
        public string FullName => $"{LastName} {FirstName}";

    }
}
