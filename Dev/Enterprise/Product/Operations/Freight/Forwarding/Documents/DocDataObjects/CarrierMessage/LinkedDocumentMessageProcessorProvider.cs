using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects
{
	public class LinkedDocumentMessageProcessorProvider : ILinkedDocumentMessageProcessorProvider
	{
		public void Process(IBusiness bizObj, IStmALog log)
		{
			new BookingConfirmationLinkedProcessor().Process(bizObj, log);
			new DraftBOLLinkedProcessor().Process(bizObj, log);
		}
	}
}
