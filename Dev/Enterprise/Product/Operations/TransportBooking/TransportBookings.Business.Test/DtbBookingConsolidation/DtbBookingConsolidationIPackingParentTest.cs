using System;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbBookingConsolidation))]
	public class DtbBookingConsolidationIPackingParentTest : PackingParentTestCase<DtbBookingConsolidation>
	{
		protected override DtbBookingConsolidation GetNewParent()
		{
			return Factory.New<DtbBookingConsolidation>();
		}

		public void TestCanAutoApplyChanges_PackingParentWithoutPackableItems()
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(GetNewParent());
			var pallet1 = packageJob.Packages.AddNew("PLT");
			var packagesToRemove = new[] { pallet1 };
			var bizO = new UnpackItemsBusinessObject(packageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), packagesToRemove);
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(true, bizO.CanAutoApplyChanges);
			});
		}

		public void TestIPackingParentSupportsImportingBookedDimensions()
		{
			var dtbBookingConsolidation = Factory.New<DtbBookingConsolidation>();
			AssertNotNull("Dtb Consignment Consolidation support importing Booked Dimensions.", dtbBookingConsolidation);
		}

		public override void TestNotificationTypeForInvalidContainerNumber()
		{
			var consolidation = TransportBookingHelper.CreateConsolidation();
			var booking = TransportBookingHelper.CreateBooking(consolidation);

			AssertEquals("Precondition: IsSendingXUSToCTO is false on the booking", false, booking.IsSendingXUSToCTO);
			AssertEquals("NotificationTypeForInvalidContainerNumber should be warning when not sending XUS To CTO", NotificationTypes.Warning, ((IPackingParent)consolidation).NotificationTypeForInvalidContainerNumber);

			booking.KM_Status = TransportStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: IsSendingXUSToCTO is true on the booking", true, booking.IsSendingXUSToCTO);
			AssertEquals("NotificationTypeForInvalidContainerNumber should be message error when sending XUS To CTO", NotificationTypes.MessageError, ((IPackingParent)consolidation).NotificationTypeForInvalidContainerNumber);
		}

		TransportBookingTestHelper TransportBookingHelper
		{
			get { return transportBookingHelper ?? (transportBookingHelper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper transportBookingHelper;
	}
}
