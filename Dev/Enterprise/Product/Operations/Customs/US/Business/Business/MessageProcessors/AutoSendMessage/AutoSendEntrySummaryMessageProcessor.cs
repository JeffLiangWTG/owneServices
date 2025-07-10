using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class AutoSendEntrySummaryMessageProcessor : USAutoSendCustomsMessageProcessor
	{
		public AutoSendEntrySummaryMessageProcessor(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration Declaration
		{
			get { return base.Declaration; }
		}

		protected override ZString MessageDescription
		{
			get { return CusEntryHeaderMessageTypeList.Descriptions.EntrySummary; }
		}

		protected override ZString EntryType
		{
			get { return CusEntryHeaderMessageTypeList.Codes.EntrySummary; }
		}

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetEntryHeadersToSendCore()
		{
			yield return Declaration.ActiveEntryHeaders.EntrySummaryEntry;
		}

		protected override MQEDIMessage SendMessagesCore(CusEntryHeader entry)
		{
			var shouldCertifyCRL = entry.ShouldCertifyCargoReleaseEnabledFromEntrySummary();
			var contactName = ZString.Empty;
			var contactPhone = ZString.Empty;
			var declaration = Declaration;
			var staff = MessageSenderContactDetailsDefaultingHelper.GetBrokerContact(declaration.Factory, declaration.PK, declaration.CompanyPK.ToGuid(), declaration.Branch.PK.ToGuid());
			if (staff != null)
			{
				contactName = staff.GS_FullName.Left(AutoUSImportMessageSendingAction.Schema.US_SE_ContactNameMaxLength);
				contactPhone = MessageSenderContactDetailsDefaultingHelper.GetPhoneNumber(staff).Left(AutoUSImportMessageSendingAction.Schema.US_SE_ContactPhoneMaxLength);
			}

			var autoSendingAction = ACEEntrySummaryMessageSendingOption.New(shouldCertifyCRL, Declaration.US_PGAExpeditedRelease, contactName, contactPhone, entry.EntryType == EntryTypeList.Codes.TemporaryImportationBond);
			var actionCode = entry.HasBeenLodgedAtCustoms ? Messaging.Business.UpdateActionCode.Replace : Messaging.Business.UpdateActionCode.Add;
			return new ACEEntrySummaryMessageBuilder(entry, autoSendingAction, actionCode).PopulateMessage();
		}

		protected override void CalculateRelatedPropertiesAfterSending(MQEDIMessage message, CusEntryHeader entry)
		{
			message.CalculateRelatedPropertiesAfterENSSending(entry);
		}
	}
}
