using System;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eServices.Encryption.Server.Decryptor;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Outbound.Helpers
{
    [Serializable]
    public class ClientRegistrationItem
    {
        public Guid RegistrationPK { get; set; }
        public string ClientID { get; set; }
        public byte? ClientFlag { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public string Code { get; set; }

        public ClientRegistrationItem(eHubClientRegistration clientRegistration)
        {
            this.RegistrationPK = clientRegistration.CX_PK;
            this.ClientID = clientRegistration.eHubClient.CC_ID;
            this.ClientFlag = clientRegistration.CX_Flag1;
            this.Username = clientRegistration.CX_Attr1;
            this.Code = clientRegistration.CX_Code;
			this.Password = clientRegistration.CX_Password1; 
        }
    }
}
