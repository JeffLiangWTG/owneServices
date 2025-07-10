using System.Collections.Generic;
using System.Linq;
using Enterprise.LandedCosting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalData = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.LandedCosting.DataTransfer.Universal
{
	public class LandedCostHistoryDataObjectWriter : DataObjectWriter<LandedCostHistory, UniversalData.LandedCostDetail>
	{
		public LandedCostHistoryDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override UniversalData.LandedCostDetail PopulateDataObject(LandedCostHistory landedCostHistoryBO)
		{
			var landedCostDetailData = new UniversalData.LandedCostDetail(writeManager.WriterStrategy);
			landedCostDetailData.GoodsItemCostPerUnit = landedCostHistoryBO.UnitPriceInLocalCurrency;
			landedCostDetailData.CustomsCostPerUnit = landedCostHistoryBO.RoundedPerUnitCustomsDisbursementCharges;
			landedCostDetailData.TransportAndLogisticsCostPerUnit = landedCostHistoryBO.RoundedPerUnitLandingCost;

			landedCostDetailData.MarkUp1 = landedCostHistoryBO.EffectiveMarkUpPercentage1;
			landedCostDetailData.MarkUp2 = landedCostHistoryBO.EffectiveMarkUpPercentage2;
			landedCostDetailData.MarkUp3 = landedCostHistoryBO.EffectiveMarkUpPercentage3;

			landedCostDetailData.SetLandedLineCostItemCollection(() => PopulateLandedLineCostItems(landedCostHistoryBO.LandedLineCostItems));
			return landedCostDetailData;
		}

		List<UniversalData.LandedLineCostItem> PopulateLandedLineCostItems(LandedLineCostItemCollection landedLineCostItems)
		{
			var result = new List<UniversalData.LandedLineCostItem>(landedLineCostItems.Count);
			var costTypes = landedLineCostItems.Factory.GetCachedValue<LandedLineCostType>();
			var history = landedLineCostItems.Master;
			var lcHeader = history == null ? null : history.LCHeader;
			var landCostGroupList = lcHeader == null ? new ZArchitecture.Core.CodeDescriptionPairList() : LandCostInputLookups.BuildLandCostGroupList(lcHeader);
			var customsChargeLCItemSettings = ((ILandedCostHistoryMaster)lcHeader)?.CustomsChargeLCItemSettings;

			foreach (LandedLineCostItem landedLineCostItem in landedLineCostItems)
			{
				var type = landedLineCostItem.LZ_CostType;
				var desciption = "";
				switch (type)
				{
					case LandedLineCostType.Codes.LandedCostGroup1:
						desciption = landCostGroupList.ContainsCode("1") ? landCostGroupList.GetDescriptionFromCode("1") : costTypes.GetDescriptionFromCode(type);
						break;
					case LandedLineCostType.Codes.LandedCostGroup2:
						desciption = landCostGroupList.ContainsCode("2") ? landCostGroupList.GetDescriptionFromCode("2") : costTypes.GetDescriptionFromCode(type);
						break;
					case LandedLineCostType.Codes.LandedCostGroup3:
						desciption = landCostGroupList.ContainsCode("3") ? landCostGroupList.GetDescriptionFromCode("3") : costTypes.GetDescriptionFromCode(type);
						break;
					case LandedLineCostType.Codes.LandedCostGroup4:
						desciption = landCostGroupList.ContainsCode("4") ? landCostGroupList.GetDescriptionFromCode("4") : costTypes.GetDescriptionFromCode(type);
						break;
					case LandedLineCostType.Codes.LandedCostGroup5:
						desciption = landCostGroupList.ContainsCode("5") ? landCostGroupList.GetDescriptionFromCode("5") : costTypes.GetDescriptionFromCode(type);
						break;
					case LandedLineCostType.Codes.LandedCostGroup6:
						desciption = landCostGroupList.ContainsCode("6") ? landCostGroupList.GetDescriptionFromCode("6") : costTypes.GetDescriptionFromCode(type);
						break;
					default:
						desciption = costTypes.GetDescriptionFromCode(type);
						break;
				}
				if (string.IsNullOrEmpty(desciption) && customsChargeLCItemSettings != null)
				{
					desciption = customsChargeLCItemSettings.FirstOrDefault(x => x.CostType == type)?.Description;
				}
				result.Add(new UniversalData.LandedLineCostItem()
				{
					CostType = new UniversalData.CodeDescriptionPair() { Code = type, Description = desciption },
					CostAmount = landedLineCostItem.LZ_CostAmount
				});
			}
			return result;
		}
	}
}
