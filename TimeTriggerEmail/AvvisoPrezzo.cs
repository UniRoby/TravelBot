using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TimeTriggerEmail.Repository;
using TimeTriggerEmail.Utils;
using TimeTriggerEmail.Models;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Azure.Communication.Email;
using Azure;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace TimeTriggerEmail
{
    public class AvvisoPrezzo
    {
        private readonly ILogger _logger;

        public AvvisoPrezzo(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AvvisoPrezzo>();
        }
        // 0 19 * * 5
        //*/60 * * * * *

        [Function("AvvisoPrezzo")]
        public void Run([TimerTrigger("0 19 * * 5")] TimerInfo myTimer) 
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            EmailService emailService = new EmailService();

            UpdatePriceService updatePriceService = new UpdatePriceService();
            updatePriceService.CheckAndUpdateBestPrice(_logger);

            emailService.SendAllEmails(_logger);

        }
    }
}
