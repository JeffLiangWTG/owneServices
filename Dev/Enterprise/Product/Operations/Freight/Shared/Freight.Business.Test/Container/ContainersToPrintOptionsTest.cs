using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ContainersToPrintOptionsTest : TestCaseWithFactory
	{
		public void TestContainersToPrint()
		{
			ContainersToPrintOptions options = new ContainersToPrintOptions();

			AssertEquals(null, options.ContainersToPrint);
			AssertEquals(false, options.IncludeUnContainerised);

			CommonContainer container1 = Factory.New<CommonContainer>();
			CommonContainer container2 = Factory.New<CommonContainer>();

			CommonContainer[] containers = new CommonContainer[] { container1, container2 };
			options.ContainersToPrint = containers;

			AssertContainsExactElementsInAnyOrder(new[] { container1, container2 }, options.ContainersToPrint);
		}
	}
}
