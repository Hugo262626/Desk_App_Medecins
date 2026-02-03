using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace rattrapageB4.Models
{
    public class Speciality
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(15)]
        public string Name { get; set; }

        public List<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
