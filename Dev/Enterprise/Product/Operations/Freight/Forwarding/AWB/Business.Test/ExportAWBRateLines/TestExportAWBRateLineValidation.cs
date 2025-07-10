using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	sealed class TestExportAWBRateLineValidation : BusinessObjectValidationTestCase
	{
		public void TestCheckER_NatureAndQtyOfGoodsType()
		{
			ExportAWBRateLine rateLine = Factory.New<ExportAWBRateLine>();
			rateLine.ER_NatureAndQtyOfGoodsType = ZString.Empty;
			AssertHasError(rateLine.ER_NatureAndQtyOfGoodsTypeInfo, "Please enter a value.");

			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;
			AssertNoError(rateLine.ER_NatureAndQtyOfGoodsTypeInfo, "Please enter a value.");

			rateLine.ER_NatureAndQtyOfGoodsType = "1";
			AssertHasError(rateLine.ER_NatureAndQtyOfGoodsTypeInfo, "Enter a valid selection.");

			rateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber;
			AssertNoError(rateLine.ER_NatureAndQtyOfGoodsTypeInfo, "Enter a valid selection.");
		}

		public void TestCheckER_WeightInLBsOrKGs()
		{
			var rateLine = Factory.New<ExportAWBRateLine>();
			var validation = new ExportAWBRateLineValidation(rateLine);

			rateLine.ER_LineCount = 1;
			rateLine.ER_WeightInLBsOrKGs = "";
			validation.ValidateER_WeightInLBsOrKGs();
			Assert(rateLine.ER_WeightInLBsOrKGsInfo.HasMessageErrors());

			rateLine.ER_LineCount = 2;
			validation.ValidateER_WeightInLBsOrKGs();
			Assert(!rateLine.ER_WeightInLBsOrKGsInfo.HasMessageErrors());

			rateLine.ER_WeightInLBsOrKGs = "X";
			validation.ValidateER_WeightInLBsOrKGs();
			Assert(rateLine.ER_WeightInLBsOrKGsInfo.HasMessageErrors());

			rateLine.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			validation.ValidateER_WeightInLBsOrKGs();
			Assert(!rateLine.ER_WeightInLBsOrKGsInfo.HasMessageErrors());

			rateLine.ER_WeightInLBsOrKGs = "";
			rateLine.ER_GrossWeight = 0;
			validation.ValidateER_WeightInLBsOrKGs();
			Assert(!rateLine.ER_WeightInLBsOrKGsInfo.HasMessageErrors());

			rateLine.ER_GrossWeight = 15;
			validation.ValidateER_WeightInLBsOrKGs();
			Assert(!rateLine.ER_WeightInLBsOrKGsInfo.HasMessageErrors());

			rateLine.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			validation.ValidateER_WeightInLBsOrKGs();
			Assert(!rateLine.ER_WeightInLBsOrKGsInfo.HasMessageErrors());
		}

		public void TestCheckER_NoOfPiecesOrRCP()
		{
			ExportAWBRateLine rateLine = Factory.New<ExportAWBRateLine>();
			ExportAWBRateLineValidation validation = new ExportAWBRateLineValidation(rateLine);

			rateLine.ER_NoOfPiecesOrRCP = "1234";
			validation.ValidateER_NoOfPiecesOrRCP();
			AssertEquals(false, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarnings());

			rateLine.ER_NoOfPiecesOrRCP = "123A";
			validation.ValidateER_NoOfPiecesOrRCP();
			AssertEquals(true, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarning("RCP should be alphabetical characters only."));
			AssertEquals(true, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarning("RCP should be 3 characters in length."));

			rateLine.ER_NoOfPiecesOrRCP = "2.3";
			validation.ValidateER_NoOfPiecesOrRCP();
			AssertEquals(true, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarning("RCP should be alphabetical characters only."));

			rateLine.ER_NoOfPiecesOrRCP = "QQ";
			validation.ValidateER_NoOfPiecesOrRCP();
			AssertEquals(true, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarning("RCP should be 3 characters in length."));

			rateLine.ER_NoOfPiecesOrRCP = "QQWE";
			validation.ValidateER_NoOfPiecesOrRCP();
			AssertEquals(true, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarning("RCP should be 3 characters in length."));

			rateLine.ER_NoOfPiecesOrRCP = "JED";
			validation.ValidateER_NoOfPiecesOrRCP();
			AssertEquals(false, rateLine.ER_NoOfPiecesOrRCPInfo.HasWarnings());
		}

		public void TestCheckER_GrossWeight()
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLineSavedBeforeNewRoundingRule1 = Factory.New<ExportAWBRateLineForTesting>();
			rateLineSavedBeforeNewRoundingRule1.ER_GrossWeight = 1.024;
			rateLineSavedBeforeNewRoundingRule1.ER_EH = header.PK;

			var rateLineSavedBeforeNewRoundingRule2 = Factory.New<ExportAWBRateLineForTesting>();
			rateLineSavedBeforeNewRoundingRule2.ER_GrossWeight = 1.2;
			rateLineSavedBeforeNewRoundingRule2.ER_EH = header.PK;

			Factory.Save();

			Action<ExportAWBRateLineForTesting, bool> assertGrossWeightValidation = (existingRateLine, messageErrorExpected) =>
				{
					var rateLine = Factory.Load<ExportAWBRateLine>(existingRateLine.PK);

					var validation = new ExportAWBRateLineValidation(rateLine);
					validation.ValidateER_GrossWeight();

					string expectedMessage = string.Format(
						"A new validation has been added to check that gross weight has only 1 decimal value.\r\n" +
						"The overridden weight is currently {0} - please re-enter the weight with only 1 decimal.", rateLine.ER_GrossWeight);

					AssertEquals(messageErrorExpected, rateLine.ER_GrossWeightInfo.GetMessageErrors().Contains(expectedMessage));
				};

			assertGrossWeightValidation(rateLineSavedBeforeNewRoundingRule1, true);
			assertGrossWeightValidation(rateLineSavedBeforeNewRoundingRule2, false);
		}

		public void TestCheckER_ChargeableWeight()
		{
			var header = Factory.New<ExportAWBHeader>();

			var rateLine1 = Factory.New<ExportAWBRateLineForTesting>();
			rateLine1.ER_ChargeableWeight = 1.024;
			rateLine1.ER_EH = header.PK;

			var rateLine2 = Factory.New<ExportAWBRateLineForTesting>();
			rateLine2.ER_ChargeableWeight = 1.9;
			rateLine2.ER_EH = header.PK;

			var rateLine3 = Factory.New<ExportAWBRateLineForTesting>();
			rateLine3.ER_ChargeableWeight = 1.51;
			rateLine3.ER_EH = header.PK;

			var rateLine4 = Factory.New<ExportAWBRateLineForTesting>();
			rateLine4.ER_ChargeableWeight = 1.5;
			rateLine4.ER_EH = header.PK;

			var rateLine5 = Factory.New<ExportAWBRateLineForTesting>();
			rateLine5.ER_ChargeableWeight = 2.0;
			rateLine5.ER_EH = header.PK;

			Factory.Save();

			Action<ExportAWBRateLineForTesting, bool> assertChargeableWeightValidation = (existingRateLine, messageErrorExpected) =>
			{
				var rateLine = Factory.Load<ExportAWBRateLine>(existingRateLine.PK);

				var validation = new ExportAWBRateLineValidation(rateLine);
				validation.ValidateER_ChargeableWeight();

				string awbChargeableRoundingRegistryPath = string.Format("{0}/{1}", FreightDataRegistry.Instance.AWBRoundings.Category, FreightDataRegistry.Instance.AWBRoundings.Caption);
				string expectedMessage = string.Format(
					"Chargeable weight should represent the gross weight with 1 decimal value, whereby this decimal is always rounded up to the next 0.5 unit.\r\n" +
					"Please refer to registry {0} to set the desired rounding behavior.", awbChargeableRoundingRegistryPath);

				AssertEquals(messageErrorExpected, rateLine.ER_ChargeableWeightInfo.GetMessageErrors().Contains(expectedMessage));
			};

			assertChargeableWeightValidation(rateLine1, true);
			assertChargeableWeightValidation(rateLine2, true);
			assertChargeableWeightValidation(rateLine3, true);
			assertChargeableWeightValidation(rateLine4, false);
			assertChargeableWeightValidation(rateLine5, false);
		}

		#region Implementation

		class ExportAWBRateLineForTesting : ExportAWBRateLine
		{
			public ExportAWBRateLineForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZDecimal ER_GrossWeight
			{
				get { return base.ER_GrossWeight; }
				set
				{
					// Avoiding base.ER_GrossWeight rounding logic
					SetPropertyValue(ER_GrossWeightInfo, value);
				}
			}

			public override ZDecimal ER_ChargeableWeight
			{
				get { return base.ER_ChargeableWeight; }
				set
				{
					// Avoiding base.ER_ChargeableWeight rounding logic
					SetPropertyValue(ER_ChargeableWeightInfo, value);
				}
			}
		}

		#endregion
	}
}
