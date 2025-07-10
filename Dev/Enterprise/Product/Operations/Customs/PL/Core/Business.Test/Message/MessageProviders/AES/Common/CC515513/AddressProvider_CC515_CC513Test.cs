using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AddressProvider_CC515_CC513Test : AddressProviderTest
{
	public void TestStreetAndNumberInAesTransitionPeriod()
	{
		docAddress.E2_AddressOverride = true;
		docAddress.E2_Address1 = new ZString("  E2_Address 1111111111  ");
		docAddress.E2_Address2 = new ZString("  E2_Address 2222222222  ");
		orgAddress.OA_Address1 = new ZString("  OA_Address 1111111111  ");
		orgAddress.OA_Address2 = new ZString("  OA_Address 2222222222  ");

		CombineAssertions(() =>
		{
			AssertEquals("E2_AddressOverride", "E2_Address 1111111111 E2_Address 22", GetProvider().StreetAndNumber);

			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = ZGuid.Empty;
			AssertNull("Not E2_AddressOverride, E2_OA_Address empty", GetProvider().StreetAndNumber);
			docAddress.E2_OA_Address = orgAddress.PK;
			AssertEquals("Not E2_AddressOverride", "OA_Address 1111111111 OA_Address 22", GetProvider().StreetAndNumber);
		});
	}

	protected override AddressProvider GetProvider() =>
		new AddressProvider_CC515_CC513(docAddress, isAesTransitionPeriod: true);
}
