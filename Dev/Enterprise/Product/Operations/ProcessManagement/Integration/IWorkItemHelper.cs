using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ProcessManagement.Integration
{
	public interface IWorkItemHelper
	{
		void CreateWorkItem(BusinessObjectFactory factory, bool checkExistingWorkItem,
			ZString type, ZString area, ZString activityType, ZString activitySubtype, ZString priority,
			ZString summary, ZString details);
	}
}
