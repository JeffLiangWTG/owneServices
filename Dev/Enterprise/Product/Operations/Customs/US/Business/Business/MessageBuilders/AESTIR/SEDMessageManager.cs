using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business
{
	public class SEDMessageManager : SingleMessageManager
	{
		public SEDMessageManager(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
		}

		readonly CusEntryHeader entryHeader;

		public override BusinessObject BusinessObject
		{
			get { return entryHeader; }
		}

		public override bool CanSendOriginal
		{
			get { return (entryHeader.EntryNumber.IsEmpty || entryHeader.CH_Status == AESDirectCustomsEntryStatus.Codes.NotSent) && !IsWaitingForResponse; }
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			CusEntryHeader entryHeader = bizo as CusEntryHeader;
			BusinessObjectFactory noneSaveFactory = new BusinessObjectFactory();
			Enterprise.Messaging.Business.EDIMessage message = noneSaveFactory.New<Enterprise.Messaging.Business.EDIMessage>();
			message.EM_MessageText = entryHeader.SEDString;
			return new Enterprise.Messaging.Business.EDIMessage[] { message };
		}

		public override bool IsWaitingForResponse
		{
			get
			{
				return entryHeader.CH_Status == AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse ||
					entryHeader.CH_Status == AESDirectCustomsEntryStatus.Codes.AwaitingReplacementResponse;
			}
		}

		public override string MessageFriendlyName
		{
			get { return "SED Message for " + entryHeader.CH_BGMReference; }
		}

		public override bool CanSendWithdrawal
		{
			get { throw new NotSupportedException(); }
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			throw new NotSupportedException();
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			throw new NotSupportedException();
		}
	}
}
