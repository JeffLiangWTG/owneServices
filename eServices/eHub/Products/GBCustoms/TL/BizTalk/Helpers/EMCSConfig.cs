using System;
using System.Collections.Generic;

using CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract;

namespace CargoWise.eHub.Products.GBCustoms.TL.BT.Helpers
{
    [Serializable]
    public class EMCSConfig
    {
        public string Service { get; set; }
        public string ServiceReference { get; set; }
        public string JobNumber { get; set; }
		public string Recipient { get; set; }
        public int ErrorCount { get; set; }
        public DateTime OutboundDate { get; set; }
        public DateTime LastPollingDate { get; set; }
		public HttpHeader[] RequestHeaders { get; set; }
		public HttpHeader[] ResponseHeaders { get; set; }
		public string CredentialsKey { get; set; }
		public string ContentType { get; set; }
	}

	public enum MessageType { GBCustomsRequest, EMCSXmlConfig };
}
