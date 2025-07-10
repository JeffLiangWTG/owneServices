using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportCommon.DataTransfer.Universal.Testing;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbBookingConsignmentDataObjectReader_DataTargetIsTransportBookingTest : DtbTransportDataObjectReader_DataTargetIsTransportBookingTest<DtbConsignmentConsolidation, DtbBookingConsignment>
	{
		#region TestDataContextType
		protected override DataContextType GetExpectedDataContextType()
		{
			return DataContextType.TransportConsignment;
		}

		#endregion
		#region GetDataObjectReader
		protected override DtbTransportDataObjectReader_DataTargetIsTransportBooking<DtbConsignmentConsolidation, DtbBookingConsignment> GetDataObjectReader(Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new DtbBookingConsignmentDataObjectReader_DataTargetIsTransportBooking(shipment, logger, factory);
		}
		#endregion
	}
}
