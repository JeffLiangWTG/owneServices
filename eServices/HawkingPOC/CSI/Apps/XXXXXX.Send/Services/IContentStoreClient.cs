using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Apps.XXXXXX.Send.Services
{
    public interface IContentStoreClient
    {
        Task<Stream> ReadContentAsync(string reference, CancellationToken cancellationToken);
    }
}