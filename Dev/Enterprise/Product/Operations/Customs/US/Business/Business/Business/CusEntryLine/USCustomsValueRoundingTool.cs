using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using SortDirection = System.ComponentModel.ListSortDirection;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// When deciding which line to apply different roundings, parent and its secondary lines should be regarded as one line
	/// X lines should be completely excluded and their Entered values should be the sum of V lines.
	/// </summary>
	public class USCustomsValueRoundingTool
	{
		public enum RoundingIssueFixMethod { None, Truncate, RoundUp }

		public void Round(CusEntryHeader entry)
		{
			var entryLinesByInvoice = GetEntryLinesPerInvoice(entry);

			foreach (List<CusEntryLine> entryLines in entryLinesByInvoice.Values)
			{
				RoundPerInvoice(entryLines, entry.IsFTZAdmission);
			}

			SumUpXLinesFromVLines(entry);
		}

		Dictionary<JobComInvoiceHeader, List<CusEntryLine>> GetEntryLinesPerInvoice(CusEntryHeader entry)
		{
			var result = new Dictionary<JobComInvoiceHeader, List<CusEntryLine>>();

			foreach (CusEntryLine entryLine in entry.MergedLines)
			{
				if (!entryLine.IsSetXLine)
				{
					JobComInvoiceHeader invoice = entryLine.RandomLine.InvoiceHeader;

					if (invoice != null)
					{
						List<CusEntryLine> entryLines;

						if (!result.TryGetValue(invoice, out entryLines))
						{
							entryLines = new List<CusEntryLine>();
							result.Add(invoice, entryLines);
						}

						if (!entryLines.Contains(entryLine))
						{
							entryLines.Add(entryLine);
						}
					}
				}
			}

			return result;
		}

		void SumUpXLinesFromVLines(CusEntryHeader entry)
		{
			if (entry.IsCustomsChargeToBeCalculated)
			{
				foreach (CusEntryLine entryLine in entry.MergedLines)
				{
					if (entryLine.IsSetXLine)
					{
						entryLine.CL_CustomsValue = 0m;

						var parentLineOrEntryLine = entryLine.ParentLine ?? entryLine;
						if (!entryLine.US_SupLine || entryLine.ParentLine == null)
						{
							foreach (CusEntryLine childLine in parentLineOrEntryLine.ChildLines)
							{
								SumupCustomsValues(entryLine, childLine);

								if (childLine.IsVParentLine)
								{
									foreach (var childVline in childLine.ChildLines)
									{
										SumupCustomsValues(entryLine, childVline);
									}
								}
							}
						}
					}
				}
			}
		}

		void SumupCustomsValues(CusEntryLine xEntryLine, CusEntryLine childLine)
		{
			var shouldAddEnteredValueForCombinedXLine = CalculateDutyForSetsHelper.IsCombinedXLine(xEntryLine) || !xEntryLine.US_SupLine || !CustomsValueDeciderForInvoiceLine.ShouldBeDeclaredAtParent(childLine.ImportTariff, childLine.DateForDutyCalculation);
			if (ShouldAddEnteredValueToXLine(xEntryLine, childLine) && shouldAddEnteredValueForCombinedXLine)
			{
				xEntryLine.CL_CustomsValue += childLine.CL_CustomsValue;
			}

			foreach (CusEntryLine secondaryLine in childLine.ChildSecondaryEntryLines)
			{
				if (ShouldAddEnteredValueToXLine(xEntryLine, secondaryLine))
				{
					xEntryLine.CL_CustomsValue += secondaryLine.CL_CustomsValue;
				}
			}
		}

		bool ShouldAddEnteredValueToXLine(CusEntryLine xEntryLine, CusEntryLine childOrSecLine)
		{
			var result = false;

			if (xEntryLine != childOrSecLine)
			{
				var parentLineOrEntryLine = xEntryLine.ParentLine ?? xEntryLine;

				if (HasTariffValidFor98Value(parentLineOrEntryLine))// If X Sup Line is 98, then X Sup Line should only have 98 goods value
				{
					result = HasTariffValidFor98Value(xEntryLine) == HasTariffValidFor98Value(childOrSecLine);
				}
				else if (xEntryLine.ParentLine == null)//this is a X sup line or stand-alone X line
				{
					result = !HasTariffValidFor98Value(childOrSecLine) && (!xEntryLine.US_SupLine || childOrSecLine.US_SupLine);
				}
				else//if X 98/99 tariff says value should be declared at its secondary, then all values except 98 goods value are added to X's secondary line
				{
					var supplementaryTariff = parentLineOrEntryLine.ImportTariff;
					if (supplementaryTariff != null && supplementaryTariff.IsValueToBeDeclaredInAlternateTariff(xEntryLine.DateForDutyCalculation))
					{
						result = !HasTariffValidFor98Value(childOrSecLine) && !xEntryLine.US_SupLine;
					}
					else
					{
						result = xEntryLine.US_SupLine == childOrSecLine.US_SupLine;
					}
				}
			}

			return result;
		}

		bool HasTariffValidFor98Value(CusEntryLine entryLine)
		{
			return CustomsValueDeciderForInvoiceLine.IsTariffValidFor98GoodsValue(entryLine.CL_AdValoremTariff);
		}

		void RoundPerInvoice(IEnumerable<CusEntryLine> entryLines, bool isFTZAdmission)
		{
			if (!isFTZAdmission)
			{
				ZDecimal totalOfRoundedEnteredValue = entryLines.Sum(x => x.CL_CustomsValue.Round(0));
				ZDecimal totalRoundedOfSumOfUnroundedCV = entryLines.Sum(x => x.CL_CustomsValue);
				totalRoundedOfSumOfUnroundedCV = totalRoundedOfSumOfUnroundedCV > 0m && totalRoundedOfSumOfUnroundedCV < 0.5m ? new ZDecimal(1m) : totalRoundedOfSumOfUnroundedCV.Round(0);

				if (totalOfRoundedEnteredValue != totalRoundedOfSumOfUnroundedCV)
				{
					var fixMethod = totalOfRoundedEnteredValue > totalRoundedOfSumOfUnroundedCV ? RoundingIssueFixMethod.Truncate : RoundingIssueFixMethod.RoundUp;
					var sortDirection = totalOfRoundedEnteredValue > totalRoundedOfSumOfUnroundedCV ? SortDirection.Ascending : SortDirection.Descending;

					List<CusEntryLine> linesToFix = GetCandidateParentOrStandAloneLinesToFix(entryLines, fixMethod);
					linesToFix.StableSort(new ComparerForRounding(sortDirection, GetEnteredValueIncludingSecondary));

					while (totalOfRoundedEnteredValue != totalRoundedOfSumOfUnroundedCV && linesToFix.Count > 0)
					{
						CusEntryLine entryLine = linesToFix[0];

						linesToFix.Remove(entryLine);

						ApplyDifferentRounding(entryLine, fixMethod, sortDirection);

						totalOfRoundedEnteredValue = entryLines.Sum(x => x.CL_CustomsValue.Round(0));
					}
				}
			}

			foreach (var entryLine in entryLines)
			{
				if (isFTZAdmission && entryLine.CL_CustomsValue <= 1m)
				{
					entryLine.CL_CustomsValue = 1m;
				}
				else
				{
					var invoiceHeader = entryLine.RandomLine?.InvoiceHeader;

					if (!DDPCalculationHelper.IsDisbursementChargeToBeAutoCalculated(invoiceHeader))
					{
						entryLine.CL_CustomsValue = entryLine.CL_CustomsValue.Round(0);
					}
				}
			}
		}

		ZDecimal GetEnteredValueIncludingSecondary(CusEntryLine entryLine)
		{
			var result = entryLine.CL_CustomsValue;

			foreach (CusEntryLine secondaryLine in entryLine.ChildSecondaryEntryLines)
			{
				result += secondaryLine.CL_CustomsValue;
			}

			return result;
		}

		List<CusEntryLine> GetCandidateParentOrStandAloneLinesToFix(IEnumerable<CusEntryLine> entryLines, RoundingIssueFixMethod fixMethod)
		{
			List<CusEntryLine> result = new List<CusEntryLine>();

			if (fixMethod != RoundingIssueFixMethod.None)
			{
				foreach (CusEntryLine line in entryLines)
				{
					ZDecimal enteredValue = line.CL_CustomsValue;

					if (enteredValue > 0)
					{
						if (fixMethod == RoundingIssueFixMethod.Truncate && enteredValue < enteredValue.Round(0)
						|| fixMethod == RoundingIssueFixMethod.RoundUp && enteredValue > enteredValue.Round(0))
						{
							CusEntryLine entryLineToFix = line.IsSecondaryTariffLine ? line.ParentLine : line;

							if (!result.Contains(entryLineToFix))
							{
								result.Add(entryLineToFix);
							}
						}
					}
				}
			}

			return result;
		}

		void ApplyDifferentRounding(CusEntryLine entryLine, RoundingIssueFixMethod fixMethod, SortDirection sortDirection)
		{
			var entryLineToFix = entryLine;

			// if a child needs to be fixed, its parent was added to the list
			if (entryLine.IsParentLine)
			{
				entryLineToFix = GetEntryLineToFix(entryLine, sortDirection);
			}

			if (entryLineToFix != null)
			{
				var customsValue = entryLineToFix.CL_CustomsValue;

				if (fixMethod == RoundingIssueFixMethod.Truncate)
				{
					customsValue = customsValue.Truncate();
				}
				else if (fixMethod == RoundingIssueFixMethod.RoundUp)
				{
					customsValue = customsValue.Round(0) + 1;
				}
				else
				{
					customsValue = customsValue.Round(0);
				}

				entryLineToFix.CL_CustomsValue = customsValue;
			}
		}

		CusEntryLine GetEntryLineToFix(CusEntryLine entryLine, SortDirection sortDirection)
		{
			CusEntryLine result = entryLine;

			if (entryLine.IsParentLine && !entryLine.IsSetXLine)
			{
				List<CusEntryLine> lines = new List<CusEntryLine>();
				lines.Add(entryLine);
				lines.AddRange(entryLine.ChildSecondaryEntryLines);

				lines = lines.Where(x => x.CL_CustomsValue > 0).ToList();

				lines.StableSort(new ComparerForRounding(sortDirection, x => x.CL_CustomsValue));

				result = lines.FirstOrDefault();
			}

			return result;
		}

		/// <summary>
		/// When one should be rounded up, the one with the biggest fraction gets rounded up
		/// When one should be truncated, the one with the smallest fraction gets truncated
		/// </summary>
		class ComparerForRounding : IComparer<CusEntryLine>
		{
			public ComparerForRounding(SortDirection sortDirection, Func<CusEntryLine, ZDecimal> getEnteredValue)
			{
				this.sortDirection = sortDirection;
				this.getEnteredValue = getEnteredValue;
			}

			readonly SortDirection sortDirection;
			readonly Func<CusEntryLine, ZDecimal> getEnteredValue;

			#region IComparer<CusEntryLine> Members

			int IComparer<CusEntryLine>.Compare(CusEntryLine x, CusEntryLine y)
			{
				ZDecimal xEnteredValue = getEnteredValue(x);
				ZDecimal yEnteredValue = getEnteredValue(y);

				ZDecimal xDecimals = xEnteredValue - xEnteredValue.Truncate();
				ZDecimal yDecimals = yEnteredValue - yEnteredValue.Truncate();

				int result = 0;

				if (sortDirection == SortDirection.Ascending)
				{
					result = xDecimals.CompareTo(yDecimals);
				}
				else
				{
					result = yDecimals.CompareTo(xDecimals);
				}

				if (result == 0)
				{
					AppendixFDutyCalculator dutyCalculator = new AppendixFDutyCalculator(x, x.Factory);
					ZDecimal xRate = dutyCalculator.DutyResult.PercentOfValue;

					dutyCalculator = new AppendixFDutyCalculator(y, y.Factory);
					ZDecimal yRate = dutyCalculator.DutyResult.PercentOfValue;

					result = sortDirection == SortDirection.Ascending ? xRate.CompareTo(yRate) : yRate.CompareTo(xRate);
				}

				if (result == 0)
				{
					result = new BaseJobComInvoiceLine.LineComparer().Compare(x.RandomLine, y.RandomLine);
				}

				return result;
			}

			#endregion
		}
	}
}
