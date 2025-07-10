using CargoWise.Types;
using Enterprise.Freight.Forwarding.Registry;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class MilestoneStatusCodeConverter : ListConverter<ZString, MilestonesMilestoneStatusCodeList>
	{
		public override MilestonesMilestoneStatusCodeList Convert(ZString data, FormattingResult formattingResult)
		{
			MilestonesMilestoneStatusCodeList result = null;
			switch (data)
			{
				case CargoIMPPhase2MSUEventCodeList.Codes.DEW:
					result = MilestonesMilestoneStatusCodeList.DepartedFromExportWarehouse;
					break;
				case CargoIMPPhase2MSUEventCodeList.Codes.DIW:
					result = MilestonesMilestoneStatusCodeList.DepartedFromImportWarehouse;
					break;
				case CargoIMPPhase2MSUEventCodeList.Codes.DOC:
					result = MilestonesMilestoneStatusCodeList.DropOffAtCarrier;
					break;
				case CargoIMPPhase2MSUEventCodeList.Codes.OFD:
					result = MilestonesMilestoneStatusCodeList.OutOfImportWarehouseForDelivery;
					break;
				case CargoIMPPhase2MSUEventCodeList.Codes.POD:
					result = MilestonesMilestoneStatusCodeList.ProofOfDeliveryConfirmed;
					break;
				case CargoIMPPhase2MSUEventCodeList.Codes.REW:
					result = MilestonesMilestoneStatusCodeList.ReceivedAtExportWarehouse;
					break;
				case CargoIMPPhase2MSUEventCodeList.Codes.RIW:
					result = MilestonesMilestoneStatusCodeList.ReceivedAtImportWarehouse;
					break;
			}

			return result;
		}
	}
}
