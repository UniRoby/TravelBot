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
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;


namespace TravelBot.Dialogs
{
    public class CityDialogResolver : CancelAndHelpDialog
    {
        private const string DestinationStepMsgText = "Dove vuoi andare?";
        private const string OriginStepMsgText = "Da dove parti?";
        private const string RepromptMsgText = "La città inserita non ha un aeroporto, reinseriscine un'altra.";

        private const string DepartureDateaMsgText = "Quando vuoi partire?";
        private const string PassengersMsgText = "Quanti passeggeri?";
        private const string BudgetMsgText = "Qual è il tuo budget?";

 
        private IataInfo iataInfo;
        private bool alreadyContainsOrigin = false;
        private bool alreadyContainsDestination = false;
        private BookingConverter bookingConverter = new BookingConverter();
        public CityDialogResolver()
            : base(nameof(CityDialogResolver))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt), TextPromptValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new TextPrompt("departureDatePrompt", DatePromptValidatorAsync));
            AddDialog(new TextPrompt("returnDatePrompt", DatePromptValidatorAsync));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                DestinationStepAsync,
                DestinationIATAStepAsync,
                ShowSelectedIataDestinationAsync,
                OriginStepAsync,
                OriginIATAStepAsync,
                ShowSelectedIataOriginAsync,
                TravelDateStepAsync,
                ReturnDateStepAsync,
                PassengersStepAsync,
                ConfirmStepAsync,
                ResultStepAsync,
                HandleRepeateStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
            this.iataInfo = new IataInfo();
        }

        private async Task<DialogTurnResult> DestinationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;
            
            bookingDetails.Destination=this.bookingConverter.RimuoviPreposizioni(bookingDetails.Destination);

            await stepContext.Context.SendActivityAsync($"Destinazione: {bookingDetails.Destination}, Partenza: {bookingDetails.Origin}");
           
            if (bookingDetails.Destination == null || !CheckCity(bookingDetails.Destination))
            {
              
                var promptMessage = MessageFactory.Text(DestinationStepMsgText, DestinationStepMsgText, InputHints.ExpectingInput);
                var repromptMessage = MessageFactory.Text(RepromptMsgText, RepromptMsgText, InputHints.ExpectingInput);

                
                return await stepContext.PromptAsync(nameof(TextPrompt),
                   new PromptOptions
                   {
                       Prompt = promptMessage,
                       RetryPrompt = repromptMessage,
                   }, cancellationToken);

            }
            

            return await stepContext.NextAsync(bookingDetails.Destination, cancellationToken);
        }

        private async Task<DialogTurnResult> DestinationIATAStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;
            bookingDetails.Destination = (string)stepContext.Result;
            bookingDetails.Destination = this.bookingConverter.RimuoviPreposizioni(bookingDetails.Destination);

            var city = ExtractCity(bookingDetails.Destination);
            this.iataInfo.IATAlistDestination = GetListIATACode(city);

            await stepContext.Context.SendActivityAsync($"Destinazione: {bookingDetails.Destination}");

            BookingConverter bookingConverter = new BookingConverter();

            this.alreadyContainsDestination = bookingConverter.ContainsIATA(bookingDetails.Destination, this.iataInfo.IATAlistDestination);

            if(this.alreadyContainsDestination)
            {
                await stepContext.Context.SendActivityAsync($"ContainsIATA: {bookingDetails.Destination}");
                this.iataInfo.destinationIATAIndex=bookingConverter.GetContainedIATA(bookingDetails.Destination, this.iataInfo.IATAlistDestination);
                this.iataInfo.allAirportsDestination = false;
                await stepContext.Context.SendActivityAsync($"IATA ottenuto: {this.iataInfo.IATAlistDestination[this.iataInfo.destinationIATAIndex]}");
            }
            else if (this.iataInfo.IATAlistDestination.Count >1)
            {
                var promptOptions = GetIataPromptOptions(this.iataInfo.IATAlistDestination, bookingDetails.Destination);
                return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
            }


            return await stepContext.NextAsync(bookingDetails.Destination, cancellationToken);
        }

        private async Task<DialogTurnResult> ShowSelectedIataDestinationAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
      
            var bookingDetails = (BookingDetails)stepContext.Options;

            if (stepContext.Result is FoundChoice result)
            {
                
                var choice = (FoundChoice)stepContext.Result;

                if (choice.Value == bookingDetails.Destination)
                {
                   
                    this.iataInfo.allAirportsDestination = true;
                   
                    return await stepContext.NextAsync(bookingDetails.Destination, cancellationToken);
                }
               else{
                    this.iataInfo.destinationIATAIndex = choice.Index;
                    this.iataInfo.allAirportsDestination = false;
                    
                    await stepContext.Context.SendActivityAsync($"Aeroporto scelto: {this.iataInfo.IATAlistDestination[this.iataInfo.destinationIATAIndex]}");
                    return await stepContext.NextAsync(bookingDetails.Destination, cancellationToken);
                }

            }
            else
            {
               
                bookingDetails.Destination = (string)stepContext.Result;
                await stepContext.Context.SendActivityAsync($"FLUSSO DI NON SCELTA PERCHE' PREVENTIVAMENTE OTTENUTO: {bookingDetails.Destination}");
                return await stepContext.NextAsync(bookingDetails.Destination, cancellationToken);
            }

        }

        private async Task<DialogTurnResult> OriginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

 
            var bookingDetails = (BookingDetails)stepContext.Options;

            bookingDetails.Destination = (string)stepContext.Result;
            bookingDetails.Destination = this.bookingConverter.RimuoviPreposizioni(bookingDetails.Destination);

            bookingDetails.Origin = this.bookingConverter.RimuoviPreposizioni(bookingDetails.Origin);

            if (bookingDetails.Origin == null || !CheckCity(bookingDetails.Origin))
            
            {
                var promptMessage = MessageFactory.Text(OriginStepMsgText, OriginStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(bookingDetails.Origin, cancellationToken);
        }

        private async Task<DialogTurnResult> OriginIATAStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;
            bookingDetails.Origin = (string)stepContext.Result;
            bookingDetails.Origin = this.bookingConverter.RimuoviPreposizioni(bookingDetails.Origin);

            this.iataInfo.IATAlistOrigin = GetListIATACode(bookingDetails.Origin);

            if (this.iataInfo.IATAlistOrigin.Count > 1)
            {
                var promptOptions = GetIataPromptOptions(this.iataInfo.IATAlistOrigin, bookingDetails.Origin);
                return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
            }


            return await stepContext.NextAsync(bookingDetails.Origin, cancellationToken);
        }

        private async Task<DialogTurnResult> ShowSelectedIataOriginAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
         
            var bookingDetails = (BookingDetails)stepContext.Options;

            if (stepContext.Result is FoundChoice result)
            {


                var choice = (FoundChoice)stepContext.Result;

              

                if (choice.Value == bookingDetails.Origin)
                {
                   
                    this.iataInfo.allAirportsOrigin = true;
                  
                    return await stepContext.NextAsync(bookingDetails.Origin, cancellationToken);
                }
                else
                {
                    this.iataInfo.originIATAIndex = choice.Index;
                    this.iataInfo.allAirportsOrigin = false;
                 
                    await stepContext.Context.SendActivityAsync($"Aeroporto scelto: {this.iataInfo.IATAlistOrigin[this.iataInfo.originIATAIndex]}");
                    return await stepContext.NextAsync(bookingDetails.Origin, cancellationToken);
                }

                
            }
            else
            {
                bookingDetails.Origin = (string)stepContext.Result;
                return await stepContext.NextAsync(bookingDetails.Origin, cancellationToken);
            }

        }


        private async Task<DialogTurnResult> TravelDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;

            DateAndTimeConverter converter = new DateAndTimeConverter();
            

            bookingDetails.Origin = (string)stepContext.Result;
             bookingDetails.Origin = this.bookingConverter.RimuoviPreposizioni(bookingDetails.Origin);

            if (bookingDetails.TravelDate == null || converter.WordToDate(bookingDetails.TravelDate)==null)
            {
                
                BookingConverter bookingConverter = new BookingConverter
                {
                    Date = bookingDetails.TravelDate,
                    PromptType = "D"
                };

                return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), bookingConverter, cancellationToken);
            }
            
            bookingDetails.TravelDate=converter.WordToDate(bookingDetails.TravelDate);

            await stepContext.Context.SendActivityAsync($"TravelDateStepAsync cityDialog: {bookingDetails.TravelDate}");

            return await stepContext.NextAsync(bookingDetails.TravelDate, cancellationToken);
        }

        private async Task<DialogTurnResult> ReturnDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;
            bookingDetails.TravelDate = (string)stepContext.Result;

            await stepContext.Context.SendActivityAsync($"ReturnDateStepAsync RIsultato travel date cityDialog: {bookingDetails.TravelDate}");

            DateAndTimeConverter converter = new DateAndTimeConverter();
            bookingDetails.TravelDate = converter.WordToDate(bookingDetails.TravelDate);

            if (bookingDetails.ReturnDate!=null && converter.WordToDate(bookingDetails.ReturnDate)==null)
            {
                BookingConverter bookingConverter = new BookingConverter
                {
                    Date = bookingDetails.ReturnDate,
                    PromptType = "R"
                };

                 return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), bookingConverter, cancellationToken);
            }

            bookingDetails.ReturnDate = converter.WordToDate(bookingDetails.ReturnDate);
            return await stepContext.NextAsync(bookingDetails.ReturnDate, cancellationToken);
        }

        private async Task<DialogTurnResult> PassengersStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;
            bookingDetails.ReturnDate = (string)stepContext.Result;
            DateAndTimeConverter converter = new DateAndTimeConverter();
            bookingDetails.ReturnDate = converter.WordToDate(bookingDetails.ReturnDate);

            if (bookingDetails.PassengersNumber != null)
            {
                WordNumberConverter wordNumberConverter = new WordNumberConverter();
                bookingDetails.PassengersNumber= wordNumberConverter.ConvertToNumbers(bookingDetails.PassengersNumber);
            }
            else 
            {
                bookingDetails.PassengersNumber = "1";
            }
            

            return await stepContext.NextAsync(bookingDetails.PassengersNumber, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;

            bookingDetails.PassengersNumber = (string)stepContext.Result;
            await stepContext.Context.SendActivityAsync($"ConfirmStepAsync  allAirportsOrigin: {this.iataInfo.allAirportsOrigin}  , allAirportsDestination: {this.iataInfo.allAirportsDestination}");

            var messageText = $"Perfavore conferma, vuoi andare a: " +
               $"{(this.iataInfo.allAirportsDestination || this.alreadyContainsDestination ? $" {bookingDetails.Destination}" : 
               $" {bookingDetails.Destination} " + this.iataInfo.IATAlistDestination[this.iataInfo.destinationIATAIndex])} " +
                $"da: {(this.iataInfo.allAirportsOrigin || this.alreadyContainsOrigin ? $" {bookingDetails.Origin}" : 
                $" {bookingDetails.Origin} " + this.iataInfo.IATAlistOrigin[this.iataInfo.originIATAIndex])} " +
                $"il: {bookingDetails.TravelDate} " +
                $"{(bookingDetails.ReturnDate != null ? $" Ritorno il: {bookingDetails.ReturnDate}" : "")}, " +
                $"numero passeggeri: {bookingDetails.PassengersNumber} {(bookingDetails.Budget != null ? $", " +
                $"budget: {bookingDetails.Budget}€" : "")}. E' corretto?";
            //var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput),
                RetryPrompt = MessageFactory.Text("Per favore, seleziona un'opzione valida."),
                Choices = new List<Choice>
                    {
                        new Choice("Si"),
                        new Choice("No"),
                        new Choice("Ricerca Fake")
                    },
                Style = ListStyle.Auto, // Puoi personalizzare lo stile dei pulsanti se necessario
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ResultStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var result = (FoundChoice)stepContext.Result;  
            var bookingDetails = (BookingDetails)stepContext.Options;

            CityDialogResult cityDialogResult = new CityDialogResult
            {
                promtResult = result.Value,
                bookingDetails = bookingDetails,
                iataInfo = this.iataInfo
            };

            

            WordNumberConverter wordNumber = new WordNumberConverter();
            if (bookingDetails.Budget == null)
            {
                bookingDetails.Budget = "0";
            }
            else
            {
                bookingDetails.Budget = wordNumber.NumberExtractor(bookingDetails.Budget);
            }

            if (result.Value == "Ricerca Fake")
            {
                
                cityDialogResult.isFake = true;
                await stepContext.Context.SendActivityAsync($"isFake: {cityDialogResult.isFake}");
                await stepContext.Context.SendActivityAsync($"travel date cityDialog: {bookingDetails.TravelDate}");
                return await stepContext.EndDialogAsync(cityDialogResult, cancellationToken);
            }

            else if (result.Value == "Si")
            {
                cityDialogResult.isFake = false;
                await stepContext.Context.SendActivityAsync($"isFake: {cityDialogResult.isFake}");
                return await stepContext.EndDialogAsync(cityDialogResult, cancellationToken);
            }
            else if (result.Value == "No")
            {

                cityDialogResult.isFake = true;

                var changeCard = CreateDynamicCardAttachment(bookingDetails,cityDialogResult);
                var response = MessageFactory.Attachment(changeCard, ssml: "Cambio");
                await stepContext.Context.SendActivityAsync(response, cancellationToken).ConfigureAwait(false);


                return await stepContext.PromptAsync("cardPrompt", new PromptOptions { Prompt = MessageFactory.Text("In attessa di conferma... ") });

            }

            return await stepContext.NextAsync(bookingDetails, cancellationToken);
        }

       
        private async Task<DialogTurnResult> HandleRepeateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            
                var bookingString = (string)stepContext.Result;
                BookingConverter converter = new BookingConverter();
                BookingDetails bookingDetails = converter.ParseFromString(bookingString);
                await stepContext.Context.SendActivityAsync($"RISULTATO BOTTONE CONFERMA: {bookingDetails.Destination}");

            return await stepContext.BeginDialogAsync(nameof(CityDialogResolver), bookingDetails, cancellationToken);

        }


        private Attachment CreateDynamicCardAttachment(BookingDetails bookingDetails, CityDialogResult cityDialogResult)
        {

            //var adaptiveCardJson = File.ReadAllText("C:\\Users\\User\\Source\\Repos\\CloudComputing\\CoreBotWithCLU\\Cards\\changeBookingDetailCard.json");
            Storage storage = new Storage();
            string adaptiveCardJson = storage.GetCardFromStorage("container-cards-progetto-cloud", "changeBookingDetailCard.json");
            dynamic obj = JsonConvert.DeserializeObject(adaptiveCardJson);

            var destination = this.iataInfo.allAirportsDestination || this.alreadyContainsDestination ?  bookingDetails.Destination :
              bookingDetails.Destination + " " + cityDialogResult.iataInfo.IATAlistDestination[cityDialogResult.iataInfo.destinationIATAIndex];

            var origin = this.iataInfo.allAirportsOrigin || this.alreadyContainsOrigin ? bookingDetails.Origin : bookingDetails.Origin + " " + cityDialogResult.iataInfo.IATAlistOrigin[cityDialogResult.iataInfo.originIATAIndex];

             obj["body"][1]["value"] = origin;
             obj["body"][1]["placeholder"] = origin;
            
            obj["body"][2]["value"] = destination;
            obj["body"][2]["placeholder"] = destination;

            obj["body"][3]["value"] = bookingDetails.TravelDate;
            obj["body"][3]["placeholder"] = bookingDetails.TravelDate;

            if (bookingDetails.ReturnDate != null)
            {
                obj["body"][4]["value"] = bookingDetails.ReturnDate;
                obj["body"][4]["placeholder"] = bookingDetails.ReturnDate;
            }


            obj["body"][5]["value"] = int.Parse(bookingDetails.PassengersNumber);
            obj["body"][5]["placeholder"] = int.Parse(bookingDetails.PassengersNumber);


            if (bookingDetails.Budget != null && bookingDetails.Budget != "0")
            {
                obj["body"][6]["value"] = double.Parse(bookingDetails.Budget);
                obj["body"][6]["placeholder"] = double.Parse(bookingDetails.Budget);
            }
            Console.WriteLine("\n--------------------------------------------------------------------------");
            Console.WriteLine(obj["body"][6]["placeholder"]);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = obj
            };

            return adaptiveCardAttachment;
        }


        private bool CheckCity(string city)
        {
            AirportRepository airportRepository = new AirportRepository();

            city=this.bookingConverter.RimuoviPreposizioni(city);
            return airportRepository.CityExists(city);
        }

        private string GetIATACode(string city)
        {
            AirportRepository airportRepository = new AirportRepository();
            
            return airportRepository.GetIataCodeByCity(city)[0];
        }

        private List<string> GetListIATACode(string city)
        {
            AirportRepository airportRepository = new AirportRepository();

            return airportRepository.GetIataCodeByCity(city);
        }

        private string ExtractCity(string city)
        {
            AirportRepository airportRepository = new AirportRepository();
            return airportRepository.ExtractCity(city);
        }

        private async Task<bool> DatePromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            DateAndTimeConverter dateAndTimeConverter = new DateAndTimeConverter();
            
            var userDate = promptContext.Context.Activity.Text;
            
            if(dateAndTimeConverter.WordToDate(userDate) != null) {
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }

        }

        private async Task<bool> TextPromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            
            
            if (CheckCity(promptContext.Context.Activity.Text))
            {
                Console.WriteLine("TRUE");
                return await Task.FromResult(true);
            }
            else
            {
                Console.WriteLine("FALSE");
                return await Task.FromResult(false);
            }
               
        }


        private IList<Choice> GetIataChoices(List<string> options, string city)
        {
            var choices = new List<Choice>();

            for (int i = 0; i < options.Count; i++)
            {
                string val = city +" "+ options[i];
                choices.Add(new Choice
                {
                    Value = val,
                    Action = new CardAction(ActionTypes.ImBack, title: $" {val}", value: val)
                });
            }

            return choices;
        }

        private PromptOptions GetIataPromptOptions(List<string> options, string city)
        {
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text($"La città di {city} ha più di un aeroporto, scegliene uno oppure clicca sul nome della città per ricercare ovunque"),
                Choices = GetIataChoices(options, city)
            };


            promptOptions.Choices.Add(new Choice
            {
                Value = city,
                Action = new CardAction(ActionTypes.ImBack, title: city, value: city)
            });

            return promptOptions;
        }

    }
}
