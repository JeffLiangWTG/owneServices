using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	class RefCusCodeListAttributeNameListTest : TestCaseWithFactory
	{
		public void TestGetList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("FR", "French");
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeType("Z!Z", "DESC");
			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice, "BOB", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, C.RefCusCodeListTypes.Codes.CustomsOffice, "B1B", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var codeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, "Z!Z", "B2B", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var codeList4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice, "B3B", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("t1", "t1 Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("t2", "t2 Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("t3", "t3 Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Ethiopia);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("t4", "t4 Desc.", "Z!Z", Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("t5", "t5 Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea);
			var refCusCodeListAttributeName6 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("t6", "t6 Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea);
			var refCusCodeListAttributeName7 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("t7", "t7 Desc.", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea);
			var refCusCodeListAttributeNameLanguage6 = Factory.New<RefCusCodeListAttributeNameLanguage>();
			refCusCodeListAttributeNameLanguage6.ZXH_ZXE_CodeListAttributeName = refCusCodeListAttributeName6.PK;
			refCusCodeListAttributeNameLanguage6.ZXH_ZX6_NKLanguage = "FR";
			refCusCodeListAttributeNameLanguage6.ZXH_Description = "t6 Décrire.";
			var refCusCodeListAttributeNameLanguage7 = Factory.New<RefCusCodeListAttributeNameLanguage>();
			refCusCodeListAttributeNameLanguage7.ZXH_ZXE_CodeListAttributeName = refCusCodeListAttributeName7.PK;
			refCusCodeListAttributeNameLanguage7.ZXH_ZX6_NKLanguage = "FR";
			refCusCodeListAttributeNameLanguage7.ZXH_Name = "t7 nom";
			refCusCodeListAttributeNameLanguage7.ZXH_Description = "t7 Décrire.";
			codeList1.Attributes.AddNew("T1", "A");
			codeList1.Attributes.AddNew("T2", "B");
			codeList2.Attributes.AddNew("T3", "C");
			codeList3.Attributes.AddNew("T4", "D");
			codeList4.Attributes.AddNew("T5", "E");
			Factory.Save();
			var list1 = RefCusCodeListAttributeNameList.GetListForFilter(Factory, "!@", Core.Constants.CountryCodes.Eritrea);
			var list2 = RefCusCodeListAttributeNameList.GetListForFilter(Factory, "!@", Core.Constants.CountryCodes.Eritrea);
			AssertEquals("Is cached", list2, list1);
			AssertEquals("list1.Count", 0, list1.Count);
			list1 = RefCusCodeListAttributeNameList.GetListForFilter(Factory, "Z!Z", Core.Constants.CountryCodes.Eritrea);
			list2 = RefCusCodeListAttributeNameList.GetListForFilter(Factory, "Z!Z", Core.Constants.CountryCodes.Eritrea);
			AssertEquals("Is cached", list2, list1);
			AssertEquals("list1.Count", 1, list1.Count);
			AssertEquals("description", "t4 Desc.", list1.GetDescriptionFromCode("T4"));
			list1 = RefCusCodeListAttributeNameList.GetListForFilter(Factory, C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea);
			list2 = RefCusCodeListAttributeNameList.GetListForFilter(Factory, C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea);
			AssertEquals("Is cached", list2, list1);
			AssertEquals("list1.Count", 5, list1.Count);
			AssertEquals("description", "t1 Desc.", list1.GetDescriptionFromCode("T1"));
			AssertEquals("description", "t2 Desc.", list1.GetDescriptionFromCode("T2"));
			AssertEquals("description", "t5 Desc.", list1.GetDescriptionFromCode("t5"));
			AssertEquals("description", "t6 Décrire.", list1.GetDescriptionFromCode("T6"));
			AssertEquals("description", "t7 nom - t7 Décrire.", list1.GetDescriptionFromCode("T7"));
			list1 = RefCusCodeListAttributeNameList.GetListForFilter(Factory, "Z!Z", ZString.Empty, true);
			list2 = RefCusCodeListAttributeNameList.GetListForFilter(Factory, "Z!Z", ZString.Empty, true);
			AssertEquals("Is cached", list2, list1);
			AssertEquals("list1.Count", 1, list1.Count);
			AssertEquals("description", "t4 Desc.", list1.GetDescriptionFromCode("T4"));
			list1 = RefCusCodeListAttributeNameList.GetListForFilter(Factory, "Z!Z", ZString.Empty, false);
			list2 = RefCusCodeListAttributeNameList.GetListForFilter(Factory, "Z!Z", ZString.Empty, false);
			AssertEquals("Is cached", list2, list1);
			AssertEquals("list1.Count", 0, list1.Count);
			list1 = RefCusCodeListAttributeNameList.GetListForFilter(Factory, ZString.Empty, Core.Constants.CountryCodes.Ethiopia, true);
			list2 = RefCusCodeListAttributeNameList.GetListForFilter(Factory, ZString.Empty, Core.Constants.CountryCodes.Ethiopia, true);
			AssertEquals("Is cached", list2, list1);
			AssertEquals("list1.Count", 1, list1.Count);
			AssertEquals("description", "t3 Desc.", list1.GetDescriptionFromCode("T3"));
		}
	}
}
