using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class ShipmentTypeChangeHandlerTest : TestCaseWithFactory
	{
		public void TestHandleShipmentTypeChangingCLD()
		{
			var args = new ShipmentTypeChangingCancelEventArgs(ZString.Empty, Core.Constants.ShipmentTypes.CoLoadMaster);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			AssertHandleShipmentTypeChanging("changing to CLD shipment", args,
@"Marking this shipment as a Co-Load Master or Assembly master will mean that inner pack-line and outer pack-line data will be removed from this shipment, as it will be calculated from the related sub-shipments.

Are you sure you want to continue?");
			AssertEquals("not cancelled", false, args.Cancel);

			args = new ShipmentTypeChangingCancelEventArgs(ZString.Empty, Core.Constants.ShipmentTypes.CoLoadMaster);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

			AssertHandleShipmentTypeChanging("changing to CLD shipment", args,
@"Marking this shipment as a Co-Load Master or Assembly master will mean that inner pack-line and outer pack-line data will be removed from this shipment, as it will be calculated from the related sub-shipments.

Are you sure you want to continue?");
			AssertEquals("cancelled", true, args.Cancel);
		}

		public void TestHandleShipmentTypeChangingBlindCoLoadMaster()
		{
			var args = new ShipmentTypeChangingCancelEventArgs(ZString.Empty, Core.Constants.ShipmentTypes.BlindCoLoadMaster);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			AssertHandleShipmentTypeChanging("changing to CLB shipment", args,
@"Marking this shipment as a Co-Load Master or Assembly master will mean that inner pack-line and outer pack-line data will be removed from this shipment, as it will be calculated from the related sub-shipments.

Are you sure you want to continue?");
			AssertEquals("not cancelled", false, args.Cancel);

			args = new ShipmentTypeChangingCancelEventArgs(ZString.Empty, Core.Constants.ShipmentTypes.CoLoadMaster);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

			AssertHandleShipmentTypeChanging("changing to CLB shipment", args,
@"Marking this shipment as a Co-Load Master or Assembly master will mean that inner pack-line and outer pack-line data will be removed from this shipment, as it will be calculated from the related sub-shipments.

Are you sure you want to continue?");
			AssertEquals("cancelled", true, args.Cancel);
		}

		public void TestHandleShipmentTypeChangingSTD()
		{
			var args = new ShipmentTypeChangingCancelEventArgs(ZString.Empty, Core.Constants.ShipmentTypes.StandardHouse);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

			AssertHandleShipmentTypeChanging("changing to STD shipment", args,
@"Changing this shipment to be a Standard shipment will remove all related shipments from it.
Related shipments can only exist on a Buyers Consol Lead, Assembly Master or Co-Load Master shipment.

Are you sure you want to continue?");
			AssertEquals("not cancelled", false, args.Cancel);

			args = new ShipmentTypeChangingCancelEventArgs(ZString.Empty, Core.Constants.ShipmentTypes.StandardHouse);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			AssertHandleShipmentTypeChanging("changing to STD shipment", args,
@"Changing this shipment to be a Standard shipment will remove all related shipments from it.
Related shipments can only exist on a Buyers Consol Lead, Assembly Master or Co-Load Master shipment.

Are you sure you want to continue?");
			AssertEquals("cancelled", true, args.Cancel);
		}

		public void TestHandleShipmentTypeChangingHLV_HLS()
		{
			foreach (var shipmentType in new[] { Core.Constants.ShipmentTypes.HighVolumeLowValue, Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy })
			{
				var args = new ShipmentTypeChangingCancelEventArgs(ZString.Empty, shipmentType);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				AssertHandleShipmentTypeChanging(string.Format("changing to {0} shipment", shipmentType), args,
@"Changing this shipment to be a High Volume Low Value shipment will remove Master / Lead shipment and all related shipments.
Related shipments can only exist on a Buyers Consol Lead, Assembly Master or Co-Load Master shipment.

Are you sure you want to continue?");
				AssertEquals("not cancelled", false, args.Cancel);

				args = new ShipmentTypeChangingCancelEventArgs(ZString.Empty, shipmentType);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				AssertHandleShipmentTypeChanging(string.Format("changing to {0} shipment", shipmentType), args,
@"Changing this shipment to be a High Volume Low Value shipment will remove Master / Lead shipment and all related shipments.
Related shipments can only exist on a Buyers Consol Lead, Assembly Master or Co-Load Master shipment.

Are you sure you want to continue?");
				AssertEquals("cancelled", true, args.Cancel);
			}
		}

		public void TestHandleShipmentTypeChangingHLV_HLS_RemovingSubsIsNotAllowed()
		{
			foreach (var shipmentType in new[] { Core.Constants.ShipmentTypes.HighVolumeLowValue, Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy })
			{
				Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = false;

				var args = new ShipmentTypeChangingCancelEventArgs(ZString.Empty, shipmentType);

				var shipment = Factory.NewWithValidTestData<CommonShipment>();
				AssertHandleShipmentTypeChanging("expected no security messages as shipment is not saved and has no master", shipment, args,
@"Changing this shipment to be a High Volume Low Value shipment will remove Master / Lead shipment and all related shipments.
Related shipments can only exist on a Buyers Consol Lead, Assembly Master or Co-Load Master shipment.

Are you sure you want to continue?");

				var master = Factory.NewWithValidTestData<CommonShipment>();
				master.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
				shipment.JS_JS_ColoadMasterShipment = master.PK;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertHandleShipmentTypeChanging("expected no security messages as shipment is not saved", shipment, args,
@"Changing this shipment to be a High Volume Low Value shipment will remove Master / Lead shipment and all related shipments.
Related shipments can only exist on a Buyers Consol Lead, Assembly Master or Co-Load Master shipment.

Are you sure you want to continue?");

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertHandleShipmentTypeChanging("expected security message", shipment, args,
@"Changing this shipment to be a High Volume Low Value shipment needs to remove master but you lack security permissions to 'Operate -> Forwarding -> Shipments -> Edit -> Allow Detaching Sub-Shipments from Master Shipment'");

				shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertHandleShipmentTypeChanging("expected no security messages as shipment has no master", shipment, args,
@"Changing this shipment to be a High Volume Low Value shipment will remove Master / Lead shipment and all related shipments.
Related shipments can only exist on a Buyers Consol Lead, Assembly Master or Co-Load Master shipment.

Are you sure you want to continue?");

				shipment.JS_JS_ColoadMasterShipment = master.PK;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertHandleShipmentTypeChanging("expected no security messages as shipment has no master saved in database", shipment, args,
@"Changing this shipment to be a High Volume Low Value shipment will remove Master / Lead shipment and all related shipments.
Related shipments can only exist on a Buyers Consol Lead, Assembly Master or Co-Load Master shipment.

Are you sure you want to continue?");
			}
		}

		void AssertHandleShipmentTypeChanging(string errorMessage, ShipmentTypeChangingCancelEventArgs args, string expectedMesssage)
		{
			AssertHandleShipmentTypeChanging(errorMessage, Factory.New<CommonShipment>(), args, expectedMesssage);
		}

		void AssertHandleShipmentTypeChanging(string errorMessage, CommonShipment shipment, ShipmentTypeChangingCancelEventArgs args, string expectedMesssage)
		{
			ShipmentTypeChangingHandler.Handle(shipment, args);
			AssertMultilineASCIIEquals(errorMessage, expectedMesssage, UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
