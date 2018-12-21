using AdaptiveCards;
using EmergencyServicesBot.Infra;
using Newtonsoft.Json;
using System.Threading;

namespace EmergencyServicesBot
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System;
    using System.Threading.Tasks;

    [Serializable]
    public class DateTimeAdaptiveDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Please select appointment date");
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message != null && message.Value != null)
            {
                DateTimeInp input = JsonConvert.DeserializeObject<DateTimeInp>(message.Value.ToString());
                var toDateTime = input.ToDateTime();
                if (toDateTime != null)
                {
                    context.Done(toDateTime.ToString());
                }
            }
            else
            {
                var json = await Utility.GetCardText("DateTimePicker");
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

                await context.PostAsync(reply, CancellationToken.None);

                context.Wait(MessageReceivedAsync);
            }
        }

    }
    internal class DateTimeInp
    {
        public string DateVal { get; set; }
        public string TimeVal { get; set; }

        public DateTime? ToDateTime()
        {
            string fullDateTime = DateVal + " " + TimeVal;
            return DateTime.TryParse(fullDateTime, out DateTime toDateTime) ? (DateTime?)toDateTime : null;
        }
    }

}
