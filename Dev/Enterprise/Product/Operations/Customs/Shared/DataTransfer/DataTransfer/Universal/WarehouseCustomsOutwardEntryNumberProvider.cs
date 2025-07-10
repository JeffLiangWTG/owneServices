using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class WarehouseCustomsOutwardEntryNumberProvider : IWarehouseCustomsOutwardEntryNumberProvider
	{
		public WarehouseCustomsOutwardEntryNumberProvider(Shipment shipment)
		{
			this.shipment = shipment;
		}
		protected readonly Shipment shipment;

		ZString? IWarehouseCustomsOutwardEntryNumberProvider.GetEntryNumber()
		{
			return GetEntryNumber();
		}

		protected virtual ZString? GetEntryNumber()
		{
			ZString? result = null;
			if (shipment != null)
			{
				var entryNumberPlaceHolderType = shipment.EntryNumberCollection?.FirstOrDefault(x => x.IsEntryNumberPlaceHolderType());
				if (entryNumberPlaceHolderType != null)
				{
					result = entryNumberPlaceHolderType.Number;
				}
				else
				{
					var entryNumber = shipment?.EntryHeaderCollection?.FirstOrDefault()?.EntryNumberCollection?.FirstOrDefault();
					if (entryNumber != null)
					{
						result = entryNumber.Number;
					}
				}
			}
			return result;
		}
	}
}
