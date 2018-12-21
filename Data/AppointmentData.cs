using EmergencyServicesBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmergencyServicesBot.Data
{
    public static class AppointmentData
    {
        private static List<Appointment> AppointmentList { get; set; }

        public static void BookAppointment(Appointment appointment)
        {
            appointment.IsActive = true;
            AppointmentList.Add(appointment);
        }

        public static void CancelAppointment(Appointment appointment)
        {
            var app = AppointmentList.FirstOrDefault(x =>
                x.AppointmentId == appointment.AppointmentId);

            if (app != null)
            {
                app.IsActive = false;
            }
        }

        public static List<Appointment> ShowBookedAppointment(int patientId)
        {
            return AppointmentList.Where(x => x.PatientId == patientId && x.IsActive == true).ToList();
        }

        public static void UpdateAppointment(Appointment appointment)
        {
            //var app = AppointmentList.FirstOrDefault(x =>
            //    x.PatientId == appointment.PatientId && x.AppointmentDateTime == appointment.AppointmentDateTime);

            //if (app != null)
            //{
            //    app. = false;
            //}
        }

        public static void LoadDefaultData()
        {
            AppointmentList = new List<Appointment>();
            AppointmentList.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                PatientId = 102,
                AppointmentDateTime = DateTime.Now.AddDays(-1).AddHours(-12),
                IsActive = true
            });
            AppointmentList.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                PatientId = 101,
                AppointmentDateTime = DateTime.Now.AddDays(-1).AddHours(-11),
                IsActive = true
            });
            AppointmentList.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                PatientId = 101,
                AppointmentDateTime = DateTime.Now.AddDays(-1).AddHours(-2),
                IsActive = true
            });
            AppointmentList.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                PatientId = 102,
                AppointmentDateTime = DateTime.Now.AddDays(-2).AddHours(-17),
                IsActive = true
            });
            AppointmentList.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                PatientId = 102,
                AppointmentDateTime = DateTime.Now.AddDays(-3).AddHours(-23),
                IsActive = true
            });
            AppointmentList.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                PatientId = 103,
                AppointmentDateTime = DateTime.Now.AddDays(-1).AddHours(-5),
                IsActive = true
            });
            AppointmentList.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                PatientId = 103,
                AppointmentDateTime = DateTime.Now.AddDays(-2).AddHours(-9),
                IsActive = true
            });
            AppointmentList.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                PatientId = 102,
                AppointmentDateTime = DateTime.Now.AddDays(-4).AddHours(-4),
                IsActive = true
            });
            AppointmentList.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                PatientId = 103,
                AppointmentDateTime = DateTime.Now.AddDays(-5).AddHours(-3),
                IsActive = true
            });
            AppointmentList.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                PatientId = 101,
                AppointmentDateTime = DateTime.Now.AddDays(-2).AddHours(-20),
                IsActive = true
            });
            AppointmentList.Add(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                PatientId = 101,
                AppointmentDateTime = DateTime.Now.AddDays(-2).AddHours(-13),
                IsActive = true
            });
        }
    }
}