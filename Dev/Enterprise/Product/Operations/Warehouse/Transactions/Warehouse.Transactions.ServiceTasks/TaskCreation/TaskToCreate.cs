using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	class TaskToCreate
	{
		public TaskToCreate(
			ZGuid id,
			ZString formflowType,
			ZString workflowName,
			ZString taskName,
			ZString staffCode,
			ZShort rawNudge,
			ZString capabilityCode,
			ZGuid releaseGroupPk,
			string taskType = "UDF")
		{
			ID = id;
			FormflowType = formflowType;
			WorkflowName = workflowName;
			TaskName = taskName;
			TaskType = taskType;
			StaffCode = staffCode;
			RawNudge = rawNudge;
			CapabilityCode = capabilityCode;
			ReleaseGroupPk = releaseGroupPk;
			TaskType = taskType;
		}

		public ZGuid ID { get; }
		public ZString FormflowType { get; }
		public ZString WorkflowName { get; }
		public ZString TaskName { get; }
		public ZString StaffCode { get; }
		public ZShort RawNudge { get; }
		public ZString CapabilityCode { get; }
		public ZGuid ReleaseGroupPk { get; }
		public ZString TaskType { get; }
	}
}
