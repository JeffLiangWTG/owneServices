using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Integration
{
	public interface IProcessTaskIterationLinkCollection : IBusinessObjectCollection
	{
		new IProcessTaskIterationLink this[int index] { get; }
	}
}
