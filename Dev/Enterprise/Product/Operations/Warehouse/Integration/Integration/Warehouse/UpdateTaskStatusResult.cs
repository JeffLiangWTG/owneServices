namespace Enterprise.Warehouse.Integration
{
	public enum UpdateTaskStatusResult
	{
		Success,
		SuccessWithNoChanges,
		AssignedUserIsDifferent,
		TaskIsNotValidWarehouseJob,
		TaskIsWrongFormFlowType,
		TaskStatusIsOpen,
		TaskStatusIsCompleted,
		TaskStatusIsCancelled,
	}
}
