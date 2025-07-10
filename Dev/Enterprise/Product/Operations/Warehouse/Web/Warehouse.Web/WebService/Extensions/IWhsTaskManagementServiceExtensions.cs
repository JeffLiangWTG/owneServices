using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public static class IWhsTaskManagementServiceExtensions
	{
		public static GetNextTaskResult GetNextTaskForWarehouseWeb(
			this IWhsTaskManagementService taskManagementService,
			BusinessObjectFactory factory,
			GlbStaff staff,
			Guid warehousePK,
			string formFlowType,
			Guid[] tasksToIgnore)
		{
			Argument.NotNull(taskManagementService, nameof(taskManagementService));

			using (Env.SetTemporaryUserContext(
				staff.GS_LoginName,
				GlbBranch.CurrentBranch.PK.ToGuid(),
				GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				return taskManagementService.GetNextTask(
				factory,
				string.Empty,
				staff.PK.ToGuid(),
				warehousePK,
				formFlowType,
				string.Empty,
				tasksToIgnore);
			}
		}
	}
}
