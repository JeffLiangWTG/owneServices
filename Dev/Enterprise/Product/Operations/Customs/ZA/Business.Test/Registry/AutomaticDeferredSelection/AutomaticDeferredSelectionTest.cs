using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(AutomaticDeferredSelection))]
	sealed class AutomaticDeferredSelectionTest : RegistryBusinessObjectTemplateTestCase<AutomaticDeferredSelection>
	{
		public void TestDaysBeforeETA()
		{
			var automaticDeferredSelection = new AutomaticDeferredSelection();
			automaticDeferredSelection.AllowAutomaticDeferredSelection = true;
			automaticDeferredSelection.DaysBeforeETA = 0;
			AssertNoError(automaticDeferredSelection.DaysBeforeETAInfo, "No. of Days Before ETA to Submit cannot be negative.");
			automaticDeferredSelection.AllowAutomaticDeferredSelection = false;
			automaticDeferredSelection.DaysBeforeETA = 0;
			AssertNoError(automaticDeferredSelection.DaysBeforeETAInfo, "No. of Days Before ETA to Submit cannot be negative.");
			automaticDeferredSelection.AllowAutomaticDeferredSelection = true;
			automaticDeferredSelection.DaysBeforeETA = -1;
			AssertHasError(automaticDeferredSelection.DaysBeforeETAInfo, "No. of Days Before ETA to Submit cannot be negative.");
			automaticDeferredSelection.DaysBeforeETA = 1;
			AssertNoError(automaticDeferredSelection.DaysBeforeETAInfo, "No. of Days Before ETA to Submit cannot be negative.");
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override AutomaticDeferredSelection GetBusinessObjectToClone() => (AutomaticDeferredSelection)GetNewBusinessObject();

		protected override AutomaticDeferredSelection GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
