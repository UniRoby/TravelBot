// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using CoreBotCLU.Utils;
using System.ComponentModel;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class DateResolverDialog : CancelAndHelpDialog
    {

        public DateResolverDialog(string id = null)
            : base(id ?? nameof(DateResolverDialog))
        {
            AddDialog(new TextPrompt("departureDatePrompt", DatePromptValidatorAsync));
            AddDialog(new TextPrompt("returnDatePrompt", DatePromptValidatorAsync));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            BookingConverter bookingConverter = (BookingConverter)stepContext.Options;

            if(bookingConverter.PromptType=="R") {
               
                return await stepContext.PromptAsync("returnDatePrompt", new PromptOptions { Prompt = MessageFactory.Text("Quando vuoi tornare? "), RetryPrompt = MessageFactory.Text("La data inserita non è valida, per favore inseriscila di nuovo.") }, cancellationToken);
            }
            else if (bookingConverter.PromptType == "D")
            {
                
                return await stepContext.PromptAsync("departureDatePrompt", new PromptOptions { Prompt = MessageFactory.Text("Quando vuoi partire? "), RetryPrompt = MessageFactory.Text("La data inserita non è valida, per favore inseriscila di nuovo.") }, cancellationToken);
            }

            return await stepContext.NextAsync(bookingConverter.Date, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var date = (string)stepContext.Result;
            return await stepContext.EndDialogAsync(date, cancellationToken);
        }


        private async Task<bool> DatePromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            DateAndTimeConverter dateAndTimeConverter = new DateAndTimeConverter();

            var userDate = promptContext.Context.Activity.Text;

            if (dateAndTimeConverter.WordToDate(userDate) != null)
            {
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }

        }
    }
}
