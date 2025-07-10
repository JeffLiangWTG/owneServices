using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public interface IRefCusProcedureContext
	{
		ZBool IsIntoWarehouse { get; }
		ZBool IsOutOfWarehouse { get; }
		ZBool IsIntoInwardProcessing { get; }
		ZBool IsOutOfInwardProcessing { get; }
		ZBool IsIntoOutwardProcessing { get; }
		ZBool IsOutOfOutwardProcessing { get; }
		ZBool IsIntoTemporaryImport { get; }
		ZBool IsOutOfTemporaryImport { get; }
		ZBool IsIntoTemporaryExport { get; }
		ZBool IsOutOfTemporaryExport { get; }
		ZBool IsIntoVATWarehouse { get; }
	}
}
