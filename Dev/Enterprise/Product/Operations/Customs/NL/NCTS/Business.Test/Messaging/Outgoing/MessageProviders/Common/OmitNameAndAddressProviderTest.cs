using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(OmitNameAndAddressPartyProvider))]
sealed class OmitNameAndAddressProviderTest : PartyProviderAbstractTest<OmitNameAndAddressPartyProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new OmitNameAndAddressPartyProvider(null));

	public void TestNew()
	{
		AssertNull(OmitNameAndAddressPartyProvider.New(null));
	}

	public void TestID_Empty()
	{
		nctsHeader.Principal.Organisation.CustomsCodes.RemoveAll();
		AssertNullOrEmpty("Empty ID", provider.Id);
	}

	public void TestID_TCU()
	{
		var customsCodes = nctsHeader.Principal.Organisation.CustomsCodes;
		customsCodes.RemoveAll();
		CreateCustomsCode(customsCodes, OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "543212345");
		AssertEquals("TCU nr", "NL543212345", provider.Id);
	}

	public void TestID_EOR()
	{
		CreateCustomsCode(nctsHeader.Principal.Organisation.CustomsCodes, OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "543212345");
		AssertEquals("EOR nr when also TCU", "NL123456789", provider.Id);
	}

	protected override bool ExpectedOmitNameAndAddressWhenIDIsFound => true;
}
