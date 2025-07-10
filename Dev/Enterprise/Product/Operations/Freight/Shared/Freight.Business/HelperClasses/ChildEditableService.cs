using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Business
{
	public class ChildEditableService : IChildEditableService
	{
		public static ChildEditableServiceStates GetState(BusinessObjectFactory factory)
		{
			ChildEditableServiceStates state = GetStateDirectly(factory);
			if (state == ChildEditableServiceStates.Unknown)
			{
				return ChildEditableServiceStates.Consol;
			}
			else
			{
				return state;
			}
		}

		public static ChildEditableServiceStates GetStateDirectly(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ChildEditableService>().State;
		}

		public static void SetState(BusinessObjectFactory factory, ChildEditableServiceStates state)
		{
			factory.GetCachedValue<ChildEditableService>().State = state;
		}

		public ChildEditableServiceStates State = ChildEditableServiceStates.Unknown;

		ChildEditableServiceStates IChildEditableService.GetStateDirectly(BusinessObjectFactory factory)
		{
			return GetStateDirectly(factory);
		}
	}
}
