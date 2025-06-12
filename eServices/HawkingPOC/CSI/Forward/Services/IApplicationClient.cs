using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Forward.Services
{
    public interface IApplicationClient
    {
        string Uri { get; set; }
        Task<IDictionary<string, StringValues>> SendAsync(IDictionary<string, StringValues> messageHeaders, CancellationToken cancellationToken);
    }
}