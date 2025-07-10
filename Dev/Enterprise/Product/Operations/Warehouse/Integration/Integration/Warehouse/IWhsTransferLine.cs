using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsTransferLine : IWhsDocketLine
	{
		ZString WE_TransferFromPalletId { get; }
		ZGuid WE_WL_TransferFrom { get; }
	}
}
