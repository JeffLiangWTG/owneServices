using CargoWise.EntityFramework;

namespace Enterprise.Freight.Integration
{
	public enum ChildEditableServiceStates
	{
		Unknown,
		Shipment,
		Consol,
		TallyContainer,
		Order,
	}

	public interface IChildEditableService
	{
		ChildEditableServiceStates GetStateDirectly(BusinessObjectFactory factory);
	}
}
