using System;
using System.Collections.Generic;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business
{
	public static class PortManifestCountryList
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
				var ports = new List<string>
					{
						"NZAKL", "NZLYT", "NZNPE", "NZPOE", "NZORR", "NZTRG", "NZWLG"
					};

				if (FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures.Value)
				{
					ports.AddRange(new string[] { "ESBCN", "ESGAN", "ESPDS", "ESVLC" });
				}

				return ports;
			}
		}

		static IEnumerable<Guid> FilterCountries()
		{
			var enableSpanishPortIntegrationFeatures = FreightDataRegistry.Instance.EnableSpanishPortIntegrationFeatures.Value;
			return enableSpanishPortIntegrationFeatures
				? new[] { Core.Constants.CountryGuids.NewZealand, Core.Constants.CountryGuids.Spain }
				: new[] { Core.Constants.CountryGuids.NewZealand };
		}
	}
}
