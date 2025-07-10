using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	class ReconEntryLineGroup : IReconEntryLineGroup
	{
		public ReconEntryLineGroup(IEnumerable<IReconEntryLine> entryLines)
		{
			secondaryLines = new List<IReconSecondaryLine>();
			totalFees = new List<IReconciliationImportEntryFee>();
			origEntryLines = new List<IReconOriginalEntryLine>();

			var sortedEntryLines = entryLines.OrderBy(x => x.SecondaryLineNumber);
			this.parentEntryLine = sortedEntryLines.FirstOrDefault();
			foreach (IReconEntryLine entryLine in sortedEntryLines)
			{
				MergeLine(entryLine);
			}
		}

		readonly IReconEntryLine parentEntryLine;
		readonly List<IReconSecondaryLine> secondaryLines;
		readonly List<IReconciliationImportEntryFee> totalFees;
		readonly List<IReconOriginalEntryLine> origEntryLines;
		ZString reconReasonText;

		void MergeLine(IReconEntryLine entryLine)
		{
			if (reconReasonText.IsEmpty && !entryLine.ReconReason.IsEmpty)
			{
				reconReasonText = entryLine.ReconReason;
			}

			foreach (var originalEntryLine in entryLine.OriginalEntryLines)
			{
				AddOrigEntryLine(originalEntryLine);
			}
			totalFees.AddRange(entryLine.Fees);

			if (!entryLine.SecondaryLineNumber.IsEmpty && entryLine.SecondaryLineNumber != "A")
			{
				secondaryLines.Add((IReconSecondaryLine)entryLine);
			}
		}

		void AddOrigEntryLine(IReconOriginalEntryLine newOriginalEntryLine)
		{
			if (!origEntryLines.Any(x => x.EntryFilerCode == newOriginalEntryLine.EntryFilerCode &&
																x.EntryLineNumber == newOriginalEntryLine.EntryLineNumber &&
																x.EntryNumber == newOriginalEntryLine.EntryNumber))
			{
				origEntryLines.Add(newOriginalEntryLine);
			}
		}

		#region IReconEntryLineGroup Members
		ZString IReconEntryLineGroup.OriginalHTS => parentEntryLine.OriginalHTS;
		ZString IReconEntryLineGroup.OriginalCoutryOfOrigin => parentEntryLine.OriginalCoutryOfOrigin;
		ZString IReconEntryLineGroup.OriginalSPI => parentEntryLine.OriginalSPI;
		ZDate IReconEntryLineGroup.OriginalHTSEffectiveDate => parentEntryLine.OriginalHTSEffectiveDate;
		ZString IReconEntryLineGroup.ReconReason => reconReasonText;
		IEnumerable<IReconSecondaryLine> IReconEntryLineGroup.SecondaryLines => secondaryLines;

		ZString IReconEntryLineGroup.ReconHTS => parentEntryLine.ReconHTS;
		ZDecimal IReconEntryLineGroup.ReconCustomsValue => parentEntryLine.ReconCustomsValue;
		ZDecimal IReconEntryLineGroup.ReconDuty => parentEntryLine.ReconDuty;
		ZString IReconEntryLineGroup.ReconSPI => parentEntryLine.ReconSPI;
		ZBool IReconEntryLineGroup.HTSChangedDueToValueIndicator => parentEntryLine.HTSChangedDueToValueIndicator;
		ZBool IReconEntryLineGroup.IsCottonFeeMandatory => parentEntryLine.IsCottonFeeMandatory;
		IEnumerable<IReconciliationImportEntryFee> IReconEntryLineGroup.Fees
		{
			get
			{
				var totalFeesAfterMerged = totalFees.GroupBy(x => x.FeeClass).Select(x => new ReconEntryFeeIReconciliationImportEntryFee()
				{
					FeeType = x.Key,
					ReconFee = x.Sum(fee => fee.EstimatedReconciliationFee),
					OriginalFee = x.Sum(fee => fee.OriginalFee)
				}).ToList();
				totalFeesAfterMerged.Sort(new ReconEntryFeeComparer());
				return totalFeesAfterMerged;
			}
		}

		IEnumerable<IReconOriginalEntryLine> IReconEntryLineGroup.OriginalEntryLines => origEntryLines;

		ZString IReconEntryLineGroup.AllAdditionalHTSs
		{
			get { return new ZStringBuilder(secondaryLines.Select(x => x.OriginalHTS)).ToStringWithDelimiterBetweenAppends(":"); }
		}
		ZBool IReconEntryLineGroup.IsNAFTARecon => parentEntryLine.IsNAFTARecon;
		ZDecimal IReconEntryLineGroup.OriginalCustomsValue => parentEntryLine.OriginalCustomsValue;
		ZDecimal IReconEntryLineGroup.OriginalDuty => parentEntryLine.OriginalDuty;
		ZString IReconEntryLineGroup.CalculateYear => parentEntryLine.CalculateYear;
		#endregion
	}
}
