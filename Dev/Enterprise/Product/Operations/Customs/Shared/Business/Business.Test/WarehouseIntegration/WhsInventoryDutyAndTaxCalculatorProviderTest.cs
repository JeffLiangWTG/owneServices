using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class WhsInventoryDutyAndTaxCalculatorProviderTest : TestCaseWithFactory
	{
		public void TestNotImplementedExceptionForCountry()
		{
			AssertExceptionThrown<NotImplementedException>("Not implemented", "WhsInventoryDutyAndTaxCalculator is not currently implemented for 'BQ'.", () => ObjectFactory.Get<Integration.Customs.IWhsInventoryDutyAndTaxCalculatorProvider>().GetProviderFor(Factory, Core.Constants.CountryCodes.BonaireSintEustatiusAndSaba));
		}

		public void TestCalculatorIsCached()
		{
			var provider = ObjectFactory.Get<Integration.Customs.IWhsInventoryDutyAndTaxCalculatorProvider>();
			var calculator1 = provider.GetProviderFor(Factory, Core.Constants.CountryCodes.SouthAfrica);
			var calculator2 = provider.GetProviderFor(Factory, Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals(true, ReferenceEquals(calculator1, calculator2));
			var calculator3 = provider.GetProviderFor(new BusinessObjectFactory(), Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals(false, ReferenceEquals(calculator1, calculator3));
		}
	}
}
