using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(BillOfLadingController))]
	class BillOfLadingBOControllerTest : AgencyShipmentControllerTest
	{
		public void TestBOLFormCannotBeOpenedWhenBookingFormAlreadyOpen()
		{
			var billOfLading = (BillOfLading)GetBusinessObjectThatIsInTheDatabase();
			using var form = new ZForm();
			OpenedFormCache.GetInstance().Add(billOfLading.PK.ToGuid(), form, ControllerIDs.AgencyBooking.ToString());

			_ = Controller.ShowEditForm(billOfLading);
			var message = "This is a Booking that has not been confirmed to Bill of Lading and is not accessible from the Bill of Lading module. Please access from the Booking module.";
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestBOLFormCannotBeOpenedForShipmentThatIsNotBOL()
		{
			var billOfLading = (BillOfLading)GetBusinessObjectThatIsInTheDatabase();
			billOfLading.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			Factory.Save();

			_ = Controller.ShowEditForm(billOfLading);
			var message = "This is a Booking that has not been confirmed to Bill of Lading and is not accessible from the Bill of Lading module. Please access from the Booking module.";
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AgencyBillOfLading;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Test Org 1";
			org1.MainAddress.OA_Address1 = "Test Address 1";
			org1.OH_IsConsignor = ZBool.True;
			org1.OH_RL_NKClosestPort = "AUSYD";
			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.JS_JX = (new SailingsForTestClasses(Factory)).SydLaxSailing.PK;
			shipment.JS_ActualVolume = new ZDecimal(4.00);
			shipment.JS_ActualWeight = new ZDecimal(300.00);
			shipment.JS_OuterPacks = new ZInt(6);
			shipment.JS_GoodsDescription = new ZString("SDFSDFSDF");
			shipment.ConsignorPK = org1.PK;
			Factory.Save();
			return shipment;
		}

		#endregion
	}
}
