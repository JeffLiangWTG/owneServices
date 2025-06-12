using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Outbound.Helpers
{
    [Serializable]
    public enum ClientStatus
    {
        ClientRegistrationNotFound,
        AccountInvalid,
        Normal
    }
}
