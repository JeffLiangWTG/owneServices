using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface ILocationConsumer : IBusiness
	{
		ZGuid LocationPK { get; }
		ZString LocationTitle { get; set; }
		ZString LocationTypeForMessages { get; }
		ZGuid WarehousePK { get; }
	}
}
