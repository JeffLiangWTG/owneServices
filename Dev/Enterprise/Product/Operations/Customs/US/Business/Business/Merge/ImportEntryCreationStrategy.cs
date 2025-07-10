using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public abstract class ImportEntryCreationStrategy : EntryCreationStrategy
	{
		protected ImportEntryCreationStrategy(JobDeclaration declaration, ZString cH_MessageType)
			: base(declaration, cH_MessageType)
		{
		}

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			return new MergeKey();
		}

		protected override bool IsEntryLineValidToBeReused(Customs.Business.CusEntryLine entryLine, BaseJobComInvoiceLine invoiceLine)
		{
			return EntryLineInvoiceLineLinkCalculator.IsEntryLineValidToBeReused((CusEntryLine)entryLine, (JobComInvoiceLine)invoiceLine);
		}

		protected override AdditionalInvoiceLineEntryLineLink LinkInvoiceLineEntryLineAndReturnPivotIfUsed(Customs.Business.CusEntryLine entryLine, BaseJobComInvoiceLine invoiceLine)
		{
			return EntryLineInvoiceLineLinkCalculator.LinkInvoiceLineEntryLineAndReturnPivotIfUsed((CusEntryLine)entryLine, (JobComInvoiceLine)invoiceLine);
		}

		protected override Customs.Business.CusEntryLine GetExistingEntryLine(BaseJobComInvoiceLine invoiceLine)
		{
			return ((JobComInvoiceLine)invoiceLine).GetEntryLineFor(CH_MessageTypeToNewEntryHeader, false);
		}

		protected override void AfterCreateOrGetEntryLine(Customs.Business.CusEntryLine rateEntryLine, BaseJobComInvoiceLine invoiceLine)
		{
			base.AfterCreateOrGetEntryLine(rateEntryLine, invoiceLine);

			((CusEntryLine)rateEntryLine).US_SupLine = false;
			((CusEntryLine)rateEntryLine).US_SupAdditionalLine = false;
		}

		protected override void ClearReferenceToEntryLineWhenLineIsNotValidForMerge(BaseJobComInvoiceLine invoiceLine)
		{
			EntryLineInvoiceLineLinkCalculator.ClearReferenceToEntryLineWhenLineIsNotValidForMerge((JobComInvoiceLine)invoiceLine);
		}

		EntryLineInvoiceLineLinkCalculator EntryLineInvoiceLineLinkCalculator
		{
			get { return fCalculator ?? (fCalculator = new EntryLineInvoiceLineLinkCalculator(CH_MessageTypeToNewEntryHeader)); }
		}
		EntryLineInvoiceLineLinkCalculator fCalculator;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static bool ShouldNotMergeThisLineWithOtherLines(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.IsSecondaryTariffLine
				|| invoiceLine.HasSecondaryTariffLines
				|| invoiceLine.IsSetXLine
				|| invoiceLine.IsSetVLine
				|| !invoiceLine.US_ADDDepositValue.IsEmpty
				|| !invoiceLine.US_CVDDepositValue.IsEmpty
				|| !invoiceLine.US_SupGoodsValue.IsEmpty
				|| !invoiceLine.US_SupAdditionalTariff1GoodsValue.IsEmpty
				|| !invoiceLine.US_SupAdditionalTariff2GoodsValue.IsEmpty
				|| !invoiceLine.US_SupAdditionalTariff3GoodsValue.IsEmpty
				|| !invoiceLine.US_SupAdditionalTariff4GoodsValue.IsEmpty
				|| !invoiceLine.US_SupAdditionalTariff5GoodsValue.IsEmpty
				|| invoiceLine.HasFDAData
				|| invoiceLine.HasFCCData
				|| invoiceLine.HasDOTData
				|| invoiceLine.HasVNEDetails
				|| invoiceLine.HasFSISDetails
				|| invoiceLine.HasPSTLines
				|| invoiceLine.HasNMFSLines
				|| invoiceLine.HasACE_FDALines
				|| invoiceLine.HasTTBLines
				|| invoiceLine.HasAPHISHeaders
				|| invoiceLine.ShouldDeclareACEDDTCData
				|| invoiceLine.HasNHTSADetails
				|| invoiceLine.HasATFDetails
				|| invoiceLine.HasFWSHeaders
				|| invoiceLine.HasOMCHeaders
				|| invoiceLine.HasAMSDetails
				|| invoiceLine.HasLaceyActData
				|| invoiceLine.IsTSCAIndBeDeclared
				|| invoiceLine.IsODSIndBeDeclared
				|| invoiceLine.HasCPSCHeaders
				|| invoiceLine.HasDEAHeaders
				|| invoiceLine.HasUSHFCHeaders
				|| invoiceLine.HasFishingInformations
				|| invoiceLine.HasMiningInformations;
		}

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetExistingEntriesCreatedThroughThisStrategy()
		{
			foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
			{
				if (entry.CH_MessageType == this.CH_MessageTypeToNewEntryHeader)
				{
					yield return entry;
				}
			}
		}

		protected override IEnumerable<Customs.Business.CusEntryLine> GetExistingEntryLinesCreatedThroughThisStrategy()
		{
			foreach (CusEntryHeader entry in GetExistingEntriesCreatedThroughThisStrategy())
			{
				foreach (CusEntryLine entryLine in entry.MergedLines)
				{
					if (!entryLine.US_SupLine && !entryLine.US_SupAdditionalLine)
					{
						yield return entryLine;
					}
				}
			}
		}

		protected override IComparer<BaseJobComInvoiceLine> GetInvoiceLineComparerForMerge(ReadOnlyBusinessObjectFactory cleanFactory)
		{
			return new LineComparer();
		}
	}
}
