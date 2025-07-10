using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.OceanCarrier.Module
{
	public sealed class CarrierShipmentHeaderFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string ReferenceNumber = "ReferenceNumber";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			var carrierShipmentReferenceFilter = filters.AddFountainFilter(
				description: Schema.ReferenceNumber,
				filterColumn: CarrierShipmentHeaderSchema.CSH_CarrierShipmentReference,
				fountainPrefix: "CS");
			carrierShipmentReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("E38A1D10-DFAB-4208-80BB-429014C50D2C", "Carrier Reference Number");
			return filters;
		}
	}
}
