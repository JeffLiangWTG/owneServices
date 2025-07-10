using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ExternalValidationResultMessage : NonPersistentBusinessObject
	{
		public ExternalValidationResultMessage(string messageType, string messageContent)
		{
			this.MessageType = messageType;
			this.MessageContent = messageContent;
		}

		public ZString MessageType { get; set; }
		public ZString MessageContent { get; set; }
	}
}
