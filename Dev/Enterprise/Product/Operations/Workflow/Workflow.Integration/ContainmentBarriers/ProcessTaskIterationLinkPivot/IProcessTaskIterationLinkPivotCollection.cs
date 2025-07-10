using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.Workflow.Integration
{
	public interface IProcessTaskIterationLinkPivotCollection : IActiveBusinessObjectCollection
	{
		IProcessTaskIterationLinkPivot AddNewForTask(IProcessTask task);

		new IProcessTaskIterationLinkPivot this[int i] { get; }
	}
}
