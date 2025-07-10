using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Module
{
	public class CostingFilterBusinessObject : RatingFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);
			return filters;
		}

		#region Filters

		protected override bool ShouldIncludeSalesRepFilter
		{
			get { return false; }
		}

		protected override string RatingHeaderOrganisationName
		{
			get { return RateFilterHelper.Constants.ServiceProvider; }
		}

		protected override MultilingualString RatingHeaderOrganisationCaption
		{
			get { return ResString.GetMultilingualString("18CD7095-8243-4753-A13D-3B07CAC38196", "Service Provider"); }
		}

		protected override MultilingualString GetOrganizationNameString()
		{
			return ResString.GetMultilingualString("97ACDEC0-880D-44AD-A963-CB2DB4FF59F6", "Service Provider Name");
		}

		#endregion

		readonly CostingCRMSecurityProvider SecurityProvider = new CostingCRMSecurityProvider();

		protected override string GetRateType() => RatingConstants.RatingHeaderTypes.Costing;
	}
}

