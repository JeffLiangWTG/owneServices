using System;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(HolderOfTheTransitProcedureProvider))]
sealed class HolderOfTheTransitProcedureProviderTest : PartyProviderAbstractTest<HolderOfTheTransitProcedureProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new HolderOfTheTransitProcedureProvider(null));

	public void TestNew()
	{
		AssertNull(HolderOfTheTransitProcedureProvider.New(null));
	}

	public void TestTIRHolderIdentificationNumber()
	{
		AssertEquals("NL987654321", provider.TIRHolderIdentificationNumber);
		nctsHeader.Principal.Organisation.CustomsCodes.RemoveAll();
		AssertEquals(string.Empty, provider.TIRHolderIdentificationNumber);
	}

	protected override bool ExpectedOmitNameAndAddressWhenIDIsFound => true;
}
