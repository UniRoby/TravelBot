using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using System.Threading;
using System.IO;
using System.Reflection.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Diagnostics;

namespace TravelBot.Models
{
    public class Storage
    {
        private string connectionString;
        private BlobServiceClient blobServiceClient;
        public Storage() {
            var builder = new ConfigurationBuilder()
     .SetBasePath(Directory.GetCurrentDirectory())
     .AddJsonFile("appsettings.json");

            var config = builder.Build();

           

            //this.connectionString=  Environment.GetEnvironmentVariable("storage_connection");
            this.connectionString= config.GetConnectionString("AZURE_STORAGE_CONNECTIONSTRING");
            this.blobServiceClient = new BlobServiceClient(this.connectionString);
        }

        public string GetCardFromStorage(string containerName, string fileName)
        {
           
            // Controlla se il nome del contenitore è nullo o vuoto
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("Il nome del contenitore non può essere nullo o vuoto.", nameof(containerName));
            }

            // Controlla se il nome del file è nullo o vuoto
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Il nome del file non può essere nullo o vuoto.", nameof(fileName));
            }
           
            // Ottieni un riferimento al container
            var blobContainerClient = this.blobServiceClient.GetBlobContainerClient(containerName);
            // Verifica se il container esiste
            if (!blobContainerClient.Exists())
            {
                throw new InvalidOperationException($"Il contenitore '{containerName}' non esiste.");
            }

            // Verifica se il container esiste

            var blobClient = blobContainerClient.GetBlobClient(fileName);

            // Verifica se il blob esiste
            if (!blobClient.Exists())
            {
                throw new InvalidOperationException($"Il blob '{fileName}' non esiste nel contenitore '{containerName}'.");
            }


            // Scarica il contenuto del blob
            using (var memoryStream = new MemoryStream())
               {
                    blobClient.DownloadTo(memoryStream);
                  memoryStream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(memoryStream))
                     {
                        return reader.ReadToEnd();
                    }
                }

        }

    }
}
