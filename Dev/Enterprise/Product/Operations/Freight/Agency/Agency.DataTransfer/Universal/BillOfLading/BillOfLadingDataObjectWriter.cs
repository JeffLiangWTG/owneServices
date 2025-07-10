using Enterprise.Freight.Agency.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class BillOfLadingDataObjectWriter : AgencyShipmentDataObjectWriter<BillOfLading>, IAgencyShipmentDataObjectWriter
	{
		public BillOfLadingDataObjectWriter(IDataWritingManager manager, BillOfLadingContainer container = null)
			: base(manager, container)
		{
		}

		#region Implementation

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.BillOfLading;
		}

		protected override void PopulateShipment(BillOfLading sourceBO, UniversalShipment dataObject)
		{
			base.PopulateShipment(sourceBO, dataObject);

			var sailing = sourceBO.Sailing;
			if (sailing != null)
			{
				dataObject.VesselName = sailing.JX_JV_NKVessel;
				dataObject.VoyageFlightNo = sailing.JX_JV_VoyageFlight;
				dataObject.PortOfLoading = UNLOCO.New(sourceBO.CalcLoadPort);
				dataObject.PortOfDischarge = UNLOCO.New(sourceBO.CalcDischargePort);
			}
			dataObject.PortOfOrigin = UNLOCO.New(sourceBO.Origin);
			dataObject.PortOfDestination = UNLOCO.New(sourceBO.Destination);
		}

		#endregion

		#region IAgencyShipmentDataObjectWriter members

		void IAgencyShipmentDataObjectWriter.PopulateAgencyShipment(AgencyShipment shipmentBizObj, UniversalShipment dataObject)
		{
			PopulateDataObject((BillOfLading)shipmentBizObj, dataObject);
		}

		#endregion
	}
}



