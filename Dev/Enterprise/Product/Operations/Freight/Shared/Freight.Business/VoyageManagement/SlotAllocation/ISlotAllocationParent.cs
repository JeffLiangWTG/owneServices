using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface ISlotAllocationParent
	{
		BusinessObjectFactory Factory { get; }
		JobVoyage Voyage { get; }

		ZGuid PK { get; }
		ZString Code { get; }
		ZString HumanReadableName { get; }
	}
}
