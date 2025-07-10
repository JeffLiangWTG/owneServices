using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public static class AllocateWeightHelper
	{
		const int DecimalPlace = 3;

		public static void AllocateNetWeight(AllocateWeight allocateWeight, IEnumerable<BaseJobComInvoiceLine> baseJobComInvoiceLines)
		{
			if (allocateWeight.NetWeight > 0)
			{
				var allocateInvoiceLines = baseJobComInvoiceLines.Where(c => (allocateWeight.OverrideExisting || c.JI_NetWeight == 0) && !IsWegithZeroFromAllocateWeightMethod(allocateWeight.IsQuantity, c));
				var areInvoiceLinesSameCurrency = allocateInvoiceLines.Select(c => c.JI_RX_NKLinePriceCurr).Distinct().Count() == 1;
				var totalValueFromMethod = SumTotalValueFromMethod(allocateWeight.IsPrice, allocateInvoiceLines, areInvoiceLinesSameCurrency);
				var firstAlocateInvoiceLine = allocateInvoiceLines.ElementAt(0);

				ZDecimal totalAllocateNetWeight = ZDecimal.Zero;
				foreach (BaseJobComInvoiceLine invoiceLine in allocateInvoiceLines)
				{
					var allocateNetWeight = Utilities.Round(allocateWeight.NetWeight * GetValueFromMethod(allocateWeight.IsPrice, invoiceLine, areInvoiceLinesSameCurrency) / totalValueFromMethod, DecimalPlace);
					invoiceLine.JI_NetWeight = allocateNetWeight;
					invoiceLine.JI_NetWeightUQ = allocateWeight.NetWeightUnit;
					totalAllocateNetWeight += allocateNetWeight;
				}
				firstAlocateInvoiceLine.JI_NetWeight += allocateWeight.NetWeight - totalAllocateNetWeight;
			}
		}

		public static void AllocateGrossWeight(AllocateWeight allocateWeight, IEnumerable<BaseJobComInvoiceLine> baseJobComInvoiceLines)
		{
			if (allocateWeight.GrossWeight > 0)
			{
				var allocateInvoiceLines = baseJobComInvoiceLines.Where(c => (allocateWeight.OverrideExisting || c.JI_Weight == 0) && !IsWegithZeroFromAllocateWeightMethod(allocateWeight.IsQuantity, c));
				var areInvoiceLinesSameCurrency = allocateInvoiceLines.Select(c => c.JI_RX_NKLinePriceCurr).Distinct().Count() == 1;
				var totalValueFromMethod = SumTotalValueFromMethod(allocateWeight.IsPrice, allocateInvoiceLines, areInvoiceLinesSameCurrency);
				var firstAlocateInvoiceLine = allocateInvoiceLines.ElementAt(0);

				ZDecimal totalAllocateGrossWeight = ZDecimal.Zero;
				foreach (BaseJobComInvoiceLine invoiceLine in allocateInvoiceLines)
				{
					var allocateGrossWeight = Utilities.Round(allocateWeight.GrossWeight * GetValueFromMethod(allocateWeight.IsPrice, invoiceLine, areInvoiceLinesSameCurrency) / totalValueFromMethod, DecimalPlace);
					invoiceLine.JI_Weight = allocateGrossWeight;
					invoiceLine.JI_WeightUQ = allocateWeight.GrossWeightUnit;
					totalAllocateGrossWeight += allocateGrossWeight;
				}
				firstAlocateInvoiceLine.JI_Weight += allocateWeight.GrossWeight - totalAllocateGrossWeight;
			}
		}

		static ZDecimal SumTotalValueFromMethod(ZBool isPrice, IEnumerable<BaseJobComInvoiceLine> allocateInvoiceLines, bool areInvoiceLinesSameCurrency)
		{
			ZDecimal totalValue = ZDecimal.Zero;
			if (isPrice)
			{
				if (areInvoiceLinesSameCurrency)
				{
					totalValue = allocateInvoiceLines.Sum(c => c.JI_LinePrice);
				}
				else
				{
					var result = Money.Empty;
					allocateInvoiceLines.ForEach(c => result = c.CurrencyConverter.Add(result, c.JI_LinePriceMoney));
					totalValue = result.Amount;
				}
			}
			else
			{
				totalValue = allocateInvoiceLines.Sum(c => c.JI_InvoiceQuantity);
			}
			return totalValue;
		}

		static ZDecimal GetValueFromMethod(ZBool isPrice, BaseJobComInvoiceLine invoiceLine, bool areInvoiceLinesSameCurrency)
		{
			ZDecimal value = ZDecimal.Zero;
			if (isPrice)
			{
				if (areInvoiceLinesSameCurrency)
				{
					value = invoiceLine.JI_LinePrice;
				}
				else
				{
					var convert = invoiceLine.CurrencyConverter;
					value = convert.ConvertExact(invoiceLine.JI_LinePriceMoney, convert.LocalCurrency).Amount;
				}
			}
			else
			{
				value = invoiceLine.JI_InvoiceQuantity;
			}
			return value;
		}

		public static bool UnableAllocated(AllocateWeight allocateWeight, IEnumerable<BaseJobComInvoiceLine> invoiceLines)
		{
			var allocateNetWeightInvoiceLine = invoiceLines.Where(c => ShouldInvoiceLineNetWeightAllocated(allocateWeight, c));
			var allocateGrossWeightInvoiceLine = invoiceLines.Where(c => ShouldInvoiceLineGrossWeightAllocated(allocateWeight, c));
			var unableAllocateNetWeight = allocateNetWeightInvoiceLine.Any() && allocateNetWeightInvoiceLine.All(c => IsWegithZeroFromAllocateWeightMethod(allocateWeight.IsQuantity, c));
			var unableAllocateGrossWeight = allocateGrossWeightInvoiceLine.Any() && allocateGrossWeightInvoiceLine.All(c => IsWegithZeroFromAllocateWeightMethod(allocateWeight.IsQuantity, c));

			return unableAllocateNetWeight || unableAllocateGrossWeight;
		}

		static bool IsWegithZeroFromAllocateWeightMethod(ZBool isQuantity, BaseJobComInvoiceLine baseJobComInvoiceLine)
		{
			var result = baseJobComInvoiceLine.JI_LinePrice == 0;
			if (isQuantity)
			{
				result = baseJobComInvoiceLine.JI_InvoiceQuantity == 0;
			}
			return result;
		}

		static bool ShouldInvoiceLineNetWeightAllocated(AllocateWeight allocateWeight, BaseJobComInvoiceLine baseJobComInvoiceLine)
		{
			return allocateWeight.NetWeight > 0 && (allocateWeight.OverrideExisting || baseJobComInvoiceLine.JI_NetWeight == 0);
		}

		static bool ShouldInvoiceLineGrossWeightAllocated(AllocateWeight allocateWeight, BaseJobComInvoiceLine baseJobComInvoiceLine)
		{
			return allocateWeight.GrossWeight > 0 && (allocateWeight.OverrideExisting || baseJobComInvoiceLine.JI_Weight == 0);
		}
	}
}
