using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Integration
{
	public interface IWorkflowSetFieldResult
	{
		object SourceValue { get; }
		object TargetValue { get; }
		string FailureReason { get; }
		WorkflowSetFieldStatus Status { get; }
		string TargetObjectName { get; }
		ZPropertyInfo PropertyInfo { get; }
		void Rollback();
		bool Validate();
	}
}
