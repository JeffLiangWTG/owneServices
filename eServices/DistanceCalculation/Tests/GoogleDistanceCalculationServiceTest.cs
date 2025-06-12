using System.IO;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Freight.DistanceCalculation.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eServices.DistanceCalculation.Tests
{
	[TestClass]
	public class GoogleDistanceCalculationServiceTest : TestCase
	{
		[TestMethod]
		public void Google_TestService_Success()
		{
			GoogleDistanceCalculationService service = new GoogleDistanceCalculationService();

			DistanceCalculationResult result = service.Process(OriginAddress, DestinationAddress);
			Assert.IsTrue(result.Distance < 32 && result.Distance > 27, "Distance in KM between 27km and 32km");
			Assert.AreEqual<string>(DistanceCalculationConstants.UnitsForCalculation.Kilometres, result.DistanceUnit, "Distance in KM");
			Assert.IsTrue(result.TravelTime < 0.68 && result.TravelTime > 0.51, "Travel Time expected between 0.51 and 0.68 hrs, but was: " + result.TravelTime);
			Assert.AreEqual<string>("", result.StatusMessage, "No errors in result status message");
		}

		[TestMethod]
		public void Google_TestService_Error()
		{
			GoogleDistanceCalculationService service = new GoogleDistanceCalculationService();

			DistanceCalculationResult result = service.Process(new DistanceCalculationAddress() { Address1 = "Australia" }, new DistanceCalculationAddress() { Address1 = "India" });
			Assert.AreEqual<double>((double)0, result.Distance, "Distance in KM");
			Assert.AreEqual<string>(null, result.DistanceUnit, "Distance in KM");
			Assert.AreEqual<double>((double)0, result.TravelTime, "Travel Time");
			Assert.AreEqual<string>("The route between the origin and destination could not be resolved by the server.", result.StatusMessage, "Errors in result status message");
		}

		[TestMethod]
		public void Google_InvalidDataReturn_Error()
		{
			var result = GoogleDistanceCalculationService.ParseData(StreamFromString("{\"destination_addresses\" : [ \"Edison, NJ, USA\" ], \"status\" : \"OK\"}"));
			Assert.AreEqual("Distance element is missing", result.StatusMessage);
		}

		[TestMethod]
		public void Test_EncodingQueryString()
		{
			var service = new GoogleDistanceCalculationServiceForTest();

			OriginAddress = new DistanceCalculationAddress("1000 Herrontown Rd #123 & *#@?", "", "Princeton", "NJ", "", "");
			DestinationAddress = new DistanceCalculationAddress("", "", "Edison @@!@#$@#11", "NJ", "", "");

			Assert.AreEqual("http://maps.googleapis.com/maps/api/distancematrix/json?origins=%201000%20Herrontown%20Rd%20%23123%20%26%20*%23%40%3F%20Princeton%20NJ&destinations=%20Edison%20%40%40!%40%23%24%40%2311%20NJ&client=gme-translogix&mode=driving&language=en-EN&sensor=false&signature=gK6tuTmrdVywxKr8I4jWURE5oZQ=", service.GetSignedUrlExposed(OriginAddress, DestinationAddress));
		}

		#region Implementation

		DistanceCalculationConfiguration DistanceCalculationConfig;
		DistanceCalculationAddress OriginAddress;
		DistanceCalculationAddress DestinationAddress;

		Stream StreamFromString(string text)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(text);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		[TestInitialize()]
		public void SetUp()
		{
			DistanceCalculationConfig = new DistanceCalculationConfiguration();

			OriginAddress = new DistanceCalculationAddress("1000 Herrontown Rd", "", "Princeton", "NJ", "", "");
			DestinationAddress = new DistanceCalculationAddress("", "", "Edison", "NJ", "", "");
		}

		class GoogleDistanceCalculationServiceForTest : GoogleDistanceCalculationService
		{
			public string GetSignedUrlExposed(DistanceCalculationAddress originAddress, DistanceCalculationAddress destinationAddress) => GetSignedUrl(originAddress, destinationAddress);
		}

		#endregion
	}
}
