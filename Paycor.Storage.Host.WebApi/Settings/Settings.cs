
using System.Configuration;

namespace Paycor.Storage.Host.WebApi.Settings
{
    public static class Settings
    {
        public const string UPLOAD_FOLDER_SETTING = "Paycor.Storage.UploadFolder";
        public const string DEFAULT_UPLOAD_FOLDER = "~/App_Data";

        public static string UploadFolder
        {
            get
            {
                var setting = ConfigurationManager.AppSettings.Get(UPLOAD_FOLDER_SETTING);
                return string.IsNullOrEmpty(setting) ? DEFAULT_UPLOAD_FOLDER : setting;
            }
        }
    }
}