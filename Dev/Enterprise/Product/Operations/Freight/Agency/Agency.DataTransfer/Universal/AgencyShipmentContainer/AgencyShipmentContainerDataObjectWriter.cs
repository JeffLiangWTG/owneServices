using Enterprise.Freight.Agency.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class AgencyShipmentContainerDataObjectWriter : TopLevelDataObjectWriter<AgencyShipmentContainer, UniversalShipment>
	{
		public AgencyShipmentContainerDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override CargoWise.Types.ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.AgencyShipmentContainer;
		}

		protected override void PopulateDataObject(AgencyShipmentContainer sourceBO, UniversalShipment dataObject)
		{
			var shipmentWriter = GetAgencyShipmentDataObjectWriter(sourceBO);
			shipmentWriter.PopulateAgencyShipment(sourceBO.Booking, dataObject);
		}

		IAgencyShipmentDataObjectWriter GetAgencyShipmentDataObjectWriter(AgencyShipmentContainer shippingContainer)
		{
			var bookingContainer = shippingContainer as AgencyBookingContainer;

			if (bookingContainer != null)
			{
				return new AgencyBookingDataObjectWriter(writeManager, bookingContainer);
			}

			return new BillOfLadingDataObjectWriter(writeManager, (BillOfLadingContainer)shippingContainer);
		}
	}
}





