using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVBookingHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckReferencesAreUnique_WhenABookingHeaderIsDeleted_ThenSkipCheckReferencesAreUnique()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader.Consignments.AddNew();
			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ShipperReference = "duplicate_reference";
			consignment2.HVC_ShipperReference = "duplicate_reference";
			bookingHeader.Delete();

			bookingHeader.RunPreSaveValidation();
			bookingHeader.RunPreSaveValidation();

			AssertNoErrors(consignment1.HVC_ShipperReferenceInfo);
			AssertNoErrors(consignment2.HVC_ShipperReferenceInfo);
		}

		public void TestCheckIsBookingReceived_WhenNotIsBookingConfirmed_AddsError()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			AssertEquals("precondition", false, bookingHeader.HVH_IsBookingConfirmed);

			bookingHeader.HVH_IsBookingReceived = true;
			AssertHasError("IsBookingReceived has error", bookingHeader.HVH_IsBookingReceivedInfo, "Please confirm the Booking Header before receival.");

			bookingHeader.HVH_IsBookingConfirmed = true;
			bookingHeader.HVH_IsBookingReceived = true;
			AssertNoErrors(bookingHeader.HVH_IsBookingReceivedInfo);

			bookingHeader.HVH_IsBookingReceived = false;
			AssertNoErrors(bookingHeader.HVH_IsBookingReceivedInfo);
		}

		public void TestValidate_Error_DeactivateBookingHeaderProcessedAtOriginDepot()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			header.HVH_IsProcessedAtOriginDepot = true;
			header.HVH_IsActive = true;

			AssertNoErrors("Expected no error when header checks if processed at Origin Depot", header.HVH_IsActiveInfo);

			header.HVH_IsActive = false;

			AssertHasErrors("Expected error when header checks if processed at Origin Depot", header.HVH_IsActiveInfo);
		}

		public void TestValidate_Error_DeactivateBookingHeaderWithActiveConsignments()
		{
			var header = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_IsActive = true;
			header.HVH_IsActive = true;

			AssertNoErrors("Expected no error when header checks consignments is active", header.HVH_IsActiveInfo);

			header.HVH_IsActive = false;

			AssertHasErrors("Expected error when header checks consignments is active", header.HVH_IsActiveInfo);
		}

		public void TestValidate_WeightUQ()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			bookingHeader.HVH_GrossWeightUQ = ZString.Empty;
			AssertHasError(bookingHeader.HVH_GrossWeightUQInfo, "Please enter a Gross Weight Unit.");

			bookingHeader.HVH_GrossWeightUQ = "ZZ";
			AssertHasError(bookingHeader.HVH_GrossWeightUQInfo, "Enter a valid Gross Weight Unit.");

			bookingHeader.HVH_GrossWeightUQ = "KG";
			AssertNoErrors(bookingHeader.HVH_GrossWeightUQInfo);
		}

		public void TestValidate_VolumeUQ()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			bookingHeader.HVH_GrossVolumeUQ = ZString.Empty;
			AssertHasError(bookingHeader.HVH_GrossVolumeUQInfo, "Please enter a Gross Volume Unit.");

			bookingHeader.HVH_GrossVolumeUQ = "ZZ";
			AssertHasError(bookingHeader.HVH_GrossVolumeUQInfo, "Enter a valid Gross Volume Unit.");

			bookingHeader.HVH_GrossVolumeUQ = "M3";
			AssertNoErrors(bookingHeader.HVH_GrossVolumeUQInfo);
		}
	}
}
