using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Apps.YYYYYY.Send.Services
{
    public interface IExternalClient
    {
        Task SendAsync(IDictionary<string, StringValues> headers, Stream message, CancellationToken cancellationToken);
    }
}