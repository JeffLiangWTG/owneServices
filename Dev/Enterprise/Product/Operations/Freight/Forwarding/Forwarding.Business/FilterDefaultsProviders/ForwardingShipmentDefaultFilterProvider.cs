using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentDefaultFilterProvider : DefaultFilterProvider
	{
		[Flags]
		public enum ShipmentTypes
		{
			None = 0,
			AssemblyMaster = 1,
			BuyersConsolLead = 2,
			ColoadMaster = 4,
			BlindCoLoadMaster = 8,
			Standard = 16,
			HighVolumeLowValue = 32,
			ThirdPartyOwnershipHouse = 64,
			All = AssemblyMaster | BuyersConsolLead | ColoadMaster | BlindCoLoadMaster | Standard | HighVolumeLowValue | ThirdPartyOwnershipHouse,
		}

		public ForwardingShipmentDefaultFilterProvider()
		{
			ShipmentType = ShipmentTypes.All;
		}

		public ZString ConsolNo { get; set; }
		public ZString TransportMode { get; set; }
		public ZString ContainerMode { get; set; }
		public ZString OriginPort { get; set; }
		public ZString DestinationPort { get; set; }
		public ZString LoadPort { get; set; }
		public ZString DischargePort { get; set; }
		public ZDateTime ETDFrom { get; set; }
		public ZDateTime ETDTo { get; set; }
		public ZDateTime ETAFrom { get; set; }
		public ZDateTime ETATo { get; set; }
		public ShipmentTypes ShipmentType { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		protected override void SetDefaultFiltersCore(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddFilterDefaults(collection, "Transport Mode", TransportMode);
			AddFilterDefaults(collection, "Container Mode", ContainerMode);
			AddFilterDefaults(collection, "Load / Discharge", LoadPort, DischargePort);
			AddFilterDefaults(collection, "Origin / Destination", OriginPort, DestinationPort);
			AddFilterDefaults(collection, "ETD", ETDFrom, ETDTo);
			AddFilterDefaults(collection, "ETA", ETAFrom, ETATo);
			AddFilterDefaults(collection, FreightConstants.NumberFilterTypes.ConsolNo, ConsolNo);

			if ((ShipmentType & ShipmentTypes.All) != ShipmentTypes.All)
			{
				const string FilterName = "Shipment Type";

				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterName, "Property0", (ZBool)((ShipmentType & ShipmentTypes.AssemblyMaster) != 0)));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterName, "Property1", (ZBool)((ShipmentType & ShipmentTypes.BuyersConsolLead) != 0)));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterName, "Property2", (ZBool)((ShipmentType & ShipmentTypes.ColoadMaster) != 0)));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterName, "Property3", (ZBool)((ShipmentType & ShipmentTypes.BlindCoLoadMaster) != 0)));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterName, "Property4", (ZBool)((ShipmentType & ShipmentTypes.Standard) != 0)));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterName, "Property5", (ZBool)((ShipmentType & ShipmentTypes.HighVolumeLowValue) != 0)));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterName, "Property6", (ZBool)((ShipmentType & ShipmentTypes.ThirdPartyOwnershipHouse) != 0)));
			}
		}
	}
}
