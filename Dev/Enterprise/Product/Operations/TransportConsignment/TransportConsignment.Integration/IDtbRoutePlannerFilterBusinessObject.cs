using CargoWise.EntityFramework;

namespace Enterprise.TransportConsignment.Integration
{
	public interface IDtbRoutePlannerFilterBusinessObject
	{
		ZQuery ChildFiltersForRunsheets { get; }
	}
}
