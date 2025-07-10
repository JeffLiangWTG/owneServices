using System;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class DeclarantProviderTest : Customs.Business.Testing.DataProviderTestCase<AESDeclarantProvider>
{
	public void TestNewOrNull()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null OrgAddress", AESDeclarantProvider.NewOrNull(null, null));
			AssertNull("Null OrgHeader", AESDeclarantProvider.NewOrNull(Factory.New<OrgAddress>(), null));
			AssertExceptionThrown<ArgumentNullException>("Null JobDeclaration", "Value cannot be null.\r\nParameter name: jobDeclaration",
				() => AESDeclarantProvider.NewOrNull(orgAddress, null));
			AssertNotNull("All data is valid", AESDeclarantProvider.NewOrNull(orgAddress, declaration));
		});
	}

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Eori exists", "PL443322", Provider.IdentificationNumber);

			orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
			AssertEquals("Eori does not exist", string.Empty, GetProvider().IdentificationNumber);
		});
	}

	public void TestName()
	{
		CombineAssertions(() =>
		{
			AssertNull("Eori exists", Provider.Name);

			orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.NIP;
			AssertEquals("Eori does not exist", "Declarant name", GetProvider().Name);
		});
	}

	public void TestAddress()
	{
		CombineAssertions(() =>
		{
			AssertNull("Eori exists", Provider.Address);

			orgCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
			AssertNotNull("Eori does not exist", GetProvider().Address);
		});
	}

	public void TestContactPerson()
	{
		CombineAssertions(() =>
		{
			GlbStaff.CurrentUser.GS_PublishWorkPhone = true;
			GlbStaff.CurrentUser.GS_WorkPhone = "12345";
			AssertNotNull("Not null", Provider.ContactPerson);
			AssertEquals("Mapped from current user", GlbStaff.CurrentUser.GS_FullName, Provider.ContactPerson.Name);

			var glbStaff = Factory.New<GlbStaff>();
			glbStaff.GS_FullName = "AgentName";
			glbStaff.GS_Code = "ZZZ";
			glbStaff.GS_PublishWorkPhone = true;
			glbStaff.GS_WorkPhone = "12345";
			declaration.JE_GS_NKCusAgent = glbStaff.GS_Code;

			AssertEquals("Mapped from cus agent", "AgentName", GetProvider().ContactPerson.Name);
			glbStaff.GS_FullName = string.Empty;
			AssertNull("Null if name is empty", GetProvider().ContactPerson);

			glbStaff.GS_FullName = "AgentName";
			glbStaff.GS_WorkPhone = string.Empty;
			AssertNull("Null if phone is empty", GetProvider().ContactPerson);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Declarant name";
		orgCusCode = orgHeader.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		orgCusCode.OK_CustomsRegNo = "443322";
		declaration.JE_OA_DeclarantAddress = orgHeader.PK;
		orgAddress = orgHeader.Addresses.AddNew();
	}

	OrgAddress orgAddress;
	OrgHeader orgHeader;
	JobDeclaration declaration;
	OrgCusCode orgCusCode;

	protected override AESDeclarantProvider GetProvider() => AESDeclarantProvider.NewOrNull(orgAddress, declaration);
}
