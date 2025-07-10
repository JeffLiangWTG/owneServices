using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public enum ReconMergeContext { None, Documents, Messaging }

	class ReconChangedLinesMerger
	{
		public ReconChangedLinesMerger(ReconDeclaration reconDeclaration, ReconMergeContext context = ReconMergeContext.None)
		{
			this.reconDeclaration = reconDeclaration;
			this.context = context;
		}

		readonly ReconDeclaration reconDeclaration;
		readonly ReconMergeContext context;

		/// <returns>Error text. If empty, then no errors exist</returns>
		public string GetErrors()
		{
			string result = "";

			if (reconDeclaration.InvoiceLines.Count == 0)
			{
				result = NoInvoiceLinesHaveBeenEntered;
			}

			return result;
		}

		public const string NoInvoiceLinesHaveBeenEntered = "No entry lines have been entered.";

		public void DoMerge(bool needCountDecreaseLine)
		{
			reconDeclaration.ChangedLines.RemoveAll();
			reconDeclaration.ChangedLinesWithDecrease.RemoveAll();

			var mergedKeyAndValue = new Dictionary<MergeKey, ReconChangedLine>(reconDeclaration.InvoiceLines.Count);

			var strategy = new ReconMergeStrategy(reconDeclaration, context);

			var supStrategy = new ReconSupMergeStrategy(reconDeclaration);

			//Sort first to make Parent lines come before its secondary lines and make merge look deterministic
			var sortedInvoiceLines = new List<JobComInvoiceLine>(new TypedEnumerable<JobComInvoiceLine>(reconDeclaration.InvoiceLines));
			sortedInvoiceLines.Sort(new InvoiceLineComparerForRecon());

			int countOfSecondaryLines = 0;
			int countOfParentOrStandAlone = 0;

			int countOfSecondaryLinesWithDecrease = 0;
			int countOfParentOrStandAloneWithDecrease = 0;

			ReconChangedLine mergedLineWithHighestMPF = null;

			foreach (JobComInvoiceLine invoiceLine in sortedInvoiceLines)
			{
				if (IsChangedForRecon(invoiceLine))
				{
					foreach (ReconMergeStrategy oneStategy in new ReconMergeStrategy[] { supStrategy, strategy })
					{
						if (oneStategy.LineIsValidForMerge(invoiceLine))
						{
							var key = oneStategy.GetKeyForLine(invoiceLine);

							ReconChangedLine mergedLine = null;

							if (!mergedKeyAndValue.TryGetValue(key, out mergedLine))
							{
								if (needCountDecreaseLine)
								{
									var hasDecrease = reconDeclaration.US_R_Waive && invoiceLine.HasDecrease;
									if (hasDecrease)
									{
										mergedLine = reconDeclaration.ChangedLinesWithDecrease.AddNew();
										mergedLine.US_ReconLineNumber = oneStategy.GetMergedLineNumber(invoiceLine, ref countOfSecondaryLinesWithDecrease, ref countOfParentOrStandAloneWithDecrease);
									}
									else
									{
										mergedLine = reconDeclaration.ChangedLines.AddNew();
										mergedLine.US_ReconLineNumber = oneStategy.GetMergedLineNumber(invoiceLine, ref countOfSecondaryLines, ref countOfParentOrStandAlone);
									}
								}
								else
								{
									mergedLine = reconDeclaration.ChangedLines.AddNew();
									mergedLine.US_ReconLineNumber = oneStategy.GetMergedLineNumber(invoiceLine, ref countOfSecondaryLines, ref countOfParentOrStandAlone);
								}

								oneStategy.Initialise(mergedLine, invoiceLine);

								mergedKeyAndValue.Add(key, mergedLine);
							}

							oneStategy.MergeValues(mergedLine, invoiceLine);

							if (mergedLine.MPFChange != 0 && (mergedLineWithHighestMPF == null || mergedLineWithHighestMPF.MPFChange < mergedLine.MPFChange))
							{
								mergedLineWithHighestMPF = mergedLine;
							}
						}
					}
				}
			}

			BackRoundPayableMPF(mergedLineWithHighestMPF);
			RoundValuesAndSumTotals();
		}

		//if a related line has changed like parent or child, it should be regarded as changed
		bool IsChangedForRecon(JobComInvoiceLine invoiceLine)
		{
			bool result = invoiceLine.IsChangedForReconForOneLine();

			if (!result)
			{
				if (invoiceLine.ParentTariffLine != null)
				{
					result = IsChangedForRecon(invoiceLine.ParentTariffLine);
				}
				else if (invoiceLine.IsParentLine)
				{
					foreach (JobComInvoiceLine childLine in invoiceLine.ChildLines)
					{
						result = childLine.IsChangedForReconForOneLine();

						if (result)
						{
							break;
						}
					}
				}
			}

			return result;
		}

		void BackRoundPayableMPF(ReconChangedLine mergedLineWithHighestMPF)
		{
			if (mergedLineWithHighestMPF != null)
			{
				var totalMPFDiffOnEntry = reconDeclaration.TotalReconMPF - reconDeclaration.TotalOriginalMPF;
				var totalMPFDiffOnMergedLines = reconDeclaration.ChangedLines.Sum(x => ((ReconChangedLine)x).MPFChange);

				var difference = totalMPFDiffOnEntry - totalMPFDiffOnMergedLines;
				if (difference != 0)
				{
					mergedLineWithHighestMPF.MPFChange += difference;
				}
			}
		}

		void RoundValuesAndSumTotals()
		{
			var totalCustomsValue = 0m;
			var totalOriginalValue = 0m;

			foreach (ReconChangedLine changedLine in reconDeclaration.ChangedLines.ToList())
			{
				changedLine.RoundValues();
				if (changedLine.US_SecondarySPI != SecondarySpecProgIndicatorList.Codes.X)
				{
					totalCustomsValue += changedLine.US_CustomsValue;
					totalOriginalValue += changedLine.US_OrigCustomsValue;
				}
			}
			reconDeclaration.TotalCustomsValue = totalCustomsValue;
			reconDeclaration.TotalOriginalValue = totalOriginalValue;

			totalCustomsValue = 0m;
			totalOriginalValue = 0m;

			foreach (ReconChangedLine changedLineWithDecrease in reconDeclaration.ChangedLinesWithDecrease.ToList())
			{
				changedLineWithDecrease.RoundValues();
				if (changedLineWithDecrease.US_SecondarySPI != SecondarySpecProgIndicatorList.Codes.X)
				{
					totalCustomsValue += changedLineWithDecrease.US_CustomsValue;
					totalOriginalValue += changedLineWithDecrease.US_OrigCustomsValue;
				}
			}
			reconDeclaration.TotalDecreaseCustomsValue = totalCustomsValue;
			reconDeclaration.TotalDecreaseOriginalValue = totalOriginalValue;
		}
	}

	class InvoiceLineComparerForRecon : IComparer<JobComInvoiceLine>
	{
		InvoiceLineComparer InvoiceLineComparer
		{
			get { return invoiceLineComparer ?? (invoiceLineComparer = new InvoiceLineComparer()); }
		}
		InvoiceLineComparer invoiceLineComparer;

		public int Compare(JobComInvoiceLine x, JobComInvoiceLine y)
		{
			int result = 0;

			if (x != y)
			{
				var reconOriginalEntryX = x.InvoiceHeader != null ? x.InvoiceHeader.ReconOriginalEntry : null;
				var reconOriginalEntryY = y.InvoiceHeader != null ? y.InvoiceHeader.ReconOriginalEntry : null;

				if (reconOriginalEntryX != null && reconOriginalEntryY != null)
				{
					result = reconOriginalEntryX.CH_OrigEntryReference.CompareTo(reconOriginalEntryY.CH_OrigEntryReference);
				}

				if (result == 0)
				{
					result = x.US_R_OrigEntryLineNo.CompareTo(y.US_R_OrigEntryLineNo);
				}

				if (result == 0)
				{
					result = ReconMergeStrategy.GetSPIToMerge(y.US_R_OrigSPI).CompareTo(ReconMergeStrategy.GetSPIToMerge(x.US_R_OrigSPI));
				}

				if (result == 0)
				{
					result = InvoiceLineComparer.Compare(x, y);//parent comes first and then its secondary
				}
			}

			return result;
		}
	}
}
