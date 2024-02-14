// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using CoreBotCLU.Utils;
using CoreBotCLU.Repository;
using CoreBotCLU.Models;
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


namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class BookingDialog : CancelAndHelpDialog
    {
        private const string DestinationStepMsgText = "Dove vuoi andare?";
        private const string OriginStepMsgText = "Da dove parti?";
        private const string RepromptMsgText = "La città inserita non ha un aeroporto, reinseriscine un'altra.";

        private const string DepartureDateaMsgText = "Quando vuoi partire?";
        private const string PassengersMsgText = "Quanti passeggeri?";
        private const string BudgetMsgText = "Qual è il tuo budget?";

        //private List<string> flightOptions;
        private List<FlightInfo> flightOptions;
        private int selectedIndex;
        private string originIATA;
        private string destinationIATA;
        private bool isFake = false;

        public BookingDialog()
            : base(nameof(BookingDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt), TextPromptValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new TextPrompt("emailPrompt", ValidateEmailAsync));
            AddDialog(new TextPrompt("departureDatePrompt", DatePromptValidatorAsync));
            AddDialog(new TextPrompt("returnDatePrompt", DatePromptValidatorAsync));
            AddDialog(new TextPrompt("cardPrompt"));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                DestinationStepAsync,
                OriginStepAsync,
                TravelDateStepAsync,
                ReturnDateStepAsync,
                PassengersStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
                HandleResultStepAsync,
                APIStepAsync,
                ShowFlightsAsync,
                ShowSelectedFlightAsync,
                SaveDemandAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> DestinationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;
            

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

        private async Task<DialogTurnResult> OriginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;

            bookingDetails.Destination = (string)stepContext.Result;

            if (bookingDetails.Origin == null || !CheckCity(bookingDetails.Origin))
            
            {
                var promptMessage = MessageFactory.Text(OriginStepMsgText, OriginStepMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(bookingDetails.Origin, cancellationToken);
        }

        private async Task<DialogTurnResult> TravelDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;

            DateAndTimeConverter converter = new DateAndTimeConverter();
            

            bookingDetails.Origin = (string)stepContext.Result;

            if (bookingDetails.TravelDate == null || converter.WordToDate(bookingDetails.TravelDate)==null)
            {
                // return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), bookingDetails.TravelDate, cancellationToken);
                //return await stepContext.PromptAsync("departureDatePrompt", new PromptOptions { Prompt = MessageFactory.Text("Quando vuoi partire? "), RetryPrompt = MessageFactory.Text("La data inserita non è valida, per favore inseriscila di nuovo.") }, cancellationToken);
                BookingConverter bookingConverter = new BookingConverter
                {
                    Date = bookingDetails.TravelDate,
                    PromptType = "D"
                };

                return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), bookingConverter, cancellationToken);
            }
            
            bookingDetails.TravelDate=converter.WordToDate(bookingDetails.TravelDate);
           

            return await stepContext.NextAsync(bookingDetails.TravelDate, cancellationToken);
        }

        private async Task<DialogTurnResult> ReturnDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;
            bookingDetails.TravelDate = (string)stepContext.Result;
            
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


            var messageText = $"Perfavore conferma, vuoi andare a: {bookingDetails.Destination} da: {bookingDetails.Origin} il: {bookingDetails.TravelDate} {(bookingDetails.ReturnDate != null ? $" Ritorno il: {bookingDetails.ReturnDate}" : "")}, numero passeggeri: {bookingDetails.PassengersNumber} {(bookingDetails.Budget != null ? $", budget: {bookingDetails.Budget}€" : "")}. E' corretto?";
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

       

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var result = (FoundChoice)stepContext.Result;
            var bookingDetails = (BookingDetails)stepContext.Options;

            WordNumberConverter wordNumber = new WordNumberConverter();
            if (bookingDetails.Budget == null)
            {
                bookingDetails.Budget = "0";
            }
            else
            {
                bookingDetails.Budget = wordNumber.NumberExtractor(bookingDetails.Budget);
            }

            if (result.Value=="Ricerca Fake")
            {
                this.isFake = true;
                return await stepContext.NextAsync(bookingDetails, cancellationToken);
            }
            
            if (result.Value == "Si")
            {
                this.isFake = false;

                return await stepContext.NextAsync(bookingDetails, cancellationToken);
            }
            else if (result.Value == "No")
            {

               
                var changeCard = CreateDynamicCardAttachment(bookingDetails.Origin,bookingDetails.Destination,bookingDetails.TravelDate,bookingDetails.ReturnDate,bookingDetails.PassengersNumber,bookingDetails.Budget);
                var response = MessageFactory.Attachment(changeCard, ssml: "Cambio");
                await stepContext.Context.SendActivityAsync(response, cancellationToken).ConfigureAwait(false);


                return await stepContext.PromptAsync("cardPrompt", new PromptOptions { Prompt = MessageFactory.Text("In attessa di conferma... ") });

            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> HandleResultStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is BookingDetails result)
            {
  
                var bookingDetails = (BookingDetails)stepContext.Result;
                return await stepContext.NextAsync(bookingDetails, cancellationToken);
                
            }
            else 
            {
                
                var bookingString = (string)stepContext.Result;
                BookingConverter converter = new BookingConverter();
                BookingDetails bookingDetails = converter.ParseFromString(bookingString);

                return await stepContext.NextAsync(bookingDetails, cancellationToken);
            }

            
        }


            private async Task<DialogTurnResult> APIStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {

            var bookingDetails = (BookingDetails)stepContext.Result;
            DateAndTimeConverter dateAndTimeConverter = new DateAndTimeConverter();
            this.originIATA = GetIATACode(bookingDetails.Origin);
            this.destinationIATA = GetIATACode(bookingDetails.Destination);

            //API VIAGGI
            SearchFlights searchFlights = new SearchFlights(originIATA, destinationIATA, dateAndTimeConverter.ItalianToEnUs(bookingDetails.TravelDate), (bookingDetails.ReturnDate != null ? dateAndTimeConverter.ItalianToEnUs(bookingDetails.ReturnDate) : null), int.Parse(bookingDetails.PassengersNumber), (bookingDetails.Budget != null ? double.Parse(bookingDetails.Budget) : double.Parse("0")));

            if (this.isFake)
            {
                this.flightOptions = GetRandomFlights();
            }
            else
            {
                this.flightOptions = searchFlights.StartSearch();
            }

            if (this.flightOptions == null || this.flightOptions.Count==0) {
                await stepContext.Context.SendActivityAsync("Nessun volo trovato");
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            return await stepContext.NextAsync(searchFlights.passengers.ToString(), cancellationToken);
        }

        private async Task<DialogTurnResult> ShowFlightsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string passengersNumber= (string)stepContext.Result;
            
            // Lista di Adaptive Cards per le tratte
            var cards = new List<Attachment>();
            foreach (var option in this.flightOptions)
            {

                var travelCard = CreateTravelDetailsAdaptiveCard(option, passengersNumber);
                cards.Add(travelCard);
            }

            // Invia il carousel di Adaptive Cards all'utente
            var message = MessageFactory.Carousel(cards);
            await stepContext.Context.SendActivityAsync(message, cancellationToken);



            // Memorizza le opzioni passate nello stato di conversazione
            if (this.flightOptions != null && this.flightOptions.Any())
            {

                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Scegli una tratta per rimanere aggiornato/a sulle variazioni di prezzo (altrimenti clicca su Termina)"),
                    Choices = GetChoices(this.flightOptions)
                };

                
                promptOptions.Choices.Add(new Choice
                {
                    Value = "Termina",
                    Action = new CardAction(ActionTypes.ImBack, title: "TERMINA", value: "Termina")
                });

                return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Nessun volo trovato");
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

        }



        private async Task<DialogTurnResult> ShowSelectedFlightAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var choice = (FoundChoice)stepContext.Result;

            if (choice.Value == "Termina")
            {
                return await stepContext.EndDialogAsync(choice.Value, cancellationToken);
            }
                

                this.selectedIndex = choice.Index;

                return await stepContext.PromptAsync("emailPrompt", new PromptOptions { Prompt = MessageFactory.Text("Inserisci la tua email: "), RetryPrompt = MessageFactory.Text("L'indirizzo email non è valido, per favore inseriscilo di nuovo.") }, cancellationToken);

        }
        private async Task<bool> ValidateEmailAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var emailRegex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");


            if (emailRegex.IsMatch(promptContext.Recognized.Value))
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


        private async Task<DialogTurnResult> SaveDemandAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            // Salva l'email inserita dall'utente
            var userEmail = (string)stepContext.Result;
            var bookingDetails = (BookingDetails)stepContext.Options;
          
            if (!SaveDemand(userEmail, bookingDetails))
            {
                await stepContext.Context.SendActivityAsync($"Reminder non salvato ", cancellationToken: cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Riceverai una notifica se il prezzo varia", cancellationToken: cancellationToken);

                string end = "Termina";
                return await stepContext.EndDialogAsync(end, cancellationToken);
            }

        }


        private  Attachment CreateDynamicCardAttachment(string partenza, string destinazione, string dataPartenza, string dataRitorno, string numeroPasseggeri, string budget)
            {

            //var adaptiveCardJson = File.ReadAllText("C:\\Users\\User\\Source\\Repos\\CloudComputing\\CoreBotWithCLU\\Cards\\changeBookingDetailCard.json");
                Storage storage = new Storage();
                string adaptiveCardJson = storage.GetCardFromStorage("container-cards-progetto-cloud", "changeBookingDetailCard.json");
                
                dynamic obj = JsonConvert.DeserializeObject(adaptiveCardJson);

                obj["body"][1]["value"] = partenza;
                obj["body"][1]["placeholder"] = partenza;

                obj["body"][2]["value"] = destinazione;
                obj["body"][2]["placeholder"] = destinazione;

                obj["body"][3]["value"] = dataPartenza;
                obj["body"][3]["placeholder"] = dataPartenza;

                if(dataRitorno!=null) {
                    obj["body"][4]["value"] = dataRitorno;
                    obj["body"][4]["placeholder"] = dataRitorno;
                }

               
                obj["body"][5]["value"] = int.Parse(numeroPasseggeri);
                obj["body"][5]["placeholder"] = int.Parse(numeroPasseggeri);


                if (budget != null && budget!="0")
                    {
                        obj["body"][6]["value"] = double.Parse(budget);
                        obj["body"][6]["placeholder"] = double.Parse(budget);
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


        private static Attachment CreateTravelDetailsAdaptiveCard(FlightInfo flightInfo,string passengersNumber)
        {
            //var adaptiveCardJson = File.ReadAllText("C:\\Users\\User\\Source\\Repos\\CloudComputing\\CoreBotWithCLU\\Cards\\flightCard.json");
            Storage storage = new Storage();
            string adaptiveCardJson = storage.GetCardFromStorage("container-cards-progetto-cloud", "flightCard.json");
            dynamic obj = JsonConvert.DeserializeObject(adaptiveCardJson);

            DateAndTimeConverter dateAndTimeConverter = new DateAndTimeConverter();

            //var mainCardJson = File.ReadAllText("C:\\Users\\User\\Source\\Repos\\CloudComputing\\CoreBotWithCLU\\Cards\\mainPart.json");
            string mainCardJson = storage.GetCardFromStorage("container-cards-progetto-cloud", "mainPart.json");

            var layover = flightInfo.flights.Count;


            obj["body"][1]["columns"][1]["items"][0]["text"] = flightInfo.price.ToString() + "€";
            obj["body"][1]["columns"][1]["items"][1]["text"] = dateAndTimeConverter.ConvertiMinutiInOre(flightInfo.totalDuration);


            // Impostazione dei valori dei campi di testo

            obj["body"][2]["columns"][1]["items"][0]["text"] = passengersNumber;

            if(layover > 1) {
                obj["body"][2]["columns"][1]["items"][1]["text"] = (layover-1).ToString();
            }
            else
            {
                obj["body"][2]["columns"][1]["items"][1]["text"] = "nessuno";
            }


            
         foreach (Flight flight in flightInfo.flights)
         {
             
             dynamic mainObj = JsonConvert.DeserializeObject(mainCardJson);

             mainObj["columns"][0]["items"][1]["text"] = flight.departureAirport;
             mainObj["columns"][0]["items"][2]["text"] = flight.departureAirportIATA;
             mainObj["columns"][0]["items"][3]["text"] = dateAndTimeConverter.EnUsWithTimeToItalian(flight.departureTime);

             mainObj["columns"][1]["items"][0]["url"] = flight.airlineLogo;

             mainObj["columns"][2]["items"][1]["text"] = flight.arrivalAirport;
             mainObj["columns"][2]["items"][2]["text"] = flight.arrivalAirportIATA;
             mainObj["columns"][2]["items"][3]["text"] = dateAndTimeConverter.EnUsWithTimeToItalian(flight.arrivalTime);


             
             obj["body"].Add(mainObj);
         }

        

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

            return airportRepository.CityExists(city);
        }

        private string GetIATACode(string city)
        {
            AirportRepository airportRepository = new AirportRepository();
            
            return airportRepository.GetIataCodeByCity(city)[0];
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
                return await Task.FromResult(true);
            }
            else
                return await Task.FromResult(false);
        }

        private bool SaveDemand(string userEmail, BookingDetails bookingDetails) {

            FlightsDemandRepository flightsDemandRepository = new FlightsDemandRepository();
            FlightsDemand flightsDemand = new FlightsDemand();

            flightsDemand.Email = userEmail;
            flightsDemand.Origin = this.originIATA;
            flightsDemand.Destination = this.destinationIATA;


            DateAndTimeConverter dateAndTimeConverter = new DateAndTimeConverter();

          

            flightsDemand.DepartureDate = dateAndTimeConverter.ItalianToEnUs(bookingDetails.TravelDate);

            if (bookingDetails.ReturnDate != null)
            {
                
                flightsDemand.ReturnDate = dateAndTimeConverter.ItalianToEnUs(bookingDetails.ReturnDate);
            }

            flightsDemand.CurrentPrice = this.flightOptions[this.selectedIndex].price;
            flightsDemand.Passengers = int.Parse(bookingDetails.PassengersNumber);
            flightsDemand.Notify = "Y";


            try
            {
                flightsDemandRepository.SaveFlightDemand(flightsDemand);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;

        }

        private IList<Choice> GetChoices(List<FlightInfo> options)
        {
            var choices = new List<Choice>();

            for (int i = 0; i < options.Count; i++)
            {
                string val = "Tratta " + (i + 1).ToString();
                choices.Add(new Choice
                {
                    Value = val,
                    Action = new CardAction(ActionTypes.ImBack, title: $" Tratta {i + 1}", value: val)
                });
            }

            return choices;
        }




        private List<FlightInfo> GetRandomFlights()
        {
            List<FlightInfo> list = new List<FlightInfo>();

            // Creazione degli oggetti Flight
            Flight flight1 = new Flight
            {
                departureAirport = "Airport1",
                arrivalAirport = "Airport2",
                departureAirportIATA = "IATA1",
                arrivalAirportIATA = "IATA2",
                airline = "Airline1",
                airlineLogo = "https://www.gstatic.com/flights/airline_logos/70px/NH.png",
                flightNumber = "FL1",
                duration = 120,
                departureTime = "2023-10-03 17:10",
                arrivalTime = "2023-10-03 21:00",
                layoverDuration = 0,
                layoverAirport = ""
            };

            Flight flight2 = new Flight
            {
                departureAirport = "Airport2",
                arrivalAirport = "Airport3",
                departureAirportIATA = "IATA12",
                arrivalAirportIATA = "IATA13",
                airline = "Airline2",
                airlineLogo="https://www.gstatic.com/flights/airline_logos/70px/UA.png",
                flightNumber = "FL2",
                duration = 180,
                departureTime = "2023-10-03 15:10",
                arrivalTime = "2023-10-03 17:00",
                layoverDuration = 0,
                layoverAirport = ""
            };

            // Creazione degli oggetti FlightInfo
            FlightInfo flightInfo1 = new FlightInfo
            {
                totalDuration = flight1.duration + flight2.duration,
                price = 300.50,
                type = "Business",
                flights = new List<Flight> { flight1, flight2 }
            };

            FlightInfo flightInfo2 = new FlightInfo
            {
                totalDuration = flight1.duration,
                price = 150.75,
                type = "Economy",
                flights = new List<Flight> { flight1 }
            };

            FlightInfo flightInfo3 = new FlightInfo
            {
                totalDuration = flight2.duration,
                price = 200.25,
                type = "Economy",
                flights = new List<Flight> { flight2 }
            };

            list.Add(flightInfo1);
            list.Add(flightInfo2);
            list.Add(flightInfo3);


            return list;
        }

        

    }
}
