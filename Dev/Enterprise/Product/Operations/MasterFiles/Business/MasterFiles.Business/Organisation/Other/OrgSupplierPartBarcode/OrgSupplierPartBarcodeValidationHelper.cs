using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business
{
	class OrgSupplierPartBarcodeValidationHelper
	{
		internal static bool CheckIfPackTypeIsConvertible(ZString packType, OrgSupplierPart supplierPart)
		{
			return supplierPart.UnitConverter.Convertible(packType, supplierPart.OP_StockKeepingUnit);
		}

		internal static bool CheckIfPackTypeIsVolumeOrWeightAndNotStockUnit(ZString packType, OrgSupplierPart supplierPart)
		{
			return (Constants.Volume.ContainsCode(packType) || Constants.Weight.ContainsCode(packType)) && packType != supplierPart.OP_StockKeepingUnit;
		}

		internal static bool CheckIfPackTypeIsStockUnit(ZString packType, bool useForDocuments, OrgSupplierPart supplierPart)
		{
			return !useForDocuments || (!packType.IsEmpty && packType.EqualsIgnoringCase(supplierPart?.OP_StockKeepingUnit ?? ZString.Empty));
		}
	}
}
