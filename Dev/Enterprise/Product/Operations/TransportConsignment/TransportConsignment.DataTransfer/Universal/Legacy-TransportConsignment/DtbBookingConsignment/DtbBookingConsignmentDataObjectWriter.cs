using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	class DtbBookingConsignmentDataObjectWriter : TopLevelDataObjectWriter<DtbBookingConsignment, UniversalShipment>
	{
		public DtbBookingConsignmentDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		#region Context

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransportConsignment;
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		#endregion

		#region Export Job

		protected override void PopulateDataObject(DtbBookingConsignment consolidation, UniversalShipment consolidationDataObject)
		{
			consolidationDataObject.SetAdditionalReferenceCollection(() => ProcessCollection(consolidation.AdditionalReferenceNumbers, new AdditionalReferenceDataObjectWriter(writeManager), CollectionContent.Complete));
		}

		#endregion
	}
}


