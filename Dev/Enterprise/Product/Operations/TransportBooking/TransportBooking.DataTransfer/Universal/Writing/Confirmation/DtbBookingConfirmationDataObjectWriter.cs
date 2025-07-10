using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public class DtbBookingConfirmationDataObjectWriter : DataObjectWriter<DtbBookingConfirmation, Confirmation>
	{
		public DtbBookingConfirmationDataObjectWriter(IDataWritingManager manager, DtbBookingInstructionPkgDivot divot = null)
			: base(manager)
		{
			Divot = divot;
		}
		readonly DtbBookingInstructionPkgDivot Divot;

		protected override Confirmation PopulateDataObject(DtbBookingConfirmation bookingConfirmation)
		{
			var bookingConfirmationDataObject = new Confirmation();

			bookingConfirmationDataObject.DateDescription = bookingConfirmation.KK_ConfirmationType;
			bookingConfirmationDataObject.ActualDate = bookingConfirmation.KK_Actual;
			bookingConfirmationDataObject.EstimatedDate = bookingConfirmation.KK_Estimated;
			bookingConfirmationDataObject.RequiredFromDate = bookingConfirmation.KK_RequiredFrom;
			bookingConfirmationDataObject.RequiredToDate = bookingConfirmation.KK_RequiredTo;
			bookingConfirmationDataObject.SlotDate = bookingConfirmation.KK_SlotDateTime;
			bookingConfirmationDataObject.SlotReference = bookingConfirmation.KK_SlotReference;

			// if confirmation has no divot, override its quanity with for each divot (passed in)
			if (bookingConfirmation.PackageDivot == null && Divot != null)
			{
				bookingConfirmationDataObject.Quantity = Divot.KD_Quantity;
			}
			else
			{
				bookingConfirmationDataObject.Quantity = bookingConfirmation.KK_Quantity;
			}

			bookingConfirmationDataObject.ReceivedBy = bookingConfirmation.KK_ReceivedBy;
			bookingConfirmationDataObject.Reference = bookingConfirmation.KK_ReferenceNum;
			bookingConfirmationDataObject.IsEmptyContainer = bookingConfirmation.KK_IsEmptyContainer;
			//bookingConfirmationDataObject.Signature = bookingConfirmation.;???

			bookingConfirmationDataObject.VehicleRegistration = bookingConfirmation.KK_VehicleRegistration;
			bookingConfirmationDataObject.DriverDocumentID = bookingConfirmation.KK_DocumentID;
			if (bookingConfirmation.KK_OC_Driver != ZGuid.Empty)
			{
				bookingConfirmationDataObject.Driver = new OrganizationContactDataObjectWriter(writeManager).GetDataObject(bookingConfirmation.Driver);
			}

			return bookingConfirmationDataObject;
		}
	}
}
