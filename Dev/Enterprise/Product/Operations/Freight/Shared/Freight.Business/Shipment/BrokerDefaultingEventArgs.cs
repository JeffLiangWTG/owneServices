using System;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business
{
	public class BrokerDefaultingEventArgs : EventArgs
	{
		public BrokerDefaultingEventArgs(DocAddressType brokerType)
		{
			this.BrokerType = brokerType;
		}

		public DocAddressType BrokerType { get; }

		public bool ShouldUpdateBroker { get; set; }
	}
}
