using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class ValueApplyToListCodeDescriptionPairListTest : TestCaseWithFactory
	{
		public void TestListForBondAmount()
		{
			CodeDescriptionPairList list = ValueApplyToListCodeDescriptionPairList.NewValueApplyToList(Core.CountryGuids.Instance.UnitedKingdom);
			Assert(!list.ContainsCode(CalculatorConstants.Text.ApplyTo.Value.SingleTransactionBondAmount));

			list = ValueApplyToListCodeDescriptionPairList.NewPercentageApplyToList(Core.CountryGuids.Instance.UnitedKingdom);
			Assert(!list.ContainsCode(CalculatorConstants.Text.ApplyTo.Value.SingleTransactionBondAmount));

			list = ValueApplyToListCodeDescriptionPairList.NewValueApplyToList(Core.CountryGuids.Instance.UnitedStates);
			Assert(list.ContainsCode(CalculatorConstants.Text.ApplyTo.Value.SingleTransactionBondAmount));

			list = ValueApplyToListCodeDescriptionPairList.NewPercentageApplyToList(Core.CountryGuids.Instance.UnitedStates);
			Assert(list.ContainsCode(CalculatorConstants.Text.ApplyTo.Value.SingleTransactionBondAmount));
		}

		public void TestNewPercentageApplyToList_IncludeLoadingAndCustomsBrokerageCharges()
		{
			var list = ValueApplyToListCodeDescriptionPairList.NewPercentageApplyToList(Core.CountryGuids.Instance.Australia);
			AssertLoadingAndCustomsBrokerageCharges("should not contain Loading and CustomsBrokerageCharges.", false);

			list = ValueApplyToListCodeDescriptionPairList.NewPercentageApplyToList(Core.CountryGuids.Instance.Australia, true);
			AssertLoadingAndCustomsBrokerageCharges("should contain Loading and CustomsBrokerageCharges.", true);

			int currentIndex = list.IndexOfCode(CalculatorConstants.Text.DestinationCharges);
			Assert("Pre-condition: currentIndex >= 0 && currentIndex + 5 < list.Count", currentIndex >= 0 && currentIndex + 5 < list.Count);

			CombineAssertions("Items should be in correct order.", () =>
			{
				AssertEquals("LoadingCharges should be after DestinationCharges.", CalculatorConstants.Text.LoadingCharges, list[++currentIndex].Code);
				AssertEquals("OriginCustomsBrokerageCharges should be after LoadingCharges.", CalculatorConstants.Text.OriginCustomsBrokerageCharges, list[++currentIndex].Code);
				AssertEquals("CustomsBrokerageCharges should be after OriginCustomsBrokerageCharges.", CalculatorConstants.Text.CustomsBrokerageCharges, list[++currentIndex].Code);
				AssertEquals("UnloadingCharges should be after CustomsBrokerageCharges.", CalculatorConstants.Text.UnloadingCharges, list[++currentIndex].Code);
				AssertEquals("Disbursements should be after UnloadingCharges.", Calculator.Items.Value.DisbursementApplyToTypes.Disbursements, list[++currentIndex].Code);
			});

			void AssertLoadingAndCustomsBrokerageCharges(string message, bool expected)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals("LoadingCharges: should meet expected value.", expected, list.ContainsCode(CalculatorConstants.Text.LoadingCharges));
					AssertEquals("OriginCustomsBrokerageCharges: should meet expected value.", expected, list.ContainsCode(CalculatorConstants.Text.OriginCustomsBrokerageCharges));
					AssertEquals("CustomsBrokerageCharges: should meet expected value.", expected, list.ContainsCode(CalculatorConstants.Text.CustomsBrokerageCharges));
					AssertEquals("UnloadingCharges: should meet expected value.", expected, list.ContainsCode(CalculatorConstants.Text.UnloadingCharges));
				});
			}
		}

		public void TestGetDescriptionFromCode_IncludeLoadingAndCustomsBrokerageCharges()
		{
			AssertDescription(CalculatorConstants.Text.LoadingCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.LoadingChargesDescription);
			AssertDescription(CalculatorConstants.Text.OriginCustomsBrokerageCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.OriginCustomsBrokerageChargesDescription);
			AssertDescription(CalculatorConstants.Text.CustomsBrokerageCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.CustomsBrokerageChargesDescription);
			AssertDescription(CalculatorConstants.Text.UnloadingCharges, Calculator.Items.Value.ChargesApplyToTypesDescription.UnloadingChargesDescription);

			void AssertDescription(ZString code, ZString expectedDesc)
			{
				AssertEquals("Description mismatch", expectedDesc, ValueApplyToListCodeDescriptionPairList.GetDescriptionFromCode(code, CountryGuids.Instance.Australia));
			}
		}
	}

	[TestedType(typeof(CustomConversionFactor))]
	public class CustomConversionFactorTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor_RateLineUnitIsEmpty_DefaultValuesToZero()
		{
			var costing = Factory.New<Costing>();
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST);

			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.TL_WeightVolume = ZString.Empty;
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 100;

			var factor = new CustomConversionFactor(rateLine);
			AssertEquals("WeightPrice", 0m, factor.WeightPrice);
			AssertEquals("VolumePrice", 0m, factor.VolumePrice);
		}

		public void TestConversionFactorProperty()
		{
			var costing = Factory.New<Costing>();
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST);
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_WeightVolume = Constants.Weight.Kilograms;

			var factor = new CustomConversionFactor(rateLine);
			factor.WeightUnit = "KG";
			factor.VolumeUnit = "M3";

			factor.WeightPrice = 0m;
			factor.VolumePrice = 0m;
			AssertEquals("", factor.ConversionFactor);

			factor.WeightPrice = 6m;
			factor.VolumePrice = 0m;
			AssertEquals("", factor.ConversionFactor);

			factor.WeightPrice = 0m;
			factor.VolumePrice = 1000m;
			AssertEquals("", factor.ConversionFactor);

			factor.WeightPrice = 6000m;
			factor.VolumePrice = 2m;
			AssertEquals("3000 M3/KG", factor.ConversionFactor);

			factor.WeightPrice = 1000m;
			factor.VolumePrice = 3m;
			AssertEquals("When KG/M3 and M3/KG both have more than three decimal places, we will pick the highest value.", "333.33333333333333333333333333 M3/KG", factor.ConversionFactor);

			factor.WeightPrice = 3m;
			factor.VolumePrice = 7m;
			AssertEquals("When KG/M3 and M3/KG both have more than three decimal places, we will pick the highest value.", "2.3333333333333333333333333333 KG/M3", factor.ConversionFactor);

			factor.WeightPrice = 2.2m;
			factor.VolumePrice = 660m;
			AssertEquals("When KG/M3 has less than three decimal places and M3/KG is not, we will pick the value of KG/M3.", "300 KG/M3", factor.ConversionFactor);

			rateLine.TL_WeightVolume = Constants.Volume.CubicMetres;
			factor = new CustomConversionFactor(rateLine);
			factor.WeightUnit = "KG";
			factor.VolumeUnit = "M3";
			factor.WeightPrice = 2;
			factor.VolumePrice = 332m;
			AssertEquals("166 KG/M3", factor.ConversionFactor);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CustomConversionFactor(null);
		}

		#endregion
	}
}
