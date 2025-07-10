using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	sealed class EBondMessageSendingAction : ImportMessageSendingAction
	{
		public EBondMessageSendingAction(CusEntryHeader entryHeader, ImportMessageSendingActionCollection actions)
			: base(entryHeader, ImportMessageStatusList.MessageType.EntrySummary, actions)
		{
			this.entryHeader = entryHeader;
		}

		readonly CusEntryHeader entryHeader;

		protected override SingleMessageManager GetMessageManager()
		{
			return null;
		}

		public override ZString US_MessageDescription => string.Format(CultureInfo.InvariantCulture, "eBond Request: {0}", entryHeader.EntryNumber);

		public override ZString US_MessageContents
		{
			get
			{
				if (messageContents == null)
				{
					var builder = new EBondRequestMessageBuilder(entryHeader.Declaration, entryHeader);
					messageContents = builder.GetMessageTextFromShipment();
				}

				return messageContents;
			}
		}

		string messageContents;
	}
}
