using EmergencyServicesBot.Data;

namespace EmergencyServicesBot
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public enum AppointmentDetails
    {
        ShowBookedAppointment,
        BookAppointment,
        CancelAppointment,
        UpdateAppointment,
        Done
    }

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public string PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }

        public AppointmentDetails? AppointmentAction;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            AppointmentData.LoadDefaultData();
            context.Call(new PatientDetailsDialog(), this.PatientSearchResultReceivedAsync);
        }

        protected virtual async Task PatientSearchResultReceivedAsync(IDialogContext context, IAwaitable<string> resPatientId)
        {
            PatientDetails.PatientId = await resPatientId;
            await context.Forward(new AppointmentDialog(), this.AppointmentDialogResult, PatientId, CancellationToken.None);
        }
        
        public virtual async Task AppointmentDialogResult(IDialogContext context, IAwaitable<string> output)
        {
            var message = await output;
            if (message != null && message == "Done")
            {
                await context.PostAsync("Thanks for using bot service.");
                context.Done(this);
            }
            else
                await context.Forward(new AppointmentDialog(), this.AppointmentDialogResult, PatientId, CancellationToken.None);
        }        
    }
}