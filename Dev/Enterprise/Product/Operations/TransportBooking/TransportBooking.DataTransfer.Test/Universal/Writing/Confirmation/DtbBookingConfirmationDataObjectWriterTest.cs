using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	public class DtbBookingConfirmationDataObjectWriterTest : DtbBookingTestCaseWithFactory
	{
		public void TestGetDataObject()
		{
			var year = ZDateTime.Now.Year;

			var date = new ZDateTime(year, 9, 27);

			var divot = Factory.New<DtbBookingInstructionPkgDivot>();
			divot.KD_Quantity = 5;

			var org = Helper.CreateOrganisation("Org");
			org.OH_RL_NKClosestPort = "AUBNE";

			var driver = Factory.NewWithValidTestData<OrgContact>();
			driver.OC_OH = org.PK;
			driver.OC_ContactName = "Hugh Driver";

			var instruction = Helper.CreateInstruction(InstructionTypes.Codes.PickUp);
			instruction.Address.E2_OA_Address = org.MainAddress.PK;

			var bookingConfirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			bookingConfirmation.KK_Actual = date;
			bookingConfirmation.KK_ConfirmationType = "CUS";
			bookingConfirmation.KK_Estimated = date.AddDays(1);
			bookingConfirmation.KK_ReceivedBy = "MAN123";
			bookingConfirmation.KK_ReferenceNum = "REF123";
			bookingConfirmation.KK_RequiredFrom = date.AddDays(2);
			bookingConfirmation.KK_RequiredTo = date.AddDays(3);
			bookingConfirmation.KK_SlotDateTime = date.AddDays(4);
			bookingConfirmation.KK_SlotReference = "REF123";
			bookingConfirmation.KK_VehicleRegistration = "VEH123";
			bookingConfirmation.KK_DocumentID = "12345678";
			bookingConfirmation.KK_OC_Driver = driver.PK;

			bookingConfirmation.KK_IsEmptyContainer = true;
			// #warning: what of signature?

			var confirmationDataObject = new DtbBookingConfirmationDataObjectWriter(new DataWritingManager(new DummyActionInfo()), divot).GetDataObject(bookingConfirmation);

			CombineAssertions("Check that all expected DtbBookingConfirmation fields were read into the data object", () =>
			{
				AssertEquals("bookingConfirmation.KK_Actual", date, confirmationDataObject.ActualDate);
				AssertEquals("bookingConfirmation.KK_ConfirmationType", "CUS", confirmationDataObject.DateDescription);
				AssertEquals("bookingConfirmation.KK_Estimated", date.AddDays(1), confirmationDataObject.EstimatedDate);
				AssertEquals("bookingConfirmation.KK_Quantity", 5, confirmationDataObject.Quantity);
				AssertEquals("bookingConfirmation.KK_ReceivedBy", "MAN123", confirmationDataObject.ReceivedBy);
				AssertEquals("bookingConfirmation.KK_ReferenceNum", "REF123", confirmationDataObject.Reference);
				AssertEquals("bookingConfirmation.KK_RequiredFrom", date.AddDays(2), confirmationDataObject.RequiredFromDate);
				AssertEquals("bookingConfirmation.KK_RequiredTo", date.AddDays(3), confirmationDataObject.RequiredToDate);
				AssertEquals("bookingConfirmation.KK_SlotDateTime", date.AddDays(4), confirmationDataObject.SlotDate);
				AssertEquals("bookingConfirmation.KK_SlotReference", "REF123", confirmationDataObject.SlotReference);
				AssertEquals("bookingConfirmation.KK_VehicleRegistration", "VEH123", confirmationDataObject.VehicleRegistration);
				AssertEquals("bookingConfirmation.KK_DocumentID", "12345678", confirmationDataObject.DriverDocumentID);
				AssertEquals("bookingConfirmation.KK_Driver", "Hugh Driver", confirmationDataObject.Driver?.FullName);

				AssertEquals("bookingConfirmation.KK_IsEmptyContainer", true, confirmationDataObject.IsEmptyContainer);
				// #warning: add assertion of signature.
			});

			bookingConfirmation.KK_OC_Driver = ZGuid.Empty;

			var confirmationDataObjectWithoutDriver = new DtbBookingConfirmationDataObjectWriter(new DataWritingManager(new DummyActionInfo()), divot).GetDataObject(bookingConfirmation);

			AssertEquals("DataObject.Driver is empty when bookingConfirmation.KK_OC_Driver is empty", null, confirmationDataObjectWithoutDriver.Driver);
		}
	}
}
