using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ConsigneeConsignorProvider_CC515_CC513Test : AESConsigneeConsignorProviderTest
{
	public void TestNameInTransitionPeriod()
	{
		CombineAssertions(() =>
		{
			orgAddress.Header.OH_FullName = new ZString('A', 70);
			AssertEquals("Header name", new string('A', 35), GetProvider().Name);
			orgAddress.OA_CompanyNameOverride = new ZString('B', 70);
			AssertEquals("Address name", new string('B', 35), GetProvider().Name);
			orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234", "PL");
			AssertNull("Identification number is not empty", GetProvider().Name);
		});
	}

	public override void TestAddressType() => AssertType<AddressProvider_CC515_CC513>(GetProvider().Address);

	protected override AESConsigneeConsignorProvider GetProvider() =>
		ConsigneeConsignorProvider_CC515_CC513.NewOrNull(orgAddress, isAesTransitionPeriod: true);
}
