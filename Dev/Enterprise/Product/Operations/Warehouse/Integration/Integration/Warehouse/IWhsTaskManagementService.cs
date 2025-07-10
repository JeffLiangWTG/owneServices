using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsTaskManagementService
	{
		string ChangeTaskPlanningStatus(ZGuid jobPK, string jobType, bool changeStatusToReady);
		string ChangeTaskPlanningStatus(ITaskPlanningJob job);
		string BatchChangeTaskPlanningStatus(IReadOnlyCollection<ZGuid> jobPKs, string jobType, bool changeStatusToReady);

		GetNextTaskResult GetNextTask(BusinessObjectFactory factory, string taskReference, Guid staffPK, Guid warehousePK, string formFlowType, string lastFormFlowType, Guid[] tasksToIgnore);

		UpdateTaskStatusResult SetTaskToPlayIfValid(IProcessTask task, string expectedRFType, string staffCode);
		UpdateTaskStatusResult SetTaskToSuspendedIfValid(IProcessTask task, string staffCode);
		UpdateTaskStatusResult SetTaskToCompletedIfValid(IProcessTask task, string staffCode);
	}
}
