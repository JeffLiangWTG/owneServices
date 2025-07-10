using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsItemReceiveConsignment : IBusiness
	{
		ZGuid PK { get; }
	}
}
