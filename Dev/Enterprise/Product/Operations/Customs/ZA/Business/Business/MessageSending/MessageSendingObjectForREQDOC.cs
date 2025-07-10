using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public partial class MessageSendingObjectForREQDOC : MessageSendingObject
	{
		public MessageSendingObjectForREQDOC(CusEntryHeader header) : base(header)
		{
		}

		public override ZString MessageType
		{
			get => base.MessageType;
			set
			{
				base.MessageType = value;
				if (LocalReferenceNumber.IsEmpty)
				{
					RefreshLocalReferenceNumber();
				}
			}
		}
	}
}
