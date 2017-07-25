using System;
using Common.Logging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Paycor.Neo.Logging.Format;

namespace Paycor.Storage.Azure.Utils
{
    public static class StorageAccountUtils
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(StorageAccountUtils));

        private const string SOURCE = "CloudStorageAccount";
        private const string ACCOUNT_EVENT = "parseAccountConnection";
        private const string ACCOUNT_ERROR_MSG =
            "Invalid storage account information provided. Please confirm the StorageAccount and AccountKey are valid in the Account.config file.";

        public static CloudStorageAccount CreateStorageAccount(string account)
        {
            _logger.Trace(m => m("CreateStorageAccount(): account={0}", account));
            //TODO: implement account lookup, for now hard code

            CloudStorageAccount storageAccount;
            try
            {
                var storageConnectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException e)
            {
                LogAccountError(account, e);
                throw;
            }
            catch (ArgumentException e)
            {
                LogAccountError(account, e);
                throw;
            }

            return storageAccount;
        }

        private static void LogAccountError(string account, Exception e)
        {
            _logger.Fatal(
                m =>
                    m("{0}",
                        LogEventFormatter.FormatError(SOURCE, e)
                            .AddEvent(ACCOUNT_EVENT)
                            .AddMessage(ACCOUNT_ERROR_MSG)
                            .Add("account", account)));
        }
    }
}
