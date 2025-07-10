using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface ILineToPutawayInfo
	{
		BusinessObjectFactory Factory { get; }

		ZGuid PK { get; }
		ZGuid DocketPK { get; }
		ZGuid ProductPK { get; }
		ZGuid WarehousePK { get; }

		ZString InventoryStatus { get; }
		ZString InventoryHeldCode { get; }
		ZString PalletID { get; set; }
		ZDecimal QuantityToPutaway { get; }
		ZDecimal PackQuantity { get; }

		ZGuid LocationPK { get; set; }
		ZString PackType { get; set; }

		bool CheckPalletIDExists { get; }
		bool IsValidToPutaway { get; }
	}
}
