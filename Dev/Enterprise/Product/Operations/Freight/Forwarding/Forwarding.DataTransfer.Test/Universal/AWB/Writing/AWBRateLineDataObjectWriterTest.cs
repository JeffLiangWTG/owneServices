using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class AWBRateLineDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWriteRateLine()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			rateLine.ER_LineCount = 2;
			rateLine.ER_NoOfPiecesOrRCP = "4";
			rateLine.ER_GrossWeight = 22.3;
			rateLine.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine.ER_RateClass = Core.Constants.AWB.RateClass.QuantityRate;
			rateLine.ER_CommodityItemNumber = "nah";
			rateLine.ER_ChargeableWeight = 34.5;
			rateLine.ER_RateChargeOrDiscount = 10;
			ZDecimal expectedTotal = 34.5 * 10;

			var writer = new AWBRateLineDataObjectWriter(new DataWritingManager(new ActionInfo(null, rateLine)));
			var rateLineDataObject = writer.GetDataObject(rateLine);

			AssertEquals(2, rateLineDataObject.LineNumber);
			AssertEquals("4", rateLineDataObject.NoOfPiecesOrRCP);
			AssertEquals(new ZDecimal(22.3), rateLineDataObject.GrossWeight);
			AssertEquals(Core.Constants.AWB.RateLineUQ.Kilos, rateLineDataObject.WeightUnit.Code);
			AssertEquals("Kilograms", rateLineDataObject.WeightUnit.Description);
			AssertEquals(Core.Constants.AWB.RateClass.QuantityRate, rateLineDataObject.RateClass.Code);
			AssertEquals("Quantity Rate", rateLineDataObject.RateClass.Description);
			AssertEquals("nah", rateLineDataObject.CommodityItem);
			AssertEquals(new ZDecimal(34.5), rateLineDataObject.ChargeableWeight);
			AssertEquals(new ZDecimal(10), rateLineDataObject.RateChargeOrDiscount);
			AssertEquals(expectedTotal, rateLineDataObject.TotalCharge);
		}

		public void TestWriteRateLine_NatureAndQtyOfGoods_GoodsDescription()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine.NatureAndQtyOfGoodsText.Text = "just some goods";

			var writer = new AWBRateLineDataObjectWriter(new DataWritingManager(new ActionInfo(null, rateLine)));
			var rateLineDataObject = writer.GetDataObject(rateLine);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, rateLineDataObject.NatureAndQtyOfGoodsType.Code);
			AssertEquals("Goods Description", rateLineDataObject.NatureAndQtyOfGoodsType.Description);
			AssertEquals("just some goods", rateLineDataObject.GoodsDescription);
		}

		public void TestWriteRateLine_NatureAndQtyOfGoods_Consolidation()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation;
			rateLine.NatureAndQtyOfGoodsText.Text = "CONSOL 27";

			var writer = new AWBRateLineDataObjectWriter(new DataWritingManager(new ActionInfo(null, rateLine)));
			var rateLineDataObject = writer.GetDataObject(rateLine);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation, rateLineDataObject.NatureAndQtyOfGoodsType.Code);
			AssertEquals("Consolidation", rateLineDataObject.NatureAndQtyOfGoodsType.Description);
			AssertEquals("CONSOL 27", rateLineDataObject.Consolidation);
		}

		public void TestWriteRateLine_NatureAndQtyOfGoods_Dimensions()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;
			rateLine.NatureAndQtyOfGoodsDimensions.Length = 4;
			rateLine.NatureAndQtyOfGoodsDimensions.Width = 2;
			rateLine.NatureAndQtyOfGoodsDimensions.Height = 3;
			rateLine.NatureAndQtyOfGoodsDimensions.Unit = Core.Constants.Length.Metres;
			rateLine.NatureAndQtyOfGoodsDimensions.Count = 2;

			var writer = new AWBRateLineDataObjectWriter(new DataWritingManager(new ActionInfo(null, rateLine)));
			var rateLineDataObject = writer.GetDataObject(rateLine);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions, rateLineDataObject.NatureAndQtyOfGoodsType.Code);
			AssertEquals("Dimensions", rateLineDataObject.NatureAndQtyOfGoodsType.Description);
			AssertEquals(4, rateLineDataObject.Dimensions.Length);
			AssertEquals(2, rateLineDataObject.Dimensions.Width);
			AssertEquals(3, rateLineDataObject.Dimensions.Height);
			AssertEquals(Core.Constants.Length.Metres, rateLineDataObject.Dimensions.Unit.Code);
			AssertEquals("Meters", rateLineDataObject.Dimensions.Unit.Description);
			AssertEquals(2, rateLineDataObject.Dimensions.Count);
		}

		public void TestWriteRateLine_NatureAndQtyOfGoods_Volume()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;
			rateLine.NatureAndQtyOfGoodsVolume.Volume = 3.54;
			rateLine.NatureAndQtyOfGoodsVolume.Unit = Core.Constants.Volume.CubicMetres;

			var writer = new AWBRateLineDataObjectWriter(new DataWritingManager(new ActionInfo(null, rateLine)));
			var rateLineDataObject = writer.GetDataObject(rateLine);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume, rateLineDataObject.NatureAndQtyOfGoodsType.Code);
			AssertEquals("Volume", rateLineDataObject.NatureAndQtyOfGoodsType.Description);
			AssertEquals(new ZDecimal(3.54), rateLineDataObject.Volume.Volume);
			AssertEquals(Core.Constants.Volume.CubicMetres, rateLineDataObject.Volume.Unit.Code);
			AssertEquals("Cubic Meters", rateLineDataObject.Volume.Unit.Description);
		}

		public void TestWriteRateLine_NatureAndQtyOfGoods_ULDNumber()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
			rateLine.NatureAndQtyOfGoodsText.Text = "CONT1231231";

			var writer = new AWBRateLineDataObjectWriter(new DataWritingManager(new ActionInfo(null, rateLine)));
			var rateLineDataObject = writer.GetDataObject(rateLine);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber, rateLineDataObject.NatureAndQtyOfGoodsType.Code);
			AssertEquals("ULD Number", rateLineDataObject.NatureAndQtyOfGoodsType.Description);
			AssertEquals("CONT1231231", rateLineDataObject.ULDNumber);
		}

		public void TestWriteRateLine_NatureAndQtyOfGoods_ShippersLoadAndCount()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount;
			rateLine.NatureAndQtyOfGoodsSLAC.Count = 15;

			var writer = new AWBRateLineDataObjectWriter(new DataWritingManager(new ActionInfo(null, rateLine)));
			var rateLineDataObject = writer.GetDataObject(rateLine);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount, rateLineDataObject.NatureAndQtyOfGoodsType.Code);
			AssertEquals("Shipper's Load and Count", rateLineDataObject.NatureAndQtyOfGoodsType.Description);
			AssertEquals(15, rateLineDataObject.ShippersLoadAndCount);
		}

		public void TestWriteRateLine_NatureAndQtyOfGoods_HarmonisedCommodityCode()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
			rateLine.NatureAndQtyOfGoodsText.Text = "CODE24";

			var writer = new AWBRateLineDataObjectWriter(new DataWritingManager(new ActionInfo(null, rateLine)));
			var rateLineDataObject = writer.GetDataObject(rateLine);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, rateLineDataObject.NatureAndQtyOfGoodsType.Code);
			AssertEquals("Harmonized Commodity Code", rateLineDataObject.NatureAndQtyOfGoodsType.Description);
			AssertEquals("CODE24", rateLineDataObject.HarmonizedCommodityCode);
		}

		public void TestWriteRateLine_NatureAndQtyOfGoods_CountryOfOrigin()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin;
			rateLine.NatureAndQtyOfGoodsOrigin.Country = Core.Constants.CountryCodes.Kazakhstan;

			var writer = new AWBRateLineDataObjectWriter(new DataWritingManager(new ActionInfo(null, rateLine)));
			var rateLineDataObject = writer.GetDataObject(rateLine);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin, rateLineDataObject.NatureAndQtyOfGoodsType.Code);
			AssertEquals("Country/Region of Origin of Goods", rateLineDataObject.NatureAndQtyOfGoodsType.Description);
			AssertEquals(Core.Constants.CountryCodes.Kazakhstan, rateLineDataObject.CountryOfOrigin.Code);
			AssertEquals("Kazakhstan", rateLineDataObject.CountryOfOrigin.Name);
		}
	}
}
