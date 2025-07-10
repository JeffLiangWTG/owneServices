using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPickTrolleyJob
	{
		ZGuid PK { get; }

		ZGuid WTJ_RQ_Equipment { get; set; }

		ZString WTJ_Status { get; set; }
	}
}
