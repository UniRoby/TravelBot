// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using CoreBotCLU.Models;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T>
        where T : Dialog
    {
        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base(conversationState, userState, dialog, logger)
        {
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    Storage storage = new Storage();
                    string adaptiveCardJson = storage.GetCardFromStorage("container-cards-progetto-cloud", "welcomeCard.json");
                    var welcomeCard = CreateDynamicCardAttachment(adaptiveCardJson);
                    //var welcomeCard = CreateAdaptiveCardAttachment();
                    var response = MessageFactory.Attachment(welcomeCard, ssml: "Benvenuto su Travel BOT!");
                    await turnContext.SendActivityAsync(response, cancellationToken);
                    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                }
            }
        }


        public Attachment CreateDynamicCardAttachment(string adaptiveCardJson)
        {
            dynamic obj = JsonConvert.DeserializeObject(adaptiveCardJson);

            // Crea un Attachment utilizzando l'oggetto dinamico come contenuto
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = obj
            };

            return adaptiveCardAttachment;
        }
    }
}
