using System;

namespace Enterprise.Freight.Forwarding.Business
{
	public class EnterShipmentNumbersEventArgs : EventArgs
	{
		public EnterShipmentNumbersEventArgs(ForwardingConsol consol)
		{
			this.Consol = consol;
		}

		public readonly ForwardingConsol Consol;
	}
}
