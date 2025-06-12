using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Exchange
{
    public interface IContentStoreClient
    {
        Task<string> WriteContentAsync(IDictionary<string, StringValues> headers, Stream message, CancellationToken cancellationToken);
    }
}