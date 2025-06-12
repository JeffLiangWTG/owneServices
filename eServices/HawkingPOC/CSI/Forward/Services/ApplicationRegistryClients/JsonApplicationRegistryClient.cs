using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Forward.Models;
using Hawking.CSI.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace Hawking.CSI.Forward.Services.ApplicationRegistryClients
{
    public class JsonApplicationRegistryClient : IApplicationRegistryClient
    {
        private readonly IConfiguration configuration;
        private readonly JObject jsonRegistry;

        public JsonApplicationRegistryClient(IHostingEnvironment env, IConfiguration configuration)
        {
            this.configuration = configuration;
            var jsonFile = env.ContentRootFileProvider.GetFileInfo($"application-registry.{env.EnvironmentName}.json");
            if (!jsonFile.Exists)
                jsonFile = env.ContentRootFileProvider.GetFileInfo("application-registry.json");
            using (var fs = jsonFile.CreateReadStream())
            using (var rdr = new StreamReader(fs))
            {
                jsonRegistry = JObject.Parse(rdr.ReadToEnd());
            }
        }

        public Task<ApplicationAddress> GetApplicationAddressAsync(IDictionary<string, StringValues> messageHeaders, CancellationToken cancellationToken)
        {
            var app = jsonRegistry.SelectToken($"$[?(@..Recipient == '{messageHeaders["X-Recipient"]}')]");
            var address = new ApplicationAddress { ApplicationName = app.Path };
            return Task.FromResult(address);
        }

        public Task<ApplicationInfo> GetApplicationAsync(ApplicationAddress address, CancellationToken cancellationToken)
        {
            var app = jsonRegistry.SelectToken($"$.{address.ApplicationName}");
            var info = new ApplicationInfo
            {
                Name = (string)app["Name"],
                Type = (string)app["Type"],
                Parallelism = int.TryParse((string)app["Parallelism"], out int parallelism) ? parallelism : 1,
                Uri = (string)app["Uri"]
            };
            return Task.FromResult(info);
        }
    }
}