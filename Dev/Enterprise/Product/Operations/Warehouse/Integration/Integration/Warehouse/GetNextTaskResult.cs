using System;

namespace Enterprise.Warehouse.Integration
{
	public readonly struct GetNextTaskResult
	{
		public GetNextTaskResult(Guid taskPK, string taskFormFlowType)
		{
			TaskPK = taskPK;
			TaskFormFlowType = taskFormFlowType;
		}

		public GetNextTaskResult(string errorMessage)
		{
			ErrorMessage = errorMessage;
		}

		public Guid TaskPK { get; }
		public string TaskFormFlowType { get; }
		public string ErrorMessage { get; }
	}
}
