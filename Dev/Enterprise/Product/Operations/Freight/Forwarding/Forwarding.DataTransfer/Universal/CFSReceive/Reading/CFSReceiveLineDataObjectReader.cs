using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class CFSReceiveLineDataObjectReader : DataObjectReader<PackingLine, JobSupplierBookingLine>
	{
		readonly JobSupplierBooking booking;

		public CFSReceiveLineDataObjectReader(PackingLine dataObject, JobSupplierBooking booking, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			this.booking = booking;
		}

		protected override JobSupplierBookingLine GetExistingBusinessObject()
		{
			var targetBO = booking.SupplierBookingLines.FirstOrDefault(x => x.JSL_BookingLineId == dataObject.PackingLineID.Value)
				?? throw new DataObjectReadFailureException(Res.GetString("46f09b78-d921-458b-a975-d81b5ea72f9b", "Supplier Booking Line {0} cannot be found in the system.", dataObject.PackingLineID));
			if ((dataObject.InnerQty < 0) != (dataObject.PackQty < 0))
			{
				throw new DataObjectReadFailureException(Res.GetString("098b96b0-b982-48fa-b02f-22680b69dc52", "Both Received Quantity and Received Packs should either be positive or negative value."));
			}
			return targetBO;
		}

		protected override void PopulateBusinessObject(JobSupplierBookingLine targetBO)
		{
			if (dataObject.LastCFSReceiptDate == null || dataObject.LastCFSReceiptDate == ZDateTime.Empty)
			{
				throw new DataObjectReadFailureException(Res.GetString("e68202d0-2b1d-467c-87f2-34608c61bd8e", "Last CFS Receipt Date cannot be empty."));
			}

			var totalReceivedQuantity = targetBO.JSL_ReceivedQuantity + dataObject.InnerQty;
			var totalReceivedPackages = targetBO.JSL_ReceivedPackages + dataObject.PackQty;
			var totalReceivedWeight = dataObject.InnerQty < 0 && dataObject.Weight > 0 ? targetBO.JSL_ReceivedWeight - dataObject.Weight : targetBO.JSL_ReceivedWeight + dataObject.Weight;
			var totalReceivedVolume = dataObject.InnerQty < 0 && dataObject.Volume > 0 ? targetBO.JSL_ReceivedVolume - dataObject.Volume : targetBO.JSL_ReceivedVolume + dataObject.Volume;

			if (totalReceivedQuantity < 0 || totalReceivedPackages < 0 || totalReceivedWeight < 0 || totalReceivedVolume < 0)
			{
				throw new DataObjectReadFailureException(Res.GetString("5ed7fe9d-e9c7-4345-8238-8d94f4588f71", "Total Received Qty, Total Received Package, Total Received Volume, or Total Received Weight would calculate to a negative value. Please adjust Received Qty, Received Package, Received Volume or Received Weight."));
			}

			if (totalReceivedQuantity > targetBO.JSL_BookedQuantity)
			{
				throw new DataObjectReadFailureException(Res.GetString("1b30c9b7-4411-430f-994a-83686db26286", "Total Received Quantity cannot be greater than the Booked Quantity."));
			}
			else if (totalReceivedQuantity < targetBO.JSL_BookedQuantity)
			{
				logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("93022f23-5011-4a33-84f1-7375ad4570e2", "Booked Quantity of {0} is not fully received.", targetBO.JSL_BookedQuantity));
			}

			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_ReceivedQuantity, totalReceivedQuantity);
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_ReceivedPackages, totalReceivedPackages);
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_ReceivedWeight, totalReceivedWeight);
			SetValue(targetBO, JobSupplierBookingLineSchema.JSL_ReceivedVolume, totalReceivedVolume);

			var receiptTime = (ZDateTime)dataObject.LastCFSReceiptDate;
			var cfsAddress = targetBO.SupplierBooking.CFSAddress;
			receiptTime = receiptTime.ToDateTimeOffset(cfsAddress?.EffectiveRelatedPortCode).ToUtcZDateTime();

			if (targetBO.JSL_FirstReceiptDateUtc == ZDateTime.Empty)
			{
				SetValue(targetBO, JobSupplierBookingLineSchema.JSL_FirstReceiptDateUtc, receiptTime);
				SetValue(targetBO, JobSupplierBookingLineSchema.JSL_LastReceiptDateUtc, receiptTime);
			}
			else if (targetBO.JSL_FirstReceiptDateUtc > receiptTime)
			{
				SetValue(targetBO, JobSupplierBookingLineSchema.JSL_FirstReceiptDateUtc, receiptTime);
			}
			else if (targetBO.JSL_LastReceiptDateUtc < receiptTime)
			{
				SetValue(targetBO, JobSupplierBookingLineSchema.JSL_LastReceiptDateUtc, receiptTime);
			}
		}
	}
}
