using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.Freight.Integration
{
	public interface ILinkedDocumentMessageProcessorProvider
	{
		void Process(IBusiness bizObj, IStmALog log);
	}
}
