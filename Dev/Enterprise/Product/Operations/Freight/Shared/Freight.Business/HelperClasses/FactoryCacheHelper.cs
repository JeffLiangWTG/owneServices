using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public static class FactoryCacheHelper
	{
		public static bool GetIsViewingFromPortTransportLegPlanner(BusinessObjectFactory factory)
		{
			var result = factory.GetCachedValue("IsViewingFromPortTransportLegPlanner", () => new IsViewingFromPortTranspotLegPlannerValueProvider());
			return result.IsViewingFromPortTranspotLegPlanner;
		}

		public static void SetIsViewingFromPortTransportLegPlanner(BusinessObjectFactory factory)
		{
			var result = factory.GetCachedValue("IsViewingFromPortTransportLegPlanner", () => new IsViewingFromPortTranspotLegPlannerValueProvider());
			result.IsViewingFromPortTranspotLegPlanner = true;
		}

		class IsViewingFromPortTranspotLegPlannerValueProvider
		{
			public bool IsViewingFromPortTranspotLegPlanner { get; set; }
		}
	}
}
