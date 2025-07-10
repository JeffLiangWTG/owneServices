using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(PermitFindBoxCollection))]
	sealed class PermitFindBoxCollectionTest : ActiveBusinessObjectCollectionTestCase<PermitFindBoxCollection>
	{
		public void TestMatchesFilterCore()
		{
			var guarantee1 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			guarantee1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;

			var permit1 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			permit1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			permit1.CPH_StartDate = ZDate.Today;
			permit1.CPH_EndDate = ZDate.Today;
			var rule1 = permit1.CusPermitRules.AddNew();
			rule1.CPR_RuleCode = "TAR";
			rule1.CPR_ValueFrom = "1122";
			rule1.CPR_ValueTo = "113";

			var permit2 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			permit2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			permit2.CPH_StartDate = ZDate.Today;
			permit2.CPH_EndDate = ZDate.Today;
			var rule2 = permit2.CusPermitRules.AddNew();
			rule2.CPR_RuleCode = "TAR";
			rule2.CPR_ValueFrom = "2211";
			rule2.CPR_ValueTo = "2222";

			Factory.Save();

			var coll = new PermitFindBoxCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, null, ZString.Empty, ZString.Empty, new Dictionary<CodeDescriptionPair, ZString>() { { new CodeDescriptionPair("TAR", "Tariff"), "1123" } }, ZDate.Today);

			AssertCollectionContains(permit1, coll);
			AssertCollectionNotContains(permit2, coll);
			AssertCollectionNotContains(guarantee1, coll);

			coll.ShouldIgnoreAdditionalFilter = () => true;
			coll.RefreshFromDb();

			AssertCollectionContains(permit1, coll);
			AssertCollectionContains(permit2, coll);
			AssertCollectionNotContains(guarantee1, coll);
		}

		public void TestMustHavePermitHolderErrorWhenAdditionalFilterNotMet()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var (permitFindBoxCollection, permitHeader) = SetupForAdditonalFilterTest(orgHeader);
			var expectedError = PermitFindBoxCollection.MustHavePermitHolder(orgHeader.OH_Code);

			CombineAssertions(() =>
			{
				var errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertContains("Error", expectedError, errors);
				permitHeader.CPH_OH_PermitHolder = orgHeader.PK;

				errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertNotContains("No Error", expectedError, errors);
			});
		}

		public void TestMustHavePermitTypeErrorWhenAdditionalFilterNotMet()
		{
			var (permitFindBoxCollection, permitHeader) = SetupForAdditonalFilterTest(Factory.NewWithValidTestData<OrgHeader>());
			var expectedError = PermitFindBoxCollection.MustHavePermitType(new ZString[] { "IMP", "EXP" });

			CombineAssertions(() =>
			{
				var errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertContains("Error", expectedError, errors);
				permitHeader.CPH_Type = "IMP";

				errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertNotContains("No Error", expectedError, errors);
			});
		}

		public void TestMustMatchPermitRuleAndCodeErrorWhenAdditionalFilterNotMet()
		{
			var (permitFindBoxCollection, permitHeader) = SetupForAdditonalFilterTest(Factory.NewWithValidTestData<OrgHeader>());
			var expectedRuleCodeError = PermitFindBoxCollection.MustMatchPermitRuleCode("Tariff");
			var expectedRuleError = PermitFindBoxCollection.MustMatchPermitRule("1234", "Tariff");

			CombineAssertions(() =>
			{
				var errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertContains("Rule Code Error", expectedRuleCodeError, errors);

				var rule = permitHeader.CusPermitRules.AddNew();
				rule.CPR_RuleCode = "TAR";
				rule.CPR_ValueFrom = "1235";
				rule.CPR_ValueTo = "1235";

				errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertNotContains("Rule Code No Error", expectedRuleCodeError, errors);

				errors = permitFindBoxCollection.GetExtraNotification(permitHeader).Message;
				AssertContains("Rule Error", expectedRuleError, errors);

				rule.CPR_ValueFrom = "1234";
				rule.CPR_ValueTo = "1234";

				errors = permitFindBoxCollection.GetExtraNotification(permitHeader)?.Message;
				AssertNotContains("Rule No Error", expectedRuleError, errors);
			});
		}

		public void TestMustMatchPermitDateErrorWhenAdditionalFilterNotMet()
		{
			var (permitFindBoxCollection, permitHeader) = SetupForAdditonalFilterTest(Factory.NewWithValidTestData<OrgHeader>());
			var today = ZDate.Today;
			var expectedError = PermitFindBoxCollection.MustMatchPermitDate(today);

			CombineAssertions(() =>
			{
				permitHeader.CPH_StartDate = today.AddDays(1);
				var errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertContains("Error", expectedError, errors);

				permitHeader.CPH_StartDate = today;
				permitHeader.CPH_EndDate = today;

				errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertNotContains("No Error", expectedError, errors);
			});
		}

		public void TestMustHavePermitQtyValIndicatorErrorWhenAdditionalFilterNotMet()
		{
			var (permitFindBoxCollection, permitHeader) = SetupForAdditonalFilterTest(Factory.NewWithValidTestData<OrgHeader>());
			var expectedError = PermitFindBoxCollection.MustHavePermitQtyValIndicator(PermitQtyValIndicatorList.Codes.VAL);

			CombineAssertions(() =>
			{
				var errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertContains("Error", expectedError, errors);

				permitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
				errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertNotContains("No Error", expectedError, errors);
			});
		}

		public void TestMustHavePositiveValueBalanceWhenAdditionalFilterNotMet()
		{
			var (permitFindBoxCollection, permitHeader) = SetupForAdditonalFilterTest(Factory.NewWithValidTestData<OrgHeader>());
			var expectedError = PermitFindBoxCollection.MustHaveValueBalance;

			CombineAssertions(() =>
			{
				permitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
				var errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertContains("0 balance", expectedError, errors);

				var transaction = permitHeader.CusPermitLineTransactions.AddNew();
				transaction.CPL_Reference = "OBL REF";
				transaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
				transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
				transaction.CPL_TranValue = 1000m;

				errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertNotContains("Positive balance", expectedError, errors);

				var transaction2 = permitHeader.CusPermitLineTransactions.AddNew();
				transaction2.CPL_Reference = "Tran 1";
				transaction2.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
				transaction2.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
				transaction2.CPL_TranValue = -600m;

				var transaction3 = permitHeader.CusPermitLineTransactions.AddNew();
				transaction3.CPL_Reference = "Tran 2";
				transaction3.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
				transaction3.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
				transaction3.CPL_TranValue = -500m;

				errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertContains("Negative balance", expectedError, errors);

				transaction3.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Deleted;
				errors = ((IActiveBusinessObjectCollection)permitFindBoxCollection).GetAllNotificationsWhenAdditionalFilterNotMet(permitHeader);
				AssertNotContains("Positive balance multiple transactions", expectedError, errors);
			});
		}

		public void TestGetTariffQuery()
		{
			var guarantee1 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			guarantee1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;

			var header1 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			header1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var rule1 = header1.CusPermitRules.AddNew();
			rule1.CPR_RuleCode = "TAR";
			rule1.CPR_ValueFrom = "076010";
			rule1.CPR_ValueTo = "076020";
			var ruleException1 = rule1.CusPermitRuleExceptions.AddNew();
			ruleException1.CPE_ValueFrom = "076015";
			ruleException1.CPE_ValueTo = "076017";

			var header2 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			header2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var rule2 = header2.CusPermitRules.AddNew();
			rule2.CPR_RuleCode = "ZZZ";
			rule2.CPR_ValueFrom = "076010";
			rule2.CPR_ValueTo = "076020";

			Factory.Save();

			var coll = new CusPermitHeaderCollection(Factory);
			coll.AdditionalFilter = PermitFindBoxCollection.GetRuleQueryForCodePartOnly(Core.Constants.CountryCodes.SouthAfrica, "TAR");
			AssertEquals(1, coll.Count);
			AssertEquals(header1.PK, coll[0].PK);

			coll.AdditionalFilter = PermitFindBoxCollection.GetRuleQueryForCodePartOnly(Core.Constants.CountryCodes.SouthAfrica, "PRD");
			AssertEquals(0, coll.Count);
		}

		public void TestGetCachedCollection()
		{
			var org = Factory.New<OrgHeader>();
			var list1 = PermitFindBoxCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, org, ZString.Empty, ZString.Empty, new Dictionary<CodeDescriptionPair, ZString>() { { new CodeDescriptionPair("TAR", "Tariff"), ZString.Empty } }, ZDate.Today);
			var list2 = PermitFindBoxCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, org, ZString.Empty, ZString.Empty, new Dictionary<CodeDescriptionPair, ZString>() { { new CodeDescriptionPair("TAR", "Tariff"), ZString.Empty } }, ZDate.Today);
			AssertSame("Cached", list1, list2);
		}

		protected override PermitFindBoxCollection GetCollectionToTest()
		{
			var filterRulesSubKey = "TAR-4";
			var key = string.Format(CultureInfo.InvariantCulture, "PermitFindBoxCollection{0}_{1}_{2}_{3}_{4}_{5}", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, importer?.PK, "IMP", "", filterRulesSubKey, ZDate.Today);
			Factory.ClearCachedValue<PermitFindBoxCollection>(key);
			return PermitFindBoxCollection.GetCachedCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, importer, "IMP", ZString.Empty, new Dictionary<CodeDescriptionPair, ZString>() { { new CodeDescriptionPair("TAR", "Tariff"), "4" } }, ZDate.Today);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			permitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;
			permitHeader.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			permitHeader.CPH_OH_PermitHolder = importer.PK;
			permitHeader.CPH_Type = "IMP";
			permitHeader.CPH_StartDate = ZDate.Today;
			permitHeader.CPH_EndDate = ZDate.Today;
			var rule = permitHeader.CusPermitRules.AddNew();
			rule.CPR_RuleCode = "TAR";
			rule.CPR_ValueFrom = "1";
			rule.CPR_ValueTo = "9";
			Factory.Save();
			return permitHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			importer = Factory.NewWithValidTestData<OrgHeader>();
		}
		OrgHeader importer;

		(PermitFindBoxCollection PermitFindBoxCollection, BaseCusPermitHeader PermitHeader) SetupForAdditonalFilterTest(OrgHeader orgHeader)
		{
			var coll = PermitFindBoxCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, orgHeader, new ZString[] { "IMP", "EXP" }, ZString.Empty, new Dictionary<CodeDescriptionPair, ZString>() { { new CodeDescriptionPair("TAR", "Tariff"), "1234" } }, ZDate.Today, PermitQtyValIndicatorList.Codes.VAL, true);
			var permit = coll.AddNew();
			return (coll, permit);
		}
	}
}
