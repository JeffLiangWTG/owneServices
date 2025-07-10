using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsItemReceiveTransportationUnit : IBusiness
	{
		IWhsWarehouse Warehouse { get; }
		IConsignment LatestReceiveConsignment { get; }
		ZString UnitType { get; }
		ZString ReferenceNumber { get; set;  }
		ZGuid PK { get; }
	}
}
