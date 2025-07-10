using CargoWise.EntityFramework;

namespace Enterprise.Freight.Integration
{
	public interface IDemandeDeTracingMessageProcessorProvider
	{
		IProcessor GetProcessor(object container, bool isImport);
	}
}
