using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class AgencyBookingDataObjectWriter : AgencyShipmentDataObjectWriter<AgencyBooking>, IAgencyShipmentDataObjectWriter
	{
		public AgencyBookingDataObjectWriter(IDataWritingManager manager, AgencyBookingContainer container = null)
			: base(manager, container)
		{
		}

		#region Implementation

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.AgencyBooking;
		}

		#endregion

		#region IAgencyShipmentDataObjectWriter members

		void IAgencyShipmentDataObjectWriter.PopulateAgencyShipment(AgencyShipment shipmentBizObj, UniversalShipment dataObject)
		{
			PopulateDataObject((AgencyBooking)shipmentBizObj, dataObject);
		}

		#endregion
	}
}




