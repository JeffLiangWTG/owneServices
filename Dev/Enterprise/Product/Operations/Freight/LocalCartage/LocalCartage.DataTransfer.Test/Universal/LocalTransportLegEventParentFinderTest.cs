using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal.Testing
{
	class LocalTransportLegEventParentFinderTest : TestCaseWithFactory
	{
		public void TestGetLogParentsForEventUsingContext()
		{
			AssertNull(GetNewEventParentFinder().GetLogParentsForEvent(new Event()));
		}

		LocalTransportLegEventParentFinder GetNewEventParentFinder()
		{
			return new LocalTransportLegEventParentFinder(Factory, new LocalTransportLegDataContextManager(), new DummyLogger());
		}
	}
}
