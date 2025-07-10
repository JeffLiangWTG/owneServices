using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsInventoryHeldCodeFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Code = "Code"; // Filter description
			public const string Description = "Description"; // Filter description
			public const string Client = "Client"; // Filter description
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddTextFilters(result);
			AddClientFilter(result);
			return result;
		}

		#endregion

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(Schema.Code, WhsInventoryHeldCodeSchema.WHC_Code).MultilingualDescription = ResString.GetMultilingualString("WhsInventoryHeldCodeFilterBusinessObject|Code", "Code");
			filters.AddFiltersForTranslatableText(Schema.Description, WhsInventoryHeldCodeSchema.WHC_Description, typeof(WhsInventoryHeldCode), ResString.GetMultilingualString("WhsInventoryHeldCodeFilterBusinessObject|Description", "Description"));
		}

		#endregion

		#region AddClientFilters

		void AddClientFilter(ModuleFilterCollection filters)
		{
			var clientFilter = filters.AddGuidFilter(Schema.Client, ModuleIDs.Organisation, WhsInventoryHeldCodeSchema.WHC_OH_Client, new WarehouseClientCollectionWithSecurityCheck(Factory));
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("448c52e7-b9a7-4b89-b55f-a3ce4ca9cbb4", "Client");
			clientFilter.Category = FilterCategories.Organisations;
			clientFilter.IsPublishedOnWeb = false;
			if (!Env.Security.WhsAllowedClients.IsAllowed && !Globals.IsWeb)
			{
				clientFilter.Visibility = FilterVisibility.AlwaysVisible;
			}
		}

		#endregion

		#region IsSystemDefinedDefaultProperty

		protected override string IsSystemDefinedDefaultProperty => DefinedStatusAllCode;

		#endregion
	}
}
