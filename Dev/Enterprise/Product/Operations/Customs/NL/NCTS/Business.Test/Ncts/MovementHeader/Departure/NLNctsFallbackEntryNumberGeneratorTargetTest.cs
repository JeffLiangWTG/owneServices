using System;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class NLNctsFallbackEntryNumberGeneratorTargetTest : NumberGeneratorTargetTest
{
	public void TestOverrides()
	{
		var customisation = new NLNctsFallbackEntryNumberCustomisation();
		NLCustomsRegistry.Instance.NLNctsFallbackEntryNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);

		var target = new NLNctsFallbackEntryNumberGeneratorTarget { Context = new NumberGeneratorContext() };
		CombineAssertions(() =>
		{
			AssertCustomisation("Should find the customisation", "2", target.NumberCustomisation, BillOfLadingNumberCustomisationElement.Keys.YearAsDigit);
			AssertLocation(NLCustomsRegistry.Instance.NLNctsFallbackEntryNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(NctsDepartureMovementHeader.Schema.FallbackEntryNumberLength, target.MaxLength);
			AssertEquals("NLNctsFallbackEntryNumberCustomisation", target.Name);
		});
	}
}
