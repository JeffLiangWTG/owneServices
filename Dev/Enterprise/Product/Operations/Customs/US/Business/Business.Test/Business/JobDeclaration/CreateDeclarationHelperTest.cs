using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class CreateDeclarationHelperTest : TestCaseWithFactory
	{
		public void TestGetNewImportJobDeclaration()
		{
			var helper = new CreateDeclarationHelper();
			var shipment = Factory.New<ForwardingShipment>();
			AssertType<ImportJobDeclaration>(helper.GetNewImportJobDeclaration(shipment));
		}
	}
}
