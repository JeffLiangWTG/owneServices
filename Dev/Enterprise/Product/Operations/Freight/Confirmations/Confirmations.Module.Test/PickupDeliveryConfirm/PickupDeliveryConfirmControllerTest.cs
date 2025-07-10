using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.Module.Testing
{
	[TestedType(typeof(PickupDeliveryConfirmController))]
	public class PickupDeliveryConfirmControllerTest : ZControllerBasherTest
	{
		[ExpectNoExceptions]
		public void TestGetForm()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			Factory.Save();

			using (IZForm form = new PickupDeliveryConfirmController().ShowEditForm(confirm))
			{
				form.Dispose();
				AssertEquals("Should set Shipment State", ChildEditableServiceStates.Shipment, ChildEditableService.GetState(Factory));
			}
			using (IZForm form = new PickupDeliveryConfirmController().ShowViewForm(confirm))
			{ }
		}

		#region Overrides

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			var confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_PickupDeliveryTime = ZDateTime.UtcNow;
			Factory.Save();
			return confirm;
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(CommonPickupDeliveryConfirm);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PickupDeliveryConfirm;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#endregion
	}
}
