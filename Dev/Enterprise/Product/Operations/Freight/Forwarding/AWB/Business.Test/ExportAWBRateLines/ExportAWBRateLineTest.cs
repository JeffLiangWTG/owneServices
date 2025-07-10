using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(ExportAWBRateLine))]
	public class ExportAWBRateLineTest : EnterpriseBusinessObjectTestCaseWithListChecking<ExportAWBRateLine>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals("", RateLine.ER_WeightInLBsOrKGs);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, RateLine.ER_NatureAndQtyOfGoodsType);
		}

		public void TestClear()
		{
			RateLine.ER_NoOfPiecesOrRCP = "10";
			RateLine.ER_WeightInLBsOrKGs = "E";
			RateLine.ER_GrossWeight = 10;
			RateLine.ER_ChargeableWeight = 10;
			RateLine.ER_RateChargeOrDiscount = 10;
			RateLine.ER_CommodityItemNumber = "L";
			RateLine.ER_RateClass = "L";

			RateLine.Clear();

			AssertEquals("0", RateLine.ER_NoOfPiecesOrRCP);
			AssertEquals("", RateLine.ER_WeightInLBsOrKGs);
			AssertEquals(0M, RateLine.ER_GrossWeight);
			AssertEquals(0M, RateLine.ER_ChargeableWeight);
			AssertEquals(0M, RateLine.ER_RateChargeOrDiscount);
			AssertEquals("", RateLine.ER_CommodityItemNumber);
			AssertEquals("", RateLine.ER_RateClass);
		}

		public void TestIsEmpty()
		{
			RateLine.ER_NoOfPiecesOrRCP = "10";
			RateLine.ER_WeightInLBsOrKGs = "E";
			RateLine.ER_GrossWeight = 0;
			RateLine.ER_ChargeableWeight = 0;
			RateLine.ER_RateChargeOrDiscount = 10;
			RateLine.ER_CommodityItemNumber = "L";
			RateLine.ER_RateClass = "L";
			AssertEquals(false, RateLine.IsEmpty);

			RateLine.Clear();
			AssertEquals(true, RateLine.IsEmpty);

			RateLine.ER_NoOfPiecesOrRCP = "0";
			AssertEquals(true, RateLine.IsEmpty);

			RateLine.ER_NoOfPiecesOrRCP = "   ";
			AssertEquals(true, RateLine.IsEmpty);

			RateLine.ER_NoOfPiecesOrRCP = "10";
			AssertEquals(false, RateLine.IsEmpty);

			RateLine.Clear();
			RateLine.ER_NoOfPiecesOrRCP = "";
			RateLine.ER_WeightInLBsOrKGs = "E";
			AssertEquals(true, RateLine.IsEmpty);

			RateLine.Clear();
			RateLine.ER_GrossWeight = 10;
			AssertEquals(false, RateLine.IsEmpty);

			RateLine.Clear();
			RateLine.ER_ChargeableWeight = 10;
			AssertEquals(false, RateLine.IsEmpty);

			RateLine.Clear();
			RateLine.ER_RateChargeOrDiscount = 10;
			AssertEquals(false, RateLine.IsEmpty);

			RateLine.Clear();
			RateLine.ER_CommodityItemNumber = "L";
			AssertEquals(false, RateLine.IsEmpty);

			RateLine.Clear();
			RateLine.ER_RateClass = "L";
			AssertEquals(false, RateLine.IsEmpty);

			RateLine.Clear();
			RateLine.NatureAndQtyOfGoods.Text = "=========";
			AssertEquals(false, RateLine.IsEmpty);
		}

		public void TestER_NoOfPiecesOrRCP()
		{
			RateLine.ER_NoOfPiecesOrRCP = "10";
			AssertEquals("10", RateLine.ER_NoOfPiecesOrRCP);
		}

		public void TestER_NoOfPiecesOrRCPAsInt()
		{
			RateLine.ER_NoOfPiecesOrRCP = "10";
			AssertEquals(10, RateLine.ER_NoOfPiecesOrRCPAsInt);
			RateLine.ER_NoOfPiecesOrRCP = "";
			AssertEquals(0, RateLine.ER_NoOfPiecesOrRCPAsInt);
			RateLine.ER_NoOfPiecesOrRCP = "WER";
			AssertEquals(0, RateLine.ER_NoOfPiecesOrRCPAsInt);
			RateLine.ER_NoOfPiecesOrRCP = "0";
			AssertEquals(0, RateLine.ER_NoOfPiecesOrRCPAsInt);
		}

		public void TestER_GrossWeight()
		{
			RateLine.ER_GrossWeight = 10.3m;
			AssertEquals(10.3m, RateLine.ER_GrossWeight);

			RateLine.ER_GrossWeight = 10.301m;
			AssertEquals("Rounded up to 1 decimal", 10.4m, RateLine.ER_GrossWeight);

			RateLine.ER_GrossWeight = 10.32m;
			AssertEquals("Rounded up to 1 decimal", 10.4m, RateLine.ER_GrossWeight);

			RateLine.ER_GrossWeight = 10.34m;
			AssertEquals("Rounded up to 1 decimal", 10.4m, RateLine.ER_GrossWeight);

			RateLine.ER_GrossWeight = 10.39m;
			AssertEquals("Rounded up to 1 decimal", 10.4m, RateLine.ER_GrossWeight);
		}

		public void TestER_GrossWeight_DecimalPlaces()
		{
			AssertDecimalPlacesAttribute(ExportAWBRateLine.Schema.ER_GrossWeight, 1);
		}

		public void TestER_ChargeableWeight()
		{
			RateLine.ER_ChargeableWeight = 10.32M;
			AssertEquals(10.5M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.2M;
			AssertEquals(10.5M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.26M;
			AssertEquals(10.5M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.7M;
			AssertEquals(11M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.75M;
			AssertEquals(11.0M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.8M;
			AssertEquals(11.0M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.9M;
			AssertEquals(11.0M, RateLine.ER_ChargeableWeight);

			ZGuid previousHeaderPK = RateLine.ER_EH;
			RateLine.ER_EH = ZGuid.Empty;
			RateLine.ER_ChargeableWeight = 10.9M;
			AssertEquals(10.9M, RateLine.ER_ChargeableWeight);

			RateLine.ER_EH = previousHeaderPK;
			RateLine.ER_ChargeableWeight = 10.9M;
			AssertEquals(11.0M, RateLine.ER_ChargeableWeight);

			AWBRoundingCollection collection = FreightDataRegistry.Instance.AWBRoundings.Value;
			collection[AWBRounding.Keys.House].RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			collection[AWBRounding.Keys.House].RoundingScale = ChargeableWeightRoundingScales.Scale10;
			FreightDataRegistry.Instance.AWBRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			RateLine.ER_ChargeableWeight = 10.0M;
			AssertEquals(10M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.32M;
			AssertEquals(11M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.7M;
			AssertEquals(11M, RateLine.ER_ChargeableWeight);

			collection = FreightDataRegistry.Instance.AWBRoundings.Value;
			collection[AWBRounding.Keys.House].RoundingMode = nameof(ChargeableWeightRoundingType.Down);
			collection[AWBRounding.Keys.House].RoundingScale = ChargeableWeightRoundingScales.Scale10;
			FreightDataRegistry.Instance.AWBRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			RateLine.ER_ChargeableWeight = 10.0M;
			AssertEquals(10M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.32M;
			AssertEquals(10M, RateLine.ER_ChargeableWeight);

			RateLine.ER_ChargeableWeight = 10.7M;
			AssertEquals(10M, RateLine.ER_ChargeableWeight);
		}

		public void TestER_ChargeableWeight_DecimalPlaces()
		{
			AssertDecimalPlacesAttribute(ExportAWBRateLine.Schema.ER_ChargeableWeight, 1);
		}

		public void TestER_ChargeableWeightRefresh()
		{
			AWBRoundingCollection collection = FreightDataRegistry.Instance.AWBRoundings.Value;
			collection[AWBRounding.Keys.House].RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			collection[AWBRounding.Keys.House].RoundingScale = ChargeableWeightRoundingScales.Scale10;
			FreightDataRegistry.Instance.AWBRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			bool isER_ChargeableWeightRefreshed = false;
			RateLine.ER_ChargeableWeightInfo.ValueChanged += (s, e) => { isER_ChargeableWeightRefreshed = true; };

			RateLine.ER_ChargeableWeight = 30.32M;
			AssertEquals(true, isER_ChargeableWeightRefreshed);
			AssertEquals(31M, RateLine.ER_ChargeableWeight);

			isER_ChargeableWeightRefreshed = false;
			RateLine.ER_ChargeableWeight = 30.88M;
			AssertEquals(true, isER_ChargeableWeightRefreshed);
			AssertEquals(31M, RateLine.ER_ChargeableWeight);
		}

		public void TestER_RateClass()
		{
			RateLine.ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;
			AssertEquals(Constants.AWB.RateClass.RatePerKilogram, RateLine.ER_RateClass);

			//test that changine ER_RateClass causes a refresh
			RateLine.ER_ChargeableWeight = 10.5M;
			RateLine.ER_Total = 152M;
			AssertEquals(14.48M, RateLine.ER_RateChargeOrDiscount);
			RateLine.ER_RateClass = Constants.AWB.RateClass.MinimumCharge;
			AssertEquals(14.48M, RateLine.ER_Total);
		}

		public void TestER_RateChargeOrDiscount()
		{
			AWBHeader.EH_Currency = "AUD";

			RateLine.ER_RateChargeOrDiscount = 10.3M;
			AssertEquals(10.3M, RateLine.ER_RateChargeOrDiscount);

			RateLine.ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;
			RateLine.ER_ChargeableWeight = 10.5M;
			RateLine.ER_Total = 152M;
			AssertEquals(14.48M, RateLine.ER_RateChargeOrDiscount);

			RateLine.ER_RateClass = Constants.AWB.RateClass.ClassRateSurcharge;
			RateLine.ER_CommodityItemNumber = "M150";
			RateLine.ER_RateChargeOrDiscount = 99.5M;
			AssertEquals(99.5M, RateLine.ER_Total);

			RateLine.ER_CommodityItemNumber = "N150";
			RateLine.ER_ChargeableWeight = 10.5M;
			AssertEquals(1044.75M, RateLine.ER_Total);

			AWBHeader.EH_Currency = "JPY";

			RateLine.ER_RateChargeOrDiscount = 10.3M;
			AssertEquals(10.3M, RateLine.ER_RateChargeOrDiscount);

			RateLine.ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;
			RateLine.ER_ChargeableWeight = 10.5M;
			RateLine.ER_Total = 152M;
			AssertEquals(14.0M, RateLine.ER_RateChargeOrDiscount);

			RateLine.ER_RateClass = Constants.AWB.RateClass.ClassRateSurcharge;
			RateLine.ER_CommodityItemNumber = "M150";
			RateLine.ER_RateChargeOrDiscount = 99.5M;
			AssertEquals(100.0M, RateLine.ER_Total);

			RateLine.ER_CommodityItemNumber = "N150";
			RateLine.ER_ChargeableWeight = 10.5M;
			AssertEquals(1045.0M, RateLine.ER_Total);
		}

		public void TestER_Total()
		{
			AWBHeader.EH_Currency = "AUD";

			RateLine.ER_ChargeableWeight = 10.31M;
			RateLine.ER_NoOfPiecesOrRCP = "10";
			RateLine.ER_RateChargeOrDiscount = 9.43M;

			RateLine.ER_RateClass = Constants.AWB.RateClass.MinimumCharge;
			AssertEquals(9.43M, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.BasicCharge;
			AssertEquals(9.43M, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.UnitLoadDeviceBasicCharge;
			AssertEquals(94.3M, RateLine.ER_Total);

			RateLine.ER_RateClass = "";
			AssertEquals(0M, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.QuantityRate;
			AssertEquals(99.02M, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;
			AssertEquals(99.02M, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.ClassRateSurcharge;
			RateLine.ER_CommodityItemNumber = "M150";
			RateLine.ER_Total = 66.6M;
			AssertEquals(66.6M, RateLine.ER_RateChargeOrDiscount);

			RateLine.ER_CommodityItemNumber = "N150";
			RateLine.ER_Total = 66.6M;
			AssertEquals(6.34M, RateLine.ER_RateChargeOrDiscount);

			AWBHeader.EH_Currency = "JPY";

			RateLine.ER_ChargeableWeight = 10.31M;
			RateLine.ER_NoOfPiecesOrRCP = "10";
			RateLine.ER_RateChargeOrDiscount = 9.43M;

			RateLine.ER_RateClass = Constants.AWB.RateClass.MinimumCharge;
			AssertEquals(9.0M, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.UnitLoadDeviceBasicCharge;
			AssertEquals(94.0M, RateLine.ER_Total);

			RateLine.ER_RateClass = "";
			AssertEquals(0M, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.BasicCharge;
			RateLine.ER_ChargeableWeight = 11.6M;
			RateLine.ER_RateChargeOrDiscount = 16m;
			AssertEquals(16m, RateLine.ER_Total);
		}

		public void TestSuppressCalculation()
		{
			RateLine.ER_ChargeableWeight = 10m;
			RateLine.ER_RateChargeOrDiscount = 2m;

			RateLine.ER_RateClass = "";
			AssertEquals("Precondition", 0m, RateLine.ER_Total);

			RateLine.ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;
			AssertEquals("Precondition: total recalculated", 20m, RateLine.ER_Total);

			RateLine.ER_RateClass = "";
			AssertEquals("Precondition", 0m, RateLine.ER_Total);

			using (RateLine.SuppressTotalAndRateChargeCalculation())
			{
				RateLine.ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;
				AssertEquals("Total not recalculated", 0m, RateLine.ER_Total);

				RateLine.ER_RateChargeOrDiscount = 4m;
				AssertEquals("Total not recalculated", 0m, RateLine.ER_Total);
			}

			RateLine.ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;
			RateLine.ER_ChargeableWeight = 10m;
			RateLine.ER_Total = 40m;
			AssertEquals("Precondition: rate charge recalculated", 4m, RateLine.ER_RateChargeOrDiscount);

			using (RateLine.SuppressTotalAndRateChargeCalculation())
			{
				RateLine.ER_RateClass = Constants.AWB.RateClass.RatePerKilogram;
				AssertEquals("Rate charge not recalculated", 4m, RateLine.ER_RateChargeOrDiscount);

				RateLine.ER_Total = 80m;
				AssertEquals("Rate charge not recalculated", 4m, RateLine.ER_RateChargeOrDiscount);
			}
		}

		public void TestCurrencyDecimals()
		{
			AWBHeader.EH_Currency = "AUD";
			AssertEquals("Currency decimals", 2, RateLine.CurrencyDecimals);

			AWBHeader.EH_Currency = "JPY";
			AssertEquals("Currency decimals", 0, RateLine.CurrencyDecimals);

			AWBHeader.EH_Currency = "XXX";
			AssertEquals("Default currency decimals", 2, RateLine.CurrencyDecimals);
		}

		public void TestRateUQList()
		{
			AssertEquals(OLookUpEditType.AWBRateUQ, RateLine.RateUQList.LookupEditType);
		}

		public virtual void TestRateClassList()
		{
			AssertEquals(OLookUpEditType.AWBRateClass, RateLine.RateClassList.LookupEditType);
		}

		public void TestSuspendSettingHasChanges()
		{
			ExportAWBRateLine rateLine = Factory.New<ExportAWBRateLine>();

			using (rateLine.SuspendSettingHasChanges())
			{
				rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;

				rateLine.NatureAndQtyOfGoodsVolume.Text = "VOL 1 M3";
				rateLine.NatureAndQtyOfGoodsVolume.Volume = 2;
				rateLine.NatureAndQtyOfGoodsVolume.Unit = Core.Constants.Volume.CubicInches;

				rateLine.NatureAndQtyOfGoodsDimensions.Text = "DIMS 1x2x3 M x 4";
				rateLine.NatureAndQtyOfGoodsDimensions.Length = 10;
				rateLine.NatureAndQtyOfGoodsDimensions.Width = 11;
				rateLine.NatureAndQtyOfGoodsDimensions.Height = 12;
				rateLine.NatureAndQtyOfGoodsDimensions.Unit = Core.Constants.Length.Inches;
				rateLine.NatureAndQtyOfGoodsDimensions.Count = 13;

				rateLine.NatureAndQtyOfGoodsText.Text = "XXX";

				rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions;
				rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			}

			AssertEquals(false, rateLine.HasChanges);
		}

		public void TestER_NatureAndQtyOfGoodsType()
		{
			ExportAWBRateLine rateLine = Factory.New<ExportAWBRateLine>();
			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume;
			AssertEquals(rateLine.NatureAndQtyOfGoodsVolume, rateLine.NatureAndQtyOfGoods);

			rateLine.NatureAndQtyOfGoodsVolume.Volume = 1.23m;
			rateLine.NatureAndQtyOfGoodsVolume.Unit = Core.Constants.Volume.CubicFeet;

			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;

			AssertEquals(rateLine.NatureAndQtyOfGoodsText, rateLine.NatureAndQtyOfGoods);
			AssertEquals("VOL 1.23 CF", rateLine.NatureAndQtyOfGoods.Text);
		}

		public void TestNatureAndQtyOfGoodsTypeList()
		{
			AssertEquals(OLookUpEditType.AWBNatureAndQtyOfGoodsType, RateLine.NatureAndQtyOfGoodsTypeList.LookupEditType);
		}

		public void TestNatureAndQtyOfGoods()
		{
			ExportAWBRateLine rateLine = RateLine;

			Action<string, NatureAndQtyOfGoods> assertCodeReturnsCorrectType = (code, expectedNatureAndQtyOfGoods) =>
				{
					rateLine.ER_NatureAndQtyOfGoodsType = code;
					AssertEquals(expectedNatureAndQtyOfGoods, rateLine.NatureAndQtyOfGoods);
				};

			assertCodeReturnsCorrectType(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, rateLine.NatureAndQtyOfGoodsText);
			assertCodeReturnsCorrectType(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions, rateLine.NatureAndQtyOfGoodsDimensions);
			assertCodeReturnsCorrectType(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume, rateLine.NatureAndQtyOfGoodsVolume);
			assertCodeReturnsCorrectType(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount, rateLine.NatureAndQtyOfGoodsSLAC);
			assertCodeReturnsCorrectType(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin, rateLine.NatureAndQtyOfGoodsOrigin);

			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			rateLine.NatureAndQtyOfGoods.Text = "XXX";

			Factory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			rateLine = factory.Load<ExportAWBRateLine>(rateLine.PK);

			AssertEquals("XXX", rateLine.ER_NatureAndQtyOfGoods);
			AssertEquals("XXX", rateLine.NatureAndQtyOfGoods.Text);
		}

		public void TestNatureAndQtyOfGoodsText()
		{
			ExportAWBRateLine rateLine = Factory.New<ExportAWBRateLine>();
			AssertNotNull(rateLine.NatureAndQtyOfGoodsText);
			AssertEquals(typeof(NatureAndQtyOfGoods), rateLine.NatureAndQtyOfGoodsText.GetType());
		}

		public void TestNatureAndQtyOfGoodsVolume()
		{
			ExportAWBRateLine rateLine = Factory.New<ExportAWBRateLine>();
			AssertNotNull(rateLine.NatureAndQtyOfGoodsVolume);
			AssertEquals(typeof(NatureAndQtyOfGoodsVolume), rateLine.NatureAndQtyOfGoodsVolume.GetType());
		}

		public void TestNatureAndQtyOfGoodsDimensions()
		{
			ExportAWBRateLine rateLine = Factory.New<ExportAWBRateLine>();
			AssertNotNull(rateLine.NatureAndQtyOfGoodsDimensions);
			AssertEquals(typeof(NatureAndQtyOfGoodsDimensions), rateLine.NatureAndQtyOfGoodsDimensions.GetType());
		}

		public void TestGetNatureAndQtyOfGoodsTypeFromText()
		{
			ExportAWBHeader header = Factory.New<ExportAWBHeader>();
			IExportAWBRateLine rateLine = header.AWBRateLine1;
			Assert("Precondition: header must allow for type to be recognized from text", header.AllowRecogniseAndUpdateNatureAndQtyOfGoodsTypeFromText);

			rateLine.NatureAndQtyOfGoodsDescription = "This won't match";
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, rateLine.NatureAndQtyOfGoodsType);

			rateLine.NatureAndQtyOfGoodsDescription = "VOL 123.000 M3";
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume, rateLine.NatureAndQtyOfGoodsType);

			rateLine.NatureAndQtyOfGoodsDescription = "DIMS 1x2x3 M x 4";
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions, rateLine.NatureAndQtyOfGoodsType);

			rateLine.NatureAndQtyOfGoodsDescription = "8 SLAC";
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount, rateLine.NatureAndQtyOfGoodsType);

			rateLine.NatureAndQtyOfGoodsDescription = "Goods Origin: AU";
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin, rateLine.NatureAndQtyOfGoodsType);

			rateLine.NatureAndQtyOfGoodsDescription = "Lithium Battery: PI970";
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery, rateLine.NatureAndQtyOfGoodsType);

			rateLine.NatureAndQtyOfGoodsDescription = Constants.AWB.NatureAndQtyOfGoodsDetails.ConsolAsPerList;
			AssertEquals(Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation, rateLine.NatureAndQtyOfGoodsType);
		}

		public void TestNatureAndQtyOfGoodsLithiumBattery()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			AssertNotNull(rateLine.NatureAndQtyOfGoodsLithiumBattery);
			AssertEquals(typeof(NatureAndQtyOfGoodsLithiumBattery), rateLine.NatureAndQtyOfGoodsLithiumBattery.GetType());
		}

		#region Implementation

		void AssertDecimalPlacesAttribute(string propertyName, int expectedDecimalPlaces)
		{
			var decimalPlacesAttribute = typeof(ExportAWBRateLine)
				.GetProperty(propertyName)
				.GetCustomAttributes(typeof(DecimalPlacesAttribute), true)[0] as DecimalPlacesAttribute;

			string message = string.Format("{0} should show {1} decimal(s)", propertyName, expectedDecimalPlaces);
			AssertEquals(message, expectedDecimalPlaces, decimalPlacesAttribute.DecimalPlaces);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<ExportAWBHeader>();
			var result = factory.NewWithValidTestData<ExportAWBRateLine>();
			result.ER_EH = header.PK;

			return result;
		}

		ExportAWBRateLine RateLine;
		ExportAWBHeader AWBHeader;
		protected override void SetUp()
		{
			RateLine = Factory.New<ExportAWBRateLine>();
			AWBHeader = Factory.New<ExportAWBHeader>();
			AWBHeader.EH_AWBType = AWBTypeList.Codes.House;
			ExportAWBRateLineCollection rateLineCollection = new ExportAWBRateLineCollection(AWBHeader);
			rateLineCollection.Add(RateLine);
		}

		#endregion
	}
}
