// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using TravelBot.Utils;
using TravelBot.Repository;
using TravelBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using AdaptiveCards;
using System.ComponentModel;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Globalization;


namespace TravelBot.Dialogs
{
    public class EmailDialog : CancelAndHelpDialog
    {
        
        private const string MailStepMsgText = "Inserisci l'email da rimuovere: ";
        private const string RepromptMsgText = "L'email non è valida o non è presente, reinseriscine un'altra.";
        private FlightsDemandRepository flightsDemandRepository = new FlightsDemandRepository();

        public EmailDialog()
            : base(nameof(EmailDialog))
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt("emailPrompt", ValidateEmailAsync));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                EmailStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
               
            }));


            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> EmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
          
            var userEmail="";

            if (stepContext.Options == null)
            {
                return await stepContext.PromptAsync("emailPrompt", new PromptOptions { Prompt = MessageFactory.Text(MailStepMsgText), RetryPrompt = MessageFactory.Text(RepromptMsgText) }, cancellationToken);
            }
            else
            {
                userEmail = (string)stepContext.Options;
            }

            if (!EmailExists(userEmail))
            {

                return await stepContext.PromptAsync("emailPrompt", new PromptOptions { Prompt = MessageFactory.Text("Email non presente o non valida, inseriscine un'altra"), RetryPrompt = MessageFactory.Text(RepromptMsgText) }, cancellationToken);
            }
            

            return await stepContext.NextAsync(userEmail, cancellationToken);
        }


        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           
            var userEmail = (string)stepContext.Options;
            var email = (string)stepContext.Result;

            userEmail = email;
            var messageText = $"Perfavore conferma, vuoi rimuovere le notifiche di prezzo per l'email: {userEmail} ?";
           
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput),
                RetryPrompt = MessageFactory.Text("Per favore, seleziona un'opzione valida."),
                Choices = new List<Choice>
                    {
                        new Choice("Si"),
                        new Choice("No")
                    },
                Style = ListStyle.Auto, 
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

       

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var result = (FoundChoice)stepContext.Result;
            var userEmail = (string)stepContext.Options;

            if (result.Value == "Si")
            {
                //Metodo delete a DB
                DeleteEmail(userEmail);
                await stepContext.Context.SendActivityAsync("Email rimossa");
            }
            else if (result.Value == "No")
            {
                await stepContext.Context.SendActivityAsync("Email non rimossa, continuerai a ricevere aggiornamenti");
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }



        private bool EmailExists(string email)
        {
           
            return this.flightsDemandRepository.EmailExists(email);
        }

        private void DeleteEmail(string email)
        {
            this.flightsDemandRepository.RemoveEmail(email);
        }

        private async Task<bool> ValidateEmailAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var emailRegex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");


            if (emailRegex.IsMatch(promptContext.Recognized.Value) && EmailExists(promptContext.Recognized.Value))
            {
                // Email valida
                return await Task.FromResult(true);
            }
            else
            {
                // Email non valida
                return await Task.FromResult(false);
            }
        }

    }
}
