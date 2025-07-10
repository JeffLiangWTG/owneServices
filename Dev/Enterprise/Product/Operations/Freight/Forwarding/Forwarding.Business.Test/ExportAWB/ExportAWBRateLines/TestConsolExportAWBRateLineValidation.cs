using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class TestConsolExportAWBRateLineValidation : BusinessObjectValidationTestCase
	{
		#region Setup

		protected override void SetUp()
		{
			RateLine = Factory.New<ConsolExportAWBRateLine>();
			Validation = new ConsolExportAWBRateLineValidation(RateLine);
		}

		ConsolExportAWBRateLine RateLine;
		ConsolExportAWBRateLineValidation Validation;

		#endregion

		public void TestCheckER_ChargeableWeight()
		{
			var header = Factory.New<ConsolExportAWBHeader>();
			var line = (ConsolExportAWBRateLine)header.AWBRateLines.AddNew();
			line.ER_ChargeableWeight = 350;
			line.ER_WeightInLBsOrKGs = Constants.AWB.RateLineUQ.Kilos;

			var validation = new ConsolExportAWBRateLineValidation(line);
			validation.ValidateER_ChargeableWeight();
			AssertHasWarning(line.ER_ChargeableWeightInfo, "Favorable weight 350KG, calculated from Autorating using Higher Break Lower Rate, is used as the Chargeable Weight for printing AWB. Refer to Achieved Quantities tab per the Weight and Volume utilization of the Consol.");
		}

		public void TestCheckER_NoOfPiecesOrRCP()
		{
			RateLine.ER_LineCount = 1;

			RateLine.ER_NoOfPiecesOrRCP = "ABCD";
			AssertHasMessageError(RateLine.ER_NoOfPiecesOrRCPInfo, "Number of pieces must be a number up to 4 digits or a 3 letter RCP code");

			RateLine.ER_NoOfPiecesOrRCP = "ABC";
			AssertNoMessageError(RateLine.ER_NoOfPiecesOrRCPInfo, "Number of pieces must be a number up to 4 digits or a 3 letter RCP code");
		}

		public void TestCheckER_RateClass()
		{
			RateLine.ER_RateClass = "A";
			Validation.ValidateER_RateClass();
			Assert(RateLine.ER_RateClassInfo.HasMessageErrors());

			RateLine.ER_RateClass = "M";
			Validation.ValidateER_RateClass();
			Assert(!RateLine.ER_RateClassInfo.HasMessageErrors());
		}

		public void TestCheckText_ULD()
		{
			var header = Factory.New<ConsolExportAWBHeader>();
			var rateLine = header.AWBRateLines[0];
			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
			rateLine.NatureAndQtyOfGoods.Text = "PMC15199OZ";

			header.RunPreSaveValidation();
			AssertNoMessageErrors(rateLine.NatureAndQtyOfGoodsLithiumBattery.TextInfo);
		}

		public void TestCheckER_NatureAndQtyOfGoodsType()
		{
			RateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			RateLine.ER_RateClass = "X";
			AssertHasMessageError(RateLine.ER_NatureAndQtyOfGoodsTypeInfo, "Nature And Qty Of Goods Type must be 'U' if Rate Class of 'X' has been selected.");

			RateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
			AssertNoMessageError(RateLine.ER_NatureAndQtyOfGoodsTypeInfo, "Nature And Qty Of Goods Type must be 'U' if Rate Class of 'X' has been selected.");
		}

		public void TestCheckER_GrossWeight()
		{
			RateLine.ER_LineCount = 2;
			RateLine.ER_RateClass = "X";
			RateLine.ER_WeightInLBsOrKGs = "K";
			RateLine.ER_CommodityItemNumber = "1Q";
			Validation.ValidateER_GrossWeight();
			AssertHasWarning(RateLine.ER_GrossWeightInfo, "You have not entered a tare weight for your ULD, which is a requirement as per the IATA Rules. If you choose to send your FWB data without the tare weight, there is a chance that the airline may reject your message. We suggest you provide a weight.");

			RateLine.ER_CommodityItemNumber = "";
			Validation.ValidateER_GrossWeight();
			AssertNoWarnings("Not required unless Commodity Item is present", RateLine.ER_GrossWeightInfo);

			RateLine.ER_RateClass = "";
			Validation.ValidateER_GrossWeight();
			AssertHasWarning(RateLine.ER_GrossWeightInfo, "Gross Weight cannot be zero.");
		}

		public void TestCheckER_GrossWeight_WarningNotVisableWithRegistryOn()
		{
			using (FreightDataRegistry.Instance.MAWBSuppressULDTareWeight.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				RateLine.ER_LineCount = 2;
				RateLine.ER_RateClass = "X";
				RateLine.ER_WeightInLBsOrKGs = "K";
				RateLine.ER_CommodityItemNumber = "1Q";
				Validation.ValidateER_GrossWeight();
				AssertNoWarnings("Warning about exclusion of tare weight should not appear when the registry is on", RateLine.ER_GrossWeightInfo);
			}
		}

		//allowable format n[4..7], n(a)(a), an[..3]
		public void TestCheckER_CommodityItemNumber()
		{
			//allowed
			AssertCommodityItemNumberValidation("1234", false);
			AssertCommodityItemNumberValidation("1234567", false);
			AssertCommodityItemNumberValidation("1", false);
			AssertCommodityItemNumberValidation("1Q", false);
			AssertCommodityItemNumberValidation("1QW", false);
			AssertCommodityItemNumberValidation("D", false);
			AssertCommodityItemNumberValidation("D1", false);
			AssertCommodityItemNumberValidation("D12", false);
			AssertCommodityItemNumberValidation("D123", false);

			//invalid
			AssertCommodityItemNumberValidation("12", true);
			AssertCommodityItemNumberValidation("1AAA", true);
			AssertCommodityItemNumberValidation("QQ", true);
			AssertCommodityItemNumberValidation("123", true);
			AssertCommodityItemNumberValidation("1E2", true);
			AssertCommodityItemNumberValidation("A1234", true);
			AssertCommodityItemNumberValidation("1AA1", true);
		}

		#region CheckER_RateClass

		void AssertER_RateClass_HavingIATACommodityCode(ZString rateClass)
		{
			var messageError = "Enter a Commodity Item Number between 4 and 7 digits long.";
			var messageWarning = "This code does not match IATA Specific Commodity.";

			RateLine.ER_RateClass = rateClass;

			AssertHasWarning(RateLine.ER_CommodityItemNumberInfo, $@"The Commodity Item Number is optional for Rate Class '{rateClass}'.");

			RateLine.ER_CommodityItemNumber = "123";
			AssertHasMessageError(RateLine.ER_CommodityItemNumberInfo, messageError);

			RateLine.ER_CommodityItemNumber = "123@";
			AssertHasMessageError(RateLine.ER_CommodityItemNumberInfo, messageError);

			RateLine.ER_CommodityItemNumber = "1234";
			AssertHasWarning(RateLine.ER_CommodityItemNumberInfo, messageWarning);

			RateLine.ER_CommodityItemNumber = "12345";
			AssertHasWarning(RateLine.ER_CommodityItemNumberInfo, messageWarning);

			RateLine.ER_CommodityItemNumber = "123456";
			AssertHasWarning(RateLine.ER_CommodityItemNumberInfo, messageWarning);

			RateLine.ER_CommodityItemNumber = "1234567";
			AssertHasWarning(RateLine.ER_CommodityItemNumberInfo, messageWarning);

			var iataCommodityCode = Factory.New<RefAirlineCommodityCode>();
			iataCommodityCode.RAC_Code = "12345";
			iataCommodityCode.RAC_AirlineID = "";
			RateLine.ER_CommodityItemNumber = iataCommodityCode.RAC_Code;
			AssertNoMessageErrors(RateLine.ER_CommodityItemNumberInfo);

			var airlineSpecificCommodityCode = Factory.New<RefAirlineCommodityCode>();
			airlineSpecificCommodityCode.RAC_Code = "1234";
			airlineSpecificCommodityCode.RAC_AirlineID = "123";
			RateLine.ER_CommodityItemNumber = airlineSpecificCommodityCode.RAC_Code;
			AssertHasWarning(RateLine.ER_CommodityItemNumberInfo, messageWarning);
		}

		public void TestCheckER_RateClass_C()
		{
			AssertER_RateClass_HavingIATACommodityCode(Constants.AWB.RateClass.SpecificCommodityRate);
		}

		public void TestCheckER_RateClass_U()
		{
			AssertER_RateClass_HavingIATACommodityCode(Constants.AWB.RateClass.UnitLoadDeviceBasicCharge);
		}

		public void TestCheckER_RateClass_R()
		{
			RateLine.ER_RateClass = Constants.AWB.RateClass.ClassRateReduction;

			AssertHasMessageError(RateLine.ER_CommodityItemNumberInfo, @"When Rate Class is 'R', the rate reduction percentage is required, preceded by the rate class to which it relates. For example: A 30% reduction of the normal rate would be N70.");

			RateLine.ER_CommodityItemNumber = "M1";
			AssertNoMessageErrors(RateLine.ER_CommodityItemNumberInfo);

			RateLine.ER_CommodityItemNumber = "M12";
			AssertNoMessageErrors(RateLine.ER_CommodityItemNumberInfo);

			RateLine.ER_CommodityItemNumber = "M123";
			AssertNoMessageErrors(RateLine.ER_CommodityItemNumberInfo);

			RateLine.ER_CommodityItemNumber = "A1";
			AssertHasMessageError(RateLine.ER_CommodityItemNumberInfo, "As Rate Class is 'R' or 'S', enter the letter from Rate Class, followed by the percentage rate that applies to that Rate (up to three numeric characters).");
		}

		public void TestCheckER_RateClass_S()
		{
			RateLine.ER_RateClass = Constants.AWB.RateClass.ClassRateSurcharge;

			AssertHasMessageError(RateLine.ER_CommodityItemNumberInfo, @"When Rate Class is 'S', the rate surcharge percentage is required, preceded by the rate class to which it relates. For example: A 50% surcharge of the minimal rate would be M150.");

			RateLine.ER_CommodityItemNumber = "M1";
			AssertNoMessageErrors(RateLine.ER_CommodityItemNumberInfo);

			RateLine.ER_CommodityItemNumber = "M12";
			AssertNoMessageErrors(RateLine.ER_CommodityItemNumberInfo);

			RateLine.ER_CommodityItemNumber = "M123";
			AssertNoMessageErrors(RateLine.ER_CommodityItemNumberInfo);

			RateLine.ER_CommodityItemNumber = "A1";
			AssertHasMessageError(RateLine.ER_CommodityItemNumberInfo, "As Rate Class is 'R' or 'S', enter the letter from Rate Class, followed by the percentage rate that applies to that Rate (up to three numeric characters).");
		}

		public void TestCheckER_RateClass_X()
		{
			RateLine.ER_GrossWeight = 100;
			RateLine.ER_RateClass = Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation;

			AssertNoMessageErrors(RateLine.ER_CommodityItemNumberInfo);

			RateLine.NatureAndQtyOfGoodsType = Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
			RateLine.Validation.ValidateER_CommodityItemNumber();

			AssertHasMessageError(RateLine.ER_CommodityItemNumberInfo, @"When Rate Class is 'X', the ULD rate class type is required.");

			RateLine.ER_GrossWeight = 0;
			RateLine.Validation.ValidateER_CommodityItemNumber();
			AssertNoMessageErrors("Commodity Item Number is only required for line with ULD TARE weight", RateLine.ER_CommodityItemNumberInfo);

			RateLine.ER_CommodityItemNumber = "2A";
			AssertNoMessageErrors(RateLine.ER_CommodityItemNumberInfo);

			RateLine.ER_CommodityItemNumber = "2MK";
			AssertHasMessageError(RateLine.ER_CommodityItemNumberInfo, "The code you have selected is not in the list.");
		}

		#endregion

		void AssertCommodityItemNumberValidation(string value, bool hasMessageError)
		{
			RateLine.ER_CommodityItemNumber = value;
			RateLine.Validation.ValidateER_CommodityItemNumber();
			AssertEquals(hasMessageError, RateLine.ER_CommodityItemNumberInfo.HasMessageErrors());
		}
	}
}
