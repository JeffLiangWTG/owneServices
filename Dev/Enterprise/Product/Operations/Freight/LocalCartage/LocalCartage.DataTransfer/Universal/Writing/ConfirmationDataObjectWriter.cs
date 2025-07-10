using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	class ConfirmationDataObjectWriter : DataObjectWriter<BusinessObject, Confirmation>
	{
		internal ConfirmationDataObjectWriter(IDataWritingManager manager, LocalTransportDataObjectWriter.ConfirmationBuilder confirmation)
			: base(manager)
		{
			this.confirmation = confirmation;
		}

		readonly LocalTransportDataObjectWriter.ConfirmationBuilder confirmation;

		protected override Confirmation PopulateDataObject(BusinessObject package)
		{
			var confirmationDataObject = new Confirmation();

			CommonContainer container;
			CommonBookedCtgMove move;

			if ((container = package as CommonContainer) != null)
			{
				confirmationDataObject.Quantity = container.JC_ContainerCount;
			}
			else if ((move = package as CommonBookedCtgMove) != null)
			{
				confirmationDataObject.Quantity = move.EW_BookedPackCount;
			}

			confirmationDataObject.DateDescription = confirmation.ConfirmationType;
			confirmationDataObject.EstimatedDate = confirmation.EstimatedIn;
			confirmationDataObject.EstimatedOutDate = confirmation.EstimatedOut;
			confirmationDataObject.ActualDate = confirmation.ActualIn;
			confirmationDataObject.ActualOutDate = confirmation.ActualOut;
			confirmationDataObject.RequiredFromDate = confirmation.RequiredFrom;
			confirmationDataObject.RequiredToDate = confirmation.RequiredTo;
			confirmationDataObject.Demurrage = confirmation.Demurrage;
			confirmationDataObject.Distance = confirmation.Distance;
			confirmationDataObject.DistanceUnit = confirmation.DistanceUnit;
			confirmationDataObject.IsEmptyContainer = confirmation.IsEmptyContainer;
			confirmationDataObject.ReceivedBy = confirmation.ReceivedBy;
			confirmationDataObject.ServiceInstruction = confirmation.ServiceInstruction;

			confirmationDataObject.LegLink = confirmation.LegLink;

			return confirmationDataObject;
		}
	}
}
