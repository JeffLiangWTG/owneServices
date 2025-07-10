using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class AutoSendCargoReleaseMessageProcessor : USAutoSendCustomsMessageProcessor
	{
		public AutoSendCargoReleaseMessageProcessor(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration Declaration
		{
			get { return base.Declaration; }
		}

		protected override ZString MessageDescription
		{
			get { return CusEntryHeaderMessageTypeList.Descriptions.ACECargoRelease; }
		}

		protected override ZString EntryType
		{
			get { return CusEntryHeaderMessageTypeList.Codes.ACECargoRelease; }
		}

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetEntryHeadersToSendCore()
		{
			yield return Declaration.ActiveEntryHeaders.SimplifiedEntry;
		}

		protected override MQEDIMessage SendMessagesCore(CusEntryHeader entry)
		{
			var contactName = ZString.Empty;
			var contactPhone = ZString.Empty;
			var declaration = Declaration;
			var staff = MessageSenderContactDetailsDefaultingHelper.GetBrokerContact(declaration.Factory, declaration.PK, declaration.CompanyPK.ToGuid(), declaration.Branch.PK.ToGuid());

			if (staff != null)
			{
				contactName = staff.GS_FullName.Left(AutoUSImportMessageSendingAction.Schema.US_SE_ContactNameMaxLength);
				contactPhone = MessageSenderContactDetailsDefaultingHelper.GetPhoneNumber(staff).Left(AutoUSImportMessageSendingAction.Schema.US_SE_ContactPhoneMaxLength);
			}

			var autoSendingAction = ACEEntrySummaryMessageSendingOption.New(true, false, contactName, contactPhone);
			var actionCode = entry.HasBeenLodgedAtCustoms ? Messaging.Business.UpdateActionCode.Replace : Messaging.Business.UpdateActionCode.Add;
			return new SimplifiedEntryMessageBuilder(entry, actionCode, autoSendingAction).PopulateMessage();
		}

		protected override void CalculateRelatedPropertiesAfterSending(MQEDIMessage message, CusEntryHeader entry)
		{
			var statusCalculator = new SimplifiedEntryMessageStatusCalculator(entry);
			statusCalculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			entry.Messages.Add(message);
			entry.PopulateEntrySubmittedDateIfRequired();
			Declaration.PopulatePaymentDueDateIfNeeded();
		}
	}
}
