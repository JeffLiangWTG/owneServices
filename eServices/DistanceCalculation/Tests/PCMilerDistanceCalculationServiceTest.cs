using CargoWise.Services.Common;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Freight.DistanceCalculation.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eServices.DistanceCalculation.Tests
{
	[TestClass]
	public class PCMilerDistanceCalculationServiceTest : TestCase
	{
		[TestMethod]
		public void PCMiler_TestErrorsInConfiguration()
		{
			PCMilerDistanceCalculationService service = new PCMilerDistanceCalculationService();

			DistanceCalculationConfig.UnitsForCalculation = "crap";
			DistanceCalculationResult result = service.Process(DistanceCalculationConfig, OriginAddress, DestinationAddress);
			Assert.AreEqual<string>("Error in configuration UnitsForCalculation - crap", result.StatusMessage, "Errors in result status message");
		}

		[TestMethod]
		public void PCMiler_TestValidateConfigurationVersion()
		{
			DistanceCalculationConfiguration config = new DistanceCalculationConfiguration();
			config.ProviderVersion = "";
			Assert.AreEqual<string>("", config.ProviderVersion, "Precondition: version is blank");

			PCMilerDistanceCalculationService service = new PCMilerDistanceCalculationService();
			service.Process(config, OriginAddress, DestinationAddress);
			Assert.AreEqual<string>("Current", config.ProviderVersion, "Blank version was mapped to Current");

			config.ProviderVersion = DistanceCalculationConstants.ProviderVersions.PCMiler.Current;
			service.Process(config, OriginAddress, DestinationAddress);
			Assert.AreEqual<string>("Current", config.ProviderVersion, "PCMiler.Current version was mapped to Current");
		}

		[TestMethod]
		public void PCMiler_TestValidateConfigurationCalculationMethod()
		{
			DistanceCalculationConfiguration config = new DistanceCalculationConfiguration();
			config.CalculationMethod = DistanceCalculationConstants.CalculationMethods.PCMiler.Practical;
			config.RoutingType = "";
			Assert.AreEqual<string>("", config.RoutingType, "Precondition: routing type is blank");

			PCMilerDistanceCalculationService service = new PCMilerDistanceCalculationService();
			service.Process(config, OriginAddress, DestinationAddress);
			Assert.AreEqual<string>(DistanceCalculationPCMilerConstants.RoutingTypes.Practical, config.RoutingType, "Practical calculation method was mapped to Practical routing type");

			config.CalculationMethod = DistanceCalculationConstants.CalculationMethods.PCMiler.Shortest;
			config.RoutingType = "";
			service.Process(config, OriginAddress, DestinationAddress);
			Assert.AreEqual<string>(DistanceCalculationPCMilerConstants.RoutingTypes.Shortest, config.RoutingType, "Shortest calculation method was mapped to Shortest routing type");
		}

		[TestMethod]
		public void PCMiler_TestServiceIsWorking()
		{
			PCMilerDistanceCalculationService service = new PCMilerDistanceCalculationService();
			DistanceCalculationResult result = service.Process(DistanceCalculationConfig, OriginAddress, DestinationAddress);
			Assert.AreEqual<double>(17.3, result.Distance, "Distance in miles"); // example
			Assert.AreEqual<string>("", result.StatusMessage, "No errors in result status message");
		}

		[TestMethod]
		public void PCMiler_TestDistanceUnits()
		{
			PCMilerDistanceCalculationService service = new PCMilerDistanceCalculationService();

			DistanceCalculationConfig.UnitsForCalculation = DistanceCalculationConstants.UnitsForCalculation.Miles;
			DistanceCalculationResult result = service.Process(DistanceCalculationConfig, OriginAddress, DestinationAddress);
			Assert.AreEqual<double>(17.3, result.Distance, "Distance in miles");
			Assert.AreEqual<string>("M", result.DistanceUnit, "Units in miles");
			Assert.AreEqual<string>("", result.StatusMessage, "No errors in result status message");

			DistanceCalculationConfig.UnitsForCalculation = DistanceCalculationConstants.UnitsForCalculation.Kilometres;
			result = service.Process(DistanceCalculationConfig, OriginAddress, DestinationAddress);

			Assert.AreEqual<double>(27.9, result.Distance, "Distance in kilometres");
			Assert.AreEqual<string>("K", result.DistanceUnit, "Units in kilometres");
			Assert.AreEqual<string>("", result.StatusMessage, "No errors in result status message");
		}

		[TestMethod]
		public void PCMiler_TestUnexistingAddresses()
		{
			OriginAddress = new DistanceCalculationAddress("ZZ", "ZZ", "ZZ", "", "", "");
			DestinationAddress = new DistanceCalculationAddress("XX", "XX", "XX", "", "", "");

			PCMilerDistanceCalculationService service = new PCMilerDistanceCalculationService();
			DistanceCalculationResult result = service.Process(DistanceCalculationConfig, OriginAddress, DestinationAddress);
			Assert.AreNotEqual<string>("", result.StatusMessage, "Errors in result status message");
		}

		[TestMethod]
		public void PCMiler_TestWrongUnitsForCalculationInConfiguration()
		{
			PCMilerDistanceCalculationService service = new PCMilerDistanceCalculationService();

			DistanceCalculationConfig.UnitsForCalculation = DistanceCalculationConstants.UnitsForCalculation.Miles;
			DistanceCalculationResult result = service.Process(DistanceCalculationConfig, OriginAddress, DestinationAddress);
			Assert.AreEqual<string>("", result.StatusMessage, "No errors in result status message");

			DistanceCalculationConfig.UnitsForCalculation = DistanceCalculationConstants.UnitsForCalculation.Kilometres;
			result = service.Process(DistanceCalculationConfig, OriginAddress, DestinationAddress);
			Assert.AreEqual<string>("", result.StatusMessage, "No errors in result status message");

			DistanceCalculationConfig.UnitsForCalculation = "crap";
			result = service.Process(DistanceCalculationConfig, OriginAddress, DestinationAddress);
			Assert.AreEqual<string>("Error in configuration UnitsForCalculation - crap", result.StatusMessage, "Errors in result status message");
		}

		[TestMethod]
		public void PCMiler_TestWrongProviderVersionInConfiguration()
		{
			PCMilerDistanceCalculationService service = new PCMilerDistanceCalculationService();

			DistanceCalculationConfig.ProviderVersion = DistanceCalculationConstants.ProviderVersions.PCMiler.Current;
			DistanceCalculationResult result = service.Process(DistanceCalculationConfig, OriginAddress, DestinationAddress);
			Assert.AreEqual<string>("", result.StatusMessage, "No errors in result status message");

			DistanceCalculationConfig.ProviderVersion = "crap";
			result = service.Process(DistanceCalculationConfig, OriginAddress, DestinationAddress);
			Assert.AreEqual<string>("", result.StatusMessage, "No errors in result status message");
		}

		[TestMethod]
		public void PCMiler_TestConfigPCMilerProviderVersion23()
		{
			var result = VerifyPCMilerVersion(DistanceCalculationConstants.ProviderVersions.PCMiler.v23);
			Assert.AreEqual<string>("", result.StatusMessage, "Errors present in result status message");
		}

		[TestMethod]
		public void PCMiler_TestConfigPCMilerProviderVersion24()
		{
			var result = VerifyPCMilerVersion(DistanceCalculationConstants.ProviderVersions.PCMiler.v24);
			Assert.AreEqual<string>("", result.StatusMessage, "Errors present in result status message");
		}

		public DistanceCalculationResult VerifyPCMilerVersion(string version)
		{
			PCMilerDistanceCalculationService service = new PCMilerDistanceCalculationService();

			DistanceCalculationConfig.ProviderVersion = version;
			DistanceCalculationResult result = service.Process(DistanceCalculationConfig, OriginAddress, DestinationAddress);
			return result;
		}


		#region Implementation

		ServiceRequestConfigurationData Configuration;
		DistanceCalculationConfiguration DistanceCalculationConfig;
		DistanceCalculationAddress OriginAddress;
		DistanceCalculationAddress DestinationAddress;

		[TestInitialize]
		public void SetUp()
		{
			Configuration = new ServiceRequestConfigurationData();
			DistanceCalculationConfig = new DistanceCalculationConfiguration();
			DistanceCalculationConfig.ProviderVersion = DistanceCalculationConstants.ProviderVersions.PCMiler.Current;

			OriginAddress = new DistanceCalculationAddress("1000 Herrontown Rd", "", "Princeton", "NJ", "", "");
			DestinationAddress = new DistanceCalculationAddress("", "", "Edison", "NJ", "", "");
		}

		#endregion
	}
}
