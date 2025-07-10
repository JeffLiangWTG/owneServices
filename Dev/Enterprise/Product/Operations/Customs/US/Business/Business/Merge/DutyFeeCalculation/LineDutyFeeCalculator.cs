using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// If you change duty calculate logic in this class, please ask Kevin Or Ian to do code review or sanity check.
	/// </summary>
	class LineDutyFeeCalculator : ILineDutyFeeCalculator
	{
		public void Calculate(IEntryLineOrInvoiceLineDutyData line, FeeCalculator feeCalculator)
		{
			CalculateNormalDuty(line);
			CalculateAntiDumpingAndCountervailingDuty(line);

			foreach (IFeeCalculationDataProvider feeDataProvider in line.FeeDataProviders)
			{
				feeCalculator.Calculate(feeDataProvider);
			}
		}

		void CalculateAntiDumpingAndCountervailingDuty(IEntryLineOrInvoiceLineDutyData line)
		{
			if (!line.IsCombinedLine() || line.IsNormalTariffLine() || (line.SupTariffs.Count > 1 && (line.IsSetXLine || line.IsSetVLine)))
			{
				FeeCalculationInternalData feeCalculationInternalData;

				var addDuty = new ADD_CVDCalculator().CalculateADDDuty(line, line.GetADDAdjustValueForCombineLines(), out feeCalculationInternalData);
				line.SetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.AntidumpingDuty, addDuty.Round(2), feeCalculationInternalData);

				var cvdDuty = new ADD_CVDCalculator().CalculateCVDDuty(line, line.GetCVDAdjustValueForCombineLines(), out feeCalculationInternalData);
				line.SetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.CountervailingDuty, cvdDuty.Round(2), feeCalculationInternalData);
			}
		}

		public class ADD_CVDCalculator
		{
			ZDecimal addDuty = ZDecimal.Zero;
			ZDecimal cvdDuty = ZDecimal.Zero;
			public ZDecimal CalculateADDDuty(IDutyData line, ZDecimal adjuestValue, out FeeCalculationInternalData feeCalculationInternalData)
			{
				feeCalculationInternalData = new FeeCalculationInternalData();
				if (line.ADDutyManual.HasValue)
				{
					addDuty = line.ADDutyManual.Value;
					feeCalculationInternalData.NoneCustomsValueAmount = addDuty;
				}
				else
				{
					var rateType = line.ADDCaseRateTypeQualifier;
					var depositRate = line.ADDDepositRate;

					if (rateType.Contains(DepositRateIndicatorList.Codes.Specific))
					{
						addDuty = depositRate * line.ADDQuantity;
						feeCalculationInternalData.NoneCustomsValueAmount = addDuty;
					}
					else
					{
						var valueForADD = line.ValueForADD;
						if (adjuestValue > 0)
						{
							valueForADD += adjuestValue;
						}

						addDuty = depositRate * valueForADD;

						if (line.CustomsValue == valueForADD)
						{
							feeCalculationInternalData.PercentOfRate = depositRate * 100;
						}
						else
						{
							feeCalculationInternalData.NoneCustomsValueAmount = addDuty;
						}
					}
				}
				return addDuty;
			}

			public ZDecimal CalculateCVDDuty(IDutyData line, ZDecimal adjuestValue, out FeeCalculationInternalData feeCalculationInternalData)
			{
				feeCalculationInternalData = new FeeCalculationInternalData();
				if (line.CVDutyManual.HasValue)
				{
					cvdDuty = line.CVDutyManual.Value;
					feeCalculationInternalData.NoneCustomsValueAmount = cvdDuty;
				}
				else
				{
					var rateType = line.CVDCaseRateTypeQualifier;
					var depositRate = line.CVDDepositRate;

					if (rateType.Contains(DepositRateIndicatorList.Codes.Specific))
					{
						cvdDuty = depositRate * line.CVDQuantity;
						feeCalculationInternalData.NoneCustomsValueAmount = cvdDuty;
					}
					else
					{
						var valueForCVD = line.ValueForCVD;
						if (adjuestValue > 0)
						{
							valueForCVD += adjuestValue;
						}

						cvdDuty = depositRate * valueForCVD;
						if (line.CustomsValue == valueForCVD)
						{
							feeCalculationInternalData.PercentOfRate = depositRate * 100;
						}
						else
						{
							feeCalculationInternalData.NoneCustomsValueAmount = cvdDuty;
						}
					}
				}

				return cvdDuty;
			}
		}

		public void CalculateNormalDuty(IEntryLineOrInvoiceLineDutyData line)
		{
			if (ShouldCalculateDuty(line))
			{
				CalculateDutyForLine(line);
			}
		}

		internal static bool ShouldCalculateDuty(IEntryLineOrInvoiceLineDutyData line)
		{
			bool result = true;

			if (line.IsDutyOverridden || line.IsDomesticMerchandise || Chapter98Helper.ShouldNotCalculateDutyForChapter98(line) && !line.IsSecondaryTariffLine || Chapter98Helper.HaveTIB9813TariffLine(line))
			{
				result = false;
			}
			else
			{
				IDutyData parentLine = line.ParentTariffLine;

				var parentTariff = parentLine?.ImportTariff;
				if (parentTariff != null)
				{
					if (line.IsCombinedLine())
					{
						var none98Line = line.GetCombineLines().FirstOrDefault(x => Chapter98Helper.Is98Tariff(x.Tariff)) == null;
						if (!none98Line)
						{
							if ((line.Tariff.IsEmpty || line.IsNormalTariffLine()))
							{
								result = false;
							}
							else if (Chapter98Helper.Is99SecondaryTariffLine(line))
							{
								if (Chapter98Helper.ShouldNotCalculateDutyForChapter98(line))
								{
									result = false;
								}
							}
							else if (Chapter98Helper.Is98SecondaryTariffLine(line))
							{
								result = Chapter98Helper.ShouldCalculateDutyOnChapter98ChildLine(line);
							}
						}

						var lineSupTariffs = line.SupTariffs;
						if (result && line.IsNormalTariffLine())
						{
							foreach (var lineSupTariff in lineSupTariffs)
							{
								var supTariff = new USCTariff.Loader(line.Factory).LoadBestMatch(lineSupTariff, line.DateForDutyCalculation);
								if (supTariff != null && supTariff.Applies(TariffRuleList.Codes.InLieuTariffs, line.DateForDutyCalculation))
								{
									result = false;
								}
							}
						}
					}
					else
					{
						var parentTariffCode = parentLine.Tariff;
						var dateForDutyCalculation = parentLine.DateForDutyCalculation;

						var derivedSetsNormalLine = CalculateDutyForSetsHelper.GetDerivedSetsNormalLine(line);
						if (derivedSetsNormalLine != null)
						{
							if (line.HasBothSupTariffAndNormalTariff())
							{
								return true;
							}
							else
							{
								parentTariff = derivedSetsNormalLine.ImportTariff;
								parentTariffCode = derivedSetsNormalLine.Tariff;
								dateForDutyCalculation = derivedSetsNormalLine.DateForDutyCalculation;
							}
						}

						if (line.IsSupTariffLine() && parentTariff.Applies(TariffRuleList.Codes.RepairTariffs, line.DateForDutyCalculation))
						{
							result = Chapter98Helper.IsDutyComputationCodeValid(line.ImportTariff);
						}
						else
						{
							var conditionOnParentLine = Chapter98Helper.ShouldCalculateDutyOnParentLine(parentTariff, parentTariffCode, dateForDutyCalculation);
							if (!conditionOnParentLine)
							{
								result = false;
							}
							else
							{
								result = Chapter98Helper.IsDutyComputationCodeValid(!line.SupTariffs.Contains(parentTariffCode) ? parentTariff : line.ImportTariff);
							}
						}
					}
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CalculateDutyForLine(IEntryLineOrInvoiceLineDutyData lineDutyData)
		{
			IEntryLineOrInvoiceLineDutyData line = lineDutyData;

			if (line.IsCombinedLine())
			{
				var normalTariffValueLine = line.GetCombineLines().FirstOrDefault(x => x.IsNormalTariffLine());
				if (normalTariffValueLine != null)
				{
					if (Chapter98Helper.Is99Tariff(line.Tariff) && !CalculateDutyForSetsHelper.HasSTNTariffInSetsWith9903(line))
					{
						if (normalTariffValueLine != null)
						{
							line = new LineDutyData(lineDutyData, normalTariffValueLine.CustomsValue);
						}
					}

					var chapter98TariffLine = line.GetCombineLines().FirstOrDefault(x => Chapter98Helper.Is98Tariff(x.Tariff));
					if (chapter98TariffLine != null)
					{
						var goodsValueOf98Tariff = chapter98TariffLine.CustomsValue;
						if (Chapter98Helper.ShouldCombineChildDutyForChapter98(line, chapter98TariffLine))
						{
							line = new LineDutyData(lineDutyData, normalTariffValueLine.CustomsValue + goodsValueOf98Tariff);
						}
					}
				}
			}
			else if (!line.IsDutyFreeSPIClaimed)
			{
				IDutyData parentDutyData = lineDutyData.ParentTariffLine;

				if (parentDutyData != null
					&& parentDutyData.ImportTariff != null
					&& parentDutyData.ImportTariff.Applies(TariffRuleList.Codes.AdditionalTariffs, line.DateForDutyCalculation))
				{
					line = new LineDutyData(lineDutyData, parentDutyData.CustomsValue);
				}
			}

			USCTariff importTariff = line.ImportTariff;
			if (importTariff != null)
			{
				if (importTariff.UE_DutyComputationCode == ComputationCodeList.Codes.Derived)
				{
					CalculateForDerived(line);
				}
				else if (importTariff.Applies(TariffRuleList.Codes.InLieuTariffs, line.DateForDutyCalculation))
				{
					CalculateForInLieu(line);
				}
				else if (importTariff.UE_DutyComputationCode == ComputationCodeList.Codes.NoComputationFormulaAvailable)
				{
					if (line.IsCombinedLine() && line.SupTariffs.Any(supTariff => Chapter98Helper.Is98Tariff(supTariff)))
					{
						var customsValueLine = line.GetCombineLines().FirstOrDefault(x => x.IsNormalTariffLine());
						if (customsValueLine != null)
						{
							line = new LineDutyData(lineDutyData, customsValueLine.CustomsValue);
						}
					}

					if (importTariff.Applies(TariffRuleList.Codes.RepairTariffs, line.DateForDutyCalculation))
					{
						CalculateForRepair(line);
					}
					else if (importTariff.Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, line.DateForDutyCalculation))
					{
						CalculateForUSProductsAssembly(line);
					}
				}
				else if (CalculateDutyForEmbroidery.IsParentLineEmbroideryTariff(line))
				{
					CalculateForEmbroidery(line);
				}
				else
				{
					IDutyData parentDutyData = lineDutyData.ParentTariffLine;
					if (!line.IsCombinedLine() && parentDutyData != null && parentDutyData.ImportTariff != null && parentDutyData.ImportTariff.UE_DutyComputationCode == ComputationCodeList.Codes.NoComputationFormulaAvailable)
					{
						if (CalculateDutyForSetsHelper.HasSTNTariffInSetsWith9903(line) && CalculateDutyForSetsHelper.ShouldCalculateProvDutyForTariffRuleSTN(line))
						{
							line = new LineDutyData(lineDutyData, CalculateDutyForSetsHelper.GetTotalSetsValues(line));
						}
						else
						{
							line = new LineDutyData(lineDutyData, lineDutyData.CustomsValue + parentDutyData.CustomsValue);
						}
					}
					else if (CalculateDutyForSetsHelper.IsCombinedXLine(line))
					{
						line = new LineDutyData(lineDutyData, CalculateDutyForSetsHelper.GetSetsCustomsValueForXVLine(line));
					}
					else
					{
						var hasDerivedSetsNormalLine = CalculateDutyForSetsHelper.GetDerivedSetsNormalLine(line) != null;
						if (hasDerivedSetsNormalLine)
						{
							line = new LineDutyData(lineDutyData, CalculateDutyForSetsHelper.GetTotalDerivedSetsValues(line));
						}
						else
						{
							if (CalculateDutyForSetsHelper.HasSTNTariffInSetsWith9903(line))
							{
								if (CalculateDutyForSetsHelper.ShouldCalculateProvDutyForTariffRuleSTN(line))
								{
									line = new LineDutyData(lineDutyData, CalculateDutyForSetsHelper.GetTotalSetsValues(line));
								}
								else
								{
									var parentInvoiceLine = line.CombineParentLine;
									if (parentInvoiceLine != null)
									{
										line = new LineDutyData(lineDutyData, lineDutyData.CustomsValue);
									}
									else if (parentDutyData != null)
									{
										line = new LineDutyData(lineDutyData, parentDutyData.CustomsValue);
									}
								}
							}
						}
					}

					var calculator = new AppendixFDutyCalculator(line, line.Factory);

					StoreDutyResult(line, calculator.DutyResult);
				}
			}
		}

		void CalculateForDerived(IEntryLineOrInvoiceLineDutyData line)
		{
			IDutyResult result = new DerivedDutyCalculator().Calculate(line);
			StoreDutyResult(line, result);
		}

		void CalculateForRepair(IEntryLineOrInvoiceLineDutyData line)
		{
			var lines = ReArrangeLines(line);
			new RepairsDutyCalculator(lines, line.Tariff).Execute();
		}

		void CalculateForEmbroidery(IEntryLineOrInvoiceLineDutyData line)
		{
			var result = new EmbroideryDutyCalculator().Calculate(line);
			StoreDutyResult(line, result);
		}

		List<IEntryLineOrInvoiceLineDutyData> ReArrangeLines(IEntryLineOrInvoiceLineDutyData line)
		{
			var lines = new List<IEntryLineOrInvoiceLineDutyData>();
			lines.Add(line);
			if (line.IsCombinedLine() && line.SupTariffs.Any(supTariff => Chapter98Helper.Is98Tariff(supTariff)))
			{
				var customsValueLine = line.GetCombineLines().FirstOrDefault(x => x.IsNormalTariffLine());
				if (customsValueLine != null)
				{
					lines.Add(customsValueLine);
				}
			}
			else
			{
				lines.AddRange(line.SecondaryLines);
			}
			return lines;
		}

		void CalculateForUSProductsAssembly(IEntryLineOrInvoiceLineDutyData line)
		{
			var lines = ReArrangeLines(line);
			new USProductsAssemblyDutyCalculator(lines, line.Tariff).Execute();
		}

		void CalculateForInLieu(IEntryLineOrInvoiceLineDutyData line)
		{
			var dutyData = new DutyDataProxy(line);
			dutyData.CustomsValue = line.TotalCustomsValueIncludingSecondaryLines;

			var calculator = new AppendixFDutyCalculator(dutyData, line.Factory);
			StoreDutyResult(line, calculator.DutyResult);
		}

		void StoreDutyResult(IEntryLineOrInvoiceLineDutyData line, IDutyResult dutyResult)
		{
			if (!InvalidDutyRate.IsInvalid(dutyResult.PercentOfValue))
			{
				line.SetDutyResult(dutyResult);
			}
		}
	}

	static class InvalidDutyRate
	{
		const decimal specialDontDeclareDutyValue = 999999m;
		const decimal specialDontDeclareDutyPerUnitAmount = 9999m;

		public static bool IsInvalid(ZDecimal value)
		{
			ZDecimal truncatedValue = value.Truncate();
			return truncatedValue == specialDontDeclareDutyValue || truncatedValue == specialDontDeclareDutyPerUnitAmount;
		}
	}
}
