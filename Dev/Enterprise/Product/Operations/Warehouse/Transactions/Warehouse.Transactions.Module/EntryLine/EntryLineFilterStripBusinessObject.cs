using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class EntryLineFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var importerFilter = filters.AddGuidFilter("Importer", ModuleIDs.Organisation, WhsInventoryViewSchema.WI_OH_Client, ImporterList);
			importerFilter.MultilingualDescription = ResString.GetMultilingualString("72cb40b4-b555-4526-8bc7-3d742fcf4237", "Importer");

			var productFilter = filters.AddGuidFilter("Product", ModuleIDs.SupplierPart, WhsInventoryViewSchema.WI_OP, PartList);
			productFilter.MultilingualDescription = ResString.GetMultilingualString("e92a5339-4ca8-4233-83b1-363ab10bb7ad", "Product");

			filters.AddTextFilter("Entry Number", WhsInventoryViewSchema.WI_BondedEntryKey).MultilingualDescription = ResString.GetMultilingualString("7c11be17-da91-4681-ae97-2a7f50ccdc3c", "Entry Number");

			var entryLineNumberFilter = filters.AddNumberFilter("Entry Line Number", GetEntryLineNumberQuery);
			entryLineNumberFilter.MultilingualDescription = ResString.GetMultilingualString("7af1c18c-02ba-4533-baaf-a9eb48b59428", "Entry Line Number");
			entryLineNumberFilter.MaxLength = WhsInventoryViewSchema.WI_BondedEntryKey.MaxLength - 1;           // GetEntryLineNumberQuery adds '-' symbol to value.

			filters.AddTextFilter("Bond ID 1", WhsInventoryViewSchema.WI_PartAttrib1).MultilingualDescription = ResString.GetMultilingualString("29e5355f-127f-405f-8b6d-17462e0075b8", "Bond ID 1");
			filters.AddTextFilter("Bond ID 2", WhsInventoryViewSchema.WI_PartAttrib2).MultilingualDescription = ResString.GetMultilingualString("be010f0b-48c8-4142-a8c8-20118fcccd13", "Bond ID 2");
			ModuleGuidFilter warehouseFilter = filters.AddGuidFilter("Warehouse", ModuleIDs.WhsConfigWarehouse, GetWarehouse, WarehouseList);
			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("82fcc8af-cde6-4b31-8384-343ad61a90f4", "Warehouse");
			warehouseFilter.Category = FilterCategories.Organisations;
			ModuleNumberFilter defaultFilter = filters.AddNumberFilter("Default Filter", GetDefaultQuery);
			defaultFilter.MultilingualDescription = ResString.GetMultilingualString("8037ab78-ea61-4288-aa40-22244a415cce", "Default Filter");
			defaultFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			return filters;
		}

		ModuleGuidFilter ImporterFilter
		{
			get { return (ModuleGuidFilter)ModuleFilters["Importer"]; }
		}

		OrgHeader Importer
		{
			get { return !ImporterFilter.Property.IsEmpty ? Factory.Load<OrgHeader>(ImporterFilter.Property) : null; }
		}

		protected ZQuery GetDefaultQuery(SQLComparisonOperator @operator, ZString value)
		{
			var result = new ZQuery();
			result.AddFilterAndZSQLParameterCollection(WhsInventoryViewSchema.WI_TotalUnits.Name + " > 0", null);
			result.OrderBy = WhsInventoryViewSchema.WI_BondedEntryKey.Name;
			return result;
		}

		protected ZQuery GetEntryLineNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			var result = new ZQuery();
			result.AddToFilter_PossiblyCommaSeparated(WhsInventoryViewSchema.WI_BondedEntryKey, SQLComparisonOperator.EndsWith, "-" + value.Replace(EnvProxy.Instance.Registry.MultiSearchSeparator, EnvProxy.Instance.Registry.MultiSearchSeparator + "-"));
			return result;
		}

		protected ZQuery GetWarehouse(ZGuid value)
		{
			var result = new ZDBOnlyQuery(typeof(WhsInventoryView));
			var locationQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsInventoryViewSchema.WI_WL);
			var rowQuery = new ZDBOnlySubQuery(typeof(WhsRow), WhsLocationViewSchema.WLV_WR);
			rowQuery.AddToFilter(WhsRowSchema.WR_WW_Whs, value);
			locationQuery.AddSubQuery(rowQuery, JoinCondition.And);
			result.AddSubQuery(locationQuery, JoinCondition.And);
			return result;
		}

		protected IList PartList()
		{
			return new WhsOrgSupplierPartCollection(Factory, null, Importer, false);
		}

		OrgHeaderCollection ImporterList
		{
			get { return new ConsigneeCollection(Factory); }
		}

		WhsWarehouseCollection WarehouseList
		{
			get { return new WhsWarehouseCollection(Factory); }
		}
	}
}
