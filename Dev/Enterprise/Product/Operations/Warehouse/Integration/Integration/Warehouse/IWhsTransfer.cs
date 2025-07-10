
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsTransfer : IWhsDocket
	{
		ZString DocketSubType { get; }
	}
}
