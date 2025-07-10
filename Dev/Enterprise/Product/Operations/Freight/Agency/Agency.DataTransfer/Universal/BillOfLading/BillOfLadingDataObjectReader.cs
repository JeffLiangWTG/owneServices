using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class BillOfLadingDataObjectReader : AgencyShipmentDataObjectReader<BillOfLading>
	{
		public BillOfLadingDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IAgencyShipmentReadStrategy<BillOfLading> readStrategy = null)
			: base(dataObject, logger, factory, readStrategy)
		{
		}

		#region Implementation

		public override DataContextType DataContextType
		{
			get { return DataContextType.BillOfLading; }
		}

		protected override IAgencyShipmentReadStrategy<BillOfLading> GetAgencyShipmentReadStrategy()
		{
			return new AgencyShipmentReadStrategy<BillOfLading, BillOfLadingContainer, BillOfLadingPackLine>(logger, factory, dataObject);
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(BillOfLading matchedShipment)
		{
			var shipmentStatus = dataObject.ShipmentStatus != null && dataObject.ShipmentStatus.Code.HasValue ?
				dataObject.ShipmentStatus.Code.Value.ToString().Trim() : null;

			if (!string.IsNullOrEmpty(shipmentStatus) && !ShipmentStatusHelperMethods.IsBillOfLadingStage(shipmentStatus))
			{
				return Res.GetString("09c9d2a4-7c6d-49e4-af52-0c9d23bccfd0", "Data object contains shipment status [{0}] that is invalid in this scope.", shipmentStatus);
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(matchedShipment);
		}

		protected override void PopulateShipment(BillOfLading billOfLading)
		{
			if (!billOfLading.IsBillOfLadingStage)
			{
				billOfLading.Confirm();
				logger.Log(LogType.Information, Res.GetString("567c9c3b-00a8-4a80-9b3d-052906767648", "Confirmed {0}.", billOfLading.HumanReadableName));
			}

			base.PopulateShipment(billOfLading);
		}

		protected override void ReadShipmentStatus(BillOfLading agencyShipment)
		{
			if (IsShippingInstruction)
			{
				agencyShipment.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
			}
			else
			{
				base.ReadShipmentStatus(agencyShipment);
			}
		}

		#endregion
	}
}




