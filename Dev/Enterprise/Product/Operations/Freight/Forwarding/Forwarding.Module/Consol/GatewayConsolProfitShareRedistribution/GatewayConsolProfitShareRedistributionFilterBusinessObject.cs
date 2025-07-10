using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public class GatewayConsolProfitShareRedistributionFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			var batchNumberFilter = filters.AddTextFilter("Batch Number", ProfitShareRedistributionSchema.PSR_BatchNumber);
			batchNumberFilter.MultilingualDescription = ResString.GetMultilingualString("9ccb5dd5-3fe1-4c8c-88bc-bdc329577bf7", "Batch Number");
			batchNumberFilter.Visibility = FilterVisibility.AlwaysVisible;

			return filters;
		}
	}
}
