using CargoWise.Types;

namespace Enterprise.TransportConsignment.Integration
{
	public interface IDtbConsignmentRunSheet
	{
		ZString KG_RunSheetNumber { get; set; }
		ZGuid PK { get; }
	}
}
