using System;

namespace CargoWise.eHub.Portal.Models.View.MessageReference
{
    public class MessageReferenceRegistry
    {
        public Guid CR_PK { get; set; }
        public Guid CR_CC_Client { get; set; }
        public string CC_ID { get; set; }
        public string CC_FriendlyName { get; set; }
        public string CR_ApplicationCode { get; set; }
        public string CR_MessageReference { get; set; }
        public string CR_Password { get; set; }
    }
}