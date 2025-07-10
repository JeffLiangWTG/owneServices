using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusAuthorisationHeader))]
	class CusAuthorisationHeaderTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2020, 5, 15)]
		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Application Code", CusPermitHeaderApplicationCodeList.Codes.Authorisation, authorizationHeader.CPH_ApplicationCode);
				AssertEquals("Country Code", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, authorizationHeader.CPH_RN_NKCountryCode);
				AssertEquals("Start Date", new ZDate(2020, 5, 15), authorizationHeader.CPH_StartDate);
				AssertEquals("End Date", ZDateTime.MaxSmallDateTimeValue.Date, authorizationHeader.CPH_EndDate);
			});
		}

		public void TestDelete()
		{
			var rule = authorizationHeader.CusAuthorisationRules.AddNew();
			authorizationHeader.Delete();
			AssertEquals("Rules should have been deleted while Header deleted.", true, rule.IsDeleted);
		}

		public void TestProvider()
		{
			CombineAssertions(() =>
			{
				authorizationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
				AssertType<CusAuthorisationHeaderProvider>("Default", authorizationHeader.Provider);
				authorizationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
				AssertEquals("Enterprise.Customs.EU.Business.CusAuthorisationHeaderProvider", authorizationHeader.Provider.GetType().FullName);
				authorizationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				AssertEquals("Enterprise.Customs.DE.Business.CusAuthorisationHeaderProvider", authorizationHeader.Provider.GetType().FullName);
			});
		}

		public void TestCAU_RN_NKCountryCode()
		{
			authorizationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var latvianProvider = authorizationHeader.Provider;
			authorizationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			var germanProvider = authorizationHeader.Provider;
			AssertNotEquals(latvianProvider.GetType(), germanProvider.GetType());
		}

		public void TestAuthorizationTypeDescription()
		{
			CombineAssertions(() =>
			{
				authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
				AssertEquals("Description from CPH_Type", CusAuthorizationHeaderTypeList.Descriptions.InwardProcessing, DescriptionPropertyAttribute.DescriptionFromBusinessObject(authorizationHeader));

				authorizationHeader.CPH_PermitDescription = "CN Code 123456";
				AssertEquals("Description from CPH_PermitDescription", "CN Code 123456", DescriptionPropertyAttribute.DescriptionFromBusinessObject(authorizationHeader));
			});
		}

		public void TestValidation()
		{
			AssertType<CusAuthorisationHeaderValidation>(authorizationHeader.Validation);
		}

		public void TestLookups()
		{
			AssertType<CusAuthorisationHeaderLookups>(authorizationHeader.Lookups);
		}

		public void TestIsCurrent()
		{
			CombineAssertions(() =>
			{
				authorizationHeader.CPH_StartDate = ZDate.Today.AddMonths(-1);
				authorizationHeader.CPH_EndDate = ZDate.Empty;
				AssertEquals("StartDate < Today, EndDate empty", true, authorizationHeader.IsCurrent);

				authorizationHeader.CPH_EndDate = ZDate.Today.AddDays(-1);
				AssertEquals("StartDate < Today, EndDate < Today", false, authorizationHeader.IsCurrent);

				authorizationHeader.CPH_EndDate = ZDate.Today;
				AssertEquals("StartDate < Today, EndDate = Today", true, authorizationHeader.IsCurrent);

				authorizationHeader.CPH_EndDate = ZDate.Today.AddDays(1);
				AssertEquals("StartDate < Today, EndDate > Today", true, authorizationHeader.IsCurrent);

				authorizationHeader.CPH_StartDate = ZDate.Today;
				authorizationHeader.CPH_EndDate = ZDate.Empty;
				AssertEquals("StartDate = Today, EndDate empty", true, authorizationHeader.IsCurrent);

				authorizationHeader.CPH_EndDate = ZDate.Today.AddDays(1);
				AssertEquals("StartDate = Today, EndDate > Today", true, authorizationHeader.IsCurrent);

				authorizationHeader.CPH_StartDate = ZDate.Today.AddDays(1);
				authorizationHeader.CPH_EndDate = ZDate.Empty;
				AssertEquals("StartDate > Today, EndDate empty", false, authorizationHeader.IsCurrent);

				authorizationHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
				AssertEquals("StartDate > Today, EndDate > Today", false, authorizationHeader.IsCurrent);
			});
		}

		public void TestAuthorizationAddress()
		{
			var orgAddress = Factory.New<OrgHeader>().Addresses.AddNew();
			orgAddress.Address1 = "166 Main Street";

			CombineAssertions(() =>
			{
				AssertEquals("No address", ZString.Empty, authorizationHeader.AuthorizationAddress);

				authorizationHeader.CPH_OA_AppliesTo = orgAddress.PK;
				AssertEquals("Has address", "166 Main Street", authorizationHeader.AuthorizationAddress);
			});
		}

		public void TestCusAuthorisationRuleCodes()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No rules", ZString.Empty, authorizationHeader.CusAuthorisationRuleCodes);

				authorizationHeader.CusAuthorisationRules.AddNew().CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				AssertEquals("1 rule", "LOC", authorizationHeader.CusAuthorisationRuleCodes);

				authorizationHeader.CusAuthorisationRules.AddNew().CPR_RuleCode = "USE";
				authorizationHeader.CusAuthorisationRules.AddNew().CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				authorizationHeader.CusAuthorisationRules.AddNew().CPR_RuleCode = "BRE";
				AssertEquals("Comma separated, ordered, duplicates not removed", "BRE, LOC, LOC, USE", authorizationHeader.CusAuthorisationRuleCodes);
			});
		}

		public void TestWarehouse()
		{
			AssertNull(authorizationHeader.Warehouse);

			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			authorizationHeader.CPH_OA_AppliesTo = orgAddress.PK;
			AssertNull(authorizationHeader.Warehouse);

			var warehouse = Factory.New<IWhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = orgAddress.PK;
			AssertNotNull(authorizationHeader.Warehouse);
		}

		public void TestAdHocReadOnly()
		{
			var orgHeaderWithValidAuthorisation = Factory.NewWithValidTestData<OrgHeader>();
			SetupCusPermit(authorizationHeader, "VALID");
			authorizationHeader.CPH_OH_PermitHolder = orgHeaderWithValidAuthorisation.PK;

			var rule = authorizationHeader.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = "RUL";
			rule.CPR_ValueFrom = "12345";

			Factory.Save();

			var user = Factory.NewWithValidTestData<GlbStaff>();

			RateSecurityTestHelper.CreateSecurityRight(Factory, user, Env.Security.FlagAuthorisationAdHoc.Code, true);
			var user2 = Factory.NewWithValidTestData<GlbStaff>();
			RateSecurityTestHelper.CreateSecurityRight(Factory, user2, Env.Security.FlagAuthorisationAdHoc.Code, false);
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("even if user is allowed for Env.Security.FlagAuthorisationAdHoc, AdHoc should be readonly", true, authorizationHeader.CPH_IsAdHocInfo.ReadOnly);
			}
			using (Env.SetTemporaryUserContext(user2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("For Lambda users, AdHoc should also be readonly", true, authorizationHeader.CPH_IsAdHocInfo.ReadOnly);
			}
		}

		public void TestRestrictedFilteredItemAttribute()
		{
			AssertNotNull(typeof(CusAuthorisationHeader).GetCustomAttribute<RestrictedFilteredItemAttribute>());
		}

		void SetupCusPermit(CommonCusPermitHeader header, ZString permitNumber)
		{
			header.CPH_Number = permitNumber;
			header.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			header.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			header.CPH_StartDate = ZDate.Today.AddDays(-1);
			header.CPH_EndDate = ZDate.Today.AddDays(1);
		}

		protected override void SetUp()
		{
			base.SetUp();
			authorizationHeader = Factory.New<CusAuthorisationHeader>();
		}
		CusAuthorisationHeader authorizationHeader;
	}

	[TestedType(typeof(CusAuthorisationHeader.Loader))]
	class CusAuthorisationHeaderLoaderTest : LoaderTestCase
	{
		public void TestGetAuthorisationNumber_EmptyCountryCode()
		{
			SetupCusPermit(authorizationHeader, "EMTPYCOUNTRYCODE");
			authorizationHeader.CPH_RN_NKCountryCode = ZString.Empty;
			AssertEquals(ZString.Empty, CusAuthorisationHeader.Loader.GetAuthorisationNumber(Factory, ZString.Empty, "TST", ZDate.Today, ZGuid.Empty));
		}

		public void TestGetAuthorisationNumber_EmptyType()
		{
			SetupCusPermit(authorizationHeader, "EMTPYTYPE");
			authorizationHeader.CPH_Type = ZString.Empty;
			AssertEquals("EMTPYTYPE", CusAuthorisationHeader.Loader.GetAuthorisationNumber(Factory, Core.Constants.CountryCodes.Germany, ZString.Empty, ZDate.Today, ZGuid.Empty));

			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			AssertEquals("", CusAuthorisationHeader.Loader.GetAuthorisationNumber(Factory, Core.Constants.CountryCodes.Germany, ZString.Empty, ZDate.Today, ZGuid.Empty));

			authorizationHeader.CPH_Type = ZString.Empty;
			AssertEquals("", CusAuthorisationHeader.Loader.GetAuthorisationNumber(Factory, Core.Constants.CountryCodes.Germany, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, ZDate.Today, ZGuid.Empty));
		}

		public void TestGetAuthorisationNumber_InvalidTransactionDate()
		{
			SetupCusPermit(authorizationHeader, "INVALIDDATE");
			AssertEquals(ZString.Empty, CusAuthorisationHeader.Loader.GetAuthorisationNumber(Factory, Core.Constants.CountryCodes.Germany, "TST", ZDate.Empty, ZGuid.Empty));
		}

		public void TestGetAuthorisationNumber_MultipleTypes()
		{
			var permitHeader = Factory.New<BaseCusPermitHeader>();
			SetupCusPermit(permitHeader, "PERMIT123");
			SetupCusPermit(authorizationHeader, "AUTHORISATION123");
			AssertEquals("AUTHORISATION123", CusAuthorisationHeader.Loader.GetAuthorisationNumber(Factory, Core.Constants.CountryCodes.Germany, "TST", ZDate.Today, ZGuid.Empty));
		}

		public void TestGetAuthorisationNumber_MultipleValidRecords()
		{
			var olderAuthorisation = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(olderAuthorisation, "OLDAUTHORISATION");
			olderAuthorisation.CPH_StartDate = ZDate.Today.AddDays(-2);
			SetupCusPermit(authorizationHeader, "NEWAUTHORISATION");
			AssertEquals("NEWAUTHORISATION", CusAuthorisationHeader.Loader.GetAuthorisationNumber(Factory, Core.Constants.CountryCodes.Germany, "TST", ZDate.Today, ZGuid.Empty));
		}

		public void TestGetAuthorisationNumber_PermitHolder()
		{
			var permitHolder = Factory.New<OrgHeader>();
			var noPermitHolderAuthorisation = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(noPermitHolderAuthorisation, "NOPERMITHOLDER");
			SetupCusPermit(authorizationHeader, "PERMITHOLDER");
			authorizationHeader.CPH_OH_PermitHolder = permitHolder.PK;
			AssertEquals("PERMITHOLDER", CusAuthorisationHeader.Loader.GetAuthorisationNumber(Factory, Core.Constants.CountryCodes.Germany, "TST", ZDate.Today, permitHolder.PK));
		}

		public void TestGetAuthorisationNumber_Inactive()
		{
			var permitHolder = Factory.New<OrgHeader>();
			SetupCusPermit(authorizationHeader, "INACTIVE");
			authorizationHeader.CPH_IsActive = false;
			authorizationHeader.CPH_OH_PermitHolder = permitHolder.PK;
			AssertEquals(ZString.Empty, CusAuthorisationHeader.Loader.GetAuthorisationNumber(Factory, Core.Constants.CountryCodes.Germany, "TST", ZDate.Today, permitHolder.PK));
		}

		public void TestGetAuthorisation()
		{
			var permitHolder = Factory.New<OrgHeader>();
			var olderAuthorisation = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(olderAuthorisation, "OLDAUTHORISATION");
			olderAuthorisation.CPH_OH_PermitHolder = permitHolder.PK;
			olderAuthorisation.CPH_StartDate = ZDate.Today.AddDays(-2);
			SetupCusPermit(authorizationHeader, "NEWAUTHORISATION");
			authorizationHeader.CPH_OH_PermitHolder = permitHolder.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Authorization with the latest CPH_StartDate is returned", "NEWAUTHORISATION", CusAuthorisationHeader.Loader.GetAuthorisation(Factory, Core.Constants.CountryCodes.Germany, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, ZDate.Today, permitHolder.PK).CPH_Number);
				AssertNull("SDE Not Exists", CusAuthorisationHeader.Loader.GetAuthorisation(Factory, Core.Constants.CountryCodes.Germany, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, ZDate.Today, permitHolder.PK));
			});
		}

		public void TestGetAuthorisations_MultipleTypes()
		{
			var permitHeader = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(permitHeader, "PERMIT123");
			permitHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			SetupCusPermit(authorizationHeader, "AUTHORISATION123");
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;

			var permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisations(Factory, Core.Constants.CountryCodes.Germany, new ZString[] { CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1 }, ZDate.Today, ZGuid.Empty);
			AssertContainsExactElementsInAnyOrder(new[] { permitHeader, authorizationHeader }, permitHeaders);
		}

		public void TestGetAuthorisations_MultiplePermitHolders()
		{
			var permitHolder1 = Factory.New<OrgHeader>();
			var permitHeader1 = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(permitHeader1, "VALID");
			permitHeader1.CPH_OH_PermitHolder = permitHolder1.PK;

			var permitHolder2 = Factory.New<OrgHeader>();
			var permitHeader2 = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(permitHeader2, "VALID2");
			permitHeader2.CPH_OH_PermitHolder = permitHolder2.PK;

			var permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisations(Factory, Core.Constants.CountryCodes.Germany, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, ZDate.Today, new[] { permitHolder1.PK });
			AssertContainsExactElementsInAnyOrder(new[] { permitHeader1 }, permitHeaders);

			permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisations(Factory, Core.Constants.CountryCodes.Germany, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, ZDate.Today, new[] { permitHolder1.PK, permitHolder2.PK });
			AssertContainsExactElementsInAnyOrder(new[] { permitHeader1, permitHeader2 }, permitHeaders);
		}

		public void TestGetAuthorisationsForAddresses_MultiplePermitAddresses()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			var permitHeader1 = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(permitHeader1, "VALID");
			permitHeader1.CPH_OA_AppliesTo = orgAddress.PK;

			var orgHeader2 = Factory.New<OrgHeader>();
			var orgAddress2 = orgHeader2.Addresses.AddNew();
			var permitHeader2 = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(permitHeader2, "VALID2");
			permitHeader2.CPH_OA_AppliesTo = orgAddress2.PK;

			var permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisationsForAddresses(Factory, Core.Constants.CountryCodes.Germany, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, ZDate.Today, new[] { orgAddress.PK });
			AssertContainsExactElementsInAnyOrder(new[] { permitHeader1 }, permitHeaders);

			permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisationsForAddresses(Factory, Core.Constants.CountryCodes.Germany, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, ZDate.Today, new[] { orgAddress.PK, orgAddress2.PK });
			AssertContainsExactElementsInAnyOrder(new[] { permitHeader1, permitHeader2 }, permitHeaders);
		}

		public void TestGetAuthorisationsForAddressesAndPermit_MultiplePermitAddresses()
		{
			var permitHolder1 = Factory.New<OrgHeader>();
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			var permitHeader1 = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(permitHeader1, "VALID");
			permitHeader1.CPH_OA_AppliesTo = orgAddress.PK;
			permitHeader1.CPH_OH_PermitHolder = permitHolder1.PK;

			var permitHolder2 = Factory.New<OrgHeader>();
			var orgHeader2 = Factory.New<OrgHeader>();
			var orgAddress2 = orgHeader2.Addresses.AddNew();
			var permitHeader2 = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(permitHeader2, "VALID2");
			permitHeader2.CPH_OA_AppliesTo = orgAddress2.PK;
			permitHeader2.CPH_OH_PermitHolder = permitHolder2.PK;

			var permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisationsForAddressesAndPermitHolder(Factory, Core.Constants.CountryCodes.Germany, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, ZDate.Today, new[] { permitHolder1.PK }, new[] { orgAddress.PK });
			AssertContainsExactElementsInAnyOrder(new[] { permitHeader1 }, permitHeaders);

			permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisationsForAddressesAndPermitHolder(Factory, Core.Constants.CountryCodes.Germany, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, ZDate.Today, new[] { permitHolder1.PK, permitHolder2.PK }, new[] { orgAddress.PK, orgAddress2.PK });
			AssertContainsExactElementsInAnyOrder(new[] { permitHeader1, permitHeader2 }, permitHeaders);
		}

		public void TestIsDuplicateAuthorisation()
		{
			var orgHeader = Factory.New<OrgHeader>();
			SetupCusPermit(authorizationHeader, "DUPLICATE");
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

			var authorizationHeader2 = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(authorizationHeader2, "DUPLICATE");
			authorizationHeader2.CPH_OH_PermitHolder = orgHeader.PK;

			CombineAssertions(() =>
			{
				authorizationHeader2.CPH_IsAdHoc = true;
				AssertEquals("CPH_IsAdHoc is true => not Duplicate", false, CusAuthorisationHeader.Loader.IsDuplicateAuthorisation(authorizationHeader2));

				authorizationHeader2.CPH_IsAdHoc = false;
				AssertEquals("CPH_IsAdHoc is false => Duplicate", true, CusAuthorisationHeader.Loader.IsDuplicateAuthorisation(authorizationHeader2));

				authorizationHeader2.CPH_Number = "DIFFERENT";
				AssertEquals("Different authorization number", false, CusAuthorisationHeader.Loader.IsDuplicateAuthorisation(authorizationHeader2));

				authorizationHeader2.CPH_Number = authorizationHeader.CPH_Number;
				authorizationHeader2.CPH_OH_PermitHolder = Factory.New<OrgHeader>().PK;
				AssertEquals("Same authorization number but different holder", false, CusAuthorisationHeader.Loader.IsDuplicateAuthorisation(authorizationHeader2));

				authorizationHeader2.CPH_OH_PermitHolder = authorizationHeader.CPH_OH_PermitHolder;
				authorizationHeader2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
				AssertEquals("Same authorization number but different country", false, CusAuthorisationHeader.Loader.IsDuplicateAuthorisation(authorizationHeader2));

				authorizationHeader2.CPH_RN_NKCountryCode = authorizationHeader.CPH_RN_NKCountryCode;
				authorizationHeader2.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
				AssertEquals("Same authorization number but different type", false, CusAuthorisationHeader.Loader.IsDuplicateAuthorisation(authorizationHeader2));
			});
		}

		public void TestHolderHasValidAuthorisationWithSpecificRule()
		{
			var orgHeaderWithValidAuthorisation = Factory.NewWithValidTestData<OrgHeader>();
			SetupCusPermit(authorizationHeader, "VALID");
			authorizationHeader.CPH_OH_PermitHolder = orgHeaderWithValidAuthorisation.PK;

			var rule = authorizationHeader.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = "RUL";
			rule.CPR_ValueFrom = "12345";

			var orgWithNoAuthorisaton = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			CombineAssertions("Check HolderHasValidAuthorisationWithSpecificRule()", () =>
			{
				AssertEquals("HolderHasValidAuthorisationWithSpecificRule", true, CusAuthorisationHeader.Loader.HolderHasValidAuthorisationWithSpecificRule(Factory, orgHeaderWithValidAuthorisation.PK, Core.Constants.CountryCodes.Germany, ZDateTime.Today, "RUL", "12345"));

				AssertEquals("Different country, HolderHasValidAuthorisationWithSpecificRule", false, CusAuthorisationHeader.Loader.HolderHasValidAuthorisationWithSpecificRule(Factory, orgHeaderWithValidAuthorisation.PK, Core.Constants.CountryCodes.Italy, ZDateTime.Today, "RUL", "12345"));
				AssertEquals("Different transaction date, HolderHasValidAuthorisationWithSpecificRule", false, CusAuthorisationHeader.Loader.HolderHasValidAuthorisationWithSpecificRule(Factory, orgHeaderWithValidAuthorisation.PK, Core.Constants.CountryCodes.Germany, ZDateTime.Today.AddMonths(-1), "RUL", "12345"));
				AssertEquals("Different holder, HolderHasValidAuthorisationWithSpecificRule", false, CusAuthorisationHeader.Loader.HolderHasValidAuthorisationWithSpecificRule(Factory, orgWithNoAuthorisaton.PK, Core.Constants.CountryCodes.Germany, ZDateTime.Today, "RUL", "12345"));
				AssertEquals("Different Rule Code, HolderHasValidAuthorisationWithSpecificRule", false, CusAuthorisationHeader.Loader.HolderHasValidAuthorisationWithSpecificRule(Factory, orgWithNoAuthorisaton.PK, Core.Constants.CountryCodes.Germany, ZDateTime.Today, "LUR", "12345"));
				AssertEquals("Different Value From, HolderHasValidAuthorisationWithSpecificRule", false, CusAuthorisationHeader.Loader.HolderHasValidAuthorisationWithSpecificRule(Factory, orgWithNoAuthorisaton.PK, Core.Constants.CountryCodes.Germany, ZDateTime.Today, "LUR", "12345"));

				AssertEquals("Empty parameters, HolderHasValidAuthorisationWithSpecificRule", false, CusAuthorisationHeader.Loader.HolderHasValidAuthorisationWithSpecificRule(Factory, ZGuid.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty));
				AssertExceptionThrown<ArgumentNullException>("Exception expected when Factory is null", () => CusAuthorisationHeader.Loader.HolderHasValidAuthorisationWithSpecificRule(null, orgHeaderWithValidAuthorisation.PK, Core.Constants.CountryCodes.Germany, ZDateTime.Today, "RUL", "12345"));
			});
		}

		public void TestHolderHasSpecificAuthorisationWithRule()
		{
			var orgHeaderWithValidAuthorisation = Factory.NewWithValidTestData<OrgHeader>();
			SetupCusPermit(authorizationHeader, "VALID");
			authorizationHeader.CPH_OH_PermitHolder = orgHeaderWithValidAuthorisation.PK;

			var rule = authorizationHeader.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = "RUL";
			rule.CPR_ValueFrom = "12345";

			var orgWithNoAuthorisation = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertHolderHasSpecificAuthorisationWithRule("HolderHasSpecificAuthorisationWithRule", true);

				AssertHolderHasSpecificAuthorisationWithRule("Different authorisation type", false, authorisationTpe: CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir);
				AssertHolderHasSpecificAuthorisationWithRule("Different authorisation number", false, authorisationNumber: "INVALID");
				AssertHolderHasSpecificAuthorisationWithRule("Different country", false, countryCode: Core.Constants.CountryCodes.Italy);
				AssertHolderHasSpecificAuthorisationWithRule("Different transaction date", false, transactionDate: ZDateTime.Today.AddMonths(-1));
				AssertHolderHasSpecificAuthorisationWithRule("Different holder", false, permitHolder: orgWithNoAuthorisation.PK);
				AssertHolderHasSpecificAuthorisationWithRule("Different Rule Code", false, ruleCode: "LUR");
				AssertHolderHasSpecificAuthorisationWithRule("Different Value From", false, valueFrom: "123456");

				AssertEquals("Empty parameters", false, CusAuthorisationHeader.Loader.HolderHasSpecificAuthorisationWithRule(Factory, ZGuid.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty));
				AssertExceptionThrown<ArgumentNullException>("Exception expected when Factory is null", () => CusAuthorisationHeader.Loader.HolderHasSpecificAuthorisationWithRule(null, orgHeaderWithValidAuthorisation.PK, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "VALID", Core.Constants.CountryCodes.Germany, ZDateTime.Today, "RUL", "12345"));
			});

			void AssertHolderHasSpecificAuthorisationWithRule(string message, bool expectedResult, ZGuid? permitHolder = null, string authorisationTpe = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, string authorisationNumber = "VALID", string countryCode = Core.Constants.CountryCodes.Germany, ZDateTime? transactionDate = null, string ruleCode = "RUL", string valueFrom = "12345")
			{
				AssertEquals(message, expectedResult, CusAuthorisationHeader.Loader.HolderHasSpecificAuthorisationWithRule(Factory, permitHolder ?? orgHeaderWithValidAuthorisation.PK, authorisationTpe, authorisationNumber, countryCode, transactionDate ?? ZDateTime.Today, ruleCode, valueFrom));
			}
		}

		public void TestGetAuthorisationsWithSpecificRule()
		{
			var permitOwner1 = Factory.NewWithValidTestData<OrgHeader>();

			SetupCusPermit(authorizationHeader, "VALID");
			authorizationHeader.CPH_OH_PermitHolder = permitOwner1.PK;
			var rule = authorizationHeader.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = "RUL";
			rule.CPR_ValueFrom = "12345";

			var permitOwner2 = Factory.NewWithValidTestData<OrgHeader>();

			var permitHeader2 = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(permitHeader2, "VALID2");
			permitHeader2.CPH_OH_PermitHolder = permitOwner2.PK;

			var permitHeader3 = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(permitHeader3, "VALID3");
			permitHeader3.CPH_OH_PermitHolder = permitOwner2.PK;
			var rule2 = permitHeader3.CusAuthorisationRules.AddNew();
			rule2.CPR_RuleCode = "RUL";
			rule2.CPR_ValueFrom = "12345";

			var permitHeader4 = Factory.New<CusAuthorisationHeader>();
			SetupCusPermit(permitHeader4, "VALID4");
			permitHeader4.CPH_OH_PermitHolder = permitOwner2.PK;
			var rule3 = permitHeader4.CusAuthorisationRules.AddNew();
			rule3.CPR_RuleCode = "RUL";
			rule3.CPR_ValueFrom = "6789";

			Factory.Save();

			var permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificRule(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, new[] { permitOwner1.PK }, Core.Constants.CountryCodes.Germany, ZDateTime.Today, "RUL", "12345");
			AssertContainsExactElementsInAnyOrder(new[] { authorizationHeader }, permitHeaders);

			permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificRule(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, new[] { permitOwner1.PK, permitOwner2.PK }, Core.Constants.CountryCodes.Germany, ZDateTime.Today, "RUL", "12345");
			AssertContainsExactElementsInAnyOrder(new[] { authorizationHeader, permitHeader3 }, permitHeaders);

			permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificRule(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, new[] { permitOwner1.PK, permitOwner2.PK }, Core.Constants.CountryCodes.Germany, ZDateTime.Today, "RUL", "12345", "6789");
			AssertContainsExactElementsInAnyOrder(new[] { authorizationHeader, permitHeader3, permitHeader4 }, permitHeaders);

			permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificRule(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, new[] { permitOwner1.PK, permitOwner2.PK }, Core.Constants.CountryCodes.Germany, ZDateTime.Today, "RUL", null);
			AssertEquals("Empty when ruleValues parameter is null", false, permitHeaders.Any());

			permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificRule(Factory, null, new[] { permitOwner1.PK }, Core.Constants.CountryCodes.Germany, ZDateTime.Today, "RUL", "12345");
			AssertEquals("Empty when types parameter is null", false, permitHeaders.Any());

			permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificRule(Factory, new ZString[] { "" }, new[] { permitOwner1.PK }, Core.Constants.CountryCodes.Germany, ZDateTime.Today, "RUL", "12345");
			AssertEquals("Empty when types parameters has only empty values", false, permitHeaders.Any());

			permitHeaders = CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificRule(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.TemporaryStorage }, null, Core.Constants.CountryCodes.Germany, ZDateTime.Today, "RUL", "12345");
			AssertEquals("Empty when permitHeaders parameters is null", false, permitHeaders.Any());

			AssertExceptionThrown<ArgumentNullException>("Exception expected when Factory is null", () => CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificRule(null, new ZString[] { "RUL" }, new[] { permitOwner1.PK }, Core.Constants.CountryCodes.Germany, ZDateTime.Today, "RUL", "12345"));
		}

		public void TestCustomsNumberProvider()
		{
			authorizationHeader.CPH_Type = ZString.Empty;
			AssertNull("Empty CPH_Type has no CustomsNumberProvider", authorizationHeader.CustomsNumberProvider);

			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			AssertNull("CW1 type has no CustomsNumberProvider.", authorizationHeader.CustomsNumberProvider);

			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			AssertNull("TST type has no CustomsNumberProvider.", authorizationHeader.CustomsNumberProvider);

			authorizationHeader.CPH_Type = "TSX";
			AssertNull("TSX starts with TS, but we should not load a TS provider.", authorizationHeader.CustomsNumberProvider);

			authorizationHeader.CPH_Type = "T1X";
			AssertNull("T1X starts with T1, but we should not load a TS provider.", authorizationHeader.CustomsNumberProvider);

			authorizationHeader.CPH_Type = "USX";
			AssertNull("USX starts with US, but we should not load an US provider.", authorizationHeader.CustomsNumberProvider);

			authorizationHeader.CPH_Type = "US";
			AssertNull("US is not a valid provider type for authorisation, it's only for company, we should not load it.", authorizationHeader.CustomsNumberProvider);
		}

		public void TestGetAuthorisationsWithSpecificNumberTypeAndCountryCode()
		{
			var permitHeader1 = Factory.New<CusAuthorisationHeader>();
			var permitHeader2 = Factory.New<CusAuthorisationHeader>();
			var permitHeader3 = Factory.New<CusAuthorisationHeader>();

			permitHeader1.CPH_Number = permitHeader2.CPH_Number = permitHeader3.CPH_Number = "NNN";
			permitHeader1.CPH_Type = permitHeader2.CPH_Type = permitHeader3.CPH_Type = "TTT";
			permitHeader1.CPH_RN_NKCountryCode = permitHeader2.CPH_RN_NKCountryCode = permitHeader3.CPH_RN_NKCountryCode = "AU";
			AssertContainsExactElementsInAnyOrder(new CusAuthorisationHeader[] { permitHeader1, permitHeader2, permitHeader3 }, CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificNumberTypeAndCountryCode(Factory, "NNN", "TTT", "AU"));

			permitHeader1.CPH_Number = "UUU";
			AssertContainsExactElementsInAnyOrder(new CusAuthorisationHeader[] { permitHeader2, permitHeader3 }, CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificNumberTypeAndCountryCode(Factory, "NNN", "TTT", "AU"));

			permitHeader2.CPH_Type = "YYY";
			AssertContainsExactElementsInAnyOrder(new CusAuthorisationHeader[] { permitHeader3 }, CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificNumberTypeAndCountryCode(Factory, "NNN", "TTT", "AU"));

			permitHeader3.CPH_RN_NKCountryCode = "US";
			AssertContainsExactElementsInAnyOrder(Array.Empty<CusAuthorisationHeader>(), CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificNumberTypeAndCountryCode(Factory, "NNN", "TTT", "AU"));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusAuthorisationHeader.Loader(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			authorizationHeader = Factory.New<CusAuthorisationHeader>();
		}
		CusAuthorisationHeader authorizationHeader;

		void SetupCusPermit(CommonCusPermitHeader header, ZString permitNumber)
		{
			header.CPH_Number = permitNumber;
			header.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			header.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			header.CPH_StartDate = ZDate.Today.AddDays(-1);
			header.CPH_EndDate = ZDate.Today.AddDays(1);
		}
	}

	class CusAuthorisationHeaderForTest : CusAuthorisationHeader
	{
		public CusAuthorisationHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusAuthorisationHeaderProvider GetProviderCore()
		{
			return new CusAuthorisationHeaderProviderForTest(CPH_RN_NKCountryCode);
		}
	}
}
