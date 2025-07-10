using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(BookingController))]
	internal class BookingControllerTest : AgencyShipmentControllerTest
	{
		public void TestModuleID()
		{
			BookingController controller = (BookingController)(ZControllerFactory.Create(ControllerIDs.AgencyBooking));
			AssertEquals(ModuleIDs.AgencyBooking, controller.ModuleID);
		}

		public void TestErrorReporterForBillOfLadingLoadedWithBooking_Loading()
		{
			IZForm testForm = null;
			try
			{
				var booking = (AgencyBooking)GetBusinessObjectThatIsInTheDatabase();
				testForm = Controller.ShowEditForm(booking);
				var billOfLading = Controller.Factory.Load<BillOfLading>(booking.PK);
				AssertEquals("BillOfLadingFromBookingContext", ErrorReporter.LastKeyReported);
				AssertContains("ErrorReporter Message", $@"BillOfLading was used from the Booking form.
Factory={Controller.Factory._Instance},{Controller.Factory.NameForDebugging},{Controller.Factory.RefreshEnabled}
Number={billOfLading.JS_UniqueConsignRef}
PK={billOfLading.PK}
Booking JS_ShipmentStatus={booking.JS_ShipmentStatus}
BillOfLading JS_ShipmentStatus={billOfLading.JS_ShipmentStatus}", ErrorReporter.LastMessageReported);
				AssertContains("we have booking creation stack trace", nameof(AgencyBooking.SetCurrentBookingInfo), ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
				if (testForm is ZForm zForm)
				{
					zForm.Close();
				}
			}
		}

		public void TestErrorReporterForBillOfLadingLoadedWithBooking_New()
		{
			IZForm testForm = null;
			try
			{
				testForm = Controller.ShowNewForm();
				AssertEquals("", ErrorReporter.LastKeyReported);
				ZForm testZForm = testForm as ZForm;
				Controller.Factory.Save();
				AssertEquals("", ErrorReporter.LastKeyReported);
				var booking = testZForm.BusinessEntity as AgencyBooking;
				var billOfLading = Controller.Factory.Load<BillOfLading>(testZForm.BusinessEntity.Identifier);
				AssertEquals("BillOfLadingFromBookingContext", ErrorReporter.LastKeyReported);
				AssertContains("ErrorReporter Message", $@"BillOfLading was used from the Booking form.
Factory={Controller.Factory._Instance},{Controller.Factory.NameForDebugging},{Controller.Factory.RefreshEnabled}
Number={billOfLading.JS_UniqueConsignRef}
PK={billOfLading.PK}
Booking JS_ShipmentStatus={booking.JS_ShipmentStatus}
BillOfLading JS_ShipmentStatus={billOfLading.JS_ShipmentStatus}", ErrorReporter.LastMessageReported);
				AssertContains("we have booking creation stack trace", nameof(AgencyBooking.SetCurrentBookingInfo), ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
				if (testForm is ZForm zForm)
				{
					zForm.Close();
				}
			}
		}

		public void TestMessageOnBookingModuleWhenBillOfLadingFormIsAccessed()
		{
			IZForm testForm = null;
			try
			{
				var booking = (AgencyBooking)GetBusinessObjectThatIsInTheDatabase();
				var message = "This Booking has been confirmed or is in the process of being converted to a Bill of Lading and is not currently accessible from the Bookings module.";
				booking.Confirm();
				Factory.Save();
				testForm = Controller.ShowDeleteForm(booking);
				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				testForm = Controller.ShowEditForm(booking);
				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				testForm = Controller.ShowViewForm(booking);
				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				testForm = Controller.ShowTemplateCopyForm(booking);
				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
			finally
			{
				ZForm testZForm = testForm as ZForm;
				if (testZForm != null)
				{
					testZForm.Close();
				}
			}
		}

		public void TestBookingFormCannotBeOpenedWhenBOLFormAlreadyOpen()
		{
			var booking = (AgencyBooking)GetBusinessObjectThatIsInTheDatabase();
			using var form = new ZForm();
			OpenedFormCache.GetInstance().Add(booking.PK.ToGuid(), form, ControllerIDs.AgencyBillOfLading.ToString());

			_ = Controller.ShowEditForm(booking);
			var message = "This Booking has been confirmed or is in the process of being converted to a Bill of Lading and is not currently accessible from the Bookings module.";
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestErrorReporterForBillOfLadingProcessTaskLoadedWithBooking()
		{
			IZForm testForm = null;
			try
			{
				var booking = (AgencyBooking)GetBusinessObjectThatIsInTheDatabase();
				var bookingProcessTask = booking.WorkflowItems.Milestones.AddNew();
				Factory.Save();
				testForm = Controller.ShowEditForm(booking);
				AssertEquals("", ErrorReporter.LastKeyReported);
				var billOfLadingProcessTask = Controller.Factory.Load<BillOfLadingProcessTask>(bookingProcessTask.PK);
				AssertEquals("BillOfLadingFromBookingContext", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			}
			finally
			{
				ErrorReporter.Clear();
				if (testForm is ZForm zForm)
				{
					zForm.Close();
				}
			}
		}

		public void TestCRMSecurityCheckpoints()
		{
			var bizObj = Factory.NewWithValidTestData<AgencyBooking>();
			CRMSecurityProviderTest<AgencyBooking>.AssertController(new BookingController(), bizObj, Env.Security.AgencyBookingCRMSecurity);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AgencyBooking;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Test Org 1";
			org1.MainAddress.OA_Address1 = "Test Address 1";
			org1.OH_IsConsignor = ZBool.True;
			org1.OH_RL_NKClosestPort = "AUSYD";
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.JS_JX = (new SailingsForTestClasses(Factory)).SydLaxSailing.PK;
			shipment.JS_ActualVolume = new ZDecimal(4.00);
			shipment.JS_ActualWeight = new ZDecimal(300.00);
			shipment.JS_OuterPacks = new ZInt(6);
			shipment.JS_GoodsDescription = new ZString("SDFSDFSDF");
			shipment.ConsignorPK = org1.PK;
			Factory.Save();
			return shipment;
		}
	}
}
