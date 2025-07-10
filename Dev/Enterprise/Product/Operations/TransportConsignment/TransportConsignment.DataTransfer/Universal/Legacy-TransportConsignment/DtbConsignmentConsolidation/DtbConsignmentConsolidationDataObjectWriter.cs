using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	class DtbConsignmentConsolidationDataObjectWriter : TopLevelDataObjectWriter<DtbConsignmentConsolidation, UniversalShipment>
	{
		public DtbConsignmentConsolidationDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override bool ShouldSendConsolCostsData(DtbConsignmentConsolidation sourceBO)
		{
			return true;
		}

		#region Context

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransportConsignmentConsolidation;
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		#endregion

		#region Export Job

		protected override void PopulateDataObject(DtbConsignmentConsolidation consolidation, UniversalShipment consolidationDataObject)
		{
		}

		#endregion
	}
}


