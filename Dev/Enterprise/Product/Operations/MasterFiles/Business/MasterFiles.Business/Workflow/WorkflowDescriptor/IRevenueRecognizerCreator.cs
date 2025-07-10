using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IRevenueRecognizerCreator
	{
		IProcessor CreateRevenueRecognizer(IWorkflowProvider pluigIn);
	}
}
