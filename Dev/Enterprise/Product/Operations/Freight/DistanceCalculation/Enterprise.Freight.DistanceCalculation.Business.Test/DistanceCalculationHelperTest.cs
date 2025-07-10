using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DistanceCalculation.Business.Testing
{
	sealed class DistanceCalculationHelperTest : TestCaseWithFactory
	{
		public void TestGetConfigurationFromRegistry()
		{
			DistanceCalculationConfiguration result = DistanceCalculationHelper.GetConfigurationFromRegistry();
			AssertEquals("Default registry configuration", DistanceCalculationConstants.Providers.CargoWise, result.ProviderCode);
			AssertEquals("Default registry configuration", "", result.ProviderVersion);
			AssertEquals("Default registry configuration", "", result.CalculationMethod);

			DistanceCalculationProviderConfiguration providerConfig = new DistanceCalculationProviderConfiguration();
			providerConfig.Provider = DistanceCalculationConstants.Providers.PCMiler;
			providerConfig.Version = "32";
			providerConfig.CalculationMethod = DistanceCalculationConstants.CalculationMethods.PCMiler.Practical;
			DistanceCalculationRegistry.Instance.DistanceCalculationProviderConfigurationItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, providerConfig);
			result = DistanceCalculationHelper.GetConfigurationFromRegistry();

			AssertEquals("Registry configuration", DistanceCalculationConstants.Providers.PCMiler, result.ProviderCode);
			AssertEquals("Registry configuration", "32", result.ProviderVersion);
			AssertEquals("Registry configuration", DistanceCalculationConstants.CalculationMethods.PCMiler.Practical, result.CalculationMethod);
		}

		public void TestGetConfigurationFromOrg()
		{
			DistanceCalculationProviderConfiguration providerConfig = new DistanceCalculationProviderConfiguration();
			providerConfig.Provider = DistanceCalculationConstants.Providers.PCMiler;
			providerConfig.Version = "32";
			providerConfig.CalculationMethod = DistanceCalculationConstants.CalculationMethods.PCMiler.Practical;
			DistanceCalculationRegistry.Instance.DistanceCalculationProviderConfigurationItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, providerConfig);

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.MiscServ.OM_CMDistanceCalculationProvider = DistanceCalculationConstants.Providers.DefaultFromRegistry;
			client.MiscServ.OM_CMDistanceCalculationVersion = "99";
			client.MiscServ.OM_CMDistanceCalculationMethod = "XXX";

			DistanceCalculationConfiguration config = DistanceCalculationHelper.GetConfigurationFromOrg(client);
			AssertEquals("Default - Registry configuration", DistanceCalculationConstants.Providers.PCMiler, config.ProviderCode);
			AssertEquals("Default - Registry configuration", "32", config.ProviderVersion);
			AssertEquals("Default - Registry configuration", DistanceCalculationConstants.CalculationMethods.PCMiler.Practical, config.CalculationMethod);

			client.MiscServ.OM_CMDistanceCalculationProvider = DistanceCalculationConstants.Providers.CargoWise;
			client.MiscServ.OM_CMDistanceCalculationVersion = "99";
			client.MiscServ.OM_CMDistanceCalculationMethod = "XXX";

			config = DistanceCalculationHelper.GetConfigurationFromOrg(client);
			AssertEquals("Configuration from the org", DistanceCalculationConstants.Providers.CargoWise, config.ProviderCode);
			AssertEquals("Configuration from the org", "99", config.ProviderVersion);
			AssertEquals("Configuration from the org", "XXX", config.CalculationMethod);
		}

		public void TestGetAddressFromAddress()
		{
			DistanceCalculationAddress result = DistanceCalculationHelper.GetAddressFromAddress(null);
			AssertEquals("Empty address for null", true, result.IsEmpty);

			OrgAddress address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "Address1";
			address.OA_Address2 = "Address2";
			address.OA_City = "City";
			address.OA_PostCode = "Code";
			address.OA_State = "State";
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			result = DistanceCalculationHelper.GetAddressFromAddress(address);
			AssertEquals("Address from dbo.OrgAddress", result.Address1, address.OA_Address1);
			AssertEquals("Address from dbo.OrgAddress", result.Address2, address.OA_Address2);
			AssertEquals("Address from dbo.OrgAddress", result.City, address.OA_City);
			AssertEquals("Address from dbo.OrgAddress", result.PostCode, address.OA_PostCode);
			AssertEquals("Address from dbo.OrgAddress", result.State, address.OA_State);
			AssertEquals("Address from dbo.OrgAddress", result.Country, "Australia");
		}

		public void TestGetAddressFromAddress_AddressFormatting()
		{
			DistanceCalculationAddress result = DistanceCalculationHelper.GetAddressFromAddress(null);
			AssertEquals("Empty address for null", true, result.IsEmpty);

			OrgAddress address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "Address1";
			address.OA_Address2 = "Address2";
			address.OA_City = "City";
			address.OA_PostCode = "Code";
			address.OA_State = "State";
			address.OA_RL_NKRelatedPortCode = "GBLON";

			result = DistanceCalculationHelper.GetAddressFromAddress(address);
			AssertEquals("Address from dbo.OrgAddress", result.Address1, address.OA_Address1);
			AssertEquals("Address from dbo.OrgAddress", result.Address2, address.OA_Address2);
			AssertEquals("Address from dbo.OrgAddress", result.City, address.OA_City);
			AssertEquals("Address from dbo.OrgAddress", result.PostCode, address.OA_PostCode);
			AssertNull("State is blank due to address formatting rule", result.State);
			AssertEquals("Address from dbo.OrgAddress", result.Country, "United Kingdom");
		}

		public void TestGetAddressFromUNLOCO()
		{
			DistanceCalculationAddress result = DistanceCalculationHelper.GetAddressFromUNLOCO(null);
			AssertEquals("Empty address for null", true, result.IsEmpty);

			RefUNLOCO port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			result = DistanceCalculationHelper.GetAddressFromUNLOCO(port);

			AssertNull("Address from UNLOCO", result.Address1);
			AssertNull("Address from UNLOCO", result.Address2);
			AssertEquals("Address from UNLOCO", result.City, "Sydney");
			AssertNull("Address from UNLOCO", result.PostCode);
			AssertEquals("Address from UNLOCO", result.State, "NSW");
			AssertEquals("Address from UNLOCO", result.Country, "Australia");
		}

		public void TestGetAddressFromUNLOCO_AddressFormatting()
		{
			DistanceCalculationAddress result = DistanceCalculationHelper.GetAddressFromUNLOCO(null);
			AssertEquals("Empty address for null", true, result.IsEmpty);

			RefUNLOCO port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBLON"));
			result = DistanceCalculationHelper.GetAddressFromUNLOCO(port);

			AssertNull("Address from UNLOCO", result.Address1);
			AssertNull("Address from UNLOCO", result.Address2);
			AssertEquals("Address from UNLOCO", result.City, "London");
			AssertNull("Address from UNLOCO", result.PostCode);
			AssertNull("State blank due to address formatting rule", result.State);
			AssertEquals("Address from UNLOCO", result.Country, "United Kingdom");
		}
	}
}
