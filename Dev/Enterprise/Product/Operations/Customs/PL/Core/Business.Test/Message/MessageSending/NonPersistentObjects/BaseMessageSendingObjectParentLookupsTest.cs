using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class BaseMessageSendingObjectParentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestProcedureList()
	{
		var sendingObj = GetJobDeclarationMessageSendingObjectParent();
		CombineAssertions(() =>
		{
			var list = sendingObj.Lookups.ProcedureList;
			AssertEquals("CodesAsString",
				"Zwykła, Na wezwanie urzędu celnego, Zgłoszenie niekompletne, Zgłoszenie uproszczone, Uproszczone w miejscu ze zgłoszeniem uproszczonym",
				list.CodesAsString);
			AssertSame("Cached", list, sendingObj.Lookups.ProcedureList);
		});
	}

	public void TestPurposeOfSendingList()
	{
		var sendingObj = GetJobDeclarationMessageSendingObjectParent();
		CombineAssertions(() =>
		{
			var list = sendingObj.Lookups.PurposeOfSendingList;
			AssertEquals("CodesAsString", "Wymagania formalne, Na wezwanie urzędu celnego", list.CodesAsString);
			AssertSame("Cached", list, sendingObj.Lookups.PurposeOfSendingList);
		});
	}

	public void TestEnquiryInformationCodeList()
	{
		var sendingObj = GetJobDeclarationMessageSendingObjectParent();
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL210;
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "3333", "description1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "4444", "description2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Iraq, codeType, "5555", "description2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();

		var codes = sendingObj.Lookups.EnquiryInformationCodeList;
		CombineAssertions(() =>
		{
			AssertNotEquals("Not Contains 5555", true, codes.ContainsCode("5555"));
			AssertEquals("Contains 3333", true, codes.ContainsCode("3333"));
			AssertEquals("Contains 4444", true, codes.ContainsCode("4444"));
			AssertEquals("1 Description", "description1", codes.GetDescriptionFromCode("3333"));
			AssertEquals("2 Description", "description2", codes.GetDescriptionFromCode("4444"));
		});
	}

	BaseMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParent() => new BaseMessageSendingObjectParent(declaration);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
}
