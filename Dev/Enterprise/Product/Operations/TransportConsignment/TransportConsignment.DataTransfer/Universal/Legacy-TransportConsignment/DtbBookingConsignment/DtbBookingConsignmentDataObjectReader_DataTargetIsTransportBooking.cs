using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	class DtbBookingConsignmentDataObjectReader_DataTargetIsTransportBooking : DtbTransportDataObjectReader_DataTargetIsTransportBooking<DtbConsignmentConsolidation, DtbBookingConsignment>
	{
		public DtbBookingConsignmentDataObjectReader_DataTargetIsTransportBooking(Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipment, logger, factory)
		{
		}

		#region DataContextType

		public override DataContextType DataContextType
		{
			get { return DataContextType.TransportConsignment; }
		}

		#endregion

		#region PopulateBusinessObject

		protected override DtbTransportConsolidationDataObjectReader<DtbConsignmentConsolidation> GetNewConsolidationReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbBookingConsignment targetBO)
		{
			return new DtbConsignmentConsolidationDataObjectReader(dataObject, logger, factory, targetBO);
		}

		#endregion
	}
}
