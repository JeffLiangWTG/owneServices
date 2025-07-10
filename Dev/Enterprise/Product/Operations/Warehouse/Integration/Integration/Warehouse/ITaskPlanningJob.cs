using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface ITaskPlanningJob
	{
		ZGuid PK { get; }
		ZGuid WarehousePK { get; }
		BusinessObjectFactory Factory { get; }
		ZString TaskPlanningStatus { get; set; }
		string JobID { get; }
		ZString HumanReadableNameWithoutID { get; }
		bool IsInDatabase { get; }
		bool HasChanges { get; }
		bool IsFinalisedOrCancelled { get; }
		string SpecialCannotUpdateTaskPlanningStatusReason { get; }
		void ClearFKForProcessTasks(ISet<ZGuid> processTaskPKs);
	}
}
