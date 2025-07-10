using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class BusinessObjectFieldStateChangeNotifier : IBusinessObjectFieldStateChangeNotifier
	{
		public void FieldChangeTrackerStateChanged(BusinessObject child, BusinessObjectFieldStateChangeEvent stateChange)
		{
			if (GetService(child.Factory).StateChangeNotifiers.TryGetValue(child, out var changeNotifier))
			{
				changeNotifier(stateChange);
			}
		}

		public static void RegisterBusinessObjectFieldStateChangedNotifier(BusinessObject child, BusinessObjectFieldStateChange onChange)
		{
			GetService(child.Factory).StateChangeNotifiers[child] = onChange;
		}

		static ObjectFieldStateChangeNotifierService GetService(BusinessObjectFactory factory)
			=> factory.ServiceContainer.GetService<ObjectFieldStateChangeNotifierService>()
				?? factory.ServiceContainer.AddService(new ObjectFieldStateChangeNotifierService());

		sealed class ObjectFieldStateChangeNotifierService : IService
		{
			public Dictionary<BusinessObject, BusinessObjectFieldStateChange> StateChangeNotifiers { get; } = new Dictionary<BusinessObject, BusinessObjectFieldStateChange>();
		}
	}
}
