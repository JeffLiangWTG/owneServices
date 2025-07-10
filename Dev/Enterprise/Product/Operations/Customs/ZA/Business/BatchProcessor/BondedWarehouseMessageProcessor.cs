using System;
using CargoWise.Types;
using Enterprise.Customs.Common.ZA;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Business
{
	class BondedWarehouseMessageProcessor : Customs.Business.MessageProcessors.BondedWarehouseEntryMessageProcessor
	{
		internal BondedWarehouseMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, Messaging.Business.EDIMessage> sendMail, bool shouldCheckMessageStatus = false)
			: base(messagePK, emailReportThatHasBeenDelayed, sendMail)
		{
			this.shouldCheckMessageStatus = shouldCheckMessageStatus;
		}
		readonly bool shouldCheckMessageStatus;

		protected override bool HasBeenWithdrawn => !shouldCheckMessageStatus && IsStatusCleared && message.EM_MessageSubType == MessageSubTypeCodes.Codes.Cancellation;
		protected override bool IsAmendmentError => message.EM_MessageSubType == MessageSubTypeCodes.Codes.Change && ((shouldCheckMessageStatus && entryHeader.CH_Status == ZAMessageStatusList.Codes.Error) || IsStatusRejected);
		protected override bool IsAmendmentClear => !shouldCheckMessageStatus && IsStatusCleared && message.EM_MessageSubType == MessageSubTypeCodes.Codes.Change;
		protected override bool IsOriginalError => message.EM_MessageSubType == MessageSubTypeCodes.Codes.Original && (IsStatusRejected || (shouldCheckMessageStatus && entryHeader.CH_Status == ZAMessageStatusList.Codes.Error));
		protected override bool IsWithdrawalError => message.EM_MessageSubType == MessageSubTypeCodes.Codes.Cancellation && (IsStatusRejected || (shouldCheckMessageStatus && entryHeader.CH_Status == ZAMessageStatusList.Codes.Error));

		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;

		protected override ZDateTime AssessmentDate => entryHeader.EntryInstructionAssessmentDate;

		protected new CusEntryHeader entryHeader
		{
			get { return (CusEntryHeader)supporter; }
		}
	}
}
