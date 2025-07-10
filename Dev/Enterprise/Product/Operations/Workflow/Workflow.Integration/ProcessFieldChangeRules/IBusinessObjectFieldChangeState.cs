using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Integration
{
	public enum BusinessObjectFieldStateChangeEvent
	{
		ObjectIntialized,
		ObjectNowRoot,
	}

	public delegate void BusinessObjectFieldStateChange(BusinessObjectFieldStateChangeEvent stateChange);

	public interface IBusinessObjectFieldChangeState
	{
		bool IsObjectInitialized(BusinessObject bizo);
		bool IsRootObject(BusinessObject bizo);
		void NotifyObjectHooked(BusinessObject bizo);
	}

	public interface IBusinessObjectFieldChangeStateFactory
	{
		IBusinessObjectFieldChangeState BusinessObjectFieldChangeState { get; }
		string ChildTableCodePrefix { get; }
	}
}
