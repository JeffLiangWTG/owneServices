using System;
using CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.GVMSReferenceData
{
	[TestFixture]
	class GVMSReferenceDataClientTest
	{
		[Test]
		public void GetApiResponseWithNullClient()
		{
			Assert.Throws(Is.TypeOf<ArgumentNullException>().And.Message.StartsWith("Value cannot be null"), () => TestHelperClasses.GVMSReferenceDataClientTester.GetGvmsApiResponse(null, "Unit Test"));
		}

		[Test]
		public void GetApiResponseExceptionOccurs()
		{
			var gvmsWebClient = new Mock<IGvmsWebClient>();
			gvmsWebClient.Setup(x => x.GetApiResponse("ExceptionURL")).Throws<Exception>();

			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith("Could not get API response for 'ExceptionURL'"), () => TestHelperClasses.GVMSReferenceDataClientTester.GetGvmsApiResponse(gvmsWebClient.Object, "ExceptionURL"));
		}

		[Test]
		public void GetApiResponseNoContent()
		{
			var gvmsWebClient = new Mock<IGvmsWebClient>();
			gvmsWebClient.Setup(x => x.GetApiResponse("NoContent")).Returns(string.Empty);

			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith("No data returned"), () => TestHelperClasses.GVMSReferenceDataClientTester.GetGvmsApiResponse(gvmsWebClient.Object, "NoContent"));
		}

		[Test]
		public void GetApiResponseInvalidContent()
		{
			var gvmsWebClient = new Mock<IGvmsWebClient>();
			gvmsWebClient.Setup(x => x.GetApiResponse("InvalidContent")).Returns("Invalid Content which is not a json file list");

			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith("Could not convert content to Reference Data object"), () => gvmsReferenceDataClientTester.GetReferenceData(gvmsWebClient.Object, "InvalidContent"));
		}

		[Test]
		public void GetApiResponseValidContent()
		{
			var content = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.GVMSReferenceData.TestFiles.Input.GVMS_ReferenceData_Valid_Response_Test.json");
			var gvmsWebClient = new Mock<IGvmsWebClient>();
			gvmsWebClient.Setup(x => x.GetApiResponse("ValidContent")).Returns(content);

			var result = gvmsReferenceDataClientTester.GetReferenceData(gvmsWebClient.Object, "ValidContent");

			Assert.That(result.Carriers, Is.Not.Null.And.Count.EqualTo(2));
			Assert.That(result.Carriers[0].CarrierId, Is.EqualTo("1"));
			Assert.That(result.Carriers[0].CarrierName, Is.EqualTo("Stena Line"));

			Assert.That(result.Ports, Is.Not.Null.And.Count.EqualTo(3));
			Assert.That(result.Ports[1].PortId, Is.EqualTo("1401"));
			Assert.That(result.Ports[1].PortDescription, Is.EqualTo("Calais"));

			Assert.That(result.Routes, Is.Not.Null.And.Count.EqualTo(3));
			Assert.That(result.Routes[2].RouteId, Is.EqualTo("104"));
			Assert.That(result.Routes[2].RouteEffectiveFrom, Is.EqualTo(new DateTime(2020, 1, 1)));

			Assert.That(result.InspectionLocations, Is.Not.Null.And.Count.EqualTo(2));
			Assert.That(result.InspectionLocations[0].LocationId, Is.EqualTo("Location 1"));
			Assert.That(result.InspectionLocations[0].LocationType, Is.EqualTo("IBF"));
			Assert.That(result.InspectionLocations[1].Address.ToString, Is.EqualTo("Line 4, Line 5, Town 2, Postcode 2"));
			Assert.That(result.InspectionLocations[1].LocationEffectiveFrom, Is.EqualTo(new DateTime(2023, 05, 22)));

			Assert.That(result.InspectionTypes, Is.Not.Null.And.Count.EqualTo(2));
			Assert.That(result.InspectionTypes[0].InspectionTypeId, Is.EqualTo("Inspection Type 1"));
			Assert.That(result.InspectionTypes[1].Description, Is.EqualTo("Description 2"));

			Assert.That(result.RuleFailures, Is.Not.Null.And.Count.EqualTo(2));
			Assert.That(result.RuleFailures[0].RuleId, Is.EqualTo("BR005"));
			Assert.That(result.RuleFailures[0].RuleDescription, Is.EqualTo("A GMR can only be used in one crossing between two customs territiories"));
		}

		[Test]
		public void GetApiResponseValidContentValidationError()
		{
			var content = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.GVMSReferenceData.TestFiles.Input.GVMS_ReferenceData_InValid_Response_Test.json");
			var gvmsWebClient = new Mock<IGvmsWebClient>();
			gvmsWebClient.Setup(x => x.GetApiResponse("ValidContent")).Returns(content);

			var result = gvmsReferenceDataClientTester.GetReferenceData(gvmsWebClient.Object, "ValidContent");

			Assert.That(result.ErrorCollector.Contains("Carrier validation error"));
			Assert.That(result.ErrorCollector.Contains("Port validation error"));
			Assert.That(result.ErrorCollector.Contains("Route validation error"));
			Assert.That(result.ErrorCollector.Contains("Inspection Location validation error"));
			Assert.That(result.ErrorCollector.Contains("Inspection Type validation error"));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			gvmsReferenceDataClientTester = new TestHelperClasses.GVMSReferenceDataClientTester();
		}

		TestHelperClasses.GVMSReferenceDataClientTester gvmsReferenceDataClientTester;
	}
}
