using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.MessageProcessors
{
	public abstract class BondedWarehouseEntryMessageProcessor : BondedWarehouseMessageProcessor
	{
		protected BondedWarehouseEntryMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, EDIMessage> sendMail)
			: base(messagePK, emailReportThatHasBeenDelayed)
		{
			this.sendMail = Argument.NotNull(sendMail, "sendMail");
		}
		readonly Action<EmailDef, EDIMessage> sendMail;

		protected CusEntryHeader entryHeader
		{
			get { return (CusEntryHeader)supporter; }
		}

		protected override string GetSubject(string subjectPrefix)
		{
			return Res.GetString("{93BF3B9B-4271-449E-951E-C9BA4AAC2E2A}", "{0} for Entry: {1}", subjectPrefix, entryHeader.EntryHeaderDescriptiveMenuItemText);
		}

		protected override IWarehouseIntegrationSupporter GetSupporter()
		{
			return (IWarehouseIntegrationSupporter)message.EM_LinkedObject;
		}

		protected abstract ZDateTime AssessmentDate { get; }
		protected abstract string CountryCode { get; }

		protected bool IsStatusCleared
		{
			get
			{
				if (!isStatusCleared.HasValue)
				{
					isStatusCleared = CustomsStatusAttributeHelper.ShouldUpdateBondedWhs(factory, entryHeader.CH_EntryStatus, CountryCode, AssessmentDate);
				}
				return isStatusCleared.Value;
			}
		}
		bool? isStatusCleared;

		protected bool IsStatusRejected
		{
			get
			{
				if (!isStatusRejected.HasValue)
				{
					isStatusRejected = CustomsStatusAttributeHelper.ShouldCancelBondedWhs(factory, entryHeader.CH_EntryStatus, CountryCode, AssessmentDate);
				}
				return isStatusRejected.Value;
			}
		}
		bool? isStatusRejected;

		protected override bool HasBeenWithdrawn
		{
			get { return IsStatusCleared && message.EM_MessageSubType == MessageSubTypeCodes.Codes.Cancellation && entryHeader.CH_Status == MessageStatusList.Codes.AcknowledgedDelete; }
		}

		protected override bool IsAmendmentError
		{
			get
			{
				return message.EM_MessageSubType == MessageSubTypeCodes.Codes.Change && (entryHeader.CH_Status == MessageStatusList.Codes.ErrorChange
					|| ((entryHeader.CH_Status == MessageStatusList.Codes.AcknowledgedChange || entryHeader.CH_Status == MessageStatusList.Codes.AcknowledgedReplace) && IsStatusRejected));
			}
		}

		protected override bool IsAmendmentClear
		{
			get { return IsStatusCleared && message.EM_MessageSubType == MessageSubTypeCodes.Codes.Change && (entryHeader.CH_Status == MessageStatusList.Codes.AcknowledgedChange || entryHeader.CH_Status == MessageStatusList.Codes.AcknowledgedReplace); }
		}

		protected override bool IsOriginalError
		{
			get { return message.EM_MessageSubType == MessageSubTypeCodes.Codes.Original && (entryHeader.CH_Status == MessageStatusList.Codes.ErrorOriginal || (IsStatusRejected && entryHeader.CH_Status == MessageStatusList.Codes.AcknowledgedOriginal)); }
		}

		protected override bool IsWithdrawalError
		{
			get { return message.EM_MessageSubType == MessageSubTypeCodes.Codes.Cancellation && (entryHeader.CH_Status == MessageStatusList.Codes.ErrorDelete || (IsStatusRejected && entryHeader.CH_Status == MessageStatusList.Codes.AcknowledgedDelete)); }
		}

		protected override void SendEmailCore(EmailDef email)
		{
			sendMail(email, message);
		}

		protected override string GetReferenceDetail()
		{
			var declaration = entryHeader.Declaration;
			return Res.GetString("{1AD6FB5A-A575-49FD-B6FE-0323B4F19D7E}", @"<strong>Declaration Reference: {0}<br />
Local Reference Number: {1}<br />
Entry Number: {2}<br />", EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference), entryHeader.CH_BGMReference, entryHeader.EntryNumber);
		}
	}
}
