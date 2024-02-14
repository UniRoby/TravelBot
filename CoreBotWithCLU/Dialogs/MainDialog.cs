// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using CoreBotCLU.Repository;
using CoreBotCLU.Models;
using System.Globalization;
using CoreBotCLU.Utils;
using System.ComponentModel;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IO;



namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly FlightBookingRecognizer _cluRecognizer;
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(FlightBookingRecognizer cluRecognizer, BookingDialog bookingDialog, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _cluRecognizer = cluRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(bookingDialog);
            AddDialog(new EmailDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_cluRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("AVVISO: CLU non è configurato. per abilitare tutte le funzionalità,  aggiungi 'CluProjectName', 'CluDeploymentName', 'CluAPIKey' e 'CluAPIHostName' all'interno del file appsettings.json ", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            CultureInfo ci = new CultureInfo("it-IT");
            var weekLaterDate = DateTime.Now.AddDays(7).ToString("d MMMM yyyy", ci);
            var messageText = stepContext.Options?.ToString() ?? $"Dici qualcosa tipo \"Cerca un volo da Milano a Napoli il {weekLaterDate} per una persona\"";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_cluRecognizer.IsConfigured)
            {
                // CLU is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
                //return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("AVVISO: CLU non è configurato. per abilitare tutte le funzionalità,  aggiungi 'CluProjectName', 'CluDeploymentName', 'CluAPIKey' e 'CluAPIHostName' all'interno del file appsettings.json ", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Call CLU and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var cluResult = await _cluRecognizer.RecognizeAsync<FlightBooking>(stepContext.Context, cancellationToken);
            switch (cluResult.GetTopIntent().intent)
            {
                case FlightBooking.Intent.BookFlight:

                    Console.Write(cluResult.GetTopIntent().intent);
                  
                    // Initialize BookingDetails with any entities we may have found in the response.
                    var bookingDetails = new BookingDetails()
                    {
                        Destination = cluResult.Entities.GetToCity(),
                        Origin = cluResult.Entities.GetFromCity(),
                        TravelDate = cluResult.Entities.GetFlightDate(),
                        ReturnDate = cluResult.Entities.GetReturnFlightDate(),
                        PassengersNumber = cluResult.Entities.GetPassengersNumber(),
                        Budget = cluResult.Entities.GetBudget(),

                    };

                    return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);

                case FlightBooking.Intent.GetEmail:
                  
                    var email = cluResult.Entities.GetEmail();
                 
                    return await stepContext.BeginDialogAsync(nameof(EmailDialog), email, cancellationToken);
                   
                case FlightBooking.Intent.Cancel:
                    // Initialize BookingDetails with any entities we may have found in the response.


                    var exitMessageText = "Annullo...";
                    var exitMessage = MessageFactory.Text(exitMessageText, exitMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(exitMessage, cancellationToken);
                    break;

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Scusa, non ho capito. Chiedimelo in modo diverso (intent was {cluResult.GetTopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            // the Result here will be null.
            if (stepContext.Result is BookingDetails result)
            {
                
                var timeProperty = new TimexProperty(result.TravelDate);
                var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                var messageText = $"Ho prenotato il volo per {result.Destination} in partenza da {result.Origin} il {travelDateMsg}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }
            else if ((string)stepContext.Result == "Termina")
            {
                var thanksCard = CreateThanksCardAttachment();
                var response = MessageFactory.Attachment(thanksCard, ssml: "Grazie da Travel BOT!");
                await stepContext.Context.SendActivityAsync(response, cancellationToken);
            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = "Che altro posso fare per te?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);

        }


        private Attachment CreateThanksCardAttachment()
        {
            var adaptiveCardJson = File.ReadAllText("C:\\Users\\User\\Source\\Repos\\CloudComputing\\CoreBotWithCLU\\Cards\\thanksCard.json");
            dynamic obj = JsonConvert.DeserializeObject(adaptiveCardJson);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = obj
            };

            return adaptiveCardAttachment;
        }
    }
 }
