using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Apps.YYYYYY.Outbound.Services
{
    public interface IContentStoreClient
    {
        Task<Stream> ReadContentAsync(string reference, CancellationToken cancellationToken);
        Task<string> WriteContentAsync(IDictionary<string, StringValues> headers, Stream message, CancellationToken cancellationToken);
    }
}