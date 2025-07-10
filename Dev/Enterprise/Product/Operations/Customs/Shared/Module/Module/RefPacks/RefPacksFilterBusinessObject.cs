using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.Customs.Module.ResString;

namespace Enterprise.Customs.Shared.Module
{
	public class RefPacksFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			filters.AddTextFilter("Commercial Packs", GetCommercialPacksQuery, CommercialPackList).MultilingualDescription = ResString.GetMultilingualString("Customs|RefPacksFilter|CommercialPacks", "Commercial Packs");
			filters.AddTextFilter("Customs Pack", GetCustomsPacksQuery, CustomsPackList).MultilingualDescription = ResString.GetMultilingualString("Customs|RefPacksFilter|CustomsPack", "Customs Pack");
			var supplierFilter = filters.AddGuidFilter("Supplier", ModuleIDs.Organisation, RefPacksSchema.RP_OH_Supplier, SupplierList);
			supplierFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|RefPacksFilter|Supplier", "Supplier");

			ModuleTextFilter countryFilter = filters.AddTextFilter("Country/Region", RefPacksSchema.RP_CustomsCountry);
			countryFilter.Property = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			countryFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			countryFilter.Visibility = FilterVisibility.AlwaysVisible;
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|RefPacksFilter|Country", "Country/Region");

			return filters;
		}

		ZQuery GetCommercialPacksQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(RefPacksSchema.RP_CommercialPack, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetCustomsPacksQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(RefPacksSchema.RP_CustomsPack, SQLComparisonOperator.Equal, value);
			return query;
		}

		public CodeDescriptionPairList CommercialPackList
		{
			get
			{
				return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits();
			}
		}

		public CodeDescriptionPairList CustomsPackList
		{
			get
			{
				CusRefPacks bizO = Factory.New<CusRefPacks>();
				return bizO.RP_CustomsPack_List;
			}
		}

		public ConsignorCollection SupplierList
		{
			get
			{
				return new ConsignorCollection(Factory);
			}
		}

		#region IsSystemDefinedDefaultProperty

		protected override string IsSystemDefinedDefaultProperty => DefinedStatusAllCode;

		#endregion
	}
}
