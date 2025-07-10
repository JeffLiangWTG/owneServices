using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class WorkflowSetFieldResult : IWorkflowSetFieldResult
	{
		internal static WorkflowSetFieldResult Success(object sourceValue, object targetValue, string targetObjectName, Validator validateFunc, Action rollbackAction, ZPropertyInfo propertyInfo)
			=> new WorkflowSetFieldResult(WorkflowSetFieldStatus.Success, sourceValue, targetValue, targetObjectName: targetObjectName, reason: null, validateFunc: validateFunc, rollbackAction: rollbackAction, propertyInfo: propertyInfo);

		internal static WorkflowSetFieldResult Success(object sourceValue, object targetValue, string targetObjectName)
			=> new WorkflowSetFieldResult(WorkflowSetFieldStatus.Success, sourceValue, targetValue, targetObjectName: targetObjectName);

		internal static WorkflowSetFieldResult SetWithWarning(string warningReason, object sourceValue, object targetValue)
			=> new WorkflowSetFieldResult(WorkflowSetFieldStatus.Warning, sourceValue, targetValue, reason: warningReason);

		internal static WorkflowSetFieldResult NoChange(object value, string targetObjectName)
			=> new WorkflowSetFieldResult(WorkflowSetFieldStatus.NoChange, value, value, targetObjectName: targetObjectName);

		internal static WorkflowSetFieldResult Failure(object sourceValue, string reason)
			=> new WorkflowSetFieldResult(WorkflowSetFieldStatus.Failure, sourceValue, null, reason: reason);

		internal static WorkflowSetFieldResult Failure(object sourceValue, object targetValue, string reason)
			=> new WorkflowSetFieldResult(WorkflowSetFieldStatus.Failure, sourceValue, targetValue, reason: reason);

		protected WorkflowSetFieldResult(WorkflowSetFieldStatus status, object sourceValue, object targetValue, string reason = null, string targetObjectName = null, Validator validateFunc = null, Action rollbackAction = null, ZPropertyInfo propertyInfo = null)
		{
			Status = status;
			SourceValue = sourceValue;
			TargetValue = targetValue;
			FailureReason = reason;
			TargetObjectName = targetObjectName;
			ValidateFunc = validateFunc;
			RollbackAction = rollbackAction;
			PropertyInfo = propertyInfo;
		}

		public delegate bool Validator(out string errorMessage);
		public WorkflowSetFieldStatus Status { get; private set; }
		public object SourceValue { get; }
		public object TargetValue { get; }
		public string FailureReason { get; private set; }
		public string TargetObjectName { get; }

		public Validator ValidateFunc { get; }
		public Action RollbackAction { get; }
		public ZPropertyInfo PropertyInfo { get; }

		public bool Validate()
		{
			if (ValidateFunc != null)
			{
				if (ValidateFunc.Invoke(out string errorMessage))
				{
					FailureReason = String.Empty;
				}
				else
				{
					FailureReason = errorMessage;
					Status = WorkflowSetFieldStatus.Failure;
				}
			}

			return FailureReason.IsNullOrEmpty();
		}

		public void Rollback()
		{
			RollbackAction?.Invoke();
		}
	}
}
