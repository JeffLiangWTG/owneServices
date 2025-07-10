using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(UnitConverterHelper))]
	sealed class UnitConverterHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConvertToCW1StandardWeightUnit()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(UnitConverterHelper.ConvertToCW1StandardWeightUnit(Constants.UnitOfQuantityCodes.Kilograms), NUnit.Framework.Is.EqualTo(Core.Constants.Weight.Kilograms));
				NUnit.Framework.Assert.That(UnitConverterHelper.ConvertToCW1StandardWeightUnit(Constants.UnitOfQuantityCodes.Tonnes), NUnit.Framework.Is.EqualTo(Core.Constants.Weight.Tonnes));
				NUnit.Framework.Assert.That(UnitConverterHelper.ConvertToCW1StandardWeightUnit(Constants.UnitOfQuantityCodes.Gram), NUnit.Framework.Is.EqualTo(Core.Constants.Weight.Grams));
				NUnit.Framework.Assert.That(UnitConverterHelper.ConvertToCW1StandardWeightUnit(Constants.UnitOfQuantityCodes.Pound), NUnit.Framework.Is.EqualTo(Core.Constants.Weight.Pounds));
				NUnit.Framework.Assert.That(UnitConverterHelper.ConvertToCW1StandardWeightUnit("XX"), NUnit.Framework.Is.EqualTo("XX"));
			});
		}

		[ExpectNoExceptions]
		public void TestCanConvertFromNetWeightToCustomsUnit()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(UnitConverterHelper.CanConvertFromNetWeightToCustomsUnit(1m, Core.Constants.Weight.Kilograms, Constants.UnitOfQuantityCodes.Kilograms), NUnit.Framework.Is.EqualTo(true), "Can Convert From NetWeight To CustomsUnit.");
				NUnit.Framework.Assert.That(UnitConverterHelper.CanConvertFromNetWeightToCustomsUnit(0, Core.Constants.Weight.Kilograms, Constants.UnitOfQuantityCodes.Kilograms), NUnit.Framework.Is.EqualTo(false), "NetWeight is 0, conversion to CustomsUnit is not allowed.");
				NUnit.Framework.Assert.That(UnitConverterHelper.CanConvertFromNetWeightToCustomsUnit(1m, "AA", Constants.UnitOfQuantityCodes.Kilograms), NUnit.Framework.Is.EqualTo(false), "Constants Weights not Contains NetWeightUQ, conversion to CustomsUnit is not allowed.");
				NUnit.Framework.Assert.That(UnitConverterHelper.CanConvertFromNetWeightToCustomsUnit(1m, Core.Constants.Weight.Kilograms, Constants.UnitOfQuantityCodes.Yards), NUnit.Framework.Is.EqualTo(false), "customsUnit is not a weight unit, conversion to CustomsUnit is not allowed.");
			});
		}
	}
}
