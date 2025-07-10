using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public class UrsNamedAccountFilterBusinessObject : FilterStripBusinessObject
	{
		public UrsNamedAccountFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "OrgCarrierNamedAccounts";
			CarrierFilterDisabled = false;
			QueryObjectType = typeof(OrgCarrierNamedAccount);
		}

		public UrsNamedAccountFilterBusinessObject(bool carrierFilterDisabled = false) : base()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "OrgCarrierNamedAccounts";
			CarrierFilterDisabled = carrierFilterDisabled;
			QueryObjectType = typeof(OrgCarrierNamedAccount);
		}

		public static class Schema
		{
			public const string Organisation = "Organisation";
			public const string OrganisationName = "OrganisationName";
			public const string Carrier = "Carrier";
			public const string CarrierName = "CarrierName";
			public const string ForeignName = "ForeignName";
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var organisationFilter = filters.AddGuidFilter(Schema.Organisation, ModuleIDs.Organisation, OrgCarrierNamedAccountSchema.ONA_OH_Organization, new OrgHeaderCollection(Factory));
			organisationFilter.Category = FilterCategories.Organisations;
			organisationFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgCarrierNamedAccountsFilter|Organization", "Organization");

			var organisationNameFilter = filters.AddTextFilter(Schema.OrganisationName, GetOrganisationNameQuery);
			organisationNameFilter.MaxLength = OrgHeaderSchema.OH_FullName.MaxLength;
			organisationNameFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgCarrierNamedAccountsFilter|OrganizationName", "Organization Name");
			organisationNameFilter.Category = FilterCategories.Organisations;

			if (CarrierFilterDisabled)
			{
				var carrierFilter = filters.AddGuidFilter(Schema.Carrier, ModuleIDs.Organisation, OrgCarrierNamedAccountSchema.ONA_OH_Carrier, new OrgHeaderCollection(Factory));
				carrierFilter.Category = FilterCategories.Organisations;
				carrierFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgCarrierNamedAccountsFilter|Carrier", "Carrier");

				var carrierNameFilter = filters.AddTextFilter(Schema.CarrierName, GetCarrierNameQuery);
				carrierNameFilter.MaxLength = OrgHeaderSchema.OH_FullName.MaxLength;
				carrierNameFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgCarrierNamedAccountsFilter|CarrierName", "Carrier Name");
				carrierNameFilter.Category = FilterCategories.Organisations;
			}

			var foreignNameFilter = filters.AddTextFilter(Schema.ForeignName, OrgCarrierNamedAccountSchema.ONA_ForeignName);
			foreignNameFilter.Category = FilterCategories.TextSearch;
			foreignNameFilter.MaxLength = OrgCarrierNamedAccountSchema.ONA_ForeignName.MaxLength;
			foreignNameFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgCarrierNamedAccountsFilter|ForeignName", "Foreign Name");

			return filters;
		}

		#endregion

		#region Organisation Name

		static ZQuery GetOrganisationNameQuery(SQLComparisonOperator comparisonOperator, ZString organisationName)
		{
			return GetOrgHeaderNameQuery(comparisonOperator, organisationName, OrgCarrierNamedAccountSchema.ONA_OH_Organization);
		}

		static ZQuery GetCarrierNameQuery(SQLComparisonOperator comparisonOperator, ZString organisationName)
		{
			return GetOrgHeaderNameQuery(comparisonOperator, organisationName, OrgCarrierNamedAccountSchema.ONA_OH_Carrier);
		}

		static ZQuery GetOrgHeaderNameQuery(SQLComparisonOperator comparisonOperator, string organisationName, SchemaColumn column)
		{
			var result = new ZDBOnlyQuery(typeof(OrgCarrierNamedAccount));

			var notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);
			var nameQuery = new ZDBOnlySubQuery(typeof(OrgHeader), column, notIn);
			nameQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, organisationName);

			result.AddSubQuery(nameQuery, JoinCondition.And);
			return result;
		}

		#endregion

		public readonly bool CarrierFilterDisabled;
	}
}
