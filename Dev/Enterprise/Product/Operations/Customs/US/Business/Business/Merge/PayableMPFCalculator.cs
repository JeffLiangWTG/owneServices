using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PayableMPFCalculator
	{
		public PayableMPFCalculator(bool shouldReportApportionError)
		{
			this.shouldReportApportionError = shouldReportApportionError;
		}

		readonly bool shouldReportApportionError;

		public void Calculate(IEntryHeaderDutyDataProvider entry)
		{
			entry.InvoiceLines.ForEach(x => x.US_PayableMPF = ZDecimal.Zero);
			var isInformalEntry = EntryTypeList.IsInformal(entry.EntryType);

			var isTotalMPFMinOrMaxed = !isInformalEntry && entry.EntryLines.Cast<IEntryLineDutyDataProvider>().Sum(x => x.MPFAmount) != entry.MPFAmountForEntry; // Currently we only get 499, we should get 311 to join this processing.
			if (isInformalEntry && entry.InformalFee > 0)
			{
				ApportionForInvoiceLines(entry.InformalFee, entry.InvoiceLines.Cast<IInvoiceLineDutyDataProvider>());
			}
			else if (isTotalMPFMinOrMaxed)
			{
				ApportionForInvoiceLines(entry.MPFAmountForEntry, entry.InvoiceLines.Cast<IInvoiceLineDutyDataProvider>());
			}
			else//apportion should happen per IEntryLineDutyDataProvider to its invoiceLines
			{
				foreach (IEntryLineDutyDataProvider entryLine in entry.EntryLines)
				{
					if (entryLine.MPFAmount > 0m)
					{
						ApportionForInvoiceLines(entryLine.MPFAmount, entryLine.InvoiceLines.Cast<IInvoiceLineDutyDataProvider>());
					}
				}
			}
		}

		void ApportionForInvoiceLines(ZDecimal totalMPF, IEnumerable<IInvoiceLineDutyDataProvider> invoiceLines)
		{
			ZDecimal totalCalculatedMPF = 0m;
			var listForRounding = new List<KeyValuePair<IInvoiceLineDutyDataProvider, ZDecimal>>();
			var totalCVAndLines = GetTotalCustomsValueOfLinesWithMPF(invoiceLines);

			foreach (IInvoiceLineDutyDataProvider invoiceLine in invoiceLines)
			{
				ZDecimal unroundedMPF = GetAmountApportionedForMPF(invoiceLine, totalCVAndLines, totalMPF);
				invoiceLine.US_PayableMPF = unroundedMPF.Round(2);
				totalCalculatedMPF += invoiceLine.US_PayableMPF;

				listForRounding.Add(new KeyValuePair<IInvoiceLineDutyDataProvider, ZDecimal>(invoiceLine, invoiceLine.US_PayableMPF - unroundedMPF));
			}
			RoundApportionedMPF(listForRounding, totalMPF, totalCalculatedMPF);
		}

		Tuple<ZDecimal, int> GetTotalCustomsValueOfLinesWithMPF(IEnumerable<IInvoiceLineDutyDataProvider> invoiceLines)
		{
			var totalCV = ZDecimal.Zero;
			int numberOfLinesMPFPayable = 0;

			foreach (IInvoiceLineDutyDataProvider invoiceLine in invoiceLines)
			{
				if (IsMPFPayable(invoiceLine))
				{
					totalCV += GetCustomsValueForPayableMPFCalculation(invoiceLine);
					numberOfLinesMPFPayable++;
				}
			}

			return new Tuple<ZDecimal, int>(totalCV, numberOfLinesMPFPayable);
		}

		/// <summary>
		/// MPF is special as some invoice lines are exempt and CusEntryHeader has an actual amount that is to be paid. 
		/// </summary>
		ZDecimal GetAmountApportionedForMPF(IInvoiceLineDutyDataProvider invoiceLine, Tuple<ZDecimal, int> totalCVAndLines, ZDecimal totalMPFReported)
		{
			ZDecimal result = 0m;

			if (IsMPFPayable(invoiceLine))
			{
				var totalCustomsValueOfLinesWithMPF = totalCVAndLines.Item1;
				var numberOfLinesMPFPayable = totalCVAndLines.Item2;
				if (totalCustomsValueOfLinesWithMPF > 0)
				{
					result = totalMPFReported * GetCustomsValueForPayableMPFCalculation(invoiceLine) / totalCustomsValueOfLinesWithMPF;
				}
				else if (numberOfLinesMPFPayable > 0)
				{
					result = totalMPFReported / numberOfLinesMPFPayable;
				}
			}

			return result;
		}

		static bool IsMPFPayable(IInvoiceLineDutyDataProvider invoiceLine)
		{
			return invoiceLine.AllEntryLines.Any(entryLine => entryLine != null && (invoiceLine.IsInformal || entryLine.US_HasMPF || entryLine.ParentLine != null && entryLine.ParentLine.US_HasMPF));
		}

		//Should not use US_CustomsValue as at this point, it is rounded
		static ZDecimal GetCustomsValueForPayableMPFCalculation(IInvoiceLineDutyDataProvider invoiceLine)
		{
			ZDecimal result = invoiceLine.JI_CustomsValue - invoiceLine.TotalOriginalGoodsValueInUSD;

			if (result < 0)
			{
				result = 0m;
			}

			return result;
		}

		void RoundApportionedMPF(List<KeyValuePair<IInvoiceLineDutyDataProvider, ZDecimal>> listForRounding, ZDecimal totalMPFReported, ZDecimal totalCalculatedMPF)
		{
			ZDecimal difference = totalMPFReported - totalCalculatedMPF;
			if (difference != 0)
			{
				ZDecimal adjustmentAmount;
				List<KeyValuePair<IInvoiceLineDutyDataProvider, ZDecimal>> adjustmentCandidates;

				if (difference > 0)
				{
					adjustmentCandidates = listForRounding.FindAll(x => x.Value <= 0m);
					adjustmentAmount = 0.01m;
				}
				else
				{
					adjustmentCandidates = listForRounding.FindAll(x => x.Value > 0m);
					adjustmentAmount = -0.01m;
				}

				difference = Math.Abs(difference);

				if (difference <= adjustmentCandidates.Count * 0.01M)
				{
					adjustmentCandidates.Sort(SortByRoundingError);
					int index = 0;

					while (difference > 0)
					{
						KeyValuePair<IInvoiceLineDutyDataProvider, ZDecimal> mpfToBump = adjustmentCandidates[index];
						difference -= 0.01m;

						ZDecimal newAmount = mpfToBump.Key.US_PayableMPF + adjustmentAmount;

						mpfToBump.Key.US_PayableMPF = newAmount;
						index++;
					}
				}
				else if (shouldReportApportionError)
				{
					var error = new StringBuilder("There are not enough invoice lines to apportion.");
					error.AppendLine($"Total MPF Reported: {totalMPFReported}, Calculated: {totalCalculatedMPF}");
					error.AppendLine("PK,LineNo,Parent,AddInfo,Value,PayableMPF,CustomsValue,GoodsValue");
					listForRounding.ForEach(x =>
					{
						var line = x.Key;
						error.AppendLine(string.Join(",", line.PK, line.JI_LineNo, line.JI_ParentID, line.JI_AddInfo, x.Value, line.US_PayableMPF, line.JI_CustomsValue, line.TotalOriginalGoodsValueInUSD));
					});
					ErrorReporter.ReportOnce("MPF Rounding Error", error.ToString());
				}
			}
		}

		int SortByRoundingError(KeyValuePair<IInvoiceLineDutyDataProvider, ZDecimal> x, KeyValuePair<IInvoiceLineDutyDataProvider, ZDecimal> y)
		{
			int result = Math.Abs(y.Value).CompareTo(Math.Abs(x.Value));

			if (result == 0 && y.Key.InvoiceDisplaySequence.HasValue && x.Key.InvoiceDisplaySequence.HasValue)
			{
				result = y.Key.InvoiceDisplaySequence.Value.CompareTo(x.Key.InvoiceDisplaySequence.Value);
			}

			if (result == 0)
			{
				result = y.Key.JI_LineNo.CompareTo(x.Key.JI_LineNo);
			}

			return result;
		}
	}
}
