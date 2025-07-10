using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class JobDeclarationValueSetStrategyTest : TestCaseWithFactory
{
	public void TestSetBox14Representation()
	{
		var importerIndirect = Factory.NewWithValidTestData<OrgHeader>();
		importerIndirect.OH_Code = "COD";
		var impAddInfo = EUOrgImpAddInfo.Get(importerIndirect, declaration.Country.Code);
		impAddInfo.ZO_Box14UseIndirectRepresentation = true;

		var importerNoIndirect = Factory.NewWithValidTestData<OrgHeader>();
		importerNoIndirect.OH_Code = "DOC";
		var impAddInfo2 = EUOrgImpAddInfo.Get(importerNoIndirect, declaration.Country.Code);
		impAddInfo2.ZO_Box14UseIndirectRepresentation = false;

		Factory.Save();

		CombineAssertions(() =>
		{
			declaration.JE_OH_Importer = importerIndirect.PK;
			AssertEquals("Representative use indirect Representation", PLRepresentationTypeList.Codes._5Indirect, declaration.JE_DeclarantType);

			declaration.JE_OH_Importer = importerNoIndirect.PK;
			AssertEquals("Representative doesn't use indirect Representation", PLRepresentationTypeList.Codes._4Direct, declaration.JE_DeclarantType);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
	}

	JobDeclaration declaration;
}
