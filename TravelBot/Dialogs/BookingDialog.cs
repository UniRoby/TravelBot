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
using System.Runtime.ConstrainedExecution;


namespace TravelBot.Dialogs
{
    public class BookingDialog : CancelAndHelpDialog
    {
        private const string DestinationStepMsgText = "Dove vuoi andare?";
        private const string OriginStepMsgText = "Da dove parti?";
        private const string RepromptMsgText = "La città inserita non ha un aeroporto, reinseriscine un'altra.";

        private const string DepartureDateaMsgText = "Quando vuoi partire?";
        private const string PassengersMsgText = "Quanti passeggeri?";
        private const string BudgetMsgText = "Qual è il tuo budget?";

        
        private List<FlightInfo> flightOptions;
        private int selectedIndex;
        private string originIATA;
        private string destinationIATA;
        private bool isFake = true;

        private IataInfo iataInfos;
        private BookingDetails bookingDetails;

        public BookingDialog()
            : base(nameof(BookingDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt), TextPromptValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new CityDialogResolver());
            AddDialog(new TextPrompt("emailPrompt", ValidateEmailAsync));
            AddDialog(new TextPrompt("departureDatePrompt", DatePromptValidatorAsync));
            AddDialog(new TextPrompt("returnDatePrompt", DatePromptValidatorAsync));
            AddDialog(new TextPrompt("cardPrompt"));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                APIStepAsync,
                ShowFlightsAsync,
                ShowSelectedFlightAsync,
                SaveDemandAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
            bookingDetails= new BookingDetails();
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var bookingDetails = (BookingDetails)stepContext.Options;
            this.bookingDetails= bookingDetails;

            return await stepContext.BeginDialogAsync(nameof(CityDialogResolver),bookingDetails, cancellationToken);
        }
       
        private async Task<DialogTurnResult> APIStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            
        {
            //var bookingDetails = (BookingDetails)stepContext.Options;
            
            CityDialogResult cityDialogResult = (CityDialogResult)stepContext.Result;

            //await stepContext.Context.SendActivityAsync($" cityDialog prompt value: {cityDialogResult.promtResult}");
            this.bookingDetails = cityDialogResult.bookingDetails;

            var result = cityDialogResult.promtResult;
            this.iataInfos = cityDialogResult.iataInfo;
            this.isFake = cityDialogResult.isFake;
            // var bookingDetails = (BookingDetails)stepContext.Result;

            
            DateAndTimeConverter dateAndTimeConverter = new DateAndTimeConverter();
            //I voli da qualsiasi aeroporto dell'origin verso qualsiasi aeroporto della destination
            if (this.isFake)
            {
                // await stepContext.Context.SendActivityAsync($" GetRandomFlights");
                this.flightOptions = GetRandomFlights();

            }
            else
            {


                if (this.iataInfos.allAirportsOrigin && this.iataInfos.allAirportsDestination)
                {
                    await stepContext.Context.SendActivityAsync($"RICERCA OVUNQUE NON ATTIVA");
                    this.flightOptions = GetRandomFlights();

                    foreach (var originIata in this.iataInfos.IATAlistOrigin)
                    {
                        foreach (var destIata in this.iataInfos.IATAlistDestination)
                        {
                            //SearchFlights searchFlights = new SearchFlights(originIata, destIata, dateAndTimeConverter.ItalianToEnUs(bookingDetails.TravelDate), (bookingDetails.ReturnDate != null ? dateAndTimeConverter.ItalianToEnUs(bookingDetails.ReturnDate) : null), int.Parse(bookingDetails.PassengersNumber), (bookingDetails.Budget != null ? double.Parse(bookingDetails.Budget) : double.Parse("0")));
                            //var flights= searchFlights.StartSearch();
                            //flights.ForEach(f=> this.flightOptions.Add(f));
                        }

                    }
                }
                else if (this.iataInfos.allAirportsOrigin)
                {
                    
                    this.destinationIATA = this.iataInfos.IATAlistDestination[this.iataInfos.destinationIATAIndex];

                    await stepContext.Context.SendActivityAsync($"RICERCA DA TUTTI GLI AEROPORTI DI ORIGINE NON ATTIVA");
                    this.flightOptions = GetRandomFlights();

                    foreach (var originIata in this.iataInfos.IATAlistOrigin)
                    {
                        //SearchFlights searchFlights = new SearchFlights(originIata,  this.destinationIATA, dateAndTimeConverter.ItalianToEnUs(bookingDetails.TravelDate), (bookingDetails.ReturnDate != null ? dateAndTimeConverter.ItalianToEnUs(bookingDetails.ReturnDate) : null), int.Parse(bookingDetails.PassengersNumber), (bookingDetails.Budget != null ? double.Parse(bookingDetails.Budget) : double.Parse("0")));
                        //var flights= searchFlights.StartSearch();
                        //flights.ForEach(f=> this.flightOptions.Add(f));
                    }
                }
                else if (this.iataInfos.allAirportsDestination)
                {
                    
                    await stepContext.Context.SendActivityAsync($"RICERCA VERSO TUTTI GLI AEROPORTI DI DESTINAZIONE NON ATTIVA");
                    this.flightOptions = GetRandomFlights();
                    this.originIATA = this.iataInfos.IATAlistOrigin[this.iataInfos.originIATAIndex];
                    foreach (var originIata in this.iataInfos.IATAlistOrigin)
                    {
                        //SearchFlights searchFlights = new SearchFlights(originIata,  this.destinationIATA, dateAndTimeConverter.ItalianToEnUs(bookingDetails.TravelDate), (bookingDetails.ReturnDate != null ? dateAndTimeConverter.ItalianToEnUs(bookingDetails.ReturnDate) : null), int.Parse(bookingDetails.PassengersNumber), (bookingDetails.Budget != null ? double.Parse(bookingDetails.Budget) : double.Parse("0")));
                        //var flights= searchFlights.StartSearch();
                        //flights.ForEach(f=> this.flightOptions.Add(f));
                    }
                }
                else
                {

                    this.originIATA = this.iataInfos.IATAlistOrigin[this.iataInfos.originIATAIndex];
                    this.destinationIATA = this.iataInfos.IATAlistDestination[this.iataInfos.destinationIATAIndex];


                    await stepContext.Context.SendActivityAsync($" Cerco voli da {this.originIATA} a {this.destinationIATA} " +
                     $"il: {bookingDetails.TravelDate}, " +
                     $"{(bookingDetails.ReturnDate != null ? $" Ritorno il: {bookingDetails.ReturnDate}" : "")}, " +
                     $"numero passeggeri: {bookingDetails.PassengersNumber}" +
                     $"{(bookingDetails.Budget != null ? $", budget: {(bookingDetails.Budget != "0" ? $"{bookingDetails.Budget}€" : "non specificato")}" : "")}");

               
                    SearchFlights searchFlights = new SearchFlights(this.originIATA, this.destinationIATA, dateAndTimeConverter.ItalianToEnUs(this.bookingDetails.TravelDate), (this.bookingDetails.ReturnDate != null ? dateAndTimeConverter.ItalianToEnUs(this.bookingDetails.ReturnDate) : null), int.Parse(this.bookingDetails.PassengersNumber), (this.bookingDetails.Budget != null ? double.Parse(this.bookingDetails.Budget) : double.Parse("0")));
                    this.flightOptions = searchFlights.StartSearch();
                        
                }
                if (this.flightOptions.IsNullOrEmpty())
                {
                    await stepContext.Context.SendActivityAsync("Nessun volo trovato");
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync($"Numero voli trovati: {this.flightOptions.Count}");
                }
            }
         
          

            //return await stepContext.NextAsync(searchFlights.passengers.ToString(), cancellationToken);
            return await stepContext.NextAsync(this.bookingDetails, cancellationToken);
        }

        private async Task<DialogTurnResult> ShowFlightsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var bookingDetails = (BookingDetails)stepContext.Result;


            // Lista di Adaptive Cards per le tratte
            var cards = new List<Attachment>();
            foreach (var option in this.flightOptions)
            {

                var travelCard = CreateTravelDetailsAdaptiveCard(option, bookingDetails.PassengersNumber);
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
                    Prompt = MessageFactory.Text("Scegli una tratta per rimanere aggiornato/a sulle variazioni di prezzo oppure Termina la ricerca"),
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


            // await stepContext.Context.SendActivityAsync($"booking Details date: {this.bookingDetails.TravelDate} ");


            if (!SaveDemand(userEmail, this.bookingDetails))
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
            
            return airportRepository.GetIataContainedInValue(city);
        }

        private List<string> GetListIATACode(string city)
        {
            AirportRepository airportRepository = new AirportRepository();

            return airportRepository.GetIataCodeByCity(city);
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
                Prompt = MessageFactory.Text("Questa città ha più di un aeroporto, scegliene uno oppure clicca sul nome della città per ricercare ovunque"),
                Choices = GetIataChoices(options, city)
            };


            promptOptions.Choices.Add(new Choice
            {
                Value = city,
                Action = new CardAction(ActionTypes.ImBack, title: city, value: city)
            });

            return promptOptions;
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
