
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.MessageBuilders
{
	public abstract class EdifactMessageBuilderFromEntryHeader : EdifactMessageBuilder
	{
		public EdifactMessageBuilderFromEntryHeader(CusEntryHeader entryHeader)
			: base()
		{
			this.entryHeader = entryHeader;
		}
		readonly CusEntryHeader entryHeader;

		protected override EDIMessage GetNewMessage()
		{
			EDIMessage message = entryHeader.Messages.AddNew();
			message.EM_IsTestMessage = entryHeader.Declaration.IsInTestMode;
			return message;
		}
	}
}
