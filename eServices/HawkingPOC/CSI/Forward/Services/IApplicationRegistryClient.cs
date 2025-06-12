using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Forward.Models;
using Hawking.CSI.Utilities;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Forward.Services
{
    public interface IApplicationRegistryClient
    {
        Task<ApplicationAddress> GetApplicationAddressAsync(IDictionary<string, StringValues> messageHeaders, CancellationToken cancellationToken);
        Task<ApplicationInfo> GetApplicationAsync(ApplicationAddress address, CancellationToken cancellationToken);
    }
}