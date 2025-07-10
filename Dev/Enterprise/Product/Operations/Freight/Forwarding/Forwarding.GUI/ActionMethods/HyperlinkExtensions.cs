using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.GUI
{
	internal static class HyperlinkExtensions
	{
		public static LogHyperlink Hyperlink(this ForwardingConsol consol)
		{
			return new LogControllerLink(consol.JK_UniqueConsignRef, ControllerIDs.JobConsol, consol.PK);
		}

		public static LogHyperlink Hyperlink(this ForwardingShipment shipment)
		{
			return new LogControllerLink(shipment.JS_UniqueConsignRef, ControllerIDs.JobShipment, shipment.PK);
		}
	}
}
