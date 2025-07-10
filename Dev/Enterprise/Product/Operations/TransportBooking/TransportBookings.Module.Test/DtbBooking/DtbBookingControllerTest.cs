using System;
using CargoWise.Common;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingController))]
	class DtbBookingControllerTest : DtbBookingControllerSharedTest<DtbBookingController>
	{
		public void TestMakeNewBookings()
		{
			var bookingController = new DtbBookingController();
			using (var form = (TransportBookingForm)bookingController.ShowNewForm())
			{
				CombineAssertions(() =>
				{
					AssertEquals("New booking should have status of available", TransportStatuses.Codes.Available, ((DtbBooking)form.BusinessEntityForPersistingForm).KM_Status);
					AssertEquals("New booking should have master flag turned off", false, ((DtbBooking)form.BusinessEntityForPersistingForm).KM_IsMaster);
				});
			}
		}

		public void TestMakeNewBookingsAsQuotes()
		{
			var bookingController = new DtbBookingController();
			bookingController.MakeNewBookingsAsQuotes();
			using (var form = (TransportBookingForm)bookingController.ShowNewForm())
			{
				CombineAssertions(() =>
				{
					AssertEquals("New booking should have status of quote", TransportStatuses.Codes.Quote, ((DtbBooking)form.BusinessEntityForPersistingForm).KM_Status);
					AssertEquals("New quote should have master flag turned off", false, ((DtbBooking)form.BusinessEntityForPersistingForm).KM_IsMaster);
				});
			}
		}

		public void TestMakeNewBookingsAsPickupMasterBookingsWhenMasterBookingsEnabled()
		{
			TestMakeNewBookingsWhenMasterBookingsEnabledCore("PIC");
		}

		public void TestMakeNewBookingsAsDeliveryMasterBookingsWhenMasterBookingsEnabled()
		{
			TestMakeNewBookingsWhenMasterBookingsEnabledCore("DLV");
		}

		void TestMakeNewBookingsWhenMasterBookingsEnabledCore(string jobDirection)
		{
			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bookingController = new DtbBookingController();

				bookingController.MakeNewBookingsAsMasterBookings(jobDirection);

				using (var form = (TransportBookingForm)bookingController.ShowNewForm())
				{
					CombineAssertions(() =>
					{
						AssertEquals("New master booking should have status of available", TransportStatuses.Codes.Available, ((DtbBooking)form.BusinessEntityForPersistingForm).KM_Status);
						AssertEquals("New master booking should have master flag turned on", true, ((DtbBooking)form.BusinessEntityForPersistingForm).KM_IsMaster);
						AssertEquals("New master booking should have correct direction", jobDirection, ((DtbBooking)form.BusinessEntityForPersistingForm).ConsolidationSingleJob.KB_JobDirection);
					});
				}
			}
		}

		public void TestMakeNewBookingsAsPickupMasterBookingsReportsErrorIfMasterBookingsNotEnabled()
		{
			TestMakeNewBookingsReportsErrorIfMasterBookingsNotEnabledCore("PIC");
		}

		public void TestMakeNewBookingsAsDeliveryMasterBookingsReportsErrorIfMasterBookingsNotEnabled()
		{
			TestMakeNewBookingsReportsErrorIfMasterBookingsNotEnabledCore("DLV");
		}

		void TestMakeNewBookingsReportsErrorIfMasterBookingsNotEnabledCore(string jobDirection)
		{
			ErrorReporter.Clear();
			using (TransportRegistry.Instance.MasterBookingsEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var bookingController = new DtbBookingController();

				bookingController.MakeNewBookingsAsMasterBookings(jobDirection);

				AssertEquals("Should report error if try to create new master booking when registry setting Master Bookings Enabled not turned on", "Master Bookings not enabled in registry, should not be creating Master Bookings", ErrorReporter.LastMessageReported);
			}
			ErrorReporter.Clear();
		}

		public void TestShowEditFormForSingleBooking()
		{
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			var booking = Helper.CreateBooking();
			Factory.Save();

			var controller = new DtbBookingController();
			using (var singleBookingForm = controller.ShowEditForm(booking))
			{
				var openedForm = controller.ShowEditFormForSingleBooking(booking);
				AssertEquals(typeof(TransportBookingForm), openedForm.GetType());
				AssertEquals(singleBookingForm, openedForm);

				openedForm.Dispose();
			}

			var multiBookingController = new DtbBookingConsolidationController();
			using (var multiBookingform = multiBookingController.ShowEditForm(booking.ConsolidationSingleJob))
			{
				var openedForm = controller.ShowEditFormForSingleBooking(booking);
				AssertEquals(typeof(TransportBookingForm), openedForm.GetType());
				AssertNotEquals(multiBookingform, openedForm);

				openedForm.Dispose();
			}
		}

		protected override ControllerID GetControllerID() => ControllerIDs.DtbBooking;
	}
}
