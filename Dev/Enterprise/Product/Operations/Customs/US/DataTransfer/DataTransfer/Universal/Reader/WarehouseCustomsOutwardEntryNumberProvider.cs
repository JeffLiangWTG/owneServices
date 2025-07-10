using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class WarehouseCustomsOutwardEntryNumberProvider : Customs.DataTransfer.Universal.WarehouseCustomsOutwardEntryNumberProvider
	{
		public WarehouseCustomsOutwardEntryNumberProvider(Shipment shipment)
			: base(shipment)
		{
		}

		protected override ZString? GetEntryNumber()
		{
			ZString? result = null;
			if (shipment != null && shipment.AddInfoCollection != null && shipment.EntryNumberCollection != null)
			{
				var entryFilerCode = shipment.AddInfoCollection.GetZStringValue(JobDeclaration.Schema.US_EntryFilerCode.Substring(3)).GetValueOrDefault();
				if (!entryFilerCode.IsEmpty)
				{
					var entryNumberObj = shipment.EntryNumberCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == CusEntryHeaderMessageTypeList.Codes.EntrySummary);
					var entryNumber = entryNumberObj?.Number.GetValueOrDefault() ?? ZString.Empty;
					if (!entryNumber.IsEmpty)
					{
						result = entryFilerCode + "-" + entryNumber;
					}
				}
			}
			return result;
		}
	}
}
