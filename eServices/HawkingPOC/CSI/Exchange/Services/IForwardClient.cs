using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Exchange
{
    public interface IForwardClient
    {
        Task SendAsync(IDictionary<string, StringValues> headers, CancellationToken cancellationToken);
    }
}