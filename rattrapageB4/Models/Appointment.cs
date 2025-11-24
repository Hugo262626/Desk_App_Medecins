using System;

namespace rattrapageB4.Data.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string Notes { get; set; }
    }
}
