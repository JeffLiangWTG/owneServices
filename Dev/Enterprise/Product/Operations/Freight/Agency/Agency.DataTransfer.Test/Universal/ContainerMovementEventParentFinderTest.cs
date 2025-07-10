using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class ContainerMovementEventParentFinderTest : TestCaseWithFactory
	{
		public void TestGetLogParentsForEventUsingContext()
		{
			var finder = new ContainerMovementEventParentFinder(Factory, new ContainerMovementDataContextManager(), new DummyLogger());
			AssertNull(finder.GetLogParentsForEvent(new Event()));
		}
	}
}
