using AdaptiveCards;
using EmergencyServicesBot.Infra;
using EmergencyServicesBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmergencyServicesBot.Data;

namespace EmergencyServicesBot
{
    [Serializable]
    public class PatientDetailsDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Please enter your Patient Id?");
            context.Wait(this.MessageReceivedAsync);
        }
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            /* If the message returned is a valid name, return it to the calling dialog. */
            if (message.Text != null && message.Text.Trim().Length > 0)
            {
                await context.PostAsync($"Looking for patient :{message.Text}..");
                try
                {
                    var patient = GetPatientDetails(message.Text);
                    if (patient != null)
                    {
                        PatientDetails.PatientName = patient.PatientName;
                        var json = await Utility.GetCardText("PatientInfo");

                        json = json.Replace("#PatientName", patient.PatientName);
                        json = json.Replace("#CreatedDate", DateTime.Now.ToString());
                        json = json.Replace("#PatientDetails", "In detail information about the patient. Need to add more details about patient illness and deceases.");
                        json = json.Replace("#PatientId", patient.PatientId.ToString());
                        json = json.Replace("#PatientDOB", DateTime.Now.AddYears(-25).ToString());
                        json = json.Replace("#PatientAddress", "Patient address.");
                        json = json.Replace("#PatientContactNumber", "Patient contact details");
                        json = json.Replace("#Age", "26");

                        var results = AdaptiveCard.FromJson(json);
                        var card = results.Card;

                        var attachment = new Attachment()
                        {
                            Content = card,
                            ContentType = AdaptiveCard.ContentType,
                            Name = "Requested Date Adaptive Card"
                        };

                        var reply = context.MakeMessage();
                        reply.Attachments.Add(attachment);

                        await context.PostAsync(reply);
                        context.Done(message.Text);
                    }
                    else
                    {
                        await context.PostAsync($"Oops! Patient with Id : {message.Text} is not found in database. Please enter valid patient Id.");
                        context.Wait(this.MessageReceivedAsync);
                    }
                }
                catch (FormCanceledException ex)
                {
                    await context.PostAsync($"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}");
                }
            }
            else
            {
                await context.PostAsync("I'm sorry, I don't understand your reply. What is your Patient Id? (e.g. 101, 105)?");

                context.Wait(this.MessageReceivedAsync);
            }
        }
        //private async Task SearchPatient(IDialogContext context, string patientId)
        //{
        //    var patient = GetPatientDetails(patientId);
        //    if (patient != null)
        //    {
        //        var patientCard = new AdaptiveCard();

        //        patientCard.Body.Add(new AdaptiveTextBlock()
        //        {
        //            Text = "Please Select A Category from the list",
        //            Size = AdaptiveTextSize.Default,
        //            Weight = AdaptiveTextWeight.Default
        //        });

        //        //AdaptiveImage img = new AdaptiveImage()
        //        //{
        //        //    UrlString = "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg",
        //        //    Size = AdaptiveImageSize.Large,
        //        //    Style = AdaptiveImageStyle.Default
        //        //};

        //        //patientCard.Body.Add(img);

        //        //AdaptiveFactSet factset = new AdaptiveFactSet()
        //        //{
        //        //    Facts = new List<AdaptiveFact>
        //        //    {
        //        //        new AdaptiveFact("Patient Id :", patient.PatientId.ToString()),
        //        //        new AdaptiveFact("Patient Name :", patient.PatientName),
        //        //        new AdaptiveFact("Patient SSN :", patient.SSN)
        //        //    }
        //        //};

        //        //patientCard.Body.Add(factset);

        //        Attachment attachment = new Attachment()
        //        {
        //            ContentType = AdaptiveCard.ContentType,
        //            Content = patientCard,
        //            Name = $"Card"
        //        };

        //        var reply = context.MakeMessage();
        //        reply.Attachments.Add(attachment);

        //        await context.PostAsync(reply);
        //        context.Done(true);
        //    }
        //    else
        //    {
        //        await context.PostAsync($"Oops! Patient with Id : {patientId} is not found in database.");
        //        context.Done(false);
        //    }
        //}

        public static Patient GetPatientDetails(string patientId)
        {
            var patientList = new List<Patient>
            {
                new Patient
                {
                    PatientId = 101,
                    PatientName = "Rahul",
                    SSN = "12345"
                },
                new Patient
                {
                    PatientId = 102,
                    PatientName = "Bharat",
                    SSN = "65421"
                },
                new Patient
                {
                    PatientId = 103,
                    PatientName = "Digvijay",
                    SSN = "785412"
                },
            };

            return patientList.FirstOrDefault(x => x.PatientId.ToString() == patientId);
        }
    }
}