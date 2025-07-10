using System.Net.Http;
using System.Reflection;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing
{
	public class EmissionSerializerTest : TestCase
	{
		public void TestFromHttpContent_UShipment()
		{
			// Arrange
			var str = resourceRetriever.GetString("Enterprise.Freight.DataTransfer.Test.Freight.Universal.TestFiles.CO2eUniversalShipment_Response.xml");
			var content = new StringContent(str);
			content.Headers.ContentType.MediaType = "application/xml";
			var serializer = new EmissionSerializer();

			// Act
			var result = serializer.FromHttpContentAsync<EmissionResult>(content).GetAwaiter().GetResult();

			// Assert
			Assert(result.IsUShipment);
			AssertEquals(10m, result.UXml.Left.GreenhouseGasEmission.CO2ePerTonne);
		}

		public void TestFromHttpContent_UEvent()
		{
			// Arrange
			var str = resourceRetriever.GetString("Enterprise.Freight.DataTransfer.Test.Freight.Universal.TestFiles.CO2eUniversalEvent_Response.xml");
			var content = new StringContent(str);
			content.Headers.ContentType.MediaType = "application/xml";
			var serializer = new EmissionSerializer();

			// Act
			var result = serializer.FromHttpContentAsync<EmissionResult>(content).GetAwaiter().GetResult();

			// Assert
			Assert(!result.IsUShipment);
			AssertEquals("Location code not found: locode:AUDNN", result.UXml.Right.EventParameters.Reason);
		}

		public void TestFromHttpContent_JsonContent()
		{
			// Arrange
			var content = new StringContent("{'name':'AuthorizationDenied', 'message': 'Authorization has been denied for this request.', 'debugId': '00000000-0000-0000-0000-000000000000'}");
			content.Headers.ContentType.MediaType = "application/json";
			var serializer = new EmissionSerializer();

			// Act
			var result = serializer.FromHttpContentAsync<EmissionResult>(content).GetAwaiter().GetResult();

			// Assert
			AssertNotNull(result.JsonContent);
			AssertEquals("Authorization has been denied for this request.", result.JsonContent.Message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(Assembly.Load("Enterprise.Freight.DataTransfer.Test"));
		}

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever?.Dispose();
		}

		EmbeddedResourceRetriever resourceRetriever;
	}
}
