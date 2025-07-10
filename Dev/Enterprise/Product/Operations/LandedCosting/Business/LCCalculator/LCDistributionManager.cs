using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business
{
	class CustomsChargeWithHighestCost
	{
		public CustomsChargeWithHighestCost(IUltimateDistributee distributee, LandedCostHistory history, ZDecimal cost)
		{
			this.Distributee = distributee;
			this.History = history;
			this.Cost = cost;
		}

		public readonly IUltimateDistributee Distributee;
		public readonly LandedCostHistory History;
		public readonly ZDecimal Cost;
	}

	public class LCDistributionManager
	{
		public LCDistributionManager(LandedCostHeader lCHeader)
		{
			this.LCHeader = lCHeader;
			this.historiesNotUsed = new List<LandedCostHistory>();
			this.chargesWithHighestCost = new Dictionary<ZString, CustomsChargeWithHighestCost>();
		}

		readonly Dictionary<ZString, CustomsChargeWithHighestCost> chargesWithHighestCost;

		public void RunLandedCosting()
		{
			LCHeader.LT_DateOfProcessing = ZDateTime.Now;
			LCHeader.Parent.DoStuffBeforeRunningLCDistribution();
			RunLandedCostingCore();
		}

		#region Implementation

		protected internal readonly LandedCostHeader LCHeader;
		readonly List<LandedCostHistory> historiesNotUsed;

		protected virtual void RunLandedCostingCore()
		{
			historiesNotUsed.AddRange(LCHeader.Histories);

			chargesWithHighestCost.Clear();

			RunLandedCostingForChargeGroups();
			RunLandedCostingForCustomsCharges();
			RunBackRounding();

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(LCHeader.Factory))
			{
				historiesNotUsed.ForEach(x => x.Delete());
			}
		}

		protected internal void RunLandedCostingForCustomsCharges()
		{
			ILandedCostHeader parent = LCHeader.Parent;
			foreach (IUltimateDistributee ultimateDistributee in parent.UltimateDistributees)
			{
				LandedCostHistory lCHistory = LCHeader.Histories.GetOrCreate(ultimateDistributee);
				historiesNotUsed.Remove(lCHistory);
				DutyTaxEntryFee dutyTaxEntryFee = ultimateDistributee.LineDutyTaxEntryFeeItems;

				var customsChargeLCItemSettings = ((ILandedCostHistoryMaster)LCHeader).CustomsChargeLCItemSettings;
				foreach (var setting in customsChargeLCItemSettings)
				{
					if (setting.IsDuty)
					{
						if (lCHistory.ShouldCalculateDutyFromLH_DutyPercent)
						{
							lCHistory.CalculateDutyAmountIfNecessary(setting.CostType);
						}
						else
						{
							lCHistory.LH_DutyPercent = ultimateDistributee.DutyPercent;
							lCHistory.SetLineValue(dutyTaxEntryFee[setting.CostType], setting.CostType);
						}
					}
					else
					{
						lCHistory.SetLineValue(dutyTaxEntryFee[setting.CostType], setting.CostType);
					}
					MarkHighestUltimateDistributeeFor(setting.CostType, ultimateDistributee, lCHistory, lCHistory.GetLineValue(setting.CostType));
				}
			}
		}

		internal void MarkHighestUltimateDistributeeFor(ZString chargeType, IUltimateDistributee distributee, LandedCostHistory history, ZDecimal amount)
		{
			CustomsChargeWithHighestCost highest;

			if (!chargesWithHighestCost.TryGetValue(chargeType, out highest) || highest.Cost < amount)
			{
				highest = new CustomsChargeWithHighestCost(distributee, history, amount);
				chargesWithHighestCost[chargeType] = highest;
			}
		}

		protected void RunLandedCostingForChargeGroups()
		{
			Dictionary<ZGuid, ZString> distinctParentIDs = GetDistinctParentIDAndTypes();

			IComparer comparer = LCHeader.Histories.SortComparer;
			LCHeader.Histories.RemoveSort();

			foreach (var history in LCHeader.Histories)
			{
				history.LH_LandedCostGroup1 = 0m;
				history.LH_LandedCostGroup2 = 0m;
				history.LH_LandedCostGroup3 = 0m;
				history.LH_LandedCostGroup4 = 0m;
				history.LH_LandedCostGroup5 = 0m;
				history.LH_LandedCostGroup6 = 0m;
				history.LH_LandedCostGroupMisc = 0m;
			}

			foreach (ZGuid parentID in distinctParentIDs.Keys)
			{
				ILandedCostDistributeTo distributeTo = (ILandedCostDistributeTo)LCHeader.ParentLoaderForLandedCostInputDistributeTo.LoadBusinessObject(LCHeader.Factory, distinctParentIDs[parentID], parentID);
				if (distributeTo != null)
				{
					RunLandedCostingForChargeGroup(distributeTo);
				}
			}
			LCHeader.Histories.ApplySort(comparer);
		}

		protected internal void RunLandedCostingForChargeGroup(ILandedCostDistributeTo distributeTo)
		{
			LCFactors totalFactors = GetTotals(distributeTo);
			LandCostInput[] lCInputs = LCHeader.CostInputs.GetLCInputsToDistributeTo(distributeTo);

			foreach (IUltimateDistributee distributee in distributeTo.UltimateDistributees)
			{
				LandedCostHistory lCHistory = LCHeader.Histories.GetOrCreate(distributee);
				historiesNotUsed.Remove(lCHistory);

				if (!lCHistory.IsNoCostApportionmentItem)
				{
					foreach (LandCostInput lCInput in lCInputs)
					{
						DistributionFactors distributionFactor = GetRatioAccordingToDistributeBy(distributee, totalFactors, lCInput.LI_DistributeCostBy);
						DistributeCostAndWriteToHistory(lCHistory, lCInput, distributionFactor);
					}
				}
			}
		}

		protected internal struct LCFactors
		{
			public ZDecimal Actual;
			public ZDecimal WeightInKG;
			public ZDecimal VolumeInM3;
			public ZDecimal ItemCount;
			public ZDecimal CostInLocalCurrency;
		}

		protected internal struct DistributionFactors
		{
			public ZDecimal Numerator;
			public ZDecimal Denominator;
		}

		protected internal void DistributeCostAndWriteToHistory(LandedCostHistory lCHistory, LandCostInput lCInput, DistributionFactors distributionFactor)
		{
			if (distributionFactor.Denominator != 0 && distributionFactor.Numerator != 0)
			{
				var costAmount = lCHistory.LCHeader.RoundingHelper.Round(lCInput.CostAmountInLocalCurrency * distributionFactor.Numerator / distributionFactor.Denominator, lCHistory.LocalCurrencyDecimals);
				switch (lCInput.LI_LandedCostGroup)
				{
					case 1:
						lCHistory.LH_LandedCostGroup1 += costAmount;
						break;
					case 2:
						lCHistory.LH_LandedCostGroup2 += costAmount;
						break;
					case 3:
						lCHistory.LH_LandedCostGroup3 += costAmount;
						break;
					case 4:
						lCHistory.LH_LandedCostGroup4 += costAmount;
						break;
					case 5:
						lCHistory.LH_LandedCostGroup5 += costAmount;
						break;
					case 6:
						lCHistory.LH_LandedCostGroup6 += costAmount;
						break;
					default:
						lCHistory.LH_LandedCostGroupMisc += costAmount;
						break;
				}
			}
		}

		protected internal DistributionFactors GetRatioAccordingToDistributeBy(IUltimateDistributee distributee, LCFactors totals, string distributeCostBy)
		{
			DistributionFactors result = new DistributionFactors();

			switch (distributeCostBy)
			{
				case CostDistributionMechanismList.Codes.Actual:
					result.Denominator = totals.Actual;
					result.Numerator = distributee.Actual;
					break;
				case CostDistributionMechanismList.Codes.ActualVolume:
					result.Denominator = totals.VolumeInM3;
					result.Numerator = distributee.ActualVolumeInM3;
					break;
				case CostDistributionMechanismList.Codes.ActualWeight:
					result.Denominator = totals.WeightInKG;
					result.Numerator = distributee.ActualWeightInKG;
					break;
				case CostDistributionMechanismList.Codes.LineValue:
					result.Denominator = totals.CostInLocalCurrency;
					result.Numerator = distributee.CostInLocalCurrency;
					break;
				case CostDistributionMechanismList.Codes.Item:
					result.Denominator = totals.ItemCount;
					result.Numerator = distributee.ItemCount;
					break;
			}

			return result;
		}

		protected void RunBackRounding()
		{
			ZDecimal totalGroup1FromLines = 0m;
			ZDecimal totalGroup2FromLines = 0m;
			ZDecimal totalGroup3FromLines = 0m;
			ZDecimal totalGroup4FromLines = 0m;
			ZDecimal totalGroup5FromLines = 0m;
			ZDecimal totalGroup6FromLines = 0m;
			ZDecimal totalGroupMiscFromLines = 0m;
			var totalFeesFromLines = new Dictionary<ZString, ZDecimal>();
			var customsChargeLCItemSettings = ((ILandedCostHistoryMaster)LCHeader).CustomsChargeLCItemSettings;

			foreach (LandedCostHistory lCHistory in LCHeader.Histories)
			{
				totalGroup1FromLines += lCHistory.RoundedGroup1;
				totalGroup2FromLines += lCHistory.RoundedGroup2;
				totalGroup3FromLines += lCHistory.RoundedGroup3;
				totalGroup4FromLines += lCHistory.RoundedGroup4;
				totalGroup5FromLines += lCHistory.RoundedGroup5;
				totalGroup6FromLines += lCHistory.RoundedGroup6;
				totalGroupMiscFromLines += lCHistory.RoundedGroupMisc;

				foreach (var setting in customsChargeLCItemSettings)
				{
					var amount = !setting.IsDuty || !lCHistory.ShouldCalculateDutyFromLH_DutyPercent ? lCHistory.GetRoundedLineValue(setting.CostType) : ZDecimal.Zero;

					if (!totalFeesFromLines.ContainsKey(setting.CostType))
					{
						totalFeesFromLines.Add(setting.CostType, amount);
					}
					else
					{
						totalFeesFromLines[setting.CostType] += amount;
					}
				}
			}

			if (LCHeader.Parent != null)
			{
				DutyTaxEntryFee total = LCHeader.Parent.TotalDutyTaxEntryFeeItems;

				if (totalGroup1FromLines != LCHeader.TotalGroup1
				|| totalGroup2FromLines != LCHeader.TotalGroup2
				|| totalGroup3FromLines != LCHeader.TotalGroup3
				|| totalGroup4FromLines != LCHeader.TotalGroup4
				|| totalGroup5FromLines != LCHeader.TotalGroup5
				|| totalGroup6FromLines != LCHeader.TotalGroup6
				|| totalGroupMiscFromLines != LCHeader.TotalGroupMisc
				|| totalFeesFromLines.Any(x => x.Value != total[x.Key]))
				{
					var list = new List<LandedCostHistory>(LCHeader.Histories);
					list.Sort(new LCHistoryCostDescendingComparer());

					using (ActiveBusinessObjectCollection.DelayListChangedEvents(LCHeader.Histories.Factory))
					{
						AddRoundingDifferenceBackToLine(list, LCHeader.TotalGroup1, totalGroup1FromLines, LandedLineCostType.Codes.LandedCostGroup1);
						AddRoundingDifferenceBackToLine(list, LCHeader.TotalGroup2, totalGroup2FromLines, LandedLineCostType.Codes.LandedCostGroup2);
						AddRoundingDifferenceBackToLine(list, LCHeader.TotalGroup3, totalGroup3FromLines, LandedLineCostType.Codes.LandedCostGroup3);
						AddRoundingDifferenceBackToLine(list, LCHeader.TotalGroup4, totalGroup4FromLines, LandedLineCostType.Codes.LandedCostGroup4);
						AddRoundingDifferenceBackToLine(list, LCHeader.TotalGroup5, totalGroup5FromLines, LandedLineCostType.Codes.LandedCostGroup5);
						AddRoundingDifferenceBackToLine(list, LCHeader.TotalGroup6, totalGroup6FromLines, LandedLineCostType.Codes.LandedCostGroup6);
						AddRoundingDifferenceBackToLine(list, LCHeader.TotalGroupMisc, totalGroupMiscFromLines, LandedLineCostType.Codes.LandedCostGroupMisc);

						foreach (var setting in customsChargeLCItemSettings)
						{
							AddRoundingDifferenceBackToLineForCustomsCharges(total[setting.CostType], totalFeesFromLines.ContainsKey(setting.CostType) ? totalFeesFromLines[setting.CostType] : ZDecimal.Zero, setting.CostType);
						}
					}
				}
			}
		}

		protected internal void AddRoundingDifferenceBackToLine(List<LandedCostHistory> sortedHistories, ZDecimal expectedTotal, ZDecimal totalFromLines, ZString costCodeType)
		{
			ZDecimal difference = expectedTotal - totalFromLines;

			if (difference != 0 && LCHeader.Company.LocalCurrency.RX_SubUnitRatio != 0)
			{
				decimal unitAmountToDistributeToEachLine = 1m / LCHeader.Company.LocalCurrency.RX_SubUnitRatio;

				int countOfDistributeesToAdd = Math.Abs(Convert.ToInt32(difference / unitAmountToDistributeToEachLine));
				ZDecimal amountToDistributeToEachLine = difference > 0 ? unitAmountToDistributeToEachLine : unitAmountToDistributeToEachLine * -1;

				for (int index = 0; index < countOfDistributeesToAdd && index < sortedHistories.Count; index++)
				{
					var history = sortedHistories[index];
					if (!history.IsNoCostApportionmentItem)
					{
						var costItem = history.LandedLineCostItems[costCodeType];
						ZDecimal apportionedAmount = 0m;
						if (costItem != null)
						{
							apportionedAmount = costItem.LZ_CostAmount;
						}
						history.SetLineValue(apportionedAmount + amountToDistributeToEachLine, costCodeType);
					}
				}
			}
		}

		internal void AddRoundingDifferenceBackToLineForCustomsCharges(ZDecimal expectedTotal, ZDecimal totalFromLines, ZString costCodeType)
		{
			var difference = expectedTotal - totalFromLines;
			CustomsChargeWithHighestCost chargeWithHigestCost;
			if (difference != 0 && chargesWithHighestCost.TryGetValue(costCodeType, out chargeWithHigestCost))
			{
				var item = chargeWithHigestCost.History.LandedLineCostItems.GetElementWithThisCode(costCodeType);
				if (item != null)
				{
					item.LZ_CostAmount += difference;
				}
			}
		}

		protected internal LCFactors GetTotals(ILandedCostDistributeTo distributeTo)
		{
			LCFactors result = new LCFactors();

			foreach (IUltimateDistributee distributee in distributeTo.UltimateDistributees)
			{
				var lCHistory = LCHeader.Histories.GetOrCreate(distributee);
				historiesNotUsed.Remove(lCHistory);
				if (!lCHistory.IsNoCostApportionmentItem)
				{
					result.Actual += distributee.Actual;
					result.WeightInKG += distributee.ActualWeightInKG;
					result.VolumeInM3 += distributee.ActualVolumeInM3;
					result.ItemCount += distributee.ItemCount;
					result.CostInLocalCurrency += distributee.CostInLocalCurrency;
				}
			}

			return result;
		}

		internal Dictionary<ZGuid, ZString> GetDistinctParentIDAndTypes()
		{
			Dictionary<ZGuid, ZString> result = new Dictionary<ZGuid, ZString>();

			foreach (LandCostInput lCInput in LCHeader.CostInputs)
			{
				if (!result.ContainsKey(lCInput.LI_ParentID))
				{
					result.Add(lCInput.LI_ParentID, lCInput.LI_ParentTableCode);
				}
			}
			return result;
		}

		#endregion
	}
}
