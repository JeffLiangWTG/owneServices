using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CreateDeclarationHelper))]
	sealed public class CreateDeclarationHelperTest : TestCaseWithFactory
	{
		public void TestGetNewImportJobDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertType<ImportJobDeclaration>(new CreateDeclarationHelper().GetNewImportJobDeclaration(shipment));
		}
	}
}
