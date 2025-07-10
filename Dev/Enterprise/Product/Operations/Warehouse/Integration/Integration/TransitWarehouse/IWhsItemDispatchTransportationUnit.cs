using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsItemDispatchTransportationUnit : IBusiness
	{
		IWhsWarehouse Warehouse { get; }
		IConsignment LatestDispatchConsignment { get; }
		ZString UnitType { get; }
		ZString ReferenceNumber { get; set; }
		ZGuid PK { get; }
	}
}
