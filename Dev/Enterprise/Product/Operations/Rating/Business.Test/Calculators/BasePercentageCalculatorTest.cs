using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	public abstract class BasePercentageCalculatorTest : CalculatorTest
	{
		#region Validation

		public void TestPercentageCalculatorValidationOfDuplicateAllChargesOnDifferentLines()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("ORG");
			var line = entry.RateLines.AddNew();
			line.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			line.TL_RateDesc = "Goose";
			line.TL_RateCalculator = CalculatorCode;

			var applyToAllItem = line.RateLineItems.FindByTM_Type(CalculatorConstants.Type.ApplyTo);
			if (applyToAllItem == null)
			{
				applyToAllItem = line.RateLineItems.AddNew();
				applyToAllItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			}

			applyToAllItem.TM_Text = CalculatorConstants.Text.AllCharges;

			line.RunPreSaveValidation();
			AssertNoRowWarnings("No errors", applyToAllItem);

			var line2 = entry.RateLines.AddNew();
			line2.TL_AC = Helper.ChargeCodes["OCART"].PK;
			line2.TL_RateDesc = "Daphne";
			line2.TL_RateCalculator = CalculatorCode;

			var applyToAllItem2 = line2.RateLineItems.AddNew();
			applyToAllItem2.TM_Type = CalculatorConstants.Type.ApplyTo;
			applyToAllItem2.TM_Text = CalculatorConstants.Text.AllCharges;

			entry.RunPreSaveValidation();
			AssertHasRowWarningContaining(applyToAllItem, "This item is in conflict with");
			AssertHasRowWarningContaining(applyToAllItem2, "This item is in conflict with");

			applyToAllItem.Delete();
			entry.RunPreSaveValidation();
			AssertEquals("No more errors", false, line2.HasErrors);
		}

		public void TestTwoPercentsOfDifferentItemsHasNoError()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("WHS");

			var line1 = entry.RateLines.AddNew();
			line1.TL_AC = TestCC1.PK;
			line1.TL_RateCalculator = CalculatorCode;

			var applyToOriginItem = line1.RateLineItems.AddNew();
			applyToOriginItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			applyToOriginItem.TM_Text = CalculatorConstants.Text.OriginCharges;

			rate.RunPreSaveValidation();
			foreach (RateLineItem item in line1.RateLineItems)
			{
				AssertNoRowErrors("No errors", item);
			}

			var line2 = entry.RateLines.AddNew();
			line2.TL_AC = TestCC1.PK;
			line2.TL_RateCalculator = CalculatorCode;
			var applyToDestItem = line2.RateLineItems.AddNew();
			applyToDestItem.TM_Type = CalculatorConstants.Type.ApplyTo;
			applyToDestItem.TM_Text = CalculatorConstants.Text.DestinationCharges;

			rate.RunPreSaveValidation();
			foreach (RateLineItem item in line1.RateLineItems)
			{
				AssertNoRowErrors("No errors - as they are separate charges codes with a percent of different items", item);
			}

			foreach (RateLineItem item in line2.RateLineItems)
			{
				AssertNoRowErrors("No errors - as they are separate charges codes with a percent of different items", item);
			}
		}

		#endregion

		#region Test Data

		protected override CalculationResult AssertCalculation(AutoRatingCalculatorParameters @params, ZDecimal expectedAmount, ZString expectedDescription, string message = null)
		{
			var result = base.AssertCalculation(@params, expectedAmount, expectedDescription);
			@params.RatingContext.PercentageLinesApplied.Clear();

			if (AutoRateInfos != null)
			{
				ClearUsedByRateLines(AutoRateInfos.WhereNotNull());
			}

			return result;
		}

		AutoRateInfoCollection fAutoRateInfos;
		protected AutoRateInfoCollection AutoRateInfos
		{
			get
			{
				if (fAutoRateInfos == null)
				{
					fAutoRateInfos = new AutoRateInfoCollection(Factory);
					fAutoRateInfos.AddNew(TestCC1, "AUD", 1500M);
					fAutoRateInfos.AddNew(TestCC2, "AUD", 5643M);
					fAutoRateInfos.AddNew(TestCC3, "AUD", 4534M);
					fAutoRateInfos.AddNew(TestCC4, "AUD", 944M);
				}
				return fAutoRateInfos;
			}
		}

		#region Charges

		AccChargeCode fTestCC1;
		protected AccChargeCode TestCC1
		{
			get { return fTestCC1 ?? (fTestCC1 = NewChargeCode("TESTCC1", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Origin, Core.Constants.ChargeType.Disbursement)); }
		}

		AccChargeCode fTestCC2;
		protected AccChargeCode TestCC2
		{
			get { return fTestCC2 ?? (fTestCC2 = NewChargeCode("TESTCC2", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Origin)); }
		}

		AccChargeCode fTestCC3;
		protected AccChargeCode TestCC3
		{
			get { return fTestCC3 ?? (fTestCC3 = NewChargeCode("TESTCC3", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Freight, Core.Constants.ChargeType.Disbursement)); }
		}

		AccChargeCode fTestCC4;
		protected AccChargeCode TestCC4
		{
			get { return fTestCC4 ?? (fTestCC4 = NewChargeCode("TESTCC4", CombinedCalculator.Code, ChargeCodeGroupList.Codes.Destination)); }
		}

		AccChargeCode NewChargeCode(ZString code, ZString calculator, ZString chargeGroup, string type = "")
		{
			var result = Helper.ChargeCodes.New(code, code, calculator, chargeGroup);
			if (!string.IsNullOrEmpty(type))
			{
				result.AC_ChargeType = type;
			}

			return result;
		}

		#endregion

		#endregion
	}
}
