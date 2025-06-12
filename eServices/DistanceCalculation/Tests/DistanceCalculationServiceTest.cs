using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enterprise.Freight.DistanceCalculation.Service;
using Enterprise.Freight.DistanceCalculation.Integration;
using CargoWise.Services.Common;
using CargoWise.Services.Common.Model;

namespace CargoWise.eServices.DistanceCalculation.Tests
{
	[TestClass]
	public class DistanceCalculationServiceTest : TestCase
	{
		[TestMethod]
		public void TestSelectingProvider()
		{
			DistanceCalculationService service = new DistanceCalculationService();

			DistanceCalculationConfig.ProviderCode = "";
			DistanceCalculationResult result = service.Calculate(Configuration, DistanceCalculationConfig, OriginAddress, DestinationAddress);
			Assert.AreEqual<int>(-1, result.StatusMessage.IndexOf("Error - unknown ProviderCode"), "Default dummy for test provider");
		}

		[TestMethod]
		public void TestEmptyAddresses()
		{
			DistanceCalculationService service = new DistanceCalculationService();
			OriginAddress = new DistanceCalculationAddress();
			DestinationAddress = new DistanceCalculationAddress();

			DistanceCalculationResult result = service.Calculate(Configuration, DistanceCalculationConfig, OriginAddress, DestinationAddress);
			Assert.AreEqual<string>("Error - Origin or Destination address is empty.", result.StatusMessage);
		}

		[TestMethod]
		public void TestAuditLogging()
		{
			Configuration.ClientSpecifiedID = Guid.NewGuid();

			DistanceCalculationService service = new DistanceCalculationService();
			OriginAddress = new DistanceCalculationAddress("1000 Herrontown Rd", "", "Princeton", "NJ", "", "");
			DestinationAddress = new DistanceCalculationAddress("", "", "Edison", "NJ", "", "");

			DistanceCalculationResult result = service.Calculate(Configuration, DistanceCalculationConfig, OriginAddress, DestinationAddress);
			Assert.IsTrue(result.Distance < 32 && result.Distance > 27, "Distance in KM between 27km and 32km");

			Entities entity = new Entities(ConnectionProvider.GetCommonConnectionString());
			var auditRecord = entity.eHubAuditRequest.Where(x => x.B0_LicenceCode == Configuration.LicenceCode && x.B0_ClientSpecifiedIdentifier == Configuration.ClientSpecifiedID).First();
			Assert.IsNotNull(auditRecord, "auditRecord");
			Assert.AreEqual<string>(TransactionTypes.DistanceCalculation.Code, auditRecord.B0_TransactionType, "TransactionType");
			Assert.AreEqual<string>(TransactionTypes.DistanceCalculation.SubTypes.GoogleProvider, auditRecord.B0_TransactionSubType, "TransactionSubType");
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
			Configuration.LicenceCode = "DISTSTTST";
			Configuration.UserName = "TestUser";
			DistanceCalculationConfig = new DistanceCalculationConfiguration();

			OriginAddress = new DistanceCalculationAddress("", "", "Los Angeles", "", "", "");
			DestinationAddress = new DistanceCalculationAddress("", "", "San Diego", "", "", "");
		}

		#endregion
	}
}
