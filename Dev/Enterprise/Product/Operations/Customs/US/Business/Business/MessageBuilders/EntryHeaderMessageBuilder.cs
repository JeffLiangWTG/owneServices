using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public abstract class EntryHeaderMessageBuilder<T> : MessageBuilder<T> where T : BlockControlGenerator
	{
		protected EntryHeaderMessageBuilder(ICusEntryHeaderMessageAttachee entryHeader, UpdateActionCode action)
			: base(entryHeader, action)
		{
		}

		protected EntryHeaderMessageBuilder(ICusEntryHeaderMessageAttachee entryHeader)
			: base(entryHeader)
		{
		}

		protected ICusEntryHeaderMessageAttachee entryHeader
		{
			get { return (ICusEntryHeaderMessageAttachee)messageAttachee; }
		}
	}
}
