using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class GoodsCatalogFilterBusinessObject : FilterStripBusinessObject
	{
		public GoodsCatalogFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddOrganisationFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var desciptionFilter = filters.AddTextFilter("Description", CusGoodsCatalogSchema.CGC_Description);
			desciptionFilter.MaxLength = CusGoodsCatalogSchema.CGC_Description.MaxLength;
			desciptionFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusGoodsCatalogFilter|Description", "Description");

			var codeFilter = filters.AddTextFilter("Lookup Code", CusGoodsCatalogSchema.CGC_CatalogCode);
			codeFilter.MaxLength = CusGoodsCatalogSchema.CGC_CatalogCode.MaxLength;
			codeFilter.Category = FilterCategories.NumbersAndReferences;
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusGoodsCatalogFilter|LookupCode", "Lookup Code");

			var tariffFilter = filters.AddTextFilter("Tariff", CusGoodsCatalogSchema.CGC_Tariff);
			tariffFilter.MaxLength = CusGoodsCatalogSchema.CGC_Tariff.MaxLength;
			tariffFilter.Category = FilterCategories.NumbersAndReferences;
			tariffFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusGoodsCatalogFilter|Tariff", "Tariff");

			var typeFilter = filters.AddTextFilter("Type", CusGoodsCatalogSchema.CGC_Type, Lookups.TypeList);
			typeFilter.MaxLength = CusGoodsCatalogSchema.CGC_Type.MaxLength;
			typeFilter.Category = FilterCategories.ModesAndTypes;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusGoodsCatalogFilter|Type", "Type");

			var identifierFilter = filters.AddTextFilter("Authority Identifier", CusGoodsCatalogSchema.CGC_AuthorityIdentifier);
			identifierFilter.MaxLength = CusGoodsCatalogSchema.CGC_AuthorityIdentifier.MaxLength;
			identifierFilter.Category = FilterCategories.NumbersAndReferences;
			identifierFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusGoodsCatalogFilter|AuthorityIdentifier", "Authority Identifier");

			var statusFilter = filters.AddTextFilter("Status", CusGoodsCatalogSchema.CGC_AuthorityStatus, Lookups.StatusTypeList);
			statusFilter.MaxLength = CusGoodsCatalogSchema.CGC_AuthorityStatus.MaxLength;
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusGoodsCatalogFilter|Status", "Status");
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			statusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var messageStatusFilter = filters.AddTextFilter("Message Status", GetMessageStatusQuery, Lookups.MessageStatusList);
			messageStatusFilter.MaxLength = CusGoodsCatalogSchema.CGC_MessageStatus.MaxLength;
			messageStatusFilter.Category = FilterCategories.StatusAndFlags;
			messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|CusGoodsCatalogFilter|MessageStatusText", "Message Status");
		}

		ZQuery GetMessageStatusQuery(ZString value)
		{
			if (value == DeclarationFilterConstants.EntryStatus.NotSentForFilter)
			{
				value = ZString.Empty;
			}
			return new ZQuery(CusGoodsCatalogSchema.CGC_MessageStatus, value);
		}

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Consignee", ModuleIDs.Organisation, CusGoodsCatalogSchema.CGC_OH_Owner, Lookups.Consignees).MultilingualDescription = ResString.GetMultilingualString("Customs|CusGoodsCatalogFilter|Consignee", "Consignee");
		}

		public GoodsCatalogFilterLookups Lookups => lookups ??= GetNewLookups();
		GoodsCatalogFilterLookups lookups;

		protected virtual GoodsCatalogFilterLookups GetNewLookups() => new GoodsCatalogFilterLookups(this);
	}
}
