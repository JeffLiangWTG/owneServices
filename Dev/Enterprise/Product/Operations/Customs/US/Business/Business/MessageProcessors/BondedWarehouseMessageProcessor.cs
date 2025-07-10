using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.US;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	class BondedWarehouseMessageProcessor : BondedWarehouseDeclarationMessageProcessor
	{
		internal BondedWarehouseMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<CusEntryHeader, EmailDef, bool> sendMail)
			: base(messagePK, emailReportThatHasBeenDelayed)
		{
			this.sendMail = Argument.NotNull(sendMail, "sendMail");
		}
		readonly Action<CusEntryHeader, EmailDef, bool> sendMail;

		protected CusEntryHeader entryHeader
		{
			get { return (CusEntryHeader)message.EM_LinkedObject; }
		}

		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		protected override bool NeedToPublishEntryDetailsForOutward => false;

		protected override bool HasBeenWithdrawn
		{
			get { return entryHeader.HasBeenWithdrawn; }
		}

		protected override bool IsAmendmentError
		{
			get { return !supporter.IsOutwardBondedWarehousingEnabled && (entryHeader.CH_Status == ImportMessageStatusList.Codes.ErrorEntrySummaryReplace || (HasWHSTransactionAndNotCreatedPending() && entryHeader.CH_Status == ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal)); }
		}

		protected override bool IsAmendmentClear
		{
			get { return entryHeader.CH_Status == ImportMessageStatusList.Codes.ClearEntrySummaryReplace || (HasWHSTransactionAndNotCreatedPending() && entryHeader.CH_Status == ImportMessageStatusList.Codes.ClearEntrySummaryOriginal); }
		}

		bool HasWHSTransactionAndNotCreatedPending()
		{
			if (!hasWHSTransactionAndNotCreatedPendingCached.HasValue)
			{
				hasWHSTransactionAndNotCreatedPendingCached = supporter.IsOutwardBondedWarehousingEnabled ? declaration.HasWHSOutwardTransactionAndNotCreatedPending() : declaration.HasWHSInwardTransactionAndNotCreatedPending();
			}
			return hasWHSTransactionAndNotCreatedPendingCached.Value;
		}
		bool? hasWHSTransactionAndNotCreatedPendingCached;

		protected override bool IsOriginalError
		{
			get { return entryHeader.CH_Status == ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal || (supporter.IsOutwardBondedWarehousingEnabled && entryHeader.CH_Status == ImportMessageStatusList.Codes.ErrorEntrySummaryReplace); }
		}

		protected override bool IsWithdrawalError
		{
			get { return entryHeader.CH_Status == ImportMessageStatusList.Codes.ErrorEntrySummaryDelete; }
		}

		protected override void SendEmailCore(EmailDef email)
		{
			sendMail(entryHeader, email, IsOriginalError || IsAmendmentError || IsWithdrawalError);
		}

		protected override void HandleInwardAmendmentError()
		{
			if (supporter.WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.InwardCreationHeld)
			{
				supporter.PublishCancelEventForWHSInwardAndSaveIfNeeded(false, ZString.Empty, isManualWhsUpdate: false);
			}
			else
			{
				PublishUniversalCancelEventToBondedWarehouseInward();
			}
		}

		protected override string GetReferenceDetail()
		{
			return string.Format(@"<strong>Reference Number: {0}<br />
Declaration Reference: {1}<br />
Entry Number: {2}<br />", entryHeader.CH_BGMReference, EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference), entryHeader.EntryNumber);
		}
	}
}
