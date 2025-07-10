using CargoWise.EntityFramework;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.DistanceCalculation.Business
{
	public static class DistanceCalculationHelper
	{
		public static DistanceCalculationConfiguration GetConfigurationFromRegistry()
		{
			DistanceCalculationConfiguration result = new DistanceCalculationConfiguration();
			result.ProviderCode = DistanceCalculationRegistry.Instance.DistanceCalculationProviderConfigurationItem.Value.Provider;
			result.ProviderVersion = DistanceCalculationRegistry.Instance.DistanceCalculationProviderConfigurationItem.Value.Version;
			result.CalculationMethod = DistanceCalculationRegistry.Instance.DistanceCalculationProviderConfigurationItem.Value.CalculationMethod;
			return result;
		}

		public static DistanceCalculationConfiguration GetConfigurationFromOrg(OrgHeader client)
		{
			DistanceCalculationConfiguration result = new DistanceCalculationConfiguration();
			if (client == null || client.MiscServ.OM_CMDistanceCalculationProvider == DistanceCalculationConstants.Providers.DefaultFromRegistry)
			{
				result = GetConfigurationFromRegistry();
			}
			else
			{
				result.ProviderCode = client.MiscServ.OM_CMDistanceCalculationProvider;
				result.ProviderVersion = client.MiscServ.OM_CMDistanceCalculationVersion;
				result.CalculationMethod = client.MiscServ.OM_CMDistanceCalculationMethod;
			}

			return result;
		}

		public static DistanceCalculationAddress GetAddressFromAddress(IDocAddress address)
		{
			DistanceCalculationAddress result = new DistanceCalculationAddress();

			if (address != null)
			{
				result.Address1 = address.E2_Address1;
				result.Address2 = address.E2_Address2;
				result.City = address.E2_City;
				result.PostCode = address.E2_Postcode;

				RefCountry country = address.CountryCode.IsEmpty ? null : RefCountry.LoadFromCountryCode(new BusinessObjectFactory() { NameForDebugging = "Distance Calculation Country Loader" }, address.CountryCode);
				if (country != null)
				{
					result.Country = country.RN_DescMultilingual;
				}

				if (ShouldIncludeState(country))
				{
					result.State = address.E2_State;
				}
			}

			return result;
		}

		public static DistanceCalculationAddress GetAddressFromUNLOCO(RefUNLOCO port)
		{
			DistanceCalculationAddress result = new DistanceCalculationAddress();

			if (port != null)
			{
				result.City = port.RL_PortName;

				if (port.Country != null)
				{
					result.Country = port.Country.RN_DescMultilingual;
				}

				if (ShouldIncludeState(port.Country) && port.CountryStates != null)
				{
					result.State = port.CountryStates.RW_Code;
				}
			}

			return result;
		}

		static bool ShouldIncludeState(RefCountry country)
		{
			return country == null || country.RN_AddressFormattingRule != CountryAddressFormattingRuleList.Codes.NoStateCityInCapitalsPostcodeAtEnd;
		}

		public static DistanceCalculationConfiguration GetConfigurationFromJobDocAddress(JobDocAddress jobDocAddress, OrgHeader orgForFallback)
		{
			if (jobDocAddress != null && !jobDocAddress.E2_AddressOverride && jobDocAddress.Organisation != null)
			{
				return GetConfigurationFromOrg(jobDocAddress.Organisation);
			}
			else
			{
				return GetConfigurationFromOrg(orgForFallback);
			}
		}
	}
}
