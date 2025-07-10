using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	public class ImportEntryStatusCalculator : StatusCalculator<CusEntryHeader>
	{
		public ImportEntryStatusCalculator(CusEntryHeader entry)
			: base(entry)
		{
		}

		protected override void DeriveStatus()
		{
			Enterprise.Messaging.Business.EDIMessage[] significantMessages = this.SignificantMessages;

			ZString result = ZString.Empty;

			//GetMatchingMessages() sorts messages 
			MQEDIMessage theRecentMessage = (MQEDIMessage)(significantMessages.Length > 0 ? significantMessages[0] : null);

			if (theRecentMessage != null && theRecentMessage.HasChanges)
			{
				result = ((IEntryStatusProvider)theRecentMessage).EntryStatus;

				if (!result.IsEmpty)
				{
					Parent.CH_EntryStatus = result;
				}
			}
		}

		Enterprise.Messaging.Business.EDIMessage[] SignificantMessages
		{
			get
			{
				return Parent.Messages.GetMatchingMessages(
					CBPEDIInterchange.ApplicationCodes.USCustomsImport
					, ApplicationIdentifierCodeList.GetApplicationCodesThatContainEntryStatus()
					, MQEDIMessage.Direction.Receive);
			}
		}
	}
}
