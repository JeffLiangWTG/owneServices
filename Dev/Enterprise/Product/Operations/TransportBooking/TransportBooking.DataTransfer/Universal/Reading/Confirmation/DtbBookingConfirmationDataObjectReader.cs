using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	class DtbBookingConfirmationDataObjectReader : DataObjectReader<Confirmation, DtbBookingConfirmation>
	{
		public DtbBookingConfirmationDataObjectReader(Confirmation confirmationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer parent)
			: base(confirmationDataObject, logger, factory)
		{
			Parent = Argument.NotNull(parent, "IColumnIndexer parent");
		}

		readonly IColumnIndexer Parent;

		protected override DtbBookingConfirmation GetExistingBusinessObject()
		{
			return null;
		}

		protected override void PopulateBusinessObject(DtbBookingConfirmation confirmation)
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

			SetValue(row, DtbBookingConfirmationSchema.KK_VehicleRegistration, dataObject.VehicleRegistration);
			SetValue(row, DtbBookingConfirmationSchema.KK_DocumentID, dataObject.DriverDocumentID);
			if (dataObject.Driver != null)
			{
				var driver = new OrganizationContactDataObjectReader(dataObject.Driver, logger, factory).GetMatched();
				if (driver == null)
				{
					logger.Log(LogType.Warning, Res.GetString("9a2465dc-bf67-4137-9090-8e34bc204e0a", "There is no contact for the specified driver found in database with name '{0}'. Make sure that name is not empty. If not create contact first.", dataObject.Driver.FullName));
				}
				else
				{
					SetValue(row, DtbBookingConfirmationSchema.KK_OC_Driver, driver.PK);
				}
			}
		}
	}
}
