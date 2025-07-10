using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class EntryLineHelper
	{
		internal EntryLineHelper(IEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}
		readonly IEntryLine entryLine;

		public ZDecimal GetTotalCustomsValueIncludingSecondaryLines()
		{
			return GetTotalAmountIncludingSecondaryLines((x) => x.CL_CustomsValue);
		}

		public ZDecimal GetTotalCustomsValueIncludingSecondaryLinesFromInvoiceLines()
		{
			var parentLineOrThis = entryLine.ParentLine ?? entryLine;
			var invoiceLines = parentLineOrThis.InvoiceLines.ToList();
			parentLineOrThis.ChildSecondaryEntryLines.ForEach(x => invoiceLines.AddRange(x.InvoiceLines));
			return invoiceLines.Distinct().Sum(x => x.US_CustomsValue);
		}

		public ZDecimal GetTotalTaxIncludingSecondaryLines()
		{
			return GetTotalAmountIncludingSecondaryLines((x) => x.ExciseTax);
		}

		public ZDecimal GetTotalFeeAmountIncludingSecondaryLines()
		{
			return GetTotalAmountIncludingSecondaryLines((x) => x.TotalFeeAmount);
		}

		public ZDecimal GetTotalMPFIncludingSecondaryLines()
		{
			return GetTotalAmountIncludingSecondaryLines((x) => x.MPFAmount);
		}

		public ZDecimal GetTotalHMFIncludingSecondaryLines()
		{
			return GetTotalAmountIncludingSecondaryLines((x) => x.HMFAmount);
		}

		public ZDecimal GetTotalDutyIncludingSecondaryLines()
		{
			return GetTotalAmountIncludingSecondaryLines((x) => x.DutyAmount);
		}

		public ZDecimal GetTotalPayableMPFIncludingSecondaryLines()
		{
			return GetTotalAmountIncludingSecondaryLines((x) => x.PayableMPFAmount);
		}

		ZDecimal GetTotalAmountIncludingSecondaryLines(Func<IEntryLine, ZDecimal> getAmountFunc)
		{
			var parentLineOrThis = entryLine.ParentLine ?? entryLine;
			var result = getAmountFunc(parentLineOrThis);
			result += parentLineOrThis.ChildSecondaryEntryLines.Sum(l => getAmountFunc(l));
			return result;
		}

		public ZString GetDutyRateDesc()
		{
			var result = entryLine.BaseDutyRateDesc;
			if (result.IsEmpty && entryLine.DutyAmount == 0m && !entryLine.NoDutyRateExists)
			{
				result = DutyResult.DutyFreeString;
			}
			return result;
		}

		public IEnumerable<IEntryLine> GetChildLines() => entryLine.AllRelatedEntryLines.Where(x => x.ParentLine == entryLine).OrderBy(x => x, new EntrySummaryEntryLineComparerForNumbering());

		public bool MatchesTariff(IInvoiceLine invoiceLine)
		{
			return invoiceLine.MatchesTariff(entryLine.US_SupLine, entryLine.US_SupAdditionalLine, entryLine.US_SupAdditionalLine2, entryLine.US_SupAdditionalLine3, entryLine.US_SupAdditionalLine4, entryLine.US_SupAdditionalLine5, entryLine.CL_AdValoremTariff);
		}

		public ZBool GetIsSecondaryTariffLine()
		{
			bool result = false;

			if (entryLine.ParentLine is IEntryLine parentEntryLine)
			{
				if (entryLine.IsSetXLine || entryLine.IsSetVLine)
				{
					if (parentEntryLine.US_SupAdditionalLine)
					{
						result = !entryLine.RandomLine.US_SupAdditionalTariff1.IsEmpty && !entryLine.US_SupAdditionalLine;
					}
					else
					{
						result = !entryLine.RandomLine.HasEmptySupTariff && !entryLine.US_SupLine;
					}
				}
				else
				{
					result = true;
				}
			}

			return result;
		}

		public IInvoiceLine GetRandomLine()
		{
			foreach (var invoiceLine in entryLine.InvoiceLines)
			{
				if (MatchesTariff(invoiceLine))
				{
					return invoiceLine;
				}
			}
			return null;
		}

		public IInvoiceLine GetFirstInvoiceLineAfterSortedOnInvoiceLineNo()
		{
			return entryLine.InvoiceLines.Any() ? entryLine.InvoiceLines.OrderBy(x => x, new Customs.Business.BaseJobComInvoiceLine.LineComparer()).FirstOrDefault() : null;
		}

		public ZDecimal GetCustomsQuantity()
		{
			return GetEffectiveCustomsQuantity(() =>
			{
				var result = ZDecimal.Zero;
				foreach (var invoiceLine in entryLine.InvoiceLines)
				{
					result += invoiceLine.JI_CustomsQuantity;
				}

				return result;
			},
			(x) => x.US_SupQty1,
			(x) => x.US_SupAdditionalTariff1Qty,
			(x) => x.US_SupAdditionalTariff2Qty,
			(x) => x.US_SupAdditionalTariff3Qty,
			(x) => x.US_SupAdditionalTariff4Qty,
			(x) => x.US_SupAdditionalTariff5Qty);
		}

		public ZString GetCustomsUnitQty()
		{
			return GetEffectiveCustomsUQ((x) => x.JI_CustomsUnitQty, (x) => x.US_SupUQ1, (x) => x.US_SupAdditionalTariff1UQ, (x) => x.US_SupAdditionalTariff2UQ, (x) => x.US_SupAdditionalTariff3UQ, (x) => x.US_SupAdditionalTariff4UQ, (x) => x.US_SupAdditionalTariff5UQ);
		}

		public ZDecimal GetSecondCustomsQuantity()
		{
			return GetEffectiveCustomsQuantity(() => SumupLineTotalsForEntry<IInvoiceLine>((x) => x.JI_CustomsSecondQuantity), (x) => x.US_SupQty2, null, null, null, null, null);
		}

		public ZString GetSecondCustomsUnitQty()
		{
			return GetEffectiveCustomsUQ((x) => x.JI_CustomsSecondUnitQty, (x) => x.US_SupUQ2, null, null, null, null, null);
		}

		public ZDecimal GetThirdCustomsQuantity()
		{
			return GetEffectiveCustomsQuantity(() => SumupLineTotalsForEntry<IInvoiceLine>((x) => x.JI_CustomsThirdQuantity), (x) => x.US_SupQty3, null, null, null, null, null);
		}

		public ZString GetThirdCustomsUnitQty()
		{
			return GetEffectiveCustomsUQ((x) => x.JI_CustomsThirdUnitQty, (x) => x.US_SupUQ3, null, null, null, null, null);
		}

		ZDecimal GetEffectiveCustomsQuantity(Func<ZDecimal> getCustomsValueForRegularTariff, Func<IInvoiceLine, ZDecimal> getCustomsValueForSupTariff, Func<IInvoiceLine, ZDecimal> getCustomsValueForSupAdditionalTariff1
			, Func<IInvoiceLine, ZDecimal> getCustomsValueForSupAdditionalTariff2, Func<IInvoiceLine, ZDecimal> getCustomsValueForSupAdditionalTariff3, Func<IInvoiceLine, ZDecimal> getCustomsValueForSupAdditionalTariff4, Func<IInvoiceLine, ZDecimal> getCustomsValueForSupAdditionalTariff5)
		{
			var result = ZDecimal.Zero;
			if (entryLine.US_SupAdditionalLine5)
			{
				if (getCustomsValueForSupAdditionalTariff5 != null)
				{
					result = SumupLineTotalsForEntry<IInvoiceLine>(x => getCustomsValueForSupAdditionalTariff5(x));
				}
			}
			else if (entryLine.US_SupAdditionalLine4)
			{
				if (getCustomsValueForSupAdditionalTariff4 != null)
				{
					result = SumupLineTotalsForEntry<IInvoiceLine>(x => getCustomsValueForSupAdditionalTariff4(x));
				}
			}
			else if (entryLine.US_SupAdditionalLine3)
			{
				if (getCustomsValueForSupAdditionalTariff3 != null)
				{
					result = SumupLineTotalsForEntry<IInvoiceLine>(x => getCustomsValueForSupAdditionalTariff3(x));
				}
			}
			else if (entryLine.US_SupAdditionalLine2)
			{
				if (getCustomsValueForSupAdditionalTariff2 != null)
				{
					result = SumupLineTotalsForEntry<IInvoiceLine>(x => getCustomsValueForSupAdditionalTariff2(x));
				}
			}
			else if (entryLine.US_SupAdditionalLine)
			{
				if (getCustomsValueForSupAdditionalTariff1 != null)
				{
					result = SumupLineTotalsForEntry<IInvoiceLine>(x => getCustomsValueForSupAdditionalTariff1(x));
				}
			}
			else if (entryLine.US_SupLine)
			{
				if (getCustomsValueForSupTariff != null)
				{
					result = SumupLineTotalsForEntry<IInvoiceLine>(x => getCustomsValueForSupTariff(x));
				}
			}
			else if (getCustomsValueForRegularTariff != null)
			{
				result = getCustomsValueForRegularTariff();
			}

			return result;
		}

		ZString GetEffectiveCustomsUQ(Func<IInvoiceLine, ZString> getCustomsUQForRegularTariff, Func<IInvoiceLine, ZString> getCustomsUQForSupTariff, Func<IInvoiceLine, ZString> getCustomsUQForSupAdditionalTariff1
			, Func<IInvoiceLine, ZString> getCustomsUQForSupAdditionalTariff2, Func<IInvoiceLine, ZString> getCustomsUQForSupAdditionalTariff3, Func<IInvoiceLine, ZString> getCustomsUQForSupAdditionalTariff4, Func<IInvoiceLine, ZString> getCustomsUQForSupAdditionalTariff5)
		{
			var result = ZString.Empty;
			if (entryLine.RandomLine is IInvoiceLine invoiceLine)
			{
				if (entryLine.US_SupAdditionalLine5)
				{
					if (getCustomsUQForSupAdditionalTariff5 != null)
					{
						result = getCustomsUQForSupAdditionalTariff5(invoiceLine);
					}
				}
				else if (entryLine.US_SupAdditionalLine4)
				{
					if (getCustomsUQForSupAdditionalTariff4 != null)
					{
						result = getCustomsUQForSupAdditionalTariff4(invoiceLine);
					}
				}
				else if (entryLine.US_SupAdditionalLine3)
				{
					if (getCustomsUQForSupAdditionalTariff3 != null)
					{
						result = getCustomsUQForSupAdditionalTariff3(invoiceLine);
					}
				}
				else if (entryLine.US_SupAdditionalLine2)
				{
					if (getCustomsUQForSupAdditionalTariff2 != null)
					{
						result = getCustomsUQForSupAdditionalTariff2(invoiceLine);
					}
				}
				else if (entryLine.US_SupAdditionalLine)
				{
					if (getCustomsUQForSupAdditionalTariff1 != null)
					{
						result = getCustomsUQForSupAdditionalTariff1(invoiceLine);
					}
				}
				else if (entryLine.US_SupLine)
				{
					if (getCustomsUQForSupTariff != null)
					{
						result = getCustomsUQForSupTariff(invoiceLine);
					}
				}
				else if (getCustomsUQForRegularTariff != null)
				{
					result = getCustomsUQForRegularTariff(invoiceLine);
				}
			}

			return result;
		}

		public ZDecimal GetRoundedCustomsValue()
		{
			var result = ZDecimal.Zero;
			var invoiceHeader = entryLine.RandomLine?.InvoiceHeader;
			if (invoiceHeader != null && invoiceHeader.JZ_IncoTerm == Core.Constants.IncoTerms.DeliveredDutyPaid && entryLine.IsACE && !entryLine.IsConsumptionFTZ)
			{
				var supLine = entryLine.US_SupLine;
				var zeroSetXLine = !entryLine.IsCustomsChargeToBeCalculated;
				foreach (var line in entryLine.InvoiceLines)
				{
					result += GetCustomsValueFromInvoiceLineAddInfo(line, zeroSetXLine, supLine);
				}
			}
			else
			{
				result = entryLine.CL_CustomsValue;
			}

			return result.Round(0);
		}

		ZDecimal GetCustomsValueFromInvoiceLineAddInfo(IInvoiceLine invoiceLine, bool zeroSetXLine, bool isSupLine)
		{
			return zeroSetXLine && invoiceLine.IsSetXLine ? ZDecimal.Zero : CustomsValueDeciderForInvoiceLine.GetCustomsValue(invoiceLine, isSupLine, false, true);
		}

		public ZDecimal SumupLineTotalsForEntry<T>(Func<T, ZDecimal> valueGetter)
			where T : IInvoiceLine
		{
			var result = ZDecimal.Zero;
			foreach (var invoiceLine in entryLine.InvoiceLines.OfType<T>())
			{
				if (MatchesTariff(invoiceLine))
				{
					result += valueGetter(invoiceLine);
					if (invoiceLine.IsVParentLine)
					{
						result += invoiceLine.ChildVLines.OfType<T>().Sum(x => valueGetter(x));
					}
				}
			}
			return result;
		}
	}
}
