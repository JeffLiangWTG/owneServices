using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class PopulateShipmentEventArgs : EventArgs
	{
		public PopulateShipmentEventArgs(ResourceString message, bool shouldPopulateShipment)
		{
			Message = message;
			ShouldPopulateShipment = shouldPopulateShipment;
		}

		public ResourceString Message { get; set; }

		public bool ShouldPopulateShipment { get; set; }
	}
}
