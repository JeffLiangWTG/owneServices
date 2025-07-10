using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class AWBRateLineDataObjectWriter : DataObjectWriter<ExportAWBRateLine, AWBRateLine>
	{
		internal AWBRateLineDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override AWBRateLine PopulateDataObject(ExportAWBRateLine rateLine)
		{
			var rateLineDataObject = new AWBRateLine();

			rateLineDataObject.LineNumber = rateLine.ER_LineCount;
			rateLineDataObject.NoOfPiecesOrRCP = rateLine.ER_NoOfPiecesOrRCP;
			rateLineDataObject.GrossWeight = rateLine.ER_GrossWeight;
			rateLineDataObject.WeightUnit = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(rateLine.ER_WeightInLBsOrKGs, rateLine.RateUQList);
			rateLineDataObject.RateClass = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(rateLine.ER_RateClass, rateLine.RateClassList);
			rateLineDataObject.CommodityItem = rateLine.ER_CommodityItemNumber;
			rateLineDataObject.ChargeableWeight = rateLine.ER_ChargeableWeight;
			rateLineDataObject.RateChargeOrDiscount = rateLine.ER_RateChargeOrDiscount;
			rateLineDataObject.TotalCharge = rateLine.ER_Total;

			rateLineDataObject.NatureAndQtyOfGoodsType = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(rateLine.ER_NatureAndQtyOfGoodsType, rateLine.NatureAndQtyOfGoodsTypeList);

			switch (rateLine.ER_NatureAndQtyOfGoodsType)
			{
				case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription:
					rateLineDataObject.GoodsDescription = rateLine.NatureAndQtyOfGoodsText.Text;
					break;
				case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation:
					rateLineDataObject.Consolidation = rateLine.NatureAndQtyOfGoodsText.Text;
					break;
				case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions:
					rateLineDataObject.Dimensions = new AWBRateLineDimensions();
					rateLineDataObject.Dimensions.Length = rateLine.NatureAndQtyOfGoodsDimensions.Length;
					rateLineDataObject.Dimensions.Width = rateLine.NatureAndQtyOfGoodsDimensions.Width;
					rateLineDataObject.Dimensions.Height = rateLine.NatureAndQtyOfGoodsDimensions.Height;
					rateLineDataObject.Dimensions.Unit = ListHelper.GetWithDescription<CodeDescriptionPair>(rateLine.NatureAndQtyOfGoodsDimensions.Unit, rateLine.NatureAndQtyOfGoodsDimensions.UnitList);
					rateLineDataObject.Dimensions.Count = rateLine.NatureAndQtyOfGoodsDimensions.Count;
					break;
				case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume:
					rateLineDataObject.Volume = new AWBRateLineVolume();
					rateLineDataObject.Volume.Volume = rateLine.NatureAndQtyOfGoodsVolume.Volume;
					rateLineDataObject.Volume.Unit = ListHelper.GetWithDescription<CodeDescriptionPair>(rateLine.NatureAndQtyOfGoodsVolume.Unit, rateLine.NatureAndQtyOfGoodsVolume.UnitList);
					break;
				case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber:
					rateLineDataObject.ULDNumber = rateLine.NatureAndQtyOfGoodsText.Text;
					break;
				case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount:
					rateLineDataObject.ShippersLoadAndCount = rateLine.NatureAndQtyOfGoodsSLAC.Count;
					break;
				case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode:
					rateLineDataObject.HarmonizedCommodityCode = rateLine.NatureAndQtyOfGoodsText.Text;
					break;
				case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin:
					rateLineDataObject.CountryOfOrigin = ListHelper.GetWithName<Country>(rateLine.NatureAndQtyOfGoodsOrigin.Country, BindToLists.GetCachedLists(rateLine.Factory).RefCountry_List);
					break;
				default:
					break;
			}

			return rateLineDataObject;
		}
	}
}
