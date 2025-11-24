using System.Collections.Generic;

namespace rattrapageB4.Data.Models
{
    public class Speciality
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
