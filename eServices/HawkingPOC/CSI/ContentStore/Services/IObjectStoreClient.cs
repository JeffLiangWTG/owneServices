using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hawking.CSI.ContentStore.Services
{
    public interface IObjectStoreClient
    {
        Task<Stream> ReadObjectAsync(string id, CancellationToken cancellationToken);
        Task<string> WriteObjectAsync(string reference, Stream body, CancellationToken cancellationToken);
    }
}