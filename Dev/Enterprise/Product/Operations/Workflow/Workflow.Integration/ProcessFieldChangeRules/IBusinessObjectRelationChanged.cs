using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Integration
{
	public interface IBusinessObjectRelationChanged
	{
		void BusinessObjectRelationChanged(BusinessObject parent, BusinessObject child, BusinessObjectParentLocatorEvent stateChange);
	}

	public static class BusinessObjectRelationExtensions
	{
		public static void BusinessObjectRelationChanged(this BusinessObject parent, BusinessObject child, BusinessObjectParentLocatorEvent stateChange)
		{
			ObjectFactory.Get<IBusinessObjectRelationChanged>().BusinessObjectRelationChanged(parent, child, stateChange);
		}
	}
}
