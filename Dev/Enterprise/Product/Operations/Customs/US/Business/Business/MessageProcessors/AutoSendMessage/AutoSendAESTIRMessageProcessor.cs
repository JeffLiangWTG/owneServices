using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class AutoSendAESTIRMessageProcessor : USAutoSendCustomsMessageProcessor
	{
		public AutoSendAESTIRMessageProcessor(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration Declaration
		{
			get { return base.Declaration; }
		}

		protected override ZString MessageDescription
		{
			get { return "SED"; }
		}

		protected override ZString EntryType
		{
			get { return CusEntryHeaderMessageTypeList.Codes.Export; }
		}

		protected override ZBool IsBondedWarehouseIntegrated
		{
			get { return false; }
		}

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetEntryHeadersToSendCore()
		{
			foreach (var entryHeader in Declaration.ActiveEntryHeaders.OfType<CusEntryHeader>())
			{
				if (entryHeader.IsExport)
				{
					yield return entryHeader;
				}
			}
		}

		protected override MQEDIMessage SendMessagesCore(CusEntryHeader entry)
		{
			var actionCode = entry.HasBeenLodgedAtCustoms ? Messaging.Business.UpdateActionCode.Replace : Messaging.Business.UpdateActionCode.Add;
			return new AESTIRMessageBuilder(entry, actionCode).PopulateMessage();
		}

		protected override void CalculateRelatedPropertiesAfterSending(MQEDIMessage message, CusEntryHeader entry)
		{
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			entry.CH_EntryStatus = entry.CH_EntryStatus.IsEmpty ? entry.CH_Status : entry.CH_EntryStatus;
			entry.PopulateEntrySubmittedDateIfRequired();
		}

		protected override ZBool CanSendEntryHeader(Customs.Business.CusEntryHeader entryHeader)
		{
			var entry = (CusEntryHeader)entryHeader;
			return base.CanSendEntryHeader(entry) && entry.US_ShouldBeReportToCustoms;
		}

		protected override ZString GetEntryReferenceNumber(Customs.Business.CusEntryHeader entryHeader)
		{
			var entry = (CusEntryHeader)entryHeader;
			return entry.CH_BGMReference;
		}
	}
}
