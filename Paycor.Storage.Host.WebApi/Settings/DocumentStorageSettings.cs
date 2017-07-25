
namespace Paycor.Storage.Host.WebApi.Settings
{
    public class DocumentStorageSettings : IDocumentStorageSettings
    {
        public string UploadFolder { get { return Settings.UploadFolder; } }
    }
}