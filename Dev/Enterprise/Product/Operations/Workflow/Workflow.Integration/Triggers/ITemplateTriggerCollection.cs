using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Workflow.Integration
{
	public interface ITemplateTriggerCollection : IBusinessObjectCollection
	{
		new ITemplateTrigger this[int index] { get; }
		void DeleteAll();
	}
}
