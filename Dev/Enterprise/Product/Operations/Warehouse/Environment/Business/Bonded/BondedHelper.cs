using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core;

namespace Enterprise.Warehouse.Environment.Business
{
	public static class BondedHelper
	{
		public static bool IsCountrySupportedForBonded(BusinessObjectFactory factory, string countryCode)
		{
			return ObjectFactory.Get<Enterprise.Integration.Customs.ISupportedForBonded>().IsSupportsBondedWarehousing(factory, countryCode);
		}

		public static IReadOnlyCollection<string> SupportedCountriesForFTZPermits
		{
			get
			{
				return new[]
				{
					Constants.CountryCodes.UnitedStates,
					Constants.CountryCodes.PuertoRico
				};
			}
		}

		public static bool IsCountrySupportedForFTZPermits(string countryCode) => SupportedCountriesForFTZPermits.Contains(countryCode);
	}
}
