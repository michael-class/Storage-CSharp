using System.IO;
using System.Threading.Tasks;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Data.Storage
{
    public interface IBlockStorage
    {
        string Account { get; }

        Task<Document> UploadAsync(Document document, Stream inputStream);

        Task<Document> DownloadAsync(Document document);

        Task<Document> DeleteAsync(Document document);
    }
}