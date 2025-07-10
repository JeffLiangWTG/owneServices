using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	public sealed class QuotationLineListHelper
	{
		public QuotationLineListHelper()
		{
			this.lookup = new Dictionary<ZGuid, QuotationLineList>();
			this.useAlternativeRateFormat = DocumentsDataRegistry.Instance.AlternativeRateFormat.Value;
		}

		public QuotationLineList GetLines(PricingPageRateLineList set, bool viewAgentRates)
		{
			MergeEqualLines(set);

			var result = new QuotationLineList();
			if (set.Count == 0)
			{
				return result;
			}

			var rateLinesToReset = SetViewAgentRates(set, viewAgentRates);
			if (set.Count == 1 && set[0].Parent != null && set[0].Parent.Container == null)
			{
				result = GetQuotationLines(set[0], false, set.ParentRateEntry);
			}
			else
			{
				result = GetConsolidatedLines(set);
			}

			foreach (var rateLine in rateLinesToReset)
			{
				rateLine.ViewAgentRates = !viewAgentRates;
			}

			return result;
		}

		List<RateLine> SetViewAgentRates(PricingPageRateLineList lineSet, bool viewAgentRates)
		{
			var linesToReset = new List<RateLine>();

			foreach (var line in lineSet)
			{
				if (line.ViewAgentRates != viewAgentRates)
				{
					linesToReset.Add(line);
					line.ViewAgentRates = viewAgentRates;
				}
			}

			return linesToReset;
		}

		void MergeEqualLines(PricingPageRateLineList set)
		{
			set.Sort((l1, l2) =>
			{
				var value1 = l1.Calculator.EquipmentType;
				var value2 = l2.Calculator.EquipmentType;
				var result = value1.CompareTo(value2);

				if (result == 0)
				{
					var container1 = l1.Parent.Container;
					var container2 = l2.Parent.Container;

					value1 = container1 == null ? ZString.Empty : container1.RC_Code;
					value2 = container2 == null ? ZString.Empty : container2.RC_Code;
					result = value1.CompareTo(value2);

					if (result == 0)
					{
						result = l1.TL_LineOrder.CompareTo(l2.TL_LineOrder);
					}
				}

				return result;
			});

			var showEquipmentType = set.ShowEquipmentType;

			for (var i = set.Count - 1; i >= 0; i--)
			{
				var equipmentType = set[i].Calculator.EquipmentType;

				for (var j = i - 1; j >= 0; j--)
				{
					if (equipmentType != set[j].Calculator.EquipmentType)
					{
						break;
					}
					else if (GetQuotationLines(set[i], showEquipmentType, set.ParentRateEntry).Equals(GetQuotationLines(set[j], showEquipmentType, set.ParentRateEntry)))
					{
						set.AddContainerType(set[j], set.GetContainerType(set[i]));
						set.RemoveAt(i);

						break;
					}
				}
			}
		}

		QuotationLineList GetConsolidatedLines(PricingPageRateLineList set)
		{
			var result = new QuotationLineList();
			var lineLists = new List<QuotationLineList>();
			var showEquipmentType = set.ShowEquipmentType;

			foreach (var line in set)
			{
				var lineList = GetQuotationLines(line, showEquipmentType, set.ParentRateEntry);

				if (lineList.Count > 0)
				{
					if (showEquipmentType && !line.Calculator.ShowEquipmentType)
					{
						lineList[0].MergeDescriptions(Res.GetString("d39dde0b-df47-45cc-96e6-d398b590abfb", "(No Equipment Specified)"));
					}

					lineLists.Add(lineList);
				}
			}

			if (lineLists.Count > 0)
			{
				var groupLinesByChargeCode = !useAlternativeRateFormat
					|| set[0].TL_WeightVolume != QuantityUnit.CN;

				if (groupLinesByChargeCode)
				{
					var type = Calculator.RateDescriptionFlags(EntryParameters(set[0].Parent));

					var pricingPageRateLineHeaders = GetPricingPageRateLineHeaders(set);
					foreach (var pricingPageRateLineHeader in pricingPageRateLineHeaders)
					{
						result.Add(QuotationLine.Header(pricingPageRateLineHeader, type, ZString.Empty));
					}
				}

				foreach (var lineList in lineLists)
				{
					foreach (var line in lineList)
					{
						if (groupLinesByChargeCode)
						{
							line.Shift(set.GetContainerType(line.Master));
						}

						result.Add(line);
					}
				}
			}

			return result;
		}

		IEnumerable<RateLine> GetPricingPageRateLineHeaders(PricingPageRateLineList pricingPageLineSet)
		{
			if (pricingPageLineSet.Count < 1)
			{
				return Enumerable.Empty<RateLine>();
			}

			var ratingHeader = pricingPageLineSet[0].Header;

			// Quotation PricingPages DocStrips doesn't not have GroupBy:
			// * Forwarding Standard Pricing
			// * Forwarding Rate Table Loose (Landscape)
			// * Forwarding Rate Table Non Loose (Landscape)
			// * Forwarding Compact Rate Table (Landscape)
			if (ratingHeader.IsQuote())
			{
				return new[] { pricingPageLineSet[0] };
			}

			// NonQuotation (ClientRate, CompanyTariff and Costing) PricingPages DocStrips:
			// * Forwarding Non-Quote Rate Table Loose(Landscape)
			// * Forwarding Non - Quote Rate Table Non - Loose(Landscape)
			// has GroupBy Rating.PageSets[ForwardingLandscapeSimpleLoose].OriginDestinationAndChargeableTableRows.RateLine.ParentEntry:
			// * Origin
			// * Destination
			// * Provider.CompanyName
			// * CommodityCode.Code
			// * ServiceLevel.Code

			var isCosting = ratingHeader.IsCosting();

			return pricingPageLineSet
				.GroupBy(rateLine => new
				{
					rateLine.Parent.TI_OriginLRC,
					rateLine.Parent.TI_DestinationLRC,
					rateLine.Parent.TI_OH_TransportProvider,
					rateLine.Parent.TI_RH_NKCommodityCode,
					serviceLevel = isCosting
						? rateLine.Parent.TI_PL_NKCarrierServiceLevel
						: rateLine.Parent.TI_RS_NKServiceLevel_NI
				})
				.Select(groupedRateLines => groupedRateLines.First());
		}

		static Calculator.GetQuotationLinesParam EntryParameters(RateEntry rateEntry)
		{
			var result = Calculator.GetQuotationLinesParam.IncludeNotes;

			if (rateEntry.IsShipping())
			{
				result |= Calculator.GetQuotationLinesParam.IncludeContractNumbers;
			}
			else if (rateEntry.IsShippingExportDetention() || rateEntry.IsShippingImportDetention())
			{
				result |= Calculator.GetQuotationLinesParam.UseChargeDescription;
			}

			return result;
		}

		QuotationLineList GetQuotationLines(RateLine rateLine, bool showEquipmentType, RateEntry parentRateEntry)
		{
			QuotationLineList result;

			if (!lookup.TryGetValue(rateLine.PK, out result))
			{
				var parameters = EntryParameters(rateLine.Parent);

				if (showEquipmentType)
				{
					parameters |= Calculator.GetQuotationLinesParam.ShowEquipmentType;
				}

				if (DocumentsDataRegistry.Instance.AlternativeRateFormat.Value)
				{
					parameters |= Calculator.GetQuotationLinesParam.AlternativeFormat;
				}

				if (!DocumentsDataRegistry.Instance.ShowContractNumbersOnShippingQuotationDocuments.Value)
				{
					parameters &= ~Calculator.GetQuotationLinesParam.IncludeContractNumbers;
				}

				if (rateLine.Calculator is CompanyTariffOrCostBasedCalculator)
				{
					throw new DeveloperNotificationException("Cost or Company Tariff based calculators should not reach this place and should be all replaced by base rate lines by this moment.");
				}

				result = rateLine.Calculator.GetQuotationLines(parameters, parentRateEntry);
				lookup.Add(rateLine.PK, result);
			}

			return result;
		}

		readonly Dictionary<ZGuid, QuotationLineList> lookup;
		readonly bool useAlternativeRateFormat;
	}
}

