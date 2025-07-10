using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using ICodeDescriptionPairListProvider = Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsAdjustmentReasonCodeDescriptionPairProvider : ICodeDescriptionPairListProvider, IWhsAdjustmentReasonCodeDescriptionPairProvider
	{
		ReadOnlyCodeDescriptionPairList ICodeDescriptionPairListProvider.GetCodeDescriptionPairList()
		{
			return WarehouseDataRegistry.Instance.AdjustmentReasonCodes.Value.GetCodeDescriptionPairList();
		}
	}
}
