using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	sealed class ExportAWBSpecialHandlingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNoExceptionIfOriginOrDestinationPortsAreNull()
		{
			AssertNoExceptionThrown(() =>
			{
				var handling = Factory.New<ExportAWBSpecialHandling>();
				var lookup = new ExportAWBSpecialHandlingLookups(handling);
				AssertNotNull("SpecialHandlingCodeDescriptionList should not be null", lookup.SpecialHandlingCodeDescriptionList);
			});
		}

		public void TestSpecialHandlingCodeDescriptionListInAirLineHasNoElements()
		{
			AssertNoExceptionThrown(() =>
			{
				var handling = Factory.New<ExportAWBSpecialHandling>();
				var lookup = new ExportAWBSpecialHandlingLookups(handling);
				AssertNotNull("SpecialHandlingCodeDescriptionList should be empty", lookup.SpecialHandlingCodeDescriptionListInAirLine);
				AssertEquals(0, lookup.SpecialHandlingCodeDescriptionListInAirLine.Count);
			});
		}
	}
}
