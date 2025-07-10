using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface IAddDocumentToEDocsTriggerActionRunnerFactory
	{
		IProcessor GetNewRunner(IProcessTaskNotification notification, IBusiness job);
	}
}
