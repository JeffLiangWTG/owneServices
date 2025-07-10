using Enterprise.EConversation.ServiceTasks;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.ServiceTasks
{
	public class NeoForwardingShipmentEmailProcessor : NeoBusinessObjectEmailProcessor<ForwardingShipment>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant")]
		public override string EmailTypeName => "Shipments";
	}
}
