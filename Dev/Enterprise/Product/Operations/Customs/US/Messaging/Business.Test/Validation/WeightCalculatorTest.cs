using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class WeightCalculatorTest : TestCase
	{
		public void TestCalculate()
		{
			var validator = new WeightValidator();
			AssertEquals(10000m, WeightCalculator.Calculate(new ZWeight(10m, Core.Constants.Weight.Tonnes)));
			AssertEquals(1m, WeightCalculator.Calculate(new ZWeight(10m, Core.Constants.Weight.Grams)));
			AssertEquals(10m, WeightCalculator.Calculate(new ZWeight(10m, Core.Constants.Weight.Pounds)));
			AssertEquals(10m, WeightCalculator.Calculate(new ZWeight(10m, Core.Constants.Weight.Kilograms)));
			AssertEquals(ZDecimal.Zero, WeightCalculator.Calculate(new ZWeight(10m, "Z!")));
			AssertEquals(validator.MaximumWholeWeightAllowed, WeightCalculator.Calculate(new ZWeight(validator.MaximumWholeWeightAllowed, Core.Constants.Weight.Kilograms)));
			AssertEquals(ZDecimal.Zero, WeightCalculator.Calculate(new ZWeight(validator.MaximumWholeWeightAllowed + 1, Core.Constants.Weight.Kilograms)));
		}

		public void TestCalculateUQ()
		{
			AssertEquals("Z!", WeightCalculator.CalculateUQ("Z!"));
			foreach (ICodeDescription pair in new CodeDescriptionPairList(OLookUpEditType.Weight))
			{
				AssertEquals(pair.Code == Core.Constants.Weight.Pounds ? Core.Constants.Weight.Pounds : Core.Constants.Weight.Kilograms, WeightCalculator.CalculateUQ(pair.Code));
			}
		}
	}
}
