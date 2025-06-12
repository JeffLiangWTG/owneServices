using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Outbound.Helpers
{
    [Serializable]
    public class ConnectionDetailItem
    {
        public string ClientId { get; set; }

        public string Secret { get; set; }
    }
}
