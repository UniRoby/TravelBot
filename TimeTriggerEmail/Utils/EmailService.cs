using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Communication.Email;
using Azure;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.IO;
using System.Reflection.Metadata;
using TimeTriggerEmail.Models;
using TimeTriggerEmail.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Diagnostics;

namespace TimeTriggerEmail.Utils
{
    public class EmailService
    {

        public string templateEmail { get; set; }
        public string emailConnectionString { get; set; }
        public string storageConnectionString { get; set; }
        private FlightsDemandRepository flightsDemandRepository = new FlightsDemandRepository();
        private DateAndTimeConverter dateAndTimeConverter = new DateAndTimeConverter();
        public EmailService() {

            this.emailConnectionString = Environment.GetEnvironmentVariable("email_service");
            this.storageConnectionString = Environment.GetEnvironmentVariable("storage_connection");
            this.templateEmail = GetTempalteFromStorage();
        }

        public List<FlightsDemand> GetCustomerEmails()
        {
           
            return flightsDemandRepository.GetFlightsToNotify();
        }

        public void SendAllEmails(ILogger logger)
        {
            GetCustomerEmails().ForEach(flight => {

                SendEmail(flight.Email, flight.Origin, flight.Destination, flight.DepartureDate, flight.NewPrice, flight.CurrentPrice,logger);

            });

            flightsDemandRepository.SwitchPrice();
        }
    

        public string GetTempalteFromStorage()
        {
            //string storageconnectionString = Environment.GetEnvironmentVariable("storage_connection");
            var blobServiceClient = new BlobServiceClient(this.storageConnectionString);

            var containerName = "container-progetto-cloud";
            // Ottieni un riferimento al container
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Verifica se il container esiste
            
            var blobName = "TemplateEmail.html";
            // Ottieni un riferimento al blob
            var blobClient = blobContainerClient.GetBlobClient(blobName);


            // Scarica il contenuto del blob
           // Leggi il contenuto del blob come stringa
            using (var memoryStream = new MemoryStream())
            {
                blobClient.DownloadTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin); 
                using (var reader = new StreamReader(memoryStream, Encoding.UTF8))
                {
                   return  reader.ReadToEnd();
                }
            }
        }


        public void  SendEmail(string emailConsumer, string destination, string origin, string departureDate, double currentPrice, double previousPrice,ILogger logger)
        {

            logger.LogInformation($"Method Send Email...");
            //string connectionString = Environment.GetEnvironmentVariable("email_service");
            EmailClient emailClient = new EmailClient(this.emailConnectionString);


            var diff = Math.Abs(currentPrice - previousPrice);
            string price = (currentPrice > previousPrice ? "aumentato" : "sceso");
            logger.LogInformation($"Prezzo: {price}");

            var subject = "Travel Bot | Avviso di prezzo per il tuo itinerario";

            logger.LogInformation($"Subject: {subject}");


            string templateContent = this.templateEmail;

            logger.LogInformation($"template");
         
            var htmlContent =  templateContent.Replace("{destination}", destination)
                                    .Replace("{origin}", origin)
                                    .Replace("{departureDate}", dateAndTimeConverter.EnUsToItalian(departureDate))
                                    .Replace("{currentPrice}", currentPrice.ToString())
                                    .Replace("{price}", price)
                                    .Replace("{diff}", diff.ToString());

            logger.LogInformation($"htmlContent");

            var sender ="DoNotReply@2f6974f8-d61f-49dd-9395-4316e015749a.azurecomm.net";
            logger.LogInformation($"Sender: {sender}");
            var recipient = emailConsumer;
            logger.LogInformation($"recipient: {recipient}");


            try
            {
                logger.LogInformation("Sending email...");
               
                Console.WriteLine("Sending email...");
               
                EmailSendOperation emailSendOperation = emailClient.Send(
                    WaitUntil.Started,
                    sender,
                    recipient,
                    subject,
                    htmlContent);

            }
            catch (RequestFailedException ex)
            {

                logger.LogInformation($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }
        }


    }
}
