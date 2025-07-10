using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Module
{
	public class WhsItemReceiveASNFilterBusinessObject : WhsTransitFilterBusinessObject
	{
		public static class FilterSchema
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter description")]
			public const string ASNID = "ASN ID";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter description")]
			public const string ASNExternalReference = "ASN External Reference";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			AddTextFilters(result);
			return result;
		}

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => WhsItemReceiveASNSchema.WRP_WW_IntendedWarehouse;

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddFountainFilter(FilterSchema.ASNID, WhsItemReceiveASNSchema.WRP_ReferenceNumber, "TRT").MultilingualDescription = ResString.GetMultilingualString("85b49dcd-50be-4f50-ab84-fd8e6c72abe7", "ASN ID");
			filters.AddTextFilter(FilterSchema.ASNExternalReference, WhsItemReceiveASNSchema.WRP_VehicleReference).MultilingualDescription = ResString.GetMultilingualString("4e76538a-fcbf-4aed-b614-060dd842463e", "ASN Reference");
		}
	}
}
