// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TravelBot.Bots
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler
        where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
           
            await base.OnTurnAsync(turnContext, cancellationToken);

            var activity = turnContext.Activity;
           

            // Execute on incoming messages
            if (activity.Type == ActivityTypes.Message)
            {
                if (string.IsNullOrWhiteSpace(activity.Text) && activity.Value != null)
                {
                    activity.Text = JsonConvert.SerializeObject(activity.Value);
                }
            }


            // Save any state changes that might have occurred during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(turnContext.Activity.Text) && turnContext.Activity.Value != null)
            {
                

                JObject values = (turnContext.Activity.Value as JObject);

                var origin = values["originId"]?.ToString();
                var destination = values["destinationId"]?.ToString();
                var travelDate = values["dateId"]?.ToString();
                var returnDate = values["returnDateId"]?.ToString();
                var passengersNumber = values["numPassengerId"]?.ToString();
                var budget = values["budgetId"]?.ToString();

                string allValues = "{" + origin + "} " + "{" + destination + "} " + "{" + travelDate + "} " + "{" + returnDate + "} " + "{" + passengersNumber + "} " + "{" + budget + "}";

                Console.WriteLine("\n----------------------------------------------------------------------------------------");
                Console.WriteLine(allValues);
                Console.WriteLine("\n----------------------------------------------------------------------------------------");

                turnContext.Activity.Text = allValues;


            }
            // Esegui il dialogo passando i dati ottenuti
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);

        }
    }
}
