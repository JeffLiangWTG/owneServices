using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsWarehouseTransaction
	{
		IOrgHeader Client { get; }
		ZGuid ExternalPK { get; }
		ZString Reference { get; }
		ZDateTime Date { get; }
		IOrgHeader TransportCompany { get; }
		IWhsWarehouseTransactionLineCollection Lines { get; }
		bool HasErrors { get; }
		bool HasWarnings { get; }
		NotificationCollection Problems { get; }
	}
}
