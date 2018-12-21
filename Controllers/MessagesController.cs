namespace EmergencyServicesBot
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new RootDialog());
            }
            // Webchat: getting an "event" activity for our js code
            else if (activity.Type == ActivityTypes.Event && activity.ChannelId == "webchat")
            {
                var receivedEvent = activity.AsEventActivity();

                if ("welComeEvent".Equals(receivedEvent.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    await EchoLocaleAsync(activity, activity.Locale);
                }
            }
            // Sample for Skype: locale is provided in ContactRelationUpdate event
            else if (activity.Type == ActivityTypes.ContactRelationUpdate && activity.ChannelId == "skype")
            {
                await EchoLocaleAsync(activity, activity.Entities[0].Properties["locale"].ToString());
            }
            // Sample for emulator, to debug locales
            else if (activity.Type == ActivityTypes.ConversationUpdate && activity.ChannelId == "emulator")
            {
                foreach (var userAdded in activity.MembersAdded)
                {
                    if (userAdded.Id == activity.From.Id)
                    {
                        await EchoLocaleAsync(activity, "fr-FR");
                    }
                }
            }
            else
            {
                this.HandleSystemMessage(activity);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task EchoLocaleAsync(Activity activity, string inputLocale)
        {
            Activity reply = activity.CreateReply("Welcome, I'm Dr.Bot ! I can help with fix an appointment with Doctor.");
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            await connector.Conversations.SendToConversationAsync(reply);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}