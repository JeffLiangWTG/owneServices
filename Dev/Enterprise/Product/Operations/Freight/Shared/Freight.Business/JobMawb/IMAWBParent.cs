using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IMAWBParent
	{
		ZGuid PK { get; }
		IMAWBAllocationParent MAWBAllocationParent { get; }
	}
}
