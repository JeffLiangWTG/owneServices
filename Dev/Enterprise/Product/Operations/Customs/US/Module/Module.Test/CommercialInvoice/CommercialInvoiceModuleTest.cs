using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceModule))]
	sealed class CommercialInvoiceModuleTest : Customs.Module.Testing.CommercialInvoiceModuleTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;
	}
}
