using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class OrdersFilterProvider : DefaultFilterProvider
	{
		public ZString TransportMode { get; set; }
		public ZString ContainerMode { get; set; }
		public ZString OriginPort { get; set; }
		public ZString DestinationPort { get; set; }
		public ZString LoadPort { get; set; }
		public ZString DischargePort { get; set; }

		public ZGuid Buyer { get; set; }
		public ZGuid Supplier { get; set; }

		public ZBool? ShowUnAttatchedOrders { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class FilterNames
		{
			public const string TransportMode = "Transport Mode";
			public const string ContainerMode = "Container Mode";
			public const string LoadDischarge = "Planned Load / Discharge";
			public const string OriginDestination = "Planned Origin / Destination";
			public const string BuyerSupplier = "Buyer / Supplier";
			public const string AttachedUnattachedOrders = "Attached / Unattached Orders";
		}

		protected override void SetDefaultFiltersCore(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddFilterDefaults(collection, FilterNames.TransportMode, TransportMode);
			AddFilterDefaults(collection, FilterNames.ContainerMode, ContainerMode);
			AddFilterDefaults(collection, FilterNames.LoadDischarge, LoadPort, DischargePort);
			AddFilterDefaults(collection, FilterNames.OriginDestination, OriginPort, DestinationPort);

			AddFilterDefaults(collection, FilterNames.BuyerSupplier, Buyer, Supplier);

			if (ShowUnAttatchedOrders.HasValue)
			{
				AddFilterDefaults(collection, FilterNames.AttachedUnattachedOrders, ShowUnAttatchedOrders.Value ? (NoResString)"All" : (NoResString)"Attached");  // Filter related
			}
		}
	}
}
