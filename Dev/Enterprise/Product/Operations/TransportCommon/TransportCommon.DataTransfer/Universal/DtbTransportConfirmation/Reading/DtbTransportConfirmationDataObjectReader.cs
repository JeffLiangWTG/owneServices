using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.TransportCommon.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.DataTransfer.Universal
{
	public abstract class DtbTransportConfirmationDataObjectReader<T> : DataObjectReader<Confirmation, T>
		where T : DtbTransportConfirmation
	{
		protected DtbTransportConfirmationDataObjectReader(Confirmation confirmationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer parent)
			: base(confirmationDataObject, logger, factory)
		{
			Parent = Argument.NotNull(parent, "IColumnIndexer parent");
		}

		readonly IColumnIndexer Parent;

		protected override T GetExistingBusinessObject()
		{
			return null;
		}

		protected override void PopulateBusinessObject(T confirmation)
		{
			var row = GetColumnIndexerFromRow(confirmation);
			SetValue(row, DtbBookingConfirmationSchema.KK_Actual, dataObject.ActualDate);
			SetValue(row, DtbBookingConfirmationSchema.KK_ConfirmationType, dataObject.DateDescription);
			SetValue(row, DtbBookingConfirmationSchema.KK_Estimated, dataObject.EstimatedDate);
			SetValue(row, DtbBookingConfirmationSchema.KK_IsEmptyContainer, dataObject.IsEmptyContainer);
			SetValue(row, DtbBookingConfirmationSchema.KK_SlotDateTime, dataObject.SlotDate);
			SetValue(row, DtbBookingConfirmationSchema.KK_SlotReference, dataObject.SlotReference);

			// Instruction Confirmations have all packages assigned, it is incorrect to set the quantity
			if (Parent.TableName == DtbBookingInstructionPkgDivotSchema.Constants.TableName)
			{
				SetValue(row, DtbBookingConfirmationSchema.KK_Quantity, dataObject.Quantity);
			}

			// utc time will be set when KK_KN_Instruction is set
			SetValue(row, DtbBookingConfirmationSchema.KK_RequiredFrom, dataObject.RequiredFromDate);
			SetValue(row, DtbBookingConfirmationSchema.KK_RequiredTo, dataObject.RequiredToDate);

			SetValue(row, DtbBookingConfirmationSchema.KK_ReceivedBy, dataObject.ReceivedBy);
			SetValue(row, DtbBookingConfirmationSchema.KK_ReferenceNum, dataObject.Reference);
		}
	}
}
