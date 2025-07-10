using System;
using CargoWise.Common;
using Enterprise.Freight.CarbonEmissions.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public static class ICO2eLegProviderExtensions
	{
		public static IDisposable WithTempCurrentCO2eCalcSupporter(this ICO2eLegProvider leg, ICO2eLegBasedSupporter supporter)
		{
			if (leg == null)
			{
				return DisposableAction.NoAction;
			}

			var previousSupporter = leg.CurrentCO2eCalcSupporter;
			leg.CurrentCO2eCalcSupporter = supporter;

			return new DisposableAction(() =>
			{
				leg.CurrentCO2eCalcSupporter = previousSupporter;
			});
		}
	}
}
