using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IScheduleChangeParent
	{
		BusinessObjectFactory Factory { get; }
		string TablePrefix { get; }
		ZGuid PK { get; }

		JobVoyage Voyage { get; }

		ZString OriginPort { get; }
		ZString DestinationPort { get; }

		SchemaGuidColumn SailingRefColumn { get; }
	}
}
