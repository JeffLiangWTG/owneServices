using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class EqualizationInfo
	{
		readonly List<EqualizationRateLineInfo> equalizationsPerRateLines;

		public EqualizationInfo()
		{
			equalizationsPerRateLines = new List<EqualizationRateLineInfo>();
		}

		public EqualizationRateLineInfo GetOrAddEqualizationRateLineInfo(RateLine rateLine)
		{
			var result = equalizationsPerRateLines.FirstOrDefault(x => x.RateLinePK == rateLine.PK);
			if (result == null)
			{
				result = new EqualizationRateLineInfo(rateLine);
				equalizationsPerRateLines.Add(result);
			}
			return result;
		}

		public ZString GetEqualizationSummary()
		{
			if (equalizationsPerRateLines.Any())
			{
				ZStringBuilder sb = new ZStringBuilder();
				foreach (var eq in equalizationsPerRateLines.OrderBy(x => x.PivotValue))
				{
					sb.AppendLine(eq.GetPivotAchievementDetails());
				}
				return sb.ToString();
			}
			return ZString.Empty;
		}

		public ZString GetEqualizationDetailsPerRateLine(ZGuid rateLinePK)
		{
			if (equalizationsPerRateLines.Any() && equalizationsPerRateLines.Any(x => x.RateLinePK == rateLinePK))
			{
				return equalizationsPerRateLines.First(x => x.RateLinePK == rateLinePK).GetPivotAchievementDetails(indentLines: true);
			}
			return ZString.Empty;
		}

		public EqualizedCostInfo GetEqualizedCostInfoForConsolPerRateLine(ZGuid rateLinePK)
		{
			var eq = equalizationsPerRateLines.Single(x => x.RateLinePK == rateLinePK);
			return eq.GetCostInformationForConsol();
		}

		public bool ConsolWasEqualizedForRateLine(ZGuid rateLinePK)
		{
			var eq = equalizationsPerRateLines.FirstOrDefault(x => x.RateLinePK == rateLinePK);
			if (eq != null)
			{
				return eq.ConsolEqualizationExists();
			}
			return false;
		}
	}

	public class EqualizationRateLineInfo
	{
		readonly ZGuid rateLinePK;
		readonly ZString contractNumber;
		readonly ZString containerCode;
		readonly ZDecimal pivotValue;
		readonly ZDecimal relevantValuePlus;
		readonly ZDecimal relevantValueMinus;
		readonly ZDecimal flatAmountPlus;
		readonly ZDecimal flatAmountMinus;
		readonly bool useInclusiveBreaks;
		readonly ZString equalizationUnit;

		readonly List<ConsolEqualizationInfo> consolEqualizations = new List<ConsolEqualizationInfo>();

		public ZGuid RateLinePK { get { return rateLinePK; } }
		public ZDecimal PivotValue { get { return pivotValue; } }

		public EqualizationRateLineInfo(RateLine rateLine)
		{
			rateLinePK = rateLine.PK;
			var container = rateLine.Factory.Load<RefContainer>(rateLine.Parent.Container.PK);
			containerCode = container.RC_Code;
			contractNumber = rateLine.Parent != null ? rateLine.Parent.TI_ContractNumber : ZString.Empty;
			useInclusiveBreaks = rateLine.Calculator.UseInclusiveBreaks;
			equalizationUnit = rateLine.TL_WeightVolume;

			var pivotBreakItem = rateLine.RateLineItems.Cast<RateLineItem>().FirstOrDefault(x => x.RateOperatorIsPlus());
			var minusBreakItem = rateLine.RateLineItems.Cast<RateLineItem>().FirstOrDefault(x => x.RateOperatorIsMinus());

			if (pivotBreakItem != null)
			{
				pivotValue = pivotBreakItem.TM_Break;
				relevantValuePlus = pivotBreakItem.TM_RelevantValue;
				flatAmountPlus = pivotBreakItem.TM_FlatAmount;
			}

			if (minusBreakItem != null)
			{
				relevantValueMinus = minusBreakItem.TM_RelevantValue;
				flatAmountMinus = minusBreakItem.TM_FlatAmount;
			}
		}

		public void AddConsolEqualizationInfo(ZDecimal unitsOnConsolCount, ZDecimal containerCount)
		{
			if (!ConsolEqualizationExists())
			{
				consolEqualizations.Add(new ConsolEqualizationInfo(_Rating.LocalSession.Target.PK, _Rating.LocalSession.Target.HumanReadableShortcutName, unitsOnConsolCount, containerCount));
			}
		}

		public ZString GetPivotAchievementDetails(bool indentLines = false)
		{
			ZStringBuilder sb = new ZStringBuilder();
			var equalizationUnitDescription = GetUnitDescription();
			var pivotNotAchieved = Res.GetString("d617bb80-35ea-4953-a2f8-42e10573b966", "Pivot {0} not achieved.", equalizationUnitDescription);
			var pivotAchieved = Res.GetString("63906daa-0520-453c-b803-4bb342fa7ac0", "Pivot {0} achieved.", equalizationUnitDescription);

			ZString indentation = indentLines ? "\t\t\t" : "";
			sb.AppendLine(Res.GetString("3611846c-1ca0-4568-a535-cb9c24da8395", "Pivot {0} per {1} as per contract '{2}' is {3} {4}", equalizationUnitDescription, containerCode, contractNumber, pivotValue.ToString("G29", CultureInfo.CurrentCulture), equalizationUnit));
			foreach (var c in consolEqualizations)
			{
				sb.AppendLine(indentation + GetEqualizationString(c));
			}

			if (equalizationUnit == QuantityUnit.CN)
			{
				sb.AppendLine(indentation + Res.GetString("e49fdbbf-5142-41e0-bb62-75fb7896d149", "Total Containers: {0}", consolEqualizations.Sum(x => x.ContainerCount)));
			}
			else
			{
				sb.AppendLine(indentation + Res.GetString("6f07bfaa-3e1d-4e8e-b3be-220acf11dda6", "Average {0}: ({1})/{2} = {3} {4}", equalizationUnitDescription, string.Join(" + ", consolEqualizations.Select(x => x.UnitsOnConsolCount).ToArray()), consolEqualizations.Sum(x => x.ContainerCount), TotalAverageUnits.ToString("f2", CultureInfo.CurrentCulture), equalizationUnit));
			}

			sb.AppendLine(indentation + (IsPivotAchieved ? pivotAchieved : pivotNotAchieved));
			return sb.ToString();
		}

		public EqualizedCostInfo GetCostInformationForConsol()
		{
			var consolPK = _Rating.LocalSession.Target.PK;
			if (consolEqualizations.Any(x => x.ConsolPK == consolPK))
			{
				var currentConsolEqualization = consolEqualizations.SingleOrDefault(x => x.ConsolPK == consolPK);

				var currentConsolContainerCount = currentConsolEqualization.ContainerCount;
				var unitsOnCurrentConsol = currentConsolEqualization.UnitsOnConsolCount;

				if (equalizationUnit == QuantityUnit.CN)
				{
					unitsOnCurrentConsol = currentConsolContainerCount;
				}

				if (!IsPivotAchieved)
				{
					if (relevantValueMinus == 0)
					{
						return new EqualizedCostInfo(contractNumber, pivotValue, false, unitsOnCurrentConsol, (relevantValuePlus * pivotValue * currentConsolContainerCount + flatAmountPlus), flatAmountPlus);
					}
					else
					{
						return new EqualizedCostInfo(contractNumber, pivotValue, false, unitsOnCurrentConsol, (relevantValueMinus * unitsOnCurrentConsol + flatAmountMinus), flatAmountMinus);
					}
				}
				else
				{
					return new EqualizedCostInfo(contractNumber, pivotValue, false, unitsOnCurrentConsol, (relevantValuePlus * unitsOnCurrentConsol + flatAmountPlus), flatAmountPlus);
				}
			}
			else
			{
				return null;
			}
		}

		bool IsPivotAchieved
		{
			get
			{
				return useInclusiveBreaks ? TotalAverageUnits >= pivotValue : TotalAverageUnits > pivotValue;
			}
		}

		ZDecimal TotalAverageUnits
		{
			get
			{
				var totalUnits = consolEqualizations.Sum(x => x.UnitsOnConsolCount);
				var totalContainerCount = consolEqualizations.Sum(x => x.ContainerCount);
				if (totalContainerCount == 0)
				{
					return 0m;
				}

				return equalizationUnit == QuantityUnit.CN ? totalContainerCount : totalUnits / totalContainerCount;
			}
		}

		ZString GetUnitDescription()
		{
			switch (equalizationUnit)
			{
				case QuantityUnit.KG:
					return Res.GetString("faf6bda7-0ab9-4899-b4fb-27a6b16c5873", "Weight");

				case QuantityUnit.M3:
					return Res.GetString("3b82212f-95f4-4137-af76-d8053e4d99d0", "Volume");

				case QuantityUnit.CN:
					return Res.GetString("aa59cacd-e5ba-48fb-8c2e-1c48873e7405", "Containers");

				case QuantityUnit.TU:
					return Res.GetString("5b8a3f03-6681-429f-bef8-045ab04586d7", "TEUs");
				default:
					return ZString.Empty;
			}
		}

		ZString GetEqualizationString(ConsolEqualizationInfo equalizationInfo)
		{
			if (equalizationUnit == QuantityUnit.CN)
			{
				return Res.GetString("0cf95acd-3e78-412d-a59a-c14b1bb4ed24", "{0}: {1}x{2} Containers", equalizationInfo.ConsolNumber, equalizationInfo.ContainerCount, containerCode);
			}
			else
			{
				return Res.GetString("60be7a6b-6e3e-4399-b693-d2d3bd55aa31", "{0}: {1} {2} in {3}x{4}", equalizationInfo.ConsolNumber, equalizationInfo.UnitsOnConsolCount, equalizationUnit, equalizationInfo.ContainerCount, containerCode);
			}
		}

		public bool ConsolEqualizationExists()
		{
			return consolEqualizations.Any(x => x.ConsolPK == _Rating.LocalSession.Target.PK);
		}
	}

	public class EqualizedCostInfo
	{
		public EqualizedCostInfo(ZString contractNo, ZDecimal pivotValue, bool pivotAchieved, ZDecimal unitsOnConsol, ZDecimal cost, ZDecimal flatRate)
		{
			ContractNo = contractNo;
			PivotValue = pivotValue;
			PivotAchieved = pivotAchieved;
			UnitsOnConsol = unitsOnConsol;
			PerUnitRate = (cost - flatRate) / unitsOnConsol;
			FlatRate = flatRate;
			Cost = cost;
		}

		public ZString ContractNo { get; }
		public ZDecimal PivotValue { get; }
		public bool PivotAchieved { get; }
		public ZDecimal UnitsOnConsol { get; }
		public ZDecimal PerUnitRate { get; }
		public ZDecimal FlatRate { get; }
		public ZDecimal Cost { get; }
	}

	public class ConsolEqualizationInfo
	{
		public ConsolEqualizationInfo(ZGuid consolPK, ZString consolNumber, ZDecimal unitsOnConsolCount, ZDecimal containerCount)
		{
			this.consolPK = consolPK;
			this.consolNumber = consolNumber;
			this.unitsOnConsolCount = unitsOnConsolCount;
			this.containerCount = containerCount;
		}

		readonly ZGuid consolPK;
		readonly ZString consolNumber;
		readonly ZDecimal unitsOnConsolCount;
		readonly ZDecimal containerCount;

		public ZGuid ConsolPK => consolPK;
		public ZString ConsolNumber => consolNumber;
		public ZDecimal UnitsOnConsolCount => unitsOnConsolCount;
		public ZDecimal ContainerCount => containerCount;
	}
}
