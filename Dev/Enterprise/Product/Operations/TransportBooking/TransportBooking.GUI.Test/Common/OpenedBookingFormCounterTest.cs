using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.GUI.Testing
{
	class OpenedBookingFormCounterTest : DtbBookingTestCaseWithFactory
	{
		public void TestGetExtraNotificationWhenBookingOpen()
		{
			var booking = Helper.CreateBooking();
			var openedBookingFormCounter = ObjectFactory.Get<IOpenedBookingFormCounter>();

			Factory.Save();

			using (var form = new TransportBookingForm(booking))
			{
				OpenedFormCache.GetInstance().Add(booking.PK.ToGuid(), form, nameof(ControllerIDs.DtbBooking));
				form.Show();
				var collection = GetCollectionToTest();
				var notificationProvider = collection as IFilterModuleExtraNotificationProvider;

				AssertEquals("Should be one open form", 1, openedBookingFormCounter.AlreadyOpenedBookingFormCount(booking.ConsolidationSingleJob));
				AssertEquals("Should have extra notification when form is open.", "Cannot be attached to the Master Booking as the job is currently open.", notificationProvider.GetExtraNotification(booking).Message);
			}
		}

		public void TestGetExtraNotificationWhenBookingNotOpen()
		{
			var booking = Helper.CreateBooking();
			var openedBookingFormCounter = ObjectFactory.Get<IOpenedBookingFormCounter>();

			Factory.Save();

			var collection = GetCollectionToTest();
			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;

			AssertEquals("Should be no open forms", 0, openedBookingFormCounter.AlreadyOpenedBookingFormCount(booking.ConsolidationSingleJob));
			AssertEquals("Notification should be null when form is not open.", null, notificationProvider.GetExtraNotification(booking));
		}

		public void TestAlreadyOpenedBookingFormCount()
		{
			var booking = Helper.CreateBooking();
			var differentBooking = Helper.CreateBooking();
			Factory.Save();

			var openedBookingFormCounter = ObjectFactory.Get<IOpenedBookingFormCounter>();
			var openedFormCount = 0;

			openedFormCount = openedBookingFormCounter.AlreadyOpenedBookingFormCount(booking.ConsolidationSingleJob);
			AssertEquals("Should be no open forms", 0, openedFormCount);

			using (var form = new TransportBookingForm(booking))
			{
				OpenedFormCache.GetInstance().Add(booking.PK.ToGuid(), form, nameof(ControllerIDs.DtbBooking));
				form.Show();
				openedFormCount = openedBookingFormCounter.AlreadyOpenedBookingFormCount(booking.ConsolidationSingleJob);
				AssertEquals("Should be one open form", 1, openedFormCount);
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}

			openedFormCount = openedBookingFormCounter.AlreadyOpenedBookingFormCount(booking.ConsolidationSingleJob);
			AssertEquals("Should be no open forms", 0, openedFormCount);

			using (var form = new TransportBookingForm(differentBooking))
			{
				OpenedFormCache.GetInstance().Add(differentBooking.PK.ToGuid(), form, nameof(ControllerIDs.DtbBooking));
				form.Show();
				openedFormCount = openedBookingFormCounter.AlreadyOpenedBookingFormCount(booking.ConsolidationSingleJob);
				AssertEquals("Expected form is not open", 0, openedFormCount);
				OpenedFormCache.GetInstance().CloseAllCachedForms();
			}

			openedFormCount = openedBookingFormCounter.AlreadyOpenedBookingFormCount(booking.ConsolidationSingleJob);
			AssertEquals("Should be no open forms", 0, openedFormCount);
		}

		DtbBookingCollectionForSubBookingsFindBox GetCollectionToTest()
		{
			return new DtbBookingCollectionForSubBookingsFindBox(Factory, null, false);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
		}
	}
}
