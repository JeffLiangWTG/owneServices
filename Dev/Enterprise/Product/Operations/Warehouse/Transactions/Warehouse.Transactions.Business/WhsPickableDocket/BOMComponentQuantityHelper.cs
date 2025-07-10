using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class BOMComponentQuantityHelper
	{
		public static ZDecimal GetComponentsQuantityToBuildKits(OrgPartBOM childComponent, ZDecimal numberOfKits)
		{
			var bomQuantity = childComponent.Component.UnitConverter.Convert(childComponent.OE_ComponentQty, childComponent.OE_F3_NKPackType, childComponent.Component.OP_StockKeepingUnit);
			return bomQuantity * numberOfKits;
		}

		public static int GetNumberOfPossibleKitsFromComponent(OrgPartBOM childComponent, ZDecimal quantity)
		{
			return GetNumberOfPossibleKitsFromBOMComponent(childComponent, quantity, childComponent.OE_F3_NKPackType);
		}

		public static int GetNumberOfPossibleKitsFromBOMComponent(OrgPartBOM childComponent, ZDecimal quantity, ZString packType)
		{
			var component = childComponent.Component;
			var availableQtyConvertedToPackType = component.UnitConverter.Convert(quantity, component.OP_StockKeepingUnit, packType);
			return availableQtyConvertedToPackType > 0 && childComponent.OE_ComponentQty > 0 ? (int)(availableQtyConvertedToPackType / childComponent.OE_ComponentQty) : 0;
		}
	}
}
