using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class ForwardingConsolDefaultFilterProvider : DefaultFilterProvider
	{
		public ZString TransportMode { get; set; }
		public ZString ContainerMode { get; set; }
		public ZString ConsolType { get; set; }
		public ZString OriginPort { get; set; }
		public ZString DestinationPort { get; set; }
		public ZString LoadPort { get; set; }
		public ZString DischargePort { get; set; }
		public ZString MasterBill { get; set; }
		public ZDateTime ETDFrom { get; set; }
		public ZDateTime ETDTo { get; set; }
		public ZDateTime ETAFrom { get; set; }
		public ZDateTime ETATo { get; set; }
		public ZGuid Carrier { get; set; }
		public ZGuid SendingAgent { get; set; }
		public ZGuid ReceivingAgent { get; set; }

		protected override void SetDefaultFiltersCore(IFilterBusinessObjectDefaultsProvider collection)
		{
			#region SuppressResourceStringsCheckRegion

			AddFilterDefaults(collection, "Transport Mode", TransportMode);
			AddFilterDefaults(collection, "Container Mode", ContainerMode);
			AddFilterDefaults(collection, "Consol Type", ConsolType);
			AddFilterDefaults(collection, "End Ports (First Load / Last Disch.)", LoadPort, DischargePort);
			AddFilterDefaults(collection, "Origin / Destination", OriginPort, DestinationPort);
			AddFilterDefaults(collection, "Master Bill", MasterBill);
			AddFilterDefaults(collection, "ETD", ETDFrom, ETDTo);
			AddFilterDefaults(collection, "ETA", ETAFrom, ETATo);
			AddFilterDefaults(collection, "Carrier", Carrier);
			AddFilterDefaults(collection, "Send / Receive Agents", SendingAgent, ReceivingAgent);

			#endregion
		}
	}
}
