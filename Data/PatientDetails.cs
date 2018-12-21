using System;

namespace EmergencyServicesBot.Data
{
    public static class PatientDetails
    {
        public static string PatientId { get; set; }
        public static string PatientName { get; set; }
        public static  DateTime AppointmentDate { get; set; }
        public static  AppointmentDetails? AppointmentAction { get; set; }
    }
}