using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	class MessageSendingObjectForTest : MessageSendingObject
	{
		public MessageSendingObjectForTest(CusEntryHeader header) : base(header)
		{
		}

		public override ZString GetMessageOwner() => ZString.Empty;
	}
}
