using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.CarbonEmissions.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public static class ICO2eLegBasedSupporterExtensions
	{
		public static void ForEachLeg(this ICO2eLegBasedSupporter supporter, Action<ICO2eLegProvider> action, ICO2eLegBasedSupporter currentSupporter = default)
		{
			supporter?.Legs?.Cast<ICO2eLegProvider>().ForEach(leg =>
			{
				using (leg.WithTempCurrentCO2eCalcSupporter(currentSupporter ?? supporter))
				{
					action.Invoke(leg);
				}
			});
		}

		public static bool AllLeg(this ICO2eLegBasedSupporter supporter, Func<ICO2eLegProvider, bool> action, ICO2eLegBasedSupporter currentSupporter = default)
		{
			return supporter?.Legs?.Cast<ICO2eLegProvider>().All(leg =>
			{
				using (leg.WithTempCurrentCO2eCalcSupporter(currentSupporter ?? supporter))
				{
					return action.Invoke(leg);
				}
			}) ?? false;
		}

		public static decimal SumLeg(this ICO2eLegBasedSupporter supporter, Func<ICO2eLegProvider, decimal> action, ICO2eLegBasedSupporter currentSupporter = default)
		{
			return supporter?.Legs?.Cast<ICO2eLegProvider>().Sum(leg =>
			{
				using (leg.WithTempCurrentCO2eCalcSupporter(currentSupporter ?? supporter))
				{
					return action.Invoke(leg);
				}
			}) ?? 0;
		}
	}
}
