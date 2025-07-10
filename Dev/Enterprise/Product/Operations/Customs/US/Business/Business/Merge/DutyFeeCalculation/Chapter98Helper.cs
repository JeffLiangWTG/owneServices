using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public static class Chapter98Helper
	{
		public static bool IsCombineSecondaryTariffLine(JobComInvoiceLine line)
		{
			var result = false;
			var parentLine = line.ParentTariffLine;

			if (parentLine != null && (line.IsSecondaryTariffLine || (parentLine.IsSetVLine && line.IsVChildLine)))
			{
				var combineLines = parentLine.ChildLines.ToList();
				combineLines.Add(parentLine);
				result = combineLines.Count(x => !x.JI_Tariff.IsEmpty) == 1 && combineLines.Count(x => Is98Tariff(x.US_SupTariff)) <= 1 && combineLines.All(x => Is98Tariff(x.US_SupTariff) || Is99Tariff(x.US_SupTariff));
			}
			return result;
		}

		public static bool Is98Tariff(ZString tariffNumber)
		{
			return tariffNumber.StartsWith("98", System.StringComparison.OrdinalIgnoreCase);
		}

		public static bool Is99Tariff(ZString tariffNumber)
		{
			return tariffNumber.StartsWith("99", System.StringComparison.OrdinalIgnoreCase);
		}

		public static bool Is99SecondaryTariffLine(IDutyData line)
		{
			return line.SupTariffs.Any(supTariff => Is99Tariff(supTariff)) && line.IsCombineSecondaryTariffLine;
		}

		public static bool Is98SecondaryTariffLine(IDutyData line)
		{
			return line.SupTariffs.Any(supTariff => Is98Tariff(supTariff)) && line.IsCombineSecondaryTariffLine;
		}

		public static IDutyData GetCombineParentLine(IDutyData line)
		{
			IDutyData result = null;

			var parentLine = line.CombineParentLine;
			if (parentLine == null && line.CombineChildLines != null && line.CombineChildLines.Any(x => x.IsCombineSecondaryTariffLine))
			{
				result = line;
			}
			else if (parentLine != null && parentLine.IsSetXLine && line.IsSetVLine && line.CombineChildLines != null && line.CombineChildLines.Any(x => x.IsCombineSecondaryTariffLine))
			{
				result = line;
			}
			else if (line.IsCombineSecondaryTariffLine || (parentLine != null && line.CombineChildLines != null && line.CombineChildLines.Any(x => x.IsCombineSecondaryTariffLine)))
			{
				result = parentLine;
			}

			return result;
		}

		#region Exempt MPF

		/// <summary>
		/// Please don't refactor this method, keep the logic clear.
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		public static bool IsExemptMPFForCombineLines(IDutyData line)
		{
			var result = false;
			if (line.IsCombinedLine())
			{
				result = true;

				if (!line.CombineAllLines.Any(x => Is98Tariff(x.Tariff)))
				{
					var conditionChecker = new MPFAndInformalFeeExemptConditionChecker(false);
					if (line.CombineAllLines.Any(x => conditionChecker.Is99TariffsWithExemption(x.Tariff, x.DateForDutyCalculation, x.Factory)))
					{
						result = true;
					}
					else
					{
						result = false;
					}
				}
				else if (!line.IsNormalTariffLine())
				{
					result = true;
				}
				else
				{
					if (line.CombineAllLines.Any(x => x.SupTariffs.Any(supTariff => ShouldNotExemptMPFFor98(supTariff))))
					{
						result = false;
					}
					else
					{
						result = true;
					}
				}
			}

			return result;
		}

		public static bool ShouldNotExemptMPFFor98(ZString tariff)
		{
			return tariff.StartsWith("98020060", System.StringComparison.OrdinalIgnoreCase) || tariff.StartsWith("98020080", System.StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		#region Calculate Prov/Prog Duty

		public static bool ShouldCalculate990388DutyForSection301(IEntryLineOrInvoiceLineDutyData line)
		{
			return ShouldCalculateDutyOnChapter98ChildLine(line);
		}

		public static bool ShouldNotCalculateDutyForChapter98(IEntryLineOrInvoiceLineDutyData line)
		{
			bool? shouldCalculateDutyOnChapter98ChildLine = null;
			var chapter98TariffLine = line.GetCombineLines().FirstOrDefault(x => Is98Tariff(x.Tariff));
			var calculateDutyOn99LineDependOn98Line = chapter98TariffLine != null && (ShouldCalculate990388DutyForSection301(chapter98TariffLine) || ShouldCalculateDutyOnChapter98ChildLineReused());
			var none98Line = line.GetCombineLines().FirstOrDefault(x => Is98Tariff(x.Tariff)) == null;
			var supTariff98 = line.SupTariffs.FirstOrDefault(x => Is98Tariff(x));

			return line.IsCombinedLine() && !calculateDutyOn99LineDependOn98Line && (
				(!none98Line && TariffValidator.IsProvOrProgDutyNoNeedForSection301(line))
				|| Is98Tariff(supTariff98) && !TariffValidator.NeedsFullValueToCalculateProvDutyFor232(supTariff98) && !ShouldCalculateDutyOnChapter98ChildLineReused());

			bool ShouldCalculateDutyOnChapter98ChildLineReused() => shouldCalculateDutyOnChapter98ChildLine ??= ShouldCalculateDutyOnChapter98ChildLine(line);
		}

		public static bool ShouldCombineChildDutyForChapter98(IEntryLineOrInvoiceLineDutyData line, IEntryLineOrInvoiceLineDutyData chapter98Line)
		{
			return chapter98Line != null && ShouldCombineDuty(line, chapter98Line);
		}

		static bool ShouldCombineDuty(IDutyData line, IDutyData combine98Line)
		{
			return line != null && line.SupTariffs.Any(supTariff => TariffValidator.IsSection232Relevant(line.Factory, supTariff, line.DateForDutyCalculation)) && combine98Line != null && combine98Line.SupTariffs.Any(supTariff => TariffValidator.NeedsFullValueToCalculateProvDutyFor232(supTariff));
		}

		#endregion

		#region regular duty

		public static bool ShouldCalculateDutyOnParentLine(USCTariff importTariff, ZString tariff, ZDateTime effectiveDate)
		{
			return !(importTariff != null && !importTariff.Applies(TariffRuleList.Codes.AdditionalTariffs, effectiveDate)
				&& (tariff.StartsWith("98", System.StringComparison.OrdinalIgnoreCase) || importTariff.Applies(TariffRuleList.Codes.InLieuTariffs, effectiveDate)));
		}

		public static bool IsDutyComputationCodeValid(USCTariff importTariff)
		{
			return importTariff != null && importTariff.UE_DutyComputationCode != ComputationCodeList.Codes.Derived && importTariff.UE_DutyComputationCode != ComputationCodeList.Codes.Free;
		}

		public static bool ShouldCalculateDutyOnChapter98ChildLine(IDutyData line)
		{
			var result = USRefTariffDataLoader.TariffViewHasRuleWithAttribute(line.Factory, line.Tariff, line.DateForDutyCalculation, UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232) != null;
			if (!result && Is98Tariff(line.Tariff) && line.ImportTariff is USCTariff importTariff)
			{
				result = importTariff.Applies(TariffRuleList.Codes.EligibleForAGOATextileClaims, line.DateForDutyCalculation) || importTariff.Applies(TariffRuleList.Codes.HaitiTariffHope, line.DateForDutyCalculation);
			}
			if (!result)
			{
				foreach (var supTariff in line.SupTariffs)
				{
					var tariff = new USCTariff.Loader(line.Factory).LoadBestMatch(supTariff, line.DateForDutyCalculation);
					if (tariff != null && tariff.UE_DutyComputationCode == ComputationCodeList.Codes.NoComputationFormulaAvailable && (tariff.Applies(TariffRuleList.Codes.RepairTariffs, line.DateForDutyCalculation) || tariff.Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, line.DateForDutyCalculation)))
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region TIB 9813

		public static bool HaveTIB9813TariffLine(IEntryLineOrInvoiceLineDutyData line)
		{
			var result = false;
			if (line != null && line.EntryType == EntryTypeList.Codes.TemporaryImportationBond && line.IsCombinedLine())
			{
				result = line.GetCombineLines().Any(x => x.SupTariffs.Any(supTariff => supTariff.StartsWith("9813", System.StringComparison.OrdinalIgnoreCase)));
			}
			return result;
		}

		public static IDutyResult CalculateTIBDutyForPrint(CusEntryLine line)
		{
			IDutyResult result = null;
			var dutyData = new EntryLineIEntryLineOrInvoiceLineDutyData(line) as IEntryLineOrInvoiceLineDutyData;

			if (dutyData.SupTariffs.Any(supTariff => !Is98Tariff(supTariff)))
			{
				var normalLine = dutyData.GetCombineLines().FirstOrDefault(x => x.IsNormalTariffLine());
				if (normalLine != null)
				{
					var adjuestLine = new LineDutyData(dutyData, normalLine.CustomsValue);
					var calculator = AppendixFDutyCalculator.NewWithCombinedCustomsValue(adjuestLine, null);
					result = calculator.DutyResult;
				}
			}
			return result;
		}

		#endregion
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public static class CalculateDutyForSetsHelper
	{
		public static bool ShouldCalculateProvDutyForTariffRuleSTN(IEntryLineOrInvoiceLineDutyData line)
		{
			return line != null && line.SupTariffs.Any(supTariff => Is9903Tariff(supTariff));
		}

		public static bool HasSTNTariffInSetsWith9903(IEntryLineOrInvoiceLineDutyData line)
		{
			var result = false;
			var parentLine = line.ParentLine ?? line;

			foreach (var secondaryLine in parentLine.SecondaryLines)
			{
				if (!secondaryLine.Tariff.IsEmpty && secondaryLine.SupTariffs.Count == parentLine.SupTariffs.Count && !secondaryLine.SupTariffs.Except(parentLine.SupTariffs).Any())
				{
					result = IsSTNTariff(secondaryLine.Tariff, secondaryLine.DateForDutyCalculation, secondaryLine.Factory);
					if (result)
					{
						break;
					}
				}
			}
			return result && parentLine != null && parentLine.SupTariffs.Any(supTariff => Is9903Tariff(supTariff));
		}

		static bool IsSTNTariff(ZString tariffCode, ZDateTime dateForDutyCalculation, BusinessObjectFactory factory)
		{
			var result = false;
			if (!tariffCode.IsEmpty)
			{
				var tariff = new USCTariff.Loader(factory).LoadBestMatch(tariffCode, dateForDutyCalculation);
				result = IsSTNTariff(tariff, dateForDutyCalculation);
			}
			return result;
		}

		public static bool IsSTNTariff(USCTariff tariff, ZDateTime dateForDutyCalculation)
		{
			return tariff != null && tariff.GetTariffRuleIfApplies(TariffRuleList.Codes.EligibleForSecondaryTariffNumbers, dateForDutyCalculation) != null;
		}

		public static IEntryLineOrInvoiceLineDutyData GetDerivedSetsNormalLine(IEntryLineOrInvoiceLineDutyData line)
		{
			IEntryLineOrInvoiceLineDutyData result = null;
			var parentLine = line.ParentLine ?? line;

			if (parentLine.SupTariffs.Any(supTariff => Is9903Tariff(supTariff)) && parentLine.SecondaryLines.Count() > 1)
			{
				var normalTariffLines = parentLine.SecondaryLines.Where(x => x.HasBothSupTariffAndNormalTariff() && !Is9903Tariff(x.Tariff));
				var normalTariffLine = normalTariffLines.Count() == 1 ? normalTariffLines.FirstOrDefault() : null;
				if (normalTariffLine != null)
				{
					var tariff = new USCTariff.Loader(line.Factory).LoadBestMatch(normalTariffLine.Tariff, line.DateForDutyCalculation);
					if (tariff != null && tariff.UE_DutyComputationCode == ComputationCodeList.Codes.Derived)
					{
						result = normalTariffLine;
					}
				}
			}
			return result;
		}

		public static ZDecimal GetTotalSetsValues(IEntryLineOrInvoiceLineDutyData line)
		{
			var result = ZDecimal.Zero;
			var parentLine = line.ParentLine ?? line;
			if (line.IsSupTariffLine())
			{
				if (!Chapter98Helper.Is98Tariff(parentLine.Tariff) || Chapter98Helper.ShouldCombineChildDutyForChapter98(line, parentLine))
				{
					result = GetTotalDerivedSetsValues(line) + parentLine.CustomsValue;
				}
				else
				{
					result = GetTotalDerivedSetsValues(line);
				}
			}
			else
			{
				result = parentLine.CustomsValue;
			}

			return result;
		}

		public static ZDecimal GetTotalDerivedSetsValues(IEntryLineOrInvoiceLineDutyData line)
		{
			var parentLine = line.ParentLine ?? line;
			return parentLine.SecondaryLines.Sum(x => x.CustomsValue);
		}

		public static bool IsCombinedXLine(IDutyData line)
		{
			return line != null && line.IsSetXLine && line.SupTariffs.Any(supTariff => Is9903Tariff(supTariff));
		}

		public static bool Is9903Tariff(ZString tariff)
		{
			return tariff.StartsWith("9903", System.StringComparison.OrdinalIgnoreCase);
		}

		public static ZDecimal GetSetsCustomsValueForXVLine(IEntryLineOrInvoiceLineDutyData line)
		{
			var result = ZDecimal.Zero;
			if (line != null)
			{
				if (line.IsRecon)
				{
					result = line.CustomsValue;
				}
				else
				{
					var parentLine = line.ParentLine ?? line;
					var childLines = parentLine.ChildLines;
					if (childLines != null)
					{
						foreach (var childLine in parentLine.ChildLines)
						{
							if (!childLine.IsSecondaryTariffLine)
							{
								result += childLine.CustomsValue;

								var xvvChildLine = childLine as IXVVLine;
								if (xvvChildLine != null && xvvChildLine.IsVParentLine)
								{
									foreach (var vChildLine in xvvChildLine.ChildVLines)
									{
										result += ((IDutyData)vChildLine).CustomsValue;
									}
								}
							}
						}
					}
					else
					{
						result = line.CustomsValue;
					}
				}
			}
			return result;
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public static class CalculateDutyForDocument
	{
		public static bool ShouldCalculateDutyOnlyForDocumnet(IEntryLineOrInvoiceLineDutyData line)
		{
			return line.IsNormalTariffLine() && line.SupTariffs.Any(supTariff => supTariff.StartsWith("98080030", System.StringComparison.OrdinalIgnoreCase));
		}

		public static IDutyResult PrinterDuty(IEntryLineOrInvoiceLineDutyData line)
		{
			var customsValue = line.CustomsValue;
			if (line.ParentTariffLine != null)
			{
				customsValue += line.ParentTariffLine.CustomsValue;
			}

			var dutyData = new DutyDataProxy(new LineDutyData(line, customsValue));
			var calculator = new AppendixFDutyCalculator(dutyData, line.Factory);
			return calculator.DutyResult;
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public static class CalculateDutyForEmbroidery
	{
		public static bool IsParentLineEmbroideryTariff(IEntryLineOrInvoiceLineDutyData line)
		{
			IEntryLineOrInvoiceLineDutyData embroideryNormalLine = null;
			var parentLine = line.ParentLine ?? line;
			if (parentLine.IsSupTariffLine())
			{
				embroideryNormalLine = parentLine.SecondaryLines.FirstOrDefault(x => x.ImportTariff != null && !x.IsSupTariffLine());
			}
			else
			{
				embroideryNormalLine = parentLine;
			}

			if (embroideryNormalLine?.ImportTariff is USCTariff importTariff && importTariff.IsEmbroideryTariff(embroideryNormalLine.DateForDutyCalculation))
			{
				return parentLine.CombineAllLines.Count(x => x.IsNormalTariffLine()) > 1;
			}

			return false;
		}
	}
}
