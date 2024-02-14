using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using System.IO;



namespace TravelBot.Utils
{


	public class EmailSender
	{

        private readonly string connectionString;

        public EmailSender()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = builder.Build();

            // Ottieni la connection string e assegnala a una variabile di istanza
            connectionString = config.GetConnectionString("AZURE_COMMUNCATIONL_SERVICE");
        }

        public async Task SendEmail(string emailConsumer, string destination, string origin, DateTime departureDate, double currentPrice, double previousPrice)
        {
            var diff = Math.Abs(currentPrice - previousPrice);
            string price = (currentPrice > previousPrice ? "aumentato" : "sceso");


            var subject = "Travel Bot | Avviso di prezzo per il tuo volo";
            var htmlContent = $@"
            <html>
                <style>
               
                    body {{
                        font-family: 'Arial', sans-serif;
                        color: #333;
                    }}
                    h1 {{
                        color: #0066cc;
                    }}
                    p {{
                        margin-bottom: 10px;
                    }}
                </style>
                <body>
                    <h1>Avviso Prezzo</h1>
                    <p>Questa email è stata inviata da Travel Bot per avvisarti che</p>
                    <p>Il prezzo del volo con destinazione {destination} e origine {origin} in data {departureDate.ToShortDateString()} è di {currentPrice} €.</p>
                    <p>Il prezzo è {price} di {diff} € rispetto al precedente.</p>
                </body>
            </html>";

            var sender = "donotreply@2f6974f8-d61f-49dd-9395-4316e015749a.azurecomm.net";
            var recipient = emailConsumer;

            EmailClient emailClient = new EmailClient(connectionString);

            try
            {
                Console.WriteLine("Sending email...");
                EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                    Azure.WaitUntil.Started,
                    sender,
                    recipient,
                    subject,
                    htmlContent);
                EmailSendResult statusMonitor = emailSendOperation.Value;

                Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

                /// Get the OperationId so that it can be used for tracking the message for troubleshooting
                string operationId = emailSendOperation.Id;
                Console.WriteLine($"Email operation id = {operationId}");
            }
            catch (RequestFailedException ex)
            {
                /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }
        }




    }
}
