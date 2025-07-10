using System.Linq;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.Testing
{
	public class RateLineConversionFactorLookupsTest : RatingTestCase
	{
		public void TestConversionFactorsList()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);
			var rateLine = rateEntry.RateLines.AddNew();

			rateLine.TL_WeightVolume = Constants.Weight.Kilograms;
			AssertConversionFactorList(rateLine.ConversionFactorForBinding.Lookups.ConversionFactors, Constants.Weight.Codes, UnitsSystem.Metric);

			rateLine.TL_WeightVolume = Constants.Weight.Pounds;
			AssertConversionFactorList(rateLine.ConversionFactorForBinding.Lookups.ConversionFactors, Constants.Weight.Codes, UnitsSystem.Imperial);

			rateLine.TL_WeightVolume = Constants.Volume.CubicMetres;
			AssertConversionFactorList(rateLine.ConversionFactorForBinding.Lookups.ConversionFactors, Constants.Volume.Codes, UnitsSystem.Metric);

			rateLine.TL_WeightVolume = Constants.Volume.CubicInches;
			AssertConversionFactorList(rateLine.ConversionFactorForBinding.Lookups.ConversionFactors, Constants.Volume.Codes, UnitsSystem.Imperial);

			rateLine.TL_WeightVolume = Constants.LoadingLength.LoadingMeters;
			AssertConversionFactorList(rateLine.ConversionFactorForBinding.Lookups.ConversionFactors, Constants.LoadingLength.Codes.ToArray(), UnitsSystem.Metric);
		}

		public void AssertConversionFactorList(CodeDescriptionPairList list, string[] expectedUnits, UnitsSystem expectedUnitsSystem)
		{
			var expectedList = ConversionFactor.Standard.All
				.Where(e => expectedUnits.Contains(e.NumeratorUnit) || expectedUnits.Contains(e.DenominatorUnit))
				.Where(e => e.UnitsSystem == expectedUnitsSystem)
				.Select(e => e.ToShortString())
				.ToList();

			expectedList.Add(ConversionFactorList.Codes.Custom);

			var actualList = list.ToArray().Select(p => p.Code);
			AssertContainsExactElementsInAnyOrder(expectedList, actualList);
		}
	}
}
