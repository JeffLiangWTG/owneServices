using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsItemDispatchConsignment : IBusiness
	{
		ZGuid PK { get; }
	}
}
