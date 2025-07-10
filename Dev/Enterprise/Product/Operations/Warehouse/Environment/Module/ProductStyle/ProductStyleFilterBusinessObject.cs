using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public class ProductStyleFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filter constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Constant Name")]
		public static class FilterConstants
		{
			public const string ProductStyleCode = "Product Style Code";
			public const string ProductStyleDescription = "Product Style Description";
			public const string ProductStyleOwner = "Product Style Owner";
		}

		#endregion

		#region ModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var codeFilter = filters.AddTextFilter(FilterConstants.ProductStyleCode, WhsProductStyleSchema.WST_Code);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("ProductStyleFilterBusinessObject|WST_Code", "Product Style Code");

			var descriptionFilter = filters.AddTextFilter(FilterConstants.ProductStyleDescription, WhsProductStyleSchema.WST_Description);
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("ProductStyleFilterBusinessObject|WST_Description", "Product Style Description");

			var ownerFilter = filters.AddGuidFilter(FilterConstants.ProductStyleOwner, ModuleIDs.Organisation, WhsProductStyleSchema.WST_OH_Owner, new WarehouseClientCollectionWithSecurityCheck(Factory));
			ownerFilter.MultilingualDescription = ResString.GetMultilingualString("ProductStyleFilterBusinessObject|WST_OH_Owner", "Product Style Owner");

			return filters;
		}

		#endregion
	}
}
