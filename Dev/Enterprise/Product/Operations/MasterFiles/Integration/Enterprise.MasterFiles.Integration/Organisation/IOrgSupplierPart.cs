using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrgSupplierPart
	{
		ZGuid PK { get; }
		ZString OP_PartNum { get; set; }
		ZString OP_StockKeepingUnit { get; }
		ZString OP_RH_NKCommodityCode { get; }
		ZDecimal OP_StockKeepingUnitPerPallet { get; }
		ZDecimal OP_Cubic { get; }
		ZString OP_CubicUQ { get; }
		ZDecimal OP_Weight { get; }
		ZString OP_WeightUQ { get; }
	}
}
