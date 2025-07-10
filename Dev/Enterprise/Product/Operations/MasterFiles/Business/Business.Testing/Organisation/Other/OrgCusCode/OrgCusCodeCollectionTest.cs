using System;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCusCodeCollection))]
	sealed class OrgCusCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestStringBasedIndexerToBeUsedByTheDocumentEngine()
		{
			var cusCode1 = CustomsCodes.AddNew();
			cusCode1.OK_CodeType = "XXX";
			cusCode1.OK_RN_NKCodeCountry = Constants.CountryCodes.Albania;

			var cusCode2 = CustomsCodes.AddNew();
			cusCode2.OK_CodeType = "WSC";
			cusCode2.OK_RN_NKCodeCountry = Constants.CountryCodes.Zaire;

			var cusCode3 = CustomsCodes.AddNew();
			cusCode3.OK_CodeType = "WSC";
			cusCode3.OK_RN_NKCodeCountry = Constants.CountryCodes.Albania;

			var cusCode4 = CustomsCodes.AddNew();
			cusCode4.OK_CodeType = "WSC";
			cusCode4.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.RN_Code;

			var cusCode5 = CustomsCodes.AddNew();
			cusCode5.OK_CodeType = "XXX";
			cusCode5.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.RN_Code;

			CombineAssertions(delegate
			{
				AssertEqualsCC("CustomsCodes[\"\"WSC\", \"ZR\"\"]", cusCode2, CustomsCodes["\"WSC\", \"ZR\""]);
				AssertEqualsCC("CustomsCodes[\"\"WSC\", \"AL\"\"]", cusCode3, CustomsCodes["\"WSC\", \"AL\""]);
				AssertEqualsCC("CustomsCodes[\"\"XXX\", \"ZR\"\"]", null, CustomsCodes["\"XXX\", \"ZR\""]);
				AssertEqualsCC("CustomsCodes[\"\"XXX\", \"AL\"\"]", cusCode1, CustomsCodes["\"XXX\", \"AL\""]);

				AssertEqualsCC("CustomsCodes[\"\"WSC\"]", cusCode4, CustomsCodes["\"WSC\""]);
				AssertEqualsCC("CustomsCodes[\"\"XXX\"]", cusCode5, CustomsCodes["\"XXX\""]);
			});
		}

		public void TestStringBasedIndexerToBeUsedByTheDocumentEngineSupportTwoLetterCode()
		{
			var cusCode1 = CustomsCodes.AddNew();
			cusCode1.OK_CodeType = "T1";
			cusCode1.OK_RN_NKCodeCountry = Constants.CountryCodes.Albania;

			var cusCode2 = CustomsCodes.AddNew();
			cusCode2.OK_CodeType = "T2";
			cusCode2.OK_RN_NKCodeCountry = Constants.CountryCodes.Zaire;

			var cusCode3 = CustomsCodes.AddNew();
			cusCode3.OK_CodeType = "T2";
			cusCode3.OK_RN_NKCodeCountry = Constants.CountryCodes.Albania;

			var cusCode4 = CustomsCodes.AddNew();
			cusCode4.OK_CodeType = "T2";
			cusCode4.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.RN_Code;

			var cusCode5 = CustomsCodes.AddNew();
			cusCode5.OK_CodeType = "T1";
			cusCode5.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.RN_Code;

			CombineAssertions(delegate
			{
				AssertEqualsCC("CustomsCodes[\"\"T2\", \"ZR\"\"]", cusCode2, CustomsCodes["\"T2\", \"ZR\""]);
				AssertEqualsCC("CustomsCodes[\"\"T2\", \"AL\"\"]", cusCode3, CustomsCodes["\"T2\", \"AL\""]);
				AssertEqualsCC("CustomsCodes[\"\"T1\", \"ZR\"\"]", null, CustomsCodes["\"T1\", \"ZR\""]);
				AssertEqualsCC("CustomsCodes[\"\"T1\", \"AL\"\"]", cusCode1, CustomsCodes["\"T1\", \"AL\""]);

				AssertEqualsCC("CustomsCodes[\"\"T2\"]", cusCode4, CustomsCodes["\"T2\""]);
				AssertEqualsCC("CustomsCodes[\"\"T1\"]", cusCode5, CustomsCodes["\"T1\""]);
			});
		}

		void AssertEqualsCC(string source, OrgCusCode expected, OrgCusCode actual)
		{
			AssertEquals(source, ccFormat(expected), ccFormat(actual));
		}

		string ccFormat(OrgCusCode input)
		{
			return input == null ? "(null)" : input.OK_CodeType + ", " + input.OK_RN_NKCodeCountry;
		}

		public void TestGetCustomsRegNoWithAddress()
		{
			ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ZString botswana = Enterprise.Core.Constants.CountryCodes.Fiji;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode cusCode0 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CorporationCode, "0");
			OrgCusCode cusCode1 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1");
			OrgCusCode cusCode2 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "2");
			cusCode2.OK_OA_PremisesAddress = org.MainAddress.PK;
			OrgCusCode cusCode3 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "3");
			cusCode3.OK_RN_NKCodeCountry = botswana;
			OrgAddress addressFor4 = org.Addresses.AddNew();
			OrgCusCode cusCode4 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "4");
			cusCode4.OK_OA_PremisesAddress = addressFor4.PK;

			AssertEquals("2", org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, org.MainAddress.PK));
			AssertEquals("4", org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, addressFor4.PK));
			AssertEquals("3", org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, botswana, org.MainAddress.PK));
			AssertEquals("3", org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, botswana, ZGuid.Empty));
			AssertEquals("1", org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, ZGuid.Empty));
			AssertEquals("1", org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, ZGuid.NewZGuid()));
			AssertEquals("", org.CustomsCodes.GetCustomsRegNo(ZString.Empty, ZString.Empty, ZGuid.Invalid));
		}

		public void TestGetOrgCusCodeWithAddress()
		{
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var botswana = Enterprise.Core.Constants.CountryCodes.Fiji;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode0 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CorporationCode, "0");
			var cusCode1 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1");
			var cusCode2 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "2");
			cusCode2.OK_OA_PremisesAddress = org.MainAddress.PK;
			var cusCode3 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "3");
			cusCode3.OK_RN_NKCodeCountry = botswana;
			var addressFor4 = org.Addresses.AddNew();
			var cusCode4 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "4");
			cusCode4.OK_OA_PremisesAddress = addressFor4.PK;

			AssertEquals(cusCode2, org.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, org.MainAddress.PK));
			AssertEquals(cusCode4, org.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, addressFor4.PK));
			AssertEquals(cusCode3, org.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.ControlledPremisesID, botswana, org.MainAddress.PK));
			AssertEquals(cusCode3, org.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.ControlledPremisesID, botswana, ZGuid.Empty));
			AssertEquals(cusCode1, org.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, ZGuid.Empty));
			AssertEquals(cusCode1, org.CustomsCodes.GetOrgCusCode(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, ZGuid.NewZGuid()));
			AssertNull(org.CustomsCodes.GetOrgCusCode(ZString.Empty, ZString.Empty, ZGuid.Invalid));
		}

		public void TestGetCustomsRegNoPremiseAddressOnly()
		{
			ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode cusCode0 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CorporationCode, "0");
			OrgCusCode cusCode1 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1");
			OrgCusCode cusCode2 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "2");
			cusCode2.OK_OA_PremisesAddress = org.MainAddress.PK;

			AssertEquals("2", org.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, org.MainAddress.PK));
			AssertEquals("1", org.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, ZGuid.Empty));
			AssertEquals("", org.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, ZGuid.NewZGuid()));
			AssertEquals("", org.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(ZString.Empty, ZString.Empty, ZGuid.Invalid));
		}

		public void TestGetOrgCusCodeForPremiseAddress()
		{
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode0 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CorporationCode, "0");
			var cusCode1 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1");
			var cusCode2 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "2");
			cusCode2.OK_OA_PremisesAddress = org.MainAddress.PK;

			AssertEquals(cusCode2, org.CustomsCodes.GetOrgCusCodeForPremiseAddress(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, org.MainAddress.PK));
			AssertEquals(cusCode1, org.CustomsCodes.GetOrgCusCodeForPremiseAddress(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, ZGuid.Empty));
			AssertNull(org.CustomsCodes.GetOrgCusCodeForPremiseAddress(OrgCusCode.CodeTypes.ControlledPremisesID, currentCountry, ZGuid.NewZGuid()));
			AssertNull(org.CustomsCodes.GetOrgCusCodeForPremiseAddress(ZString.Empty, ZString.Empty, ZGuid.Invalid));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var org = Factory.New<OrgHeader>();
			return new OrgCusCodeCollection(org, Factory);
		}

		public void TestUpdateOrAddCustomsCodesIfNoneExists()
		{
			var org = Factory.New<OrgHeader>();
			OrgCusCode cusCode = org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AAA", "123", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(1, org.CustomsCodes.Count);
			AssertEquals(cusCode, org.CustomsCodes[0]);
			AssertEquals("123", org.CustomsCodes.GetCustomsRegNo("AAA", Core.Constants.CountryCodes.UnitedStates));

			cusCode = org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AAA", "234", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("Same type", 1, org.CustomsCodes.Count);
			AssertEquals(cusCode, org.CustomsCodes[0]);
			AssertEquals("234", org.CustomsCodes.GetCustomsRegNo("AAA", Core.Constants.CountryCodes.UnitedStates));

			OrgCusCode cusCode2 = org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("AAA", "633", Core.Constants.CountryCodes.Australia);
			AssertEquals("New Code", 2, org.CustomsCodes.Count);
			AssertEquals(cusCode, org.CustomsCodes[0]);
			AssertEquals(cusCode2, org.CustomsCodes[1]);
			AssertEquals("234", org.CustomsCodes.GetCustomsRegNo("AAA", Core.Constants.CountryCodes.UnitedStates));
			AssertEquals("633", org.CustomsCodes.GetCustomsRegNo("AAA", Core.Constants.CountryCodes.Australia));
		}

		public void TestGetCustomsCode()
		{
			CreateOrgCusCodesForTest(CreateOrgCusCodesForTestParams.CreateOrgCusCodesFor1Country);

			AssertEquals(CustomsRegNoForTest1, CustomsCodes.GetCustomsRegNoMatching(CodeTypeForTest1, CodeTypeForTest2));
			AssertEquals(CustomsRegNoForTest2, CustomsCodes.GetCustomsRegNoMatching(CodeTypeForTest2, CodeTypeForTest1));
			AssertEquals(ZString.Empty, CustomsCodes.GetCustomsRegNoMatching(UnusedCodeTypeForTest1, UnusedCodeTypeForTest2));
			AssertEquals(ZString.Empty, CustomsCodes.GetCustomsRegNoMatching(UnusedCodeTypeForTest2, UnusedCodeTypeForTest1));
		}

		public void TestGetOrgCusCodeObjectMatching()
		{
			CreateOrgCusCodesForTest(CreateOrgCusCodesForTestParams.CreateOrgCusCodesFor1Country);

			AssertEquals(cusCodeForTest1, CustomsCodes.GetOrgCusCodeObjectMatching(CodeTypeForTest1, CodeTypeForTest2));
			AssertEquals(cusCodeForTest2, CustomsCodes.GetOrgCusCodeObjectMatching(CodeTypeForTest2, CodeTypeForTest1));
			AssertNull(CustomsCodes.GetOrgCusCodeObjectMatching(UnusedCodeTypeForTest1, UnusedCodeTypeForTest2));
			AssertNull(CustomsCodes.GetOrgCusCodeObjectMatching(UnusedCodeTypeForTest2, UnusedCodeTypeForTest1));
		}

		public void TestGetOrgCusCodeObjectMatchingCountryAndCodes()
		{
			CreateOrgCusCodesForTest(CreateOrgCusCodesForTestParams.CreateOrgCusCodesFor2Countries);

			AssertEquals(cusCodeForTest1, CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Country1.Code, CodeTypeForTest1, CodeTypeForTest2));
			AssertEquals(cusCodeForTest2, CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Country1.Code, CodeTypeForTest2, CodeTypeForTest1));
			AssertEquals(cusCodeForTest3, CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Country2.Code, CodeTypeForTest1, CodeTypeForTest2));
			AssertEquals(cusCodeForTest4, CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Country2.Code, CodeTypeForTest2, CodeTypeForTest1));
			AssertNull(CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Country2.Code, UnusedCodeTypeForTest1, UnusedCodeTypeForTest2));
			AssertNull(CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Country1.Code, UnusedCodeTypeForTest1, UnusedCodeTypeForTest2));
		}

		public void TestGetCustomsRegNoMatchingCountryAndCodes()
		{
			CreateOrgCusCodesForTest(CreateOrgCusCodesForTestParams.CreateOrgCusCodesFor2Countries);

			AssertEquals(cusCodeForTest1.OK_CustomsRegNo, CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Country1.Code, CodeTypeForTest1, CodeTypeForTest2));
			AssertEquals(cusCodeForTest2.OK_CustomsRegNo, CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Country1.Code, CodeTypeForTest2, CodeTypeForTest1));
			AssertEquals(cusCodeForTest3.OK_CustomsRegNo, CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Country2.Code, CodeTypeForTest1, CodeTypeForTest2));
			AssertEquals(cusCodeForTest4.OK_CustomsRegNo, CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Country2.Code, CodeTypeForTest2, CodeTypeForTest1));
			AssertEquals(ZString.Empty, CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Country2.Code, UnusedCodeTypeForTest1, UnusedCodeTypeForTest2));
			AssertEquals(ZString.Empty, CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Country1.Code, UnusedCodeTypeForTest1, UnusedCodeTypeForTest2));
		}

		public void TestGetOrgCusCodesForCodeAndCountry()
		{
			CreateOrgCusCodesForTest(CreateOrgCusCodesForTestParams.CreateOrgCusCodesFor2CountriesWithRepeatebleCodeTypes);

			OrgCusCode[] cusCodes = CustomsCodes.GetOrgCusCodesForCodeAndCountry(CodeTypeForTest1, Country1.Code);
			AssertEquals(1, cusCodes.Length);
			cusCodes = CustomsCodes.GetOrgCusCodesForCodeAndCountry(CodeTypeForTest1, Country2.Code);
			AssertEquals(2, cusCodes.Length);
			cusCodes = CustomsCodes.GetOrgCusCodesForCodeAndCountry(CodeTypeForTest2, Country1.Code);
			AssertEquals(1, cusCodes.Length);
			cusCodes = CustomsCodes.GetOrgCusCodesForCodeAndCountry(CodeTypeForTest2, Country2.Code);
			AssertEquals(1, cusCodes.Length);
			cusCodes = CustomsCodes.GetOrgCusCodesForCodeAndCountry(UnusedCodeTypeForTest1, Country1.Code);
			AssertEquals(0, cusCodes.Length);
			cusCodes = CustomsCodes.GetOrgCusCodesForCodeAndCountry(UnusedCodeTypeForTest1, Country2.Code);
			AssertEquals(0, cusCodes.Length);
		}

		public void TestGetOrgCusCodesForCodeIgnoringCountry()
		{
			string codeType1 = "T!1";
			string codeType2 = "T!2";
			string codeType3 = "T!3";

			var aUCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			var uSCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);
			var cusCode1 = CustomsCodes.AddNew(codeType1, "AUCODE1", aUCountry);
			var cusCode2 = CustomsCodes.AddNew(codeType2, "AUCODE2", aUCountry);
			var cusCode3 = CustomsCodes.AddNew(codeType1, "USCODE1", uSCountry);
			var cusCode4 = CustomsCodes.AddNew(codeType3, "USCODE3", uSCountry);

			OrgCusCode[] cusCodes = CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(codeType1);
			AssertEquals(2, cusCodes.Length);
			AssertCollectionContains(cusCode1, cusCodes);
			AssertCollectionContains(cusCode3, cusCodes);
			cusCodes = CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(codeType2);
			AssertEquals(1, cusCodes.Length);
			AssertEquals(cusCode2, cusCodes[0]);
		}

		public void TestGetOrgCusCodesForMatchingCodesIgnoringCountry()
		{
			string codeType1 = "T!1";
			string codeType2 = "T!2";
			string codeType3 = "T!3";
			string codeType4 = "T!4";

			var aUCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			var uSCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);
			var cusCode1 = CustomsCodes.AddNew(codeType1, "AUCODE1", aUCountry);
			var cusCode2 = CustomsCodes.AddNew(codeType2, "AUCODE2", aUCountry);
			var cusCode3 = CustomsCodes.AddNew(codeType1, "USCODE1", uSCountry);
			var cusCode4 = CustomsCodes.AddNew(codeType3, "USCODE3", uSCountry);

			OrgCusCode[] cusCodes = CustomsCodes.GetOrgCusCodesForMatchingCodesIgnoringCountry(codeType1, codeType2);
			AssertEquals(3, cusCodes.Length);
			AssertContainsExactElementsInAnyOrder("All 3 matching custom codes should be returned", new OrgCusCode[] { cusCode1, cusCode2, cusCode3 }, cusCodes);
			cusCodes = CustomsCodes.GetOrgCusCodesForMatchingCodesIgnoringCountry(codeType3);
			AssertEquals("Only one matching custom code", 1, cusCodes.Length);
			AssertEquals("cusCode4 should match with passed code type", cusCode4, cusCodes[0]);
			cusCodes = CustomsCodes.GetOrgCusCodesForMatchingCodesIgnoringCountry(codeType4);
			AssertEquals("No matching custom codes", 0, cusCodes.Length);
		}

		public void TestAddNewWithExtraParameters()
		{
			var organisation = Factory.New<OrgHeader>();

			OrgCusCode added = organisation.CustomsCodes.AddNew("AAA", "123456");
			AssertEquals("new element is contained in the collection", true, organisation.CustomsCodes.Contains(added));
			AssertEquals("new element  has 'AAA'", "AAA", added.OK_CodeType);
			AssertEquals("new element has '123456'", "123456", added.OK_CustomsRegNo);
			AssertEquals("new element has a current country", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, added.OK_RN_NKCodeCountry);

			var otherCountry = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

			added = organisation.CustomsCodes.AddNew("BBB", "222222", otherCountry);
			AssertEquals("new element is contained in the collection", true, organisation.CustomsCodes.Contains(added));
			AssertEquals("new element has 'BBB'", "BBB", added.OK_CodeType);
			AssertEquals("new element has '222222'", "222222", added.OK_CustomsRegNo);
			AssertEquals("new element has a current country", otherCountry.Code, added.OK_RN_NKCodeCountry);

			added = organisation.CustomsCodes.AddNew("CCC", "333333", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("new element is contained in the collection", true, organisation.CustomsCodes.Contains(added));
			AssertEquals("new element has 'CCC'", "CCC", added.OK_CodeType);
			AssertEquals("new element has '333333'", "333333", added.OK_CustomsRegNo);
			AssertEquals("new element has a set country", Core.Constants.CountryCodes.UnitedStates, added.OK_RN_NKCodeCountry);
		}

		public void TestGetCustomsNo()
		{
			CreateOrgCusCodesForTest(CreateOrgCusCodesForTestParams.CreateOrgCusCodesFor2Countries);

			AssertEquals("GetCustomsNo", CustomsRegNoForTest1, CustomsCodes.GetCustomsRegNo(CodeTypeForTest1));
			AssertEquals("GetCustomsNo", CustomsRegNoForTest2, CustomsCodes.GetCustomsRegNo(CodeTypeForTest2));
			AssertEquals("GetCustomsNo", ZString.Empty, CustomsCodes.GetCustomsRegNo(UnusedCodeTypeForTest1));
			AssertEquals("GetCustomsRegNoForCodeAndCountry", CustomsRegNoForTest3, CustomsCodes.GetCustomsRegNo(CodeTypeForTest1, Country2));
			AssertEquals("GetCustomsRegNoForCodeAndCountry", CustomsRegNoForTest4, CustomsCodes.GetCustomsRegNo(CodeTypeForTest2, Country2));
			AssertEquals("GetCustomsRegNoForCodeAndCountry", ZString.Empty, CustomsCodes.GetCustomsRegNo(UnusedCodeTypeForTest1, Country2));
		}

		public void TestDefaultIndex()
		{
			OrgCusCode customsCode = CustomsCodes.AddNew();
			AssertEquals("Indexer returns added business object", customsCode, CustomsCodes[0]);
		}

		public void TestGetUOCAndGetUNC()
		{
			AssertEquals("Return value before adding object", "", CustomsCodes.GetUOC());
			OrgCusCode customsCode1 = CustomsCodes.AddNew();
			customsCode1.OK_CustomsRegNo = "TEST123";
			customsCode1.OK_CodeType = "XXX";
			AssertEquals("Searching on invalid code", "", CustomsCodes.GetUOC());
			customsCode1.OK_CodeType = OrgCusCode.CodeTypes.UniversalOfficeCode;
			AssertEquals("Searching on correct code", "TEST123", CustomsCodes.GetUOC());

			AssertEquals("Return value before adding object", "", CustomsCodes.GetUNC());
			OrgCusCode customsCode2 = CustomsCodes.AddNew();
			customsCode2.OK_CustomsRegNo = "REG1290";
			customsCode2.OK_CodeType = OrgCusCode.CodeTypes.UniversalNettingCode;
			AssertEquals("Searching on correct code", "REG1290", CustomsCodes.GetUNC());
		}

		public void TestGetCustomsRegNoForCode()
		{
			AssertEquals("Return value before adding object", "", CustomsCodes.GetCustomsRegNo("WSC", Country1));

			OrgCusCode customsCode1 = CustomsCodes.AddNew();
			customsCode1.OK_CustomsRegNo = "REGO1";
			customsCode1.OK_CodeType = "XXX";
			customsCode1.OK_RN_NKCodeCountry = Country1.Code;
			AssertEquals("Searching on invalid code", "", CustomsCodes.GetCustomsRegNo("WSC", Country1));

			OrgCusCode customsCode2 = CustomsCodes.AddNew();
			customsCode2.OK_CustomsRegNo = "REGO2";
			customsCode2.OK_CodeType = "WSC";
			customsCode2.OK_RN_NKCodeCountry = Country2.Code;
			AssertEquals("Searching in invalid country", "", CustomsCodes.GetCustomsRegNo("WSC", Country1));
			AssertEquals("Searching on correct country and code", "REGO2", CustomsCodes.GetCustomsRegNo("WSC", Country2));
		}

		public void TestGetCusCodeObjectForCode()
		{
			AssertNull("Return value should be null before adding object", CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry("WSC", Country1));

			OrgCusCode cusCode1 = CustomsCodes.AddNew();
			cusCode1.OK_CodeType = "XXX";
			cusCode1.OK_RN_NKCodeCountry = Country1.Code;
			AssertNull("Return value is null (incorrect code)", CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry("WSC", Country1));

			OrgCusCode cusCode2 = CustomsCodes.AddNew();
			cusCode2.OK_CodeType = "WSC";
			cusCode2.OK_RN_NKCodeCountry = Country2.Code;
			AssertNull("Return value is null (incorrect country)", CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry("WSC", Country1));
			AssertNotNull("Return value is not null (correct code and country)", CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry("WSC", Country2));
		}

		public void TestAddNew_WithParamenters()
		{
			OrgCusCode cusCode = CustomsCodes.AddNew("GST", "123459876", Country1);
			AssertNotNull("cusCode", cusCode);
			AssertEquals("CodeType", "GST", cusCode.OK_CodeType);
			AssertEquals("CustomsRegNo", "123459876", cusCode.OK_CustomsRegNo);
			AssertEquals("CodeType", Country1.Code, cusCode.OK_RN_NKCodeCountry);
		}

		public void TestSettingPatternRequiresRegen()
		{
			OrgCusCode bo = CustomsCodes.AddNew();
			CustomsCodes.Master.PatternMatchRequiresRegen = false;
			Assert(!CustomsCodes.Master.PatternMatchRequiresRegen);
			CustomsCodes.Remove(bo);
			Assert(CustomsCodes.Master.PatternMatchRequiresRegen);

			bo = CustomsCodes.AddNew();
			CustomsCodes.Master.PatternMatchRequiresRegen = false;
			Assert(!CustomsCodes.Master.PatternMatchRequiresRegen);
			CustomsCodes.RemoveAndDelete(bo);
			Assert(CustomsCodes.Master.PatternMatchRequiresRegen);
		}

		public void TestRemoveAndDelete()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			var oldOrgConfigModifyFinancialNonARAPRegistrationNumbers = Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = false;

			try
			{
				Company.FillWithValidTestData();
				Company.Factory.Save();
				var isFindingDuplicates = false;
				Company.DeduplicationStarted += (o, e) => isFindingDuplicates = true;
				((IDeduplicatable)Company).ShouldRunDeduplication = true;
				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = true;
				CreateCodeTryDeleteAndAssertCollectionCount(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123459876", Core.Constants.CountryGuids.UnitedStates, 0);
				AssertEquals(true, isFindingDuplicates);
				isFindingDuplicates = false;

				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = false;
				CreateCodeTryDeleteAndAssertCollectionCount(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123459876", Core.Constants.CountryGuids.UnitedStates, 1);
				AssertEquals(true, isFindingDuplicates);
				isFindingDuplicates = false;

				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = true;
				CreateCodeTryDeleteAndAssertCollectionCount(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345", Core.Constants.CountryGuids.Australia, 1);
				AssertEquals(true, isFindingDuplicates);
				isFindingDuplicates = false;

				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = false;
				CreateCodeTryDeleteAndAssertCollectionCount(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345", Core.Constants.CountryGuids.Australia, 2);
				AssertEquals(true, isFindingDuplicates);
				isFindingDuplicates = false;
			}
			finally
			{
				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyFinancialNonARAPRegistrationNumbers;
			}
		}

		void CreateCodeTryDeleteAndAssertCollectionCount(string codeType, string codeValue, Guid refCountryPK, int expectedCountAfterDelete)
		{
			var cusCode = CustomsCodes.AddNew(codeType, codeValue, Factory.Load<RefCountry>(refCountryPK));
			Factory.Save();
			CustomsCodes.RemoveAndDelete(cusCode);
			AssertEquals(string.Format("Should have [{0}] codes after attempting to delete code of type [{1}]", expectedCountAfterDelete, codeType),
				expectedCountAfterDelete, CustomsCodes.Count);
		}

		public void TestCheckpointDeniesDelete()
		{
			Company.FillWithValidTestData();
			Company.Factory.Save();

			var oldSecurityModifyFinancialARAPRegistrationNumbers = Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = false;

			var oldOrgConfigModifyFinancialNonARAPRegistrationNumbers = Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = false;

			var oldOrgConfigModifyNonFinancialARAPRegistrationNumbers = Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = false;

			var oldOrgConfigModifyNonFinancialNonARAPRegistrationNumbers = Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed;
			Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = false;

			try
			{
				cusCodeForTest1 = CustomsCodes.AddNew();
				Company.OH_IsDebtor = true;
				Company.OH_IsCreditor = true;
				cusCodeForTest1.OK_CustomsRegNo = "24097959902";
				cusCodeForTest1.OK_RN_NKCodeCountry = "AU";
				cusCodeForTest1.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
				Factory.Save();

				cusCodeForTest1 = Factory.Load<OrgCusCode>(cusCodeForTest1.PK);
				AssertEquals(true, cusCodeForTest1.IsOriginalCompanyCodeTypePrimary);
				AssertEquals(true, CustomsCodes.ExposedCheckpointDeniesDelete(cusCodeForTest1));

				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = true;
				AssertEquals(true, cusCodeForTest1.IsOriginalCompanyCodeTypePrimary);
				AssertEquals(false, CustomsCodes.ExposedCheckpointDeniesDelete(cusCodeForTest1));

				cusCodeForTest2 = CustomsCodes.AddNew();
				cusCodeForTest2.OK_CustomsRegNo = "123456";
				cusCodeForTest1.OK_RN_NKCodeCountry = "AU";
				cusCodeForTest2.OK_CodeType = OrgCusCode.CodeTypes.WorldCargoAssociationNumber;
				Factory.Save();

				cusCodeForTest2 = Factory.Load<OrgCusCode>(cusCodeForTest2.PK);
				AssertEquals(false, cusCodeForTest2.IsOriginalCompanyCodeTypePrimary);
				AssertEquals(true, CustomsCodes.ExposedCheckpointDeniesDelete(cusCodeForTest2));

				Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = true;
				AssertEquals(false, cusCodeForTest2.IsOriginalCompanyCodeTypePrimary);
				AssertEquals(false, CustomsCodes.ExposedCheckpointDeniesDelete(cusCodeForTest2));

				Company.OH_IsDebtor = false;
				Company.OH_IsCreditor = false;

				cusCodeForTest3 = Factory.Load<OrgCusCode>(cusCodeForTest1.PK);
				AssertEquals(true, cusCodeForTest3.IsOriginalCompanyCodeTypePrimary);
				AssertEquals(true, CustomsCodes.ExposedCheckpointDeniesDelete(cusCodeForTest3));

				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = true;
				AssertEquals(true, cusCodeForTest3.IsOriginalCompanyCodeTypePrimary);
				AssertEquals(false, CustomsCodes.ExposedCheckpointDeniesDelete(cusCodeForTest3));

				cusCodeForTest4 = Factory.Load<OrgCusCode>(cusCodeForTest2.PK);
				AssertEquals(false, cusCodeForTest4.IsOriginalCompanyCodeTypePrimary);
				AssertEquals(true, CustomsCodes.ExposedCheckpointDeniesDelete(cusCodeForTest4));

				Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = true;
				AssertEquals(false, cusCodeForTest4.IsOriginalCompanyCodeTypePrimary);
				AssertEquals(false, CustomsCodes.ExposedCheckpointDeniesDelete(cusCodeForTest4));
			}
			finally
			{
				Env.Security.OrgConfigModifyFinancialARAPRegistrationNumbers.IsAllowed = oldSecurityModifyFinancialARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyFinancialNonARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyFinancialNonARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyNonFinancialARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyNonFinancialARAPRegistrationNumbers;
				Env.Security.OrgConfigModifyNonFinancialNonARAPRegistrationNumbers.IsAllowed = oldOrgConfigModifyNonFinancialNonARAPRegistrationNumbers;
			}
		}

		public void TestIBODocDataProviderGetRow()
		{
			var org = Factory.New<OrgHeader>();
			OrgCusCode cusCode1 = org.CustomsCodes.AddNew("AAA", "1111");
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;

			OrgCusCode cusCode2 = org.CustomsCodes.AddNew("AAA", "2222");
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;

			OrgCusCode cusCode3 = org.CustomsCodes.AddNew();
			cusCode3.OK_CodeType = "AAA";
			cusCode3.OK_CustomsRegNo = "3333";
			cusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;

			IBODocDataProviderCollection collection = org.CustomsCodes;
			AssertEquals(cusCode1, BODocDataProvider.GetBusinessObject(collection["AAA:" + Core.Constants.CountryCodes.Brazil]));
			AssertEquals(cusCode3, BODocDataProvider.GetBusinessObject(collection["AAA:" + Core.Constants.CountryCodes.Canada]));

			var cusCode = (OrgCusCode)BODocDataProvider.GetBusinessObject(collection["AAA:" + Core.Constants.CountryCodes.Australia]);
			AssertEquals(ZString.Empty, cusCode.OK_CustomsRegNo);

			cusCode = (OrgCusCode)BODocDataProvider.GetBusinessObject(collection["BBB:" + Core.Constants.CountryCodes.Brazil]);
			AssertEquals(ZString.Empty, cusCode.OK_CustomsRegNo);
		}

		public void TestGetAllOrgCusCodeObjectMatchingCountryAndCodes()
		{
			string codeType1 = "T!1";
			string codeType2 = "T!2";
			string codeType3 = "T!3";

			var aRCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Argentina);
			var bRSCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Brazil);
			var cusCode1 = CustomsCodes.AddNew(codeType1, "AAA", aRCountry);
			var cusCode2 = CustomsCodes.AddNew(codeType2, "BBB", aRCountry);
			var cusCode3 = CustomsCodes.AddNew(codeType3, "CCC", aRCountry);
			var cusCode4 = CustomsCodes.AddNew(codeType1, "DDD", bRSCountry);
			var cusCode5 = CustomsCodes.AddNew(codeType3, "EEE", bRSCountry);
			var cusCode6 = CustomsCodes.AddNew(codeType3, "EEE", "");

			OrgCusCode[] cusCodes = CustomsCodes.GetAllOrgCusCodesForCountryAndCodes(Constants.CountryCodes.Argentina, codeType1, codeType2);

			AssertEquals(2, cusCodes.Length);
			AssertCollectionContains(cusCode1, cusCodes);
			AssertCollectionContains(cusCode2, cusCodes);

			cusCodes = CustomsCodes.GetAllOrgCusCodesForCountryAndCodes(Constants.CountryCodes.Brazil, codeType3);
			AssertEquals(1, cusCodes.Length);
			AssertCollectionContains(cusCode5, cusCodes);

			cusCodes = CustomsCodes.GetAllOrgCusCodesForCountryAndCodes(Constants.CountryCodes.Brazil);
			AssertEquals(2, cusCodes.Length);

			cusCodes = CustomsCodes.GetAllOrgCusCodesForCountryAndCodes(Constants.CountryCodes.Australia);
			AssertEquals(0, cusCodes.Length);

			cusCodes = CustomsCodes.GetAllOrgCusCodesForCountryAndCodes(Constants.CountryCodes.Australia, codeType3);
			AssertEquals(0, cusCodes.Length);

			cusCodes = CustomsCodes.GetAllOrgCusCodesForCountryAndCodes("XXX");
			AssertEquals(0, cusCodes.Length);

			cusCodes = CustomsCodes.GetAllOrgCusCodesForCountryAndCodes("", codeType3);
			AssertEquals(1, cusCodes.Length);

			cusCodes = CustomsCodes.GetAllOrgCusCodesForCountryAndCodes("", codeType1);
			AssertEquals(0, cusCodes.Length);
		}

		#region Implementation

		const string CodeTypeForTest1 = "AAA";
		const string CodeTypeForTest2 = "BBB";
		const string UnusedCodeTypeForTest1 = "CCC";
		const string UnusedCodeTypeForTest2 = "DDD";

		const string CustomsRegNoForTest1 = "111";
		const string CustomsRegNoForTest2 = "222";
		const string CustomsRegNoForTest3 = "333";
		const string CustomsRegNoForTest4 = "444";
		const string CustomsRegNoForTest5 = "555";

		enum CreateOrgCusCodesForTestParams
		{
			CreateOrgCusCodesFor1Country,
			CreateOrgCusCodesFor2Countries,
			CreateOrgCusCodesFor2CountriesWithRepeatebleCodeTypes
		}

		OrgCusCode cusCodeForTest1;
		OrgCusCode cusCodeForTest2;
		OrgCusCode cusCodeForTest3;
		OrgCusCode cusCodeForTest4;
		OrgCusCode cusCodeForTest5;

		void CreateOrgCusCodesForTest(CreateOrgCusCodesForTestParams createParams)
		{
			cusCodeForTest1 = CustomsCodes.AddNew();
			cusCodeForTest1.OK_CodeType = CodeTypeForTest1;
			cusCodeForTest1.OK_CustomsRegNo = CustomsRegNoForTest1;
			cusCodeForTest1.OK_RN_NKCodeCountry = Country1.Code;

			cusCodeForTest2 = CustomsCodes.AddNew();
			cusCodeForTest2.OK_CodeType = CodeTypeForTest2;
			cusCodeForTest2.OK_CustomsRegNo = CustomsRegNoForTest2;
			cusCodeForTest2.OK_RN_NKCodeCountry = Country1.Code;

			bool createCusCodeForTest5 = (createParams == CreateOrgCusCodesForTestParams.CreateOrgCusCodesFor2CountriesWithRepeatebleCodeTypes);

			if ((createParams == CreateOrgCusCodesForTestParams.CreateOrgCusCodesFor2Countries) || createCusCodeForTest5)
			{
				if (createCusCodeForTest5)
				{
					cusCodeForTest5 = CustomsCodes.AddNew();
					cusCodeForTest5.OK_CodeType = CodeTypeForTest1;
					cusCodeForTest5.OK_CustomsRegNo = CustomsRegNoForTest5;
					cusCodeForTest5.OK_RN_NKCodeCountry = Country2.Code;
				}

				cusCodeForTest3 = CustomsCodes.AddNew();
				cusCodeForTest3.OK_CodeType = CodeTypeForTest1;
				cusCodeForTest3.OK_CustomsRegNo = CustomsRegNoForTest3;
				cusCodeForTest3.OK_RN_NKCodeCountry = Country2.Code;

				cusCodeForTest4 = CustomsCodes.AddNew();
				cusCodeForTest4.OK_CodeType = CodeTypeForTest2;
				cusCodeForTest4.OK_CustomsRegNo = CustomsRegNoForTest4;
				cusCodeForTest4.OK_RN_NKCodeCountry = Country2.Code;
			}
		}

		OrgHeader Company;
		OrgCusCodeCollectionForTest CustomsCodes;

		RefCountry Country1;
		RefCountry Country2;

		protected override void SetUp()
		{
			base.SetUp();
			Company = Factory.New<OrgHeader>();
			CustomsCodes = new OrgCusCodeCollectionForTest(Company, Factory);

			Country1 = GlbCompany.CurrentCompany.Country;
			Country2 = Factory.New<RefCountry>();
			Country2.RN_Code = "XX";
		}

		#endregion
	}
}
