using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.Module
{
	public class ClientRatesFilterBusinessObject : RatingFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);
			return filters;
		}

		readonly ClientRatesCRMSecurityProvider SecurityProvider = new ClientRatesCRMSecurityProvider();

		protected override string GetRateType() => RatingConstants.RatingHeaderTypes.ClientRate;
	}
}

