using System;
using CargoWise.eHub.DataAccess.Sql;

namespace CargoWise.eHub.Products.GBCustoms.TL.BT.Helpers
{
    [Serializable]
    public class EndpointConfiguration
    {
        public string EndpointUrl { get; set; }
        public string Method { get; set; }
        public string ContentType { get; set; }
        public string Version { get; set; }

        public static EndpointConfiguration GetConfig(string destinationParty, string service)
        {
            var transformAccessor = new TransformAccessor();

            return new EndpointConfiguration
            {
                EndpointUrl = transformAccessor.GetRecipientCode("GBCustoms", "GBCustoms", "GBCustoms Transport Layer Configuration", "Endpoints", "Endpoint URL", destinationParty, service, null, null, null) ?? string.Empty,
                Method = transformAccessor.GetRecipientCode("GBCustoms", "GBCustoms", "GBCustoms Transport Layer Configuration", "Endpoints", "Method", destinationParty, service, null, null, null) ?? string.Empty,
                ContentType = transformAccessor.GetRecipientCode("GBCustoms", "GBCustoms", "GBCustoms Transport Layer Configuration", "Endpoints", "Content Type", destinationParty, service, null, null, null) ?? string.Empty,
                Version = transformAccessor.GetRecipientCode("GBCustoms", "GBCustoms", "GBCustoms Transport Layer Configuration", "Endpoints", "Version", destinationParty, service, null, null, null) ?? string.Empty
            };
        }
    }
}
