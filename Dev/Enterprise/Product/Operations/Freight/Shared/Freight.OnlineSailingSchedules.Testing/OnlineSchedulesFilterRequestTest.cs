using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	class OnlineSchedulesFilterRequestTest : TestCase
	{
		const string MinimumParameterRequirementsNotMetMessage = "Filter doesn't meet the minimum parameter requirements. Please specify Origin, Destination and either of ETD or ETA, or Vessel information.";

		public void TestValidate_LoadDischargeAndDates()
		{
			var expectedMessage = MinimumParameterRequirementsNotMetMessage;

			var request = new OnlineSchedulesFilterRequest
			{
				LoadPort = "AUSYD",
				DischargePort = "SGSIN",
				EtaFrom = "2017-01-01",
				EtaTo = "2017-02-01",
				EtdFrom = "2017-03-01",
				EtdTo = "2017-04-01"
			};

			AssertNotEquals(expectedMessage, request.Validate());

			request.LoadPort = string.Empty;

			AssertEquals(expectedMessage, request.Validate());

			request.LoadPort = "AUSYD";
			request.DischargePort = string.Empty;

			AssertEquals(expectedMessage, request.Validate());

			request.LoadPort = "AUSYD";
			request.DischargePort = "SGSIN";

			request.EtaFrom = string.Empty;
			AssertNotEquals(expectedMessage, request.Validate());

			request.EtaTo = string.Empty;
			AssertNotEquals(expectedMessage, request.Validate());

			request.EtdFrom = string.Empty;
			AssertNotEquals(expectedMessage, request.Validate());

			request.EtdTo = string.Empty;
			AssertEquals(expectedMessage, request.Validate());
		}

		public void TestValidate_VoyageNumberAndVesselNameAndIMONumber()
		{
			var expectedMessage = MinimumParameterRequirementsNotMetMessage;

			var request = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "AAA",
				VesselName = "BBB",
				ImoNumber = "CCC"
			};

			AssertNotEquals(expectedMessage, request.Validate());

			request.VoyageNumber = string.Empty;
			AssertNotEquals(expectedMessage, request.Validate());

			request.VesselName = string.Empty;
			AssertNotEquals(expectedMessage, request.Validate());

			request.ImoNumber = string.Empty;
			AssertEquals(expectedMessage, request.Validate());
		}

		public void TestValidate_VoyageNumberRequiresImoOrVesselName()
		{
			var expectedMessage = MinimumParameterRequirementsNotMetMessage;

			var request = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "AAA",
				VesselName = "BBB",
				ImoNumber = "CCC"
			};

			AssertNotEquals(expectedMessage, request.Validate());

			request.VesselName = string.Empty;
			AssertNotEquals(expectedMessage, request.Validate());

			request.ImoNumber = string.Empty;
			AssertEquals(expectedMessage, request.Validate());

			request.VesselName = "BBB";
			AssertNotEquals(expectedMessage, request.Validate());
		}

		public void TestValidate_ZeroNullEmptyOrWhitespaceImoConsideredInvalid()
		{
			var expectedMessage = MinimumParameterRequirementsNotMetMessage;

			var request = new OnlineSchedulesFilterRequest
			{
				ImoNumber = "CCC"
			};

			AssertNullOrEmpty(request.Validate());

			request.ImoNumber = null;
			AssertEquals("Null IMO number should not be accepted", expectedMessage, request.Validate());

			request.ImoNumber = "";
			AssertEquals("Empty IMO number should not be accepted", expectedMessage, request.Validate());

			request.ImoNumber = " ";
			AssertEquals("Whitespace IMO number should not be accepted", expectedMessage, request.Validate());

			request.ImoNumber = "0";
			AssertEquals("Zero IMO number should not be accepted", expectedMessage, request.Validate());
		}

		public void TestDefaultFlagsIncludeRelatedPortsFilterAndSameCarrierRoutes()
		{
			var request = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "AAA",
				VesselName = "BBB",
				ImoNumber = "CCC",
				IncludeRelatedPorts = true.ToString(),
				SameCarrierRoutes = false.ToString()
			};

			AssertEquals("True", request.IncludeRelatedPorts);
			AssertEquals("False", request.SameCarrierRoutes);
		}
	}
}
