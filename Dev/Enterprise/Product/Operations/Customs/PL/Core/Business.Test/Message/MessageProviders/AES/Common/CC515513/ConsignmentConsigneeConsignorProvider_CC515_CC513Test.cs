using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

class ConsignmentConsigneeConsignorProvider_CC515_CC513Test : AESConsignmentConsigneeConsignorProviderTest
{
	public void TestNameInTransitionPeriod()
	{
		docAddress.E2_AddressOverride = true;
		docAddress.E2_CompanyName = new('A', 70);
		orgHeader.OH_FullName = new('B', 70);

		CombineAssertions(() =>
		{
			AssertEquals("Name from DocAddress Company", new string('A', 35), GetProvider().Name);
			docAddress.E2_AddressOverride = false;
			AssertEquals("Name from Org", new string('B', 35), GetProvider().Name);

			orgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234", "PL");
			AssertNull("Identification number not empty", GetProvider().Name);
		});
	}

	public override void TestAddressType() => AssertType<AddressProvider_CC515_CC513>(GetProvider().Address);

	protected override AESConsignmentConsigneeConsignorProvider GetProvider() =>
		ConsignmentConsigneeConsignorProvider_CC515_CC513.NewOrNull(docAddress, isAesTransitionPeriod: true);
}
