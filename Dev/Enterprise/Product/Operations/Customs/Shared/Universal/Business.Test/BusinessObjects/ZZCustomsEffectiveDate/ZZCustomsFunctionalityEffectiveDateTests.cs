using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	public class ZZCustomsFunctionalityEffectiveDateTests : TestCaseWithFactory
	{
		public void TestFUNCSGetEffectiveCusCodeAttribute()
		{
			var attributeName = Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MaxEntryLines;
			helper.CreateCusCodeListAttribute(cusCodeFUNCSPK, attributeName, "100");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS);
			Factory.Save();
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Has attribute", "100", ZZCustomsFunctionalityEffectiveDate.GetEffectiveCusCodeAttribute(FUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now, attributeName));
				AssertEquals("No attribute: empty", "", ZZCustomsFunctionalityEffectiveDate.GetEffectiveCusCodeAttribute(FUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now, "XXX"));
				AssertEquals("No attribute: INVALID code", "", ZZCustomsFunctionalityEffectiveDate.GetEffectiveCusCodeAttribute("INVALID", Core.Constants.CountryCodes.Japan, ZDateTime.Now, "XXX"));
				AssertEquals("No attribute: invalid country", "", ZZCustomsFunctionalityEffectiveDate.GetEffectiveCusCodeAttribute(FUNCCusCode, Core.Constants.CountryCodes.Australia, ZDateTime.Now, attributeName));
				AssertEquals("No attribute: invalid date", "", ZZCustomsFunctionalityEffectiveDate.GetEffectiveCusCodeAttribute(FUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now.AddDays(8), attributeName));
			}

			);
		}

		public void TestPFUNCGetEffectiveCusCodeAttribute()
		{
			var attributeName = Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MaxEntryLines;
			helper.CreateCusCodeListAttribute(cusCodePFUNCPK, attributeName, "200");
			helper.CreateCusCodeListAttribute(cusCodePFUNCPK, RefCusCodeListAttributeTypes.Codes.Company, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Company, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC);
			Factory.Save();
			AssertEquals("Has attribute", "200", ZZCustomsFunctionalityEffectiveDate.GetEffectiveCusCodeAttribute(PFUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now, attributeName));
		}

		[TestDate(2021, 4, 28)]
		public void TestTemporarilySetFunctionalityAttribute()
		{
			var attributeName = Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MaxEntryLines;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(FUNCCusCode, Core.Constants.CountryCodes.Latvia, ZDate.Today, attributeName, "100"))
			{
				AssertEquals("Has attribute", "100", ZZCustomsFunctionalityEffectiveDate.GetEffectiveCusCodeAttribute(FUNCCusCode, Core.Constants.CountryCodes.Latvia, ZDateTime.Now, attributeName));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(FUNCCusCode, Core.Constants.CountryCodes.Latvia, ZDate.Today, attributeName, ""))
			{
				AssertEquals("No attribute", "", ZZCustomsFunctionalityEffectiveDate.GetEffectiveCusCodeAttribute(FUNCCusCode, Core.Constants.CountryCodes.Latvia, ZDateTime.Now, attributeName));
			}

			AssertEquals("No attribute as default", "", ZZCustomsFunctionalityEffectiveDate.GetEffectiveCusCodeAttribute(FUNCCusCode, Core.Constants.CountryCodes.Latvia, ZDateTime.Now, attributeName));
		}

		public void TestFUNCSEffectiveDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Past", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now.AddDays(-8)));
				AssertEquals("Future", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now.AddDays(8)));
				AssertEquals("Within Range", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now));
			}

			);
		}

		public void TestFUNCSCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Invalid", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid("INVALID", Core.Constants.CountryCodes.Japan, ZDateTime.Now));
				AssertEquals("Valid", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now));
			}

			);
		}

		public void TestFUNCSCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Invalid", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.Germany, ZDateTime.Now));
				AssertEquals("Valid", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now));
			}

			);
		}

		public void TestPFUNCEffectiveDate()
		{
			helper.CreateCusCodeListAttribute(cusCodePFUNCPK, RefCusCodeListAttributeTypes.Codes.Company, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Company, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Past", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now.AddDays(-8)));
				AssertEquals("Future", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now.AddDays(8)));
				AssertEquals("Within Range", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now));
			}

			);
		}

		public void TestPFUNCCode()
		{
			helper.CreateCusCodeListAttribute(cusCodePFUNCPK, RefCusCodeListAttributeTypes.Codes.Company, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Company, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Invalid", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid("INVALID", Core.Constants.CountryCodes.Japan, ZDateTime.Now));
				AssertEquals("Valid", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now));
			}

			);
		}

		public void TestPFUNCCountry()
		{
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCodePFUNCPK, RefCusCodeListAttributeTypes.Codes.Company, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Invalid", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Now));
				AssertEquals("Valid", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now));
			}

			);
		}

		public void TestPFUNCInvalidCompanyCode()
		{
			var cusCode = CreateOrUpdateCodeList(helper, Core.Constants.CountryCodes.Albania, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, PFUNCCusCode, "INVALID COMPANY CODE", ZDateTime.Now.AddDays(-7), ZDateTime.Now.AddDays(7));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.System, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, Core.Constants.CountryCodes.Albania, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC);
			helper.CreateCusCodeListAttribute(cusCode.PK, RefCusCodeListAttributeTypes.Codes.System, "HYECMR");
			Factory.Save();
			AssertEquals("Invalid company code record", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.Albania, ZDateTime.Now));
		}

		public void TestPFUNCCompanyKey()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(PFUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now, false))
			{
				AssertEquals(false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now, true))
			{
				AssertEquals(true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.Japan, ZDateTime.Now));
			}
		}

		[TestDate(2018, 5, 24)]
		public void TestIsFunctionalityEnabled()
		{
			CreateOrUpdateCodeList(helper, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, FUNCCusCode, "ZA Customs Changes", ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(2));
			var cusCode = CreateOrUpdateCodeList(helper, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, PFUNCCusCode, "ZA Test Customers", ZDateTime.Now.AddDays(-7), ZDateTime.Now.AddDays(7));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Company, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC);
			helper.CreateCusCodeListAttribute(cusCode.PK, RefCusCodeListAttributeTypes.Codes.Company, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			Factory.Save();
			CombineAssertions("Via Static", () =>
			{
				AssertEquals("Uses Customs Functionality date is in effective", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now.AddDays(3)));
				AssertEquals("Uses Customs Functionality date valid", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now));
				AssertEquals("Uses Pilot Functionality date valid, Customs date invalid, Company Invalid", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now.AddDays(8)));
				AssertEquals("Uses Pilot Functionality all valid", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now.AddDays(-4)));
			});
			CombineAssertions("Via interface", () =>
			{
				IZZCustomsFunctionalityEffectiveDate function = new ZZCustomsFunctionalityEffectiveDate();
				AssertEquals("Uses Customs Functionality date is in effective", false, function.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now.AddDays(3)));
				AssertEquals("Uses Customs Functionality date valid", true, function.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now));
				AssertEquals("Uses Pilot Functionality date valid, Customs date invalid, Company Invalid", false, function.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now.AddDays(8)));
				AssertEquals("Uses Pilot Functionality all valid", true, function.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now.AddDays(-4)));
			});
		}

		[TestDate(2018, 5, 24)]
		public void TestIsFunctionalityEnabled_PriorityToPilotFunctionality()
		{
			var cusCodeFunc = CreateOrUpdateCodeList(helper, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, FUNCCusCode, "ZA Customs Changes", ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(2));
			var cusCode = CreateOrUpdateCodeList(helper, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, PFUNCCusCode, "ZA Test Customers", ZDateTime.Now.AddDays(-7), ZDateTime.Now.AddDays(7));
			Factory.Save();

			CombineAssertions("When Pilot Attribute is not added", () =>
			{
				AssertEquals("When date is in range of Pilot Functionality", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now.AddDays(3), true));
				AssertEquals("When date is in range of Customs Functionality", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now, true));
			});

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Company, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC);
			helper.CreateCusCodeListAttribute(cusCode.PK, RefCusCodeListAttributeTypes.Codes.Company, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			Factory.Save();

			CombineAssertions("When Pilot Attribute is added", () =>
			{
				AssertEquals("When date is in range of Pilot Functionality", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now.AddDays(3), true));
				AssertEquals("When date is in range of Customs Functionality", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now, true));
				AssertEquals("When date not in range of Pilot Functionality", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(PFUNCCusCode, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Now.AddDays(8), true));
			});
		}

		[TestDate(2018, 4, 21)]
		public void TestFunctionalityCache()
		{
			var cusCode = CreateOrUpdateCodeList(helper, Core.Constants.CountryCodes.Afghanistan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, FUNCCusCode, "TEST CACHE", ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(2));
			Factory.Save();
			AssertEquals("The Dictionary is set", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.Afghanistan, ZDateTime.Now));
			cusCode.Delete();
			Factory.Save();
			AssertEquals("The Dictionary is used", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.Afghanistan, ZDateTime.Now));
		}

		[TestDate(2018, 5, 23)]
		public void TestTemporarilySetFunctionality()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FUNCCusCode, Core.Constants.CountryCodes.Latvia, ZDate.Today, false))
			{
				AssertEquals("Test Set is working", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.Latvia, ZDateTime.Now));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FUNCCusCode, Core.Constants.CountryCodes.Latvia, ZDate.Today, true))
			{
				AssertEquals("Test Set is working", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.Latvia, ZDateTime.Now));
			}

			Assert("Test Set defaults back to orgiinal value of false as nothing in DB", !ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FUNCCusCode, Core.Constants.CountryCodes.Latvia, ZDateTime.Now));
		}

		public void TestIsFunctionalityDefined()
		{
			const string funcsCusCodeInEffective = "CODE1";
			const string funcsCusCodeInPast = "CODE2";
			const string funcsCusCodeInFuture = "CODE3";

			const string pfuncCusCodeInEffective = "CODE4";
			const string pfuncCusCodeInPast = "CODE5";
			const string pfuncCusCodeInFuture = "CODE6";
			const string pfuncCusCodeWithoutAttribute = "CODE7";
			const string funcsCusCodeIsNotDefined = "CODE8";

			const string testCountry = Core.Constants.CountryCodes.Latvia;
			const string companyAttribute = RefCusCodeListAttributeTypes.Codes.Company;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(testCountry))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				CreateOrUpdateCodeList(helper, testCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, funcsCusCodeInEffective, "TEST COUNTRY", ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(2));
				CreateOrUpdateCodeList(helper, testCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, funcsCusCodeInPast, "TEST COUNTRY", ZDateTime.Now.AddDays(-4), ZDateTime.Now.AddDays(-2));
				CreateOrUpdateCodeList(helper, testCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, funcsCusCodeInFuture, "TEST COUNTRY", ZDateTime.Now.AddDays(2), ZDateTime.Now.AddDays(4));

				CreateOrUpdateCodeList(helper, testCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, pfuncCusCodeWithoutAttribute, "TEST COMPANY", ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(2));

				helper.CreateNewOrGetExistingRefCusCodeListAttributeName(companyAttribute, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, testCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC);
				var cusCode = CreateOrUpdateCodeList(helper, testCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, pfuncCusCodeInEffective, "TEST COMPANY", ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(4));
				helper.CreateCusCodeListAttribute(cusCode.PK, companyAttribute, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
				cusCode = CreateOrUpdateCodeList(helper, testCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, pfuncCusCodeInPast, "TEST COMPANY", ZDateTime.Now.AddDays(-4), ZDateTime.Now.AddDays(-2));
				helper.CreateCusCodeListAttribute(cusCode.PK, companyAttribute, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
				cusCode = CreateOrUpdateCodeList(helper, testCountry, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, pfuncCusCodeInFuture, "TEST COMPANY", ZDateTime.Now.AddDays(2), ZDateTime.Now.AddDays(4));
				helper.CreateCusCodeListAttribute(cusCode.PK, companyAttribute, GlbCompany.CurrentCompany.LicenceKeyIdentifier);

				Factory.Save();
				CombineAssertions("Via Static", () =>
				{
					AssertEquals("Uses Customs Functionality date is in effective", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityDefined(funcsCusCodeInEffective));
					AssertEquals("Uses Customs Functionality date is overdue", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityDefined(funcsCusCodeInPast));
					AssertEquals("Uses Customs Functionality date is before", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityDefined(funcsCusCodeInFuture));

					AssertEquals("Uses Pilot Functionality without attribute date is in effective", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityDefined(pfuncCusCodeWithoutAttribute));

					AssertEquals("Uses Pilot Functionality date is in effective", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityDefined(pfuncCusCodeInEffective));
					AssertEquals("Uses Pilot Functionality date is overdue", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityDefined(pfuncCusCodeInPast));
					AssertEquals("Uses Pilot Functionality date is before", true, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityDefined(pfuncCusCodeInFuture));

					AssertEquals("Uses Customs Functionality that is not defined", false, ZZCustomsFunctionalityEffectiveDate.IsFunctionalityDefined(funcsCusCodeIsNotDefined));
				});
			}
		}
		protected override void SetUp()
		{
			base.SetUp();
			ZZCustomsFunctionalityEffectiveDate.ClearDictionary();
			helper = new UniversalReferenceTestDataHelper(Factory);
			CreateOrUpdateCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, "Customs Effective Dates for New Functionality", Core.Constants.CountryCodes.Japan);
			CreateOrUpdateCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, "Customs Effective Dates for New Functionality for PILOT systems", Core.Constants.CountryCodes.Japan);
			var cusCodeFUNCS = CreateOrUpdateCodeList(helper, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, FUNCCusCode, "AFR17 Changes", ZDateTime.Now.AddDays(-7), ZDateTime.Now.AddDays(7));
			var cusCode = CreateOrUpdateCodeList(helper, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PFUNC, PFUNCCusCode, "AFR17 Test Customers", ZDateTime.Now.AddDays(-7), ZDateTime.Now.AddDays(7));
			cusCodePFUNCPK = cusCode.PK;
			cusCodeFUNCSPK = cusCodeFUNCS.PK;
			Factory.Save();
		}

		RefCusCodeType CreateOrUpdateCodeType(ZString code, ZString description, ZString dataGrouping)
		{
			var query = new ZQuery(RefCusCodeTypeSchema.ZZK_CodeType, code);
			query.AddToFilter(RefCusCodeTypeSchema.ZZK_ZZZ_NKDataGrouping, dataGrouping);
			var cusCodeType = Factory.LoadTop1<RefCusCodeType>(query);
			if (cusCodeType == null)
			{
				cusCodeType = Factory.New<RefCusCodeType>();
				cusCodeType.ZZK_CodeType = code;
				cusCodeType.ZZK_ZZZ_NKDataGrouping = dataGrouping;
			}

			cusCodeType.ZZK_Description = description;
			return cusCodeType;
		}

		RefCusCodeList CreateOrUpdateCodeList(UniversalReferenceTestDataHelper helper, ZString dataGrouping, ZString codeType, ZString code, ZString description, ZDateTime startDate, ZDateTime endDate)
		{
			var query = new ZQuery(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, dataGrouping);
			query.AddToFilter(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, codeType);
			query.AddToFilter(RefCusCodeListSchema.ZZD_Code, code);
			var cusCodeList = Factory.LoadTop1<RefCusCodeList>(query);
			if (cusCodeList == null)
			{
				cusCodeList = Factory.New<RefCusCodeList>();
				helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
				CreateOrUpdateCodeType(codeType, codeType, dataGrouping);
				cusCodeList.ZZD_ZZZ_NKDataGrouping = dataGrouping;
				cusCodeList.ZZD_ZZK_NKCodeType = codeType;
				cusCodeList.ZZD_Code = code;
			}

			cusCodeList.ZZD_Description = description;
			cusCodeList.ZZD_StartDate = startDate;
			cusCodeList.ZZD_EndDate = endDate;
			return cusCodeList;
		}

		UniversalReferenceTestDataHelper helper;
		ZGuid cusCodePFUNCPK;
		ZGuid cusCodeFUNCSPK;
		const string FUNCCusCode = "AFR17";
		const string PFUNCCusCode = "AFR18";
	}
}
