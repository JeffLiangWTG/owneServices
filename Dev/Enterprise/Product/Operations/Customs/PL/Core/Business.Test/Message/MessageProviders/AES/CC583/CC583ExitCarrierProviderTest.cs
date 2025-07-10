using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC583ExitCarrierProviderTest : DataProviderTestCase<CC583ExitCarrierProvider>
{
	public void TestIdentificationNumber()
	{
		var carrierCusCode = shippingLine.CustomsCodes.AddNew();
		carrierCusCode.OK_CustomsRegNo = "123";
		CombineAssertions(() =>
		{
			carrierCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.ConsigneeExemptNo;
			AssertNullOrEmpty("Declaration Shipping Line without OrgCusCode of type EOR", GetProvider().IdentificationNumber);

			carrierCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			AssertEquals("Declaration Shipping Line with OrgCusCode of type EOR", "PL123", GetProvider().IdentificationNumber);
		});
	}

	public void TestName()
	{
		shippingLine.OH_FullName = "TestName";
		AssertEquals("TestName", GetProvider().Name);
	}

	public void TestAddress()
	{
		var address = shippingLine.MainAddress;
		address.OA_PostCode = "TST_CODE";
		CombineAssertions(() =>
		{
			AssertType<AddressProvider>("Type of address provider.", GetProvider().Address);
			AssertEquals("Shipping line main address post code.", "TST_CODE", GetProvider().Address.PostCode);
		});
	}

	protected override CC583ExitCarrierProvider GetProvider() => new(declaration);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		shippingLine = Factory.New<OrgHeader>();
		declaration.JE_OH_ShippingLine = shippingLine.PK;
	}

	JobDeclaration declaration;
	OrgHeader shippingLine;
}
