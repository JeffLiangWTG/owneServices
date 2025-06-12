using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoWise.eHub.Portal.Models.View.AirRouting
{
    public class AirlineProviderMappingGridView
    {
        public string RowId { get; set; }
        public string MessageType { get; set; }
        public string Airline { get; set; }
        public string ServiceProvider { get; set; }
        public string RecipientAddress { get; set; }
        public string ClientPIMA {get; set;}
        public string MessagePriority { get; set; }
        public string DoubleSignatureCode { get; set; }
        public string ShipmentOrigin { get; set; }

    }
}