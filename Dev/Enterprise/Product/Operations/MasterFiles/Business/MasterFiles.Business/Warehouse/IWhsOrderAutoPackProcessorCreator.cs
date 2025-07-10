using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IWhsOrderAutoPackProcessorCreator
	{
		IProcessor CreateOrderAutoPackProcessor(IWorkflowProvider provider);
	}
}
