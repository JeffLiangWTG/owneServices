using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.Module.Testing
{
	[TestedType(typeof(PickupDeliveryConfirmModule))]
	public class PickupDeliveryConfirmModuleTest : ZModuleBasherTest
	{
		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			CommonShipment shipment = collection.Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm confirm2 = shipment.DeliveryConfirms.AddNew();
			((CommonPickupDeliveryConfirmCollection)collection).AdditionalFilter = new ZQuery(); //ReLoad ?
			base.AddTestObjects(collection);
		}
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.PickupDeliveryConfirm;
		}

		public void TestAllowCopyFilterGridHyperlinkToClipboard()
		{
			using (var pickUp = new PickupDeliveryConfirmModule())
			{
				Assert(!pickUp.AllowCopyFilterGridHyperlinkToClipboard);
			}
		}
	}
}
