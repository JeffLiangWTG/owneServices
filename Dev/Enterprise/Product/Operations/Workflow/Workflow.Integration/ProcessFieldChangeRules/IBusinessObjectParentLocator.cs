using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Integration
{
	public enum BusinessObjectParentLocatorEvent
	{
		ParentAdded,
		ParentRemoved,
	}

	public delegate void BusinessObjectRelationChange(BusinessObjectParentLocatorEvent stateChange, BusinessObject relation);

	public interface IBusinessObjectParentLocator
	{
		bool TryLocateParentBusinessObjects(BusinessObject child, out IEnumerable<BusinessObject> parents);
	}

	public interface IBusinessObjectParentLocatorFactory
	{
		IBusinessObjectParentLocator BusinessObjectParentLocator { get; }
		string ChildTableCodePrefix { get; }
	}
}
