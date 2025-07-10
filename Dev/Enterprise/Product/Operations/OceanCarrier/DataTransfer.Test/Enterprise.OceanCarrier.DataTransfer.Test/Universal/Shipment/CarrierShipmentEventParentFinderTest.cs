using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.OceanCarrier.DataTransfer.Universal.Testing
{
	class CarrierShipmentEventParentFinderTest : TestCaseWithFactory
	{
		public void TestGetLogParentsForEventUsingContext()
		{
			var finder = new CarrierShipmentEventParentFinder(Factory, new CarrierShipmentDataContextManager(), new DummyLogger());
			AssertNull(finder.GetLogParentsForEvent(new Event()));
		}
	}
}
