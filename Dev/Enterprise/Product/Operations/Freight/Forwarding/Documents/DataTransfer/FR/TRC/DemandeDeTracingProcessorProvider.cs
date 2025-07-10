using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	public sealed class DemandeDeTracingProcessorProvider : IDemandeDeTracingMessageProcessorProvider
	{
		public IProcessor GetProcessor(object container, bool isImport)
		{
			if (container is ForwardingContainer forwardingContainer)
			{
				var direction = isImport
					? DemandeDeTracingDirection.Import
					: DemandeDeTracingDirection.Export;

				return new DemandeDeTracingMessageProcessor(forwardingContainer, direction);
			}

			return null;
		}
	}
}
