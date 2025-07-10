using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.AWB;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class AWBRateLineDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestReadRateLine_BasicFields()
		{
			var rateLineDO = new AWBRateLine();

			rateLineDO.LineNumber = 4;
			rateLineDO.NoOfPiecesOrRCP = "5";
			rateLineDO.GrossWeight = 12.2;
			rateLineDO.WeightUnit = new CodeDescriptionPair1Char { Code = Core.Constants.AWB.RateLineUQ.Kilos };
			rateLineDO.RateClass = new CodeDescriptionPair1Char { Code = Core.Constants.AWB.RateClass.QuantityRate };
			rateLineDO.CommodityItem = "456";
			rateLineDO.ChargeableWeight = 10.5;
			rateLineDO.RateChargeOrDiscount = 2.5;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var awbHeaderBO = consol.AWBHeader;
			var reader = new AWBRateLineDataObjectReader(rateLineDO, Logger, Factory, awbHeaderBO);
			var rateLineBO = reader.ReadIntoBusinessObject();

			AssertEquals(new ZByte(4), rateLineBO.ER_LineCount);
			AssertEquals("5", rateLineBO.ER_NoOfPiecesOrRCP);
			AssertEquals(new ZDecimal(12.2), rateLineBO.ER_GrossWeight);
			AssertEquals(Core.Constants.AWB.RateLineUQ.Kilos, rateLineBO.ER_WeightInLBsOrKGs);
			AssertEquals(Core.Constants.AWB.RateClass.QuantityRate, rateLineBO.ER_RateClass);
			AssertEquals("456", rateLineBO.ER_CommodityItemNumber);
			AssertEquals(new ZDecimal(10.5), rateLineBO.ER_ChargeableWeight);
			AssertEquals(new ZDecimal(2.5), rateLineBO.ER_RateChargeOrDiscount);
			AssertEquals(new ZDecimal(26.25), rateLineBO.ER_Total);
		}

		public void TestReadRateLine_NatureAndQtyOfGoods_WithNoNatureAndQtyOfGoodsTypes()
		{
			OverrideDecimalRegistryValueForTest();
			const int natureAndQtyOfGoodsTypesToTest = 3;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var awbHeaderBO = consol.AWBHeader;
			var rateLineDOs = new List<AWBRateLine>();
			for (int i = 0; i < natureAndQtyOfGoodsTypesToTest; i++)
			{
				rateLineDOs.Add(new AWBRateLine { LineNumber = i + 1 });
			}

			rateLineDOs[0].NatureAndQtyOfGoodsType = new CodeDescriptionPair1Char { Code = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription };
			rateLineDOs[0].GoodsDescription = "Description";

			rateLineDOs[1].NatureAndQtyOfGoodsType = new CodeDescriptionPair1Char { Code = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation };
			rateLineDOs[1].Consolidation = "Consol";

			rateLineDOs[2].GoodsDescription = "Description";

			for (int i = 0; i < natureAndQtyOfGoodsTypesToTest; i++)
			{
				var reader = new AWBRateLineDataObjectReader(rateLineDOs[i], Logger, Factory, awbHeaderBO);
				reader.ReadIntoBusinessObject();
			}

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, awbHeaderBO.AWBRateLine1.ER_NatureAndQtyOfGoodsType);
			AssertEquals("Description", awbHeaderBO.AWBRateLine1.NatureAndQtyOfGoodsText.Text);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation, awbHeaderBO.AWBRateLine2.ER_NatureAndQtyOfGoodsType);
			AssertEquals("Consol", awbHeaderBO.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, awbHeaderBO.AWBRateLine3.ER_NatureAndQtyOfGoodsType);
			AssertEquals("", awbHeaderBO.AWBRateLine3.NatureAndQtyOfGoodsText.Text);
		}

		public void TestReadRateLine_NatureAndQtyOfGoods()
		{
			OverrideDecimalRegistryValueForTest();
			const int natureAndQtyOfGoodsTypesToTest = 8;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var awbHeaderBO = consol.AWBHeader;
			var rateLineDOs = new List<AWBRateLine>();
			for (int i = 0; i < natureAndQtyOfGoodsTypesToTest; i++)
			{
				rateLineDOs.Add(new AWBRateLine { LineNumber = i + 1 });
			}

			rateLineDOs[0].NatureAndQtyOfGoodsType = new CodeDescriptionPair1Char { Code = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription };
			rateLineDOs[0].GoodsDescription = "Description";

			rateLineDOs[1].NatureAndQtyOfGoodsType = new CodeDescriptionPair1Char { Code = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation };
			rateLineDOs[1].Consolidation = "Consol";

			rateLineDOs[2].NatureAndQtyOfGoodsType = new CodeDescriptionPair1Char { Code = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions };
			rateLineDOs[2].Dimensions = new AWBRateLineDimensions();
			rateLineDOs[2].Dimensions.Length = 6;
			rateLineDOs[2].Dimensions.Width = 5;
			rateLineDOs[2].Dimensions.Height = 1;
			rateLineDOs[2].Dimensions.Unit = new CodeDescriptionPair { Code = Core.Constants.Length.Metres };
			rateLineDOs[2].Dimensions.Count = 2;

			rateLineDOs[3].NatureAndQtyOfGoodsType = new CodeDescriptionPair1Char { Code = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume };
			rateLineDOs[3].Volume = new AWBRateLineVolume();
			rateLineDOs[3].Volume.Volume = 30.526;
			rateLineDOs[3].Volume.Unit = new CodeDescriptionPair { Code = Core.Constants.Volume.CubicMetres };

			rateLineDOs[4].NatureAndQtyOfGoodsType = new CodeDescriptionPair1Char { Code = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber };
			rateLineDOs[4].ULDNumber = "ABCD1112223";

			rateLineDOs[5].NatureAndQtyOfGoodsType = new CodeDescriptionPair1Char { Code = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount };
			rateLineDOs[5].ShippersLoadAndCount = 4;

			rateLineDOs[6].NatureAndQtyOfGoodsType = new CodeDescriptionPair1Char { Code = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode };
			rateLineDOs[6].HarmonizedCommodityCode = "AAA27";

			rateLineDOs[7].NatureAndQtyOfGoodsType = new CodeDescriptionPair1Char { Code = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin };
			rateLineDOs[7].CountryOfOrigin = new Country { Code = Core.Constants.CountryCodes.UnitedKingdom };

			for (int i = 0; i < natureAndQtyOfGoodsTypesToTest; i++)
			{
				var reader = new AWBRateLineDataObjectReader(rateLineDOs[i], Logger, Factory, awbHeaderBO);
				reader.ReadIntoBusinessObject();
			}

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, awbHeaderBO.AWBRateLine1.ER_NatureAndQtyOfGoodsType);
			AssertEquals("Description", awbHeaderBO.AWBRateLine1.NatureAndQtyOfGoodsText.Text);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation, awbHeaderBO.AWBRateLine2.ER_NatureAndQtyOfGoodsType);
			AssertEquals("Consol", awbHeaderBO.AWBRateLine2.NatureAndQtyOfGoodsText.Text);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions, awbHeaderBO.AWBRateLine3.ER_NatureAndQtyOfGoodsType);
			AssertEquals(6, awbHeaderBO.AWBRateLine3.NatureAndQtyOfGoodsDimensions.Length);
			AssertEquals(5, awbHeaderBO.AWBRateLine3.NatureAndQtyOfGoodsDimensions.Width);
			AssertEquals(1, awbHeaderBO.AWBRateLine3.NatureAndQtyOfGoodsDimensions.Height);
			AssertEquals(Core.Constants.Length.Metres, awbHeaderBO.AWBRateLine3.NatureAndQtyOfGoodsDimensions.Unit);
			AssertEquals(2, awbHeaderBO.AWBRateLine3.NatureAndQtyOfGoodsDimensions.Count);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume, awbHeaderBO.AWBRateLine4.ER_NatureAndQtyOfGoodsType);
			AssertEquals(new ZDecimal(30.526), awbHeaderBO.AWBRateLine4.NatureAndQtyOfGoodsVolume.Volume);
			AssertEquals(Core.Constants.Volume.CubicMetres, awbHeaderBO.AWBRateLine4.NatureAndQtyOfGoodsVolume.Unit);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber, awbHeaderBO.AWBRateLine5.ER_NatureAndQtyOfGoodsType);
			AssertEquals("ABCD1112223", awbHeaderBO.AWBRateLine5.NatureAndQtyOfGoodsText.Text);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount, awbHeaderBO.AWBRateLine6.ER_NatureAndQtyOfGoodsType);
			AssertEquals(4, awbHeaderBO.AWBRateLine6.NatureAndQtyOfGoodsSLAC.Count);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode, awbHeaderBO.AWBRateLine7.ER_NatureAndQtyOfGoodsType);
			AssertEquals("AAA27", awbHeaderBO.AWBRateLine7.NatureAndQtyOfGoodsText.Text);

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin, awbHeaderBO.AWBRateLine8.ER_NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, awbHeaderBO.AWBRateLine8.NatureAndQtyOfGoodsOrigin.Country);
		}

		void OverrideDecimalRegistryValueForTest()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_AWBWeight = collection.AddNew();
			defaultNumberOfDecimals_AWBWeight.TransportMode = Core.Constants.TransportModes.Air;
			defaultNumberOfDecimals_AWBWeight.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_AWBWeight.NumberOfDecimals = 3;
			defaultNumberOfDecimals_AWBWeight.RoundingMode = RoundingModes.Up;
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
