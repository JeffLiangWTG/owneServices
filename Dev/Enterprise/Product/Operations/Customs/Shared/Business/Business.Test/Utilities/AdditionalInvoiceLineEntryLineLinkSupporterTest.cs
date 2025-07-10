using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AdditionalInvoiceLineEntryLineLinkSupporterTest : TestCase
	{
		public void TestDoesSupport()
		{
			AssertEquals("DoesSupport", false, AdditionalInvoiceLineEntryLineLinkSupporter.DoesSupport(Core.Constants.CountryCodes.NewZealand));
			AssertEquals("ZA should not support", false, AdditionalInvoiceLineEntryLineLinkSupporter.DoesSupport(Core.Constants.CountryCodes.SouthAfrica));
			foreach (var code in
				new[]
				{
					Core.Constants.CountryCodes.PuertoRico,
					Core.Constants.CountryCodes.UnitedStates,
					Core.Constants.CountryCodes.Canada,
					Core.Constants.CountryCodes.China,
					Core.Constants.CountryCodes.KoreaSouth,
					Core.Constants.CountryCodes.Brazil,
					"ER"
				})
			{
				AssertEquals("DoesSupport", true, AdditionalInvoiceLineEntryLineLinkSupporter.DoesSupport(code));
			}
		}
	}
}
