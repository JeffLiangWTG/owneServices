using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class BusinessObjectRelationNotifier : IBusinessObjectRelationChanged
	{
		public void BusinessObjectRelationChanged(BusinessObject parent, BusinessObject child, BusinessObjectParentLocatorEvent stateChange)
		{
			if (GetService(parent.Factory).RelationChangeNotifiers.TryGetValue(child, out var changeNotifier))
			{
				changeNotifier(stateChange, parent);
			}
		}

		public static void RegisterBusinessObjectRelationChangedNotifier(BusinessObject child, BusinessObjectRelationChange onChange)
		{
			GetService(child.Factory).RelationChangeNotifiers[child] = onChange;
		}

		static ObjectRelationNotifierService GetService(BusinessObjectFactory factory)
			=> factory.ServiceContainer.GetService<ObjectRelationNotifierService>()
				?? factory.ServiceContainer.AddService(new ObjectRelationNotifierService());

		sealed class ObjectRelationNotifierService : IService
		{
			public Dictionary<BusinessObject, BusinessObjectRelationChange> RelationChangeNotifiers { get; } = new Dictionary<BusinessObject, BusinessObjectRelationChange>();
		}
	}
}
