using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IConvertToShipmentCreator
	{
		IProcessor CreateConvertToShipmentProcessor(IWorkflowProvider provider);
	}
}
