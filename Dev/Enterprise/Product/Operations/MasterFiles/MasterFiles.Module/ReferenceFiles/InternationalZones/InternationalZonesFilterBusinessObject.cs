using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class InternationalZonesFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddTextFilter("Code", RefZoneHeaderSchema.FZ_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefZoneHeaderFilter|Code", "Code");
			filters.AddFiltersForTranslatableText("Description", RefZoneHeaderSchema.FZ_Description, typeof(RefZoneHeader), ResString.GetMultilingualString("MasterFiles|RefZoneHeaderFilter|Description", "Description"));
			filters.AddTextFilter("Zone Type", RefZoneHeaderSchema.FZ_ZoneType, ZoneTypeList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefZoneHeaderFilter|ZoneType", "Zone Type");
			filters.AddTextFilter("Zone Mode", RefZoneHeaderSchema.FZ_ZoneMode, ZoneModeList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefZoneHeaderFilter|ZoneMode", "Zone Mode");
			filters.AddGuidFilter("Carrier/Customer/Gateway", ModuleIDs.Organisation, RefZoneHeaderSchema.FZ_OH_RelatedParty, OrganisationList).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefZoneHeaderFilter|CarrierCustomerGateway", "Carrier/Customer/Gateway");

			return filters;
		}

		#endregion

		#region Lookups

		protected virtual CodeDescriptionPairList ZoneTypeList
		{
			get { return new RefZoneHeaderLookups(null).ZoneTypes; }
		}

		OrganisationsFindBoxCollection OrganisationList
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		CodeDescriptionPairList ZoneModeList
		{
			get { return zoneModeList ?? (zoneModeList = new CodeDescriptionPairList(OLookUpEditType.RateModes)); }
		}
		CodeDescriptionPairList zoneModeList;

		#endregion
	}
}
