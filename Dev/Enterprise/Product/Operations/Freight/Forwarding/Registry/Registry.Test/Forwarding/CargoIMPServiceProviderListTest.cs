using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Registry.Business.eServices;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	public class CargoIMPServiceProviderListTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			AssertEquals("REGISTRY PRECONDITION", false, eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.Value);

			CargoIMPServiceProviderList list = CargoIMPServiceProviderList.New();
			AssertEquals(3, list.Count);
			Assert(list.ContainsCode(Constants.AWB.CargoIMPServiceProviderConstants.CCN));
			Assert(list.ContainsCode(Constants.AWB.CargoIMPServiceProviderConstants.Descartes));
			Assert(list.ContainsCode(Constants.AWB.CargoIMPServiceProviderConstants.HUB));
		}
	}
}
