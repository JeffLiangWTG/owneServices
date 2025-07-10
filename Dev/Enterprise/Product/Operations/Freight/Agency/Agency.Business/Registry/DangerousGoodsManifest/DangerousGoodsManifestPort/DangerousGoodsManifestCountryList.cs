using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Freight.Agency.Business
{
	public static class DangerousGoodsManifestCountryList
	{
		public static IEnumerable<Guid> EnabledCountries
		{
			get
			{
				return FilterCountries();
			}
		}

		public static IEnumerable<string> EnabledPorts
		{
			get
			{
				return enabledPorts ?? (enabledPorts = new[]
				{
					"AUBNE",
					"AUMEL",
					"AUSYD"
				});
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static IEnumerable<string> enabledPorts;

		static IEnumerable<Guid> FilterCountries()
		{
			return new[]
				{
					Core.Constants.CountryGuids.Australia
				};
		}
	}
}
