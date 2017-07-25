
namespace Paycor.Storage.Domain.Entity
{
    public enum DocumentStatus : byte
    {
        Created,
        Committed,
        PendingDelete,
        Deleted
    }
}
