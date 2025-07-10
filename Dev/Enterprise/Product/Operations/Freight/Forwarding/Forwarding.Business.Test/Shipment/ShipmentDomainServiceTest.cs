using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ShipmentDomainServiceTest : TestCaseWithFactory
	{
		public void TestOnNewSupplierBuyerEvent()
		{
			ShipmentDomainService service = ShipmentDomainService.GetInstance(Factory);
			CommonConsol consol = Factory.New<CommonConsol>();
			AssertEquals("Event not hooked, default result should be No.", ZDialogResult.Cancel, service.QueryNewSupplierBuyerLink());

			service.NewSupplierBuyerLink += (sender, eventArgs) => eventArgs.Result = ZDialogResult.OK;
			AssertEquals("OnNewSupplierBuyer event hooked.", ZDialogResult.OK, service.QueryNewSupplierBuyerLink());
		}
	}
}
