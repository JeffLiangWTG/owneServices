using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using C = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	class ZZRefCusCodeListAttributeCombinedLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNameList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("T1", "Desc. T1", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("T2", "Desc. T2", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("T3", "Desc. T3", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Ethiopia, C.RefCusCodeListTypes.Codes.CustomsOffice);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("T4", "Desc. T4", C.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.Facilities);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("T5", "Desc. T5", C.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice);
			var codeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice, "BOB", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			codeList1.Attributes.AddNew("T1", "A");
			codeList1.Attributes.AddNew("T2", "B");
			var codeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Ethiopia, C.RefCusCodeListTypes.Codes.CustomsOffice, "B1B", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			codeList2.Attributes.AddNew("T3", "C");
			var codeList3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.Facilities, "B2B", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			codeList3.Attributes.AddNew("T4", "D");
			var codeList4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, C.RefCusCodeListTypes.Codes.CustomsOffice, "B3B", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			codeList4.Attributes.AddNew("T5", "E");
			Factory.Save();
			var cusCode = Factory.New<ZZRefCusCodeListCombined>();
			var attrib1 = cusCode.Attributes.AddNew();
			var attrib2 = cusCode.Attributes.AddNew();
			var list1 = attrib1.Lookups.NameList;
			var list2 = attrib2.Lookups.NameList;
			AssertEquals("Is cached", list2, list1);
			AssertEquals("list1.Count", 0, list1.Count);
			cusCode.ZZD_CodeType = C.RefCusCodeListTypes.Codes.Facilities;
			cusCode.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			list1 = attrib1.Lookups.NameList;
			list2 = attrib2.Lookups.NameList;
			AssertEquals("Is cached", list2, list1);
			AssertEquals("list1.Count", 1, list1.Count);
			AssertEquals("description", "Desc. T4", list1.Cast<RefCusCodeListAttributeName>().ElementAt(0).ZXE_Description);
			AssertEquals("column", "T4", list1.Cast<RefCusCodeListAttributeName>().ElementAt(0).ZXE_ColumnCaption);
			cusCode.ZZD_CodeType = C.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCode.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			list1 = attrib1.Lookups.NameList;
			list2 = attrib2.Lookups.NameList;
			AssertEquals("Is cached", list2, list1);
			AssertEquals("list1.Count", 3, list1.Count);
			AssertEquals("description", "Desc. T1", list1.Cast<RefCusCodeListAttributeName>().ElementAt(0).ZXE_Description);
			AssertEquals("column", "T1", list1.Cast<RefCusCodeListAttributeName>().ElementAt(0).ZXE_ColumnCaption);
			AssertEquals("description", "Desc. T2", list1.Cast<RefCusCodeListAttributeName>().ElementAt(1).ZXE_Description);
			AssertEquals("column", "T2", list1.Cast<RefCusCodeListAttributeName>().ElementAt(1).ZXE_ColumnCaption);
			AssertEquals("description", "Desc. T5", list1.Cast<RefCusCodeListAttributeName>().ElementAt(2).ZXE_Description);
			AssertEquals("column", "T5", list1.Cast<RefCusCodeListAttributeName>().ElementAt(2).ZXE_ColumnCaption);
		}

		public void TestValueList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeListType1 = helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.Facilities, "Facilities", Core.Constants.CountryCodes.Eritrea);
			var codeListType2 = helper.CreateCusCodeType(C.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice", Core.Constants.CountryCodes.Eritrea);
			var officeCodeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeListType2.ZZK_CodeType, "OF1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var officeCodeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeListType2.ZZK_CodeType, "OF2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var codeListAttributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ABC", "DESC ABC", codeListType1.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, codeListType1.ZZK_CodeType);
			var codeListAttributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DEF", "DESC DEF", codeListType1.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, codeListType2.ZZK_CodeType);
			Factory.Save();
			var cusCode = Factory.New<ZZRefCusCodeListCombined>();
			cusCode.ZZD_CodeType = codeListType1.ZZK_CodeType;
			cusCode.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			var attrib1 = cusCode.Attributes.AddNew();
			var list1 = attrib1.Lookups.ValueList;
			AssertEquals("list1.Count", 0, list1.Count);
			attrib1.ZZE_ZXE_NKName = codeListAttributeName1.ZXE_Name;
			list1 = attrib1.Lookups.ValueList;
			AssertEquals("list1.Count", 0, list1.Count);
			codeListAttributeName1.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean;
			list1 = attrib1.Lookups.ValueList;
			AssertSame(Factory.GetCachedValue<YesNoList>(), list1);
			attrib1.ZZE_ZXE_NKName = codeListAttributeName2.ZXE_Name;
			list1 = attrib1.Lookups.ValueList;
			AssertEquals("list1.Count", 2, list1.Count);
			Assert(list1.ContainsCode("OF1"));
			Assert(list1.ContainsCode("OF2"));
		}
	}
}
