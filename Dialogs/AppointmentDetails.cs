using AdaptiveCards;
using EmergencyServicesBot.Data;
using EmergencyServicesBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EmergencyServicesBot
{
    [Serializable]
    public class AppointmentDetailsDialog : IDialog<string>
    {
        private string SelectedOption { get; set; }
        public async Task StartAsync(IDialogContext context)
        {
            var message = context.Activity as IMessageActivity;
            SelectedOption = message.Text;

            try
            {
                context.Wait(this.MessageReceivedAsync);
            }
            catch (FormCanceledException ex)
            {
                await context.PostAsync(
                    $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}");
            }
        }
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if (message.Value != null && message.Value.ToString().Trim().Length > 0)
            {
                // Cancel appointment
                var obj = JsonConvert.DeserializeObject<Appointment>(message.Value.ToString());
                AppointmentData.CancelAppointment(obj);
                await context.PostAsync("Appointment cancelled for the date : "+obj.AppointmentDateTime.ToString(DateTimeFormatInfo.InvariantInfo));
                context.Done("Cancelled Appointment");
            }
            else if (SelectedOption != null && SelectedOption.Trim().Length > 0)
            {
                await HandleAppointment(context, SelectedOption);
            }
            else
            {
                await context.PostAsync("Getting patient details");
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task<IMessageActivity> HandleAppointment(IDialogContext context, string appointment)
        {
            Enum.TryParse(appointment, out AppointmentDetails details);
            PatientDetails.AppointmentAction = details;
            switch (details)
            {
                case AppointmentDetails.BookAppointment:
                    await context.Forward(new DateTimeAdaptiveDialog(), this.AppointmentDateResultDialogAsync, null, CancellationToken.None);
                    break;

                case AppointmentDetails.CancelAppointment:
                    await context.PostAsync(await GetAppointmentCancelCard(context, AppointmentData.ShowBookedAppointment(Convert.ToInt32(PatientDetails.PatientId))));
                    break;

                case AppointmentDetails.ShowBookedAppointment:
                    await context.PostAsync(await GetAppointmentListCard(context, AppointmentData.ShowBookedAppointment(Convert.ToInt32(PatientDetails.PatientId))));
                    context.Done(appointment);
                    break;

                case AppointmentDetails.UpdateAppointment:
                    return await GetAppointmentListCard(context, AppointmentData.ShowBookedAppointment(Convert.ToInt32(PatientDetails.PatientId)));

                case AppointmentDetails.Done:
                    context.Done(appointment);
                    break;
            }

            return null;
        }

        public virtual async Task AppointmentDateResultDialogAsync(IDialogContext context, IAwaitable<string> appointmentDate)
        {
            var datetime = await appointmentDate;
            PatientDetails.AppointmentDate = Convert.ToDateTime(datetime);
            AppointmentData.BookAppointment(new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                PatientId = Convert.ToInt32(PatientDetails.PatientId),
                AppointmentDateTime = PatientDetails.AppointmentDate
            });
            await context.PostAsync(String.Format("Hello {0}, Your appointment is booked on :{1}. Reference number : {2}", PatientDetails.PatientName, PatientDetails.AppointmentDate.ToString(DateTimeFormatInfo.InvariantInfo), Guid.NewGuid().ToString().Substring(0, 5)));
            context.Done("Appointment booked");
        }

        private async Task<IMessageActivity> GetAppointmentListCard(IDialogContext context, List<Appointment> appointments)
        {
            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            var customColumns = new List<AdaptiveColumn>()
            {
                new AdaptiveColumn()
                {
                    Items = new List<AdaptiveElement>()
                    {
                        new AdaptiveTextBlock()
                        {
                            Text = "Patient Id",
                            IsSubtle = true,
                            Weight = AdaptiveTextWeight.Bolder
                        }
                    }
                },
                new AdaptiveColumn()
                {
                    Items = new List<AdaptiveElement>()
                    {
                        new AdaptiveTextBlock()
                        {
                            Text = "Appointment Date",
                            IsSubtle = true,
                            Weight = AdaptiveTextWeight.Bolder
                        }
                    }
                }
            };

            foreach (var appointment in appointments)
            {
                customColumns[0].Items.Add(
                    new AdaptiveTextBlock()
                    {
                        Text = appointment.PatientId.ToString(),
                        Spacing = AdaptiveSpacing.Small
                    });

                customColumns[1].Items.Add(
                    new AdaptiveTextBlock()
                    {
                        Text = appointment.AppointmentDateTime.ToString(DateTimeFormatInfo.InvariantInfo),
                        Spacing = AdaptiveSpacing.Small
                    });
            }

            adaptiveCard.Body.Add(new AdaptiveColumnSet()
            {
                Spacing = AdaptiveSpacing.Medium,
                Separator = true,
                Columns = customColumns
            });

            // serialize the card to JSON
            string json = adaptiveCard.ToJson();

            var results = AdaptiveCard.FromJson(json);
            var card = results.Card;

            var attachment = new Attachment()
            {
                Content = card,
                ContentType = AdaptiveCard.ContentType,
                Name = "Appointment List"
            };

            var reply = context.MakeMessage();
            reply.Attachments.Add(attachment);
            return reply;
        }

        private async Task<IMessageActivity> GetAppointmentCancelCard(IDialogContext context, List<Appointment> appointments)
        {
            await context.PostAsync("Click on date to cancel the appointment");

            var adaptiveCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            var customColumns = new List<AdaptiveColumn>()
            {
                new AdaptiveColumn()
                {
                    Items = new List<AdaptiveElement>()
                    {
                        new AdaptiveTextBlock()
                        {
                            Text = "Appointment Date",
                            IsSubtle = true
                        }
                    }
                }
            };

            adaptiveCard.Body.Add(new AdaptiveColumnSet()
            {
                Spacing = AdaptiveSpacing.Medium,
                Separator = true,
                Columns = customColumns
            });

            foreach (var appointment in appointments)
            {
                adaptiveCard.Actions.Add(new AdaptiveSubmitAction()
                {
                    Title = appointment.AppointmentDateTime.ToString(DateTimeFormatInfo.InvariantInfo),
                    DataJson = JsonConvert.SerializeObject(appointment),
                });
            }

            // serialize the card to JSON
            string json = adaptiveCard.ToJson();

            var results = AdaptiveCard.FromJson(json);
            var card = results.Card;

            var attachment = new Attachment()
            {
                Content = card,
                ContentType = AdaptiveCard.ContentType,
                Name = "Appointment List"
            };

            var reply = context.MakeMessage();
            reply.Attachments.Add(attachment);
            return reply;
        }
    }



    [Serializable]
    public class AppointmentDialog : IDialog<string>
    {
        private string PatientId { get; set; }

        public async Task StartAsync(IDialogContext context)
        {
            var message = context.Activity as IMessageActivity;
            PatientId = message.Text;
            try
            {
                context.Wait(this.MessageReceivedAsync);
            }
            catch (FormCanceledException ex)
            {
                await context.PostAsync(
                    $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}");
            }
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            PromptDialog.Choice(
                context,
                this.ForwardToDialog,
                (IEnumerable<AppointmentDetails>)Enum.GetValues(typeof(AppointmentDetails)),
                "Select Option",
                "Selected choice is not available . Please try again.");
        }
        private async Task ForwardToDialog(IDialogContext context, IAwaitable<AppointmentDetails> argument)
        {
            IMessageActivity message = context.MakeMessage();
            AppointmentDetails app = await argument;
            message.Text = app.ToString();
            await context.Forward(new AppointmentDetailsDialog(), this.ResumeAfterGpf, message, CancellationToken.None);
        }

        private async Task ResumeAfterGpf(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            context.Done(message);
        }
    }
}