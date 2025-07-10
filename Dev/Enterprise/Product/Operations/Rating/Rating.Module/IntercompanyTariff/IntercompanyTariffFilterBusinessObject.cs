using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Module
{
	public class IntercompanyTariffFilterBusinessObject : RatingFilterBusinessObject
	{
		#region Filters

		protected override bool ShouldIncludeAllStandardRateEntryFilters => false;

		protected override bool ShouldIncludeGlobalRateFilter => false;

		protected override bool ShouldIncludeSalesRepFilter => false;

		protected override string RatingHeaderOrganisationName =>
			RateFilterHelper.Constants.ServiceProvider;

		protected override MultilingualString RatingHeaderOrganisationCaption =>
			ResString.GetMultilingualString("18CD7095-8243-4753-A13D-3B07CAC38196", "Service Provider");

		protected override MultilingualString GetOrganizationNameString() =>
			ResString.GetMultilingualString("97ACDEC0-880D-44AD-A963-CB2DB4FF59F6", "Service Provider Name");

		#endregion

		protected override string GetRateType() => RatingConstants.RatingHeaderTypes.IntercompanyTariff;
	}
}

