using System.Linq;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class AWBRateLineDataObjectReader : DataObjectReader<AWBRateLine, ExportAWBRateLine>
	{
		public AWBRateLineDataObjectReader(AWBRateLine rateLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ExportAWBHeader awbHeaderBO)
			: base(rateLineDataObject, logger, factory)
		{
			this.awbHeaderBO = awbHeaderBO;
		}

		readonly ExportAWBHeader awbHeaderBO;

		protected override ExportAWBRateLine GetExistingBusinessObject()
		{
			return awbHeaderBO.AWBRateLines.Cast<ExportAWBRateLine>().FirstOrDefault(line => line.ER_LineCount == dataObject.LineNumber);
		}

		protected override void PopulateBusinessObject(ExportAWBRateLine rateLineBO)
		{
			SetValue(rateLineBO, ExportAWBRateLineSchema.ER_LineCount, (byte)dataObject.LineNumber.GetValueOrDefault());
			SetValue(rateLineBO, ExportAWBRateLineSchema.ER_NoOfPiecesOrRCP, dataObject.NoOfPiecesOrRCP);
			SetValue(rateLineBO, ExportAWBRateLineSchema.ER_WeightInLBsOrKGs, dataObject.WeightUnit);
			SetValue(rateLineBO, ExportAWBRateLineSchema.ER_GrossWeight, dataObject.GrossWeight);
			SetValue(rateLineBO, ExportAWBRateLineSchema.ER_RateClass, dataObject.RateClass);
			SetValue(rateLineBO, ExportAWBRateLineSchema.ER_CommodityItemNumber, dataObject.CommodityItem);
			SetValue(rateLineBO, ExportAWBRateLineSchema.ER_ChargeableWeight, dataObject.ChargeableWeight);
			SetValue(rateLineBO, ExportAWBRateLineSchema.ER_RateChargeOrDiscount, dataObject.RateChargeOrDiscount);

			SetValue(rateLineBO, ExportAWBRateLineSchema.ER_NatureAndQtyOfGoodsType, dataObject.NatureAndQtyOfGoodsType);

			if (dataObject.NatureAndQtyOfGoodsType != null)
			{
				switch (rateLineBO.ER_NatureAndQtyOfGoodsType)
				{
					case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription:
						rateLineBO.NatureAndQtyOfGoodsText.Text = dataObject.GoodsDescription.GetValueOrDefault();
						break;
					case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation:
						rateLineBO.NatureAndQtyOfGoodsText.Text = dataObject.Consolidation.GetValueOrDefault();
						break;
					case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions:
						if (dataObject.Dimensions != null)
						{
							rateLineBO.NatureAndQtyOfGoodsDimensions.Unit = dataObject.Dimensions.Unit.GetCodeAsUpperCase();
							rateLineBO.NatureAndQtyOfGoodsDimensions.Length = dataObject.Dimensions.Length.GetValueOrDefault();
							rateLineBO.NatureAndQtyOfGoodsDimensions.Width = dataObject.Dimensions.Width.GetValueOrDefault();
							rateLineBO.NatureAndQtyOfGoodsDimensions.Height = dataObject.Dimensions.Height.GetValueOrDefault();
							rateLineBO.NatureAndQtyOfGoodsDimensions.Count = dataObject.Dimensions.Count.GetValueOrDefault();
						}
						break;
					case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume:
						if (dataObject.Volume != null)
						{
							rateLineBO.NatureAndQtyOfGoodsVolume.Unit = dataObject.Volume.Unit.GetCodeAsUpperCase();
							rateLineBO.NatureAndQtyOfGoodsVolume.Volume = dataObject.Volume.Volume.GetValueOrDefault();
						}
						break;
					case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber:
						rateLineBO.NatureAndQtyOfGoodsText.Text = dataObject.ULDNumber.GetValueOrDefault();
						break;
					case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount:
						rateLineBO.NatureAndQtyOfGoodsSLAC.Count = dataObject.ShippersLoadAndCount.GetValueOrDefault();
						break;
					case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode:
						rateLineBO.NatureAndQtyOfGoodsText.Text = dataObject.HarmonizedCommodityCode.GetValueOrDefault();
						break;
					case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin:
						rateLineBO.NatureAndQtyOfGoodsOrigin.Country = dataObject.CountryOfOrigin.GetCodeAsUpperCase();
						break;
				}
			}
		}
	}
}
