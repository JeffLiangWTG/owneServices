using System.Linq;
using Enterprise.Core;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Moq;
using static Enterprise.ZArchitecture.Business.ConversionFactorValidation;

namespace Enterprise.Rating.Business
{
	public class RateLineConversionFactorValidationTest : RatingTestCase
	{
		public void TestConversionFactorStringValidation()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			var rateLine = rateEntry.RateLines.AddNew();

			var model = new ConversionFactorViewModel(
				m => new Mock<IConversionFactorLookups>().Object,
				m => new RateLineConversionFactorValidation(m, rateLine));

			var supportedUnits = new[]
			{
				new UnitsList(MeasureUnitType.Weight, Constants.Weight.Codes),
				new UnitsList(MeasureUnitType.Volume, Constants.Volume.Codes),
				new UnitsList(MeasureUnitType.LoadingLength, Constants.LoadingLength.Codes.ToArray()),
			};

			model.ConversionFactorString = "-10 KG/M3";
			AssertHasError("Factor is negative", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "10 XX/M3";
			AssertHasError("Numerator is unknown", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "60000 250KG/M3";
			AssertHasError("Numerator is unknown", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "10 KG/XX";
			AssertHasError("Denominator is unknown", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "60000 KG/M3170";
			AssertHasError("Denominator is unknown", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "60000 250KG/M3170";
			AssertHasError("Denominator is unknown", model.ConversionFactorStringInfo, ConversionFactorValidation.GetInvalidFormatErrorMessage(supportedUnits));

			model.ConversionFactorString = "10 CI/M3";
			AssertHasError("Numerator and denominator are of smae measurement type", model.ConversionFactorStringInfo, ConversionFactorValidation.GetMeasurementTypeErrorMessage("CI", "M3", supportedUnits));

			model.ConversionFactorString = "10 KG/G";
			AssertHasError("Numerator and denominator are of smae measurement type", model.ConversionFactorStringInfo, ConversionFactorValidation.GetMeasurementTypeErrorMessage("KG", "G", supportedUnits));

			model.ConversionFactorString = "10 KG/M3";
			AssertNoErrors(model.ConversionFactorStringInfo);

			model.ConversionFactorString = "	10		KG	/M3	";
			AssertNoErrors(model.ConversionFactorStringInfo);

			rateLine.UseOnlyActualWeightMeasure = false;
			rateLine.TL_WeightVolume = Constants.LoadingLength.LoadingMeters;
			model.ConversionFactorString = string.Empty;
			AssertHasError("Factor is empty string and the TL_WeightVolume is loading meter", model.ConversionFactorStringInfo, ErrorMessages.ConversionFactorMustBeEntered);

			rateLine.ConversionFactor = new ConversionFactor(1000m, "250KG", "M3");
			AssertEquals("ConversionFactorForBinding.ConversionFactor", new ConversionFactor(1000m, "250KG", "M3"), rateLine.ConversionFactorForBinding.ConversionFactor);

			rateLine.ConversionFactor = new ConversionFactor(2000m, "KG", "M3170");
			AssertEquals("ConversionFactorForBinding.ConversionFactor", new ConversionFactor(2000m, "KG", "M3170"), rateLine.ConversionFactorForBinding.ConversionFactor);
		}
	}
}
