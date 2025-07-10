using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Statistics;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CreditCheckerTest : TestCaseWithFactory
	{
		public void TestLockRootIsCreatedByConstructor()
		{
			var originalChecker = Org.CreditChecker;
			var checker = new CreditChecker(Org);
			AssertNotNull("lockRoot", checker.lockRoot);
			AssertNotEquals("CreditCheckers are different", originalChecker, checker);
			AssertNotEquals("CreditCheckers have different lockRoots", originalChecker.lockRoot, checker.lockRoot);
		}

		public void TestIsThreadSafe()
		{
			AssertNotNull("Pre-fetch CurrentCompany to avoid its loading in background thread", GlbCompany.CurrentCompany);
			AssertNotNull("Pre-fetch Org.CompanyData to avoid its loading in background thread", Org.CompanyData);
			AssertNotNull("Pre-fetch Org.CreditChecker to use ObjectFactory synchronously in main thread as it is not thread safe", Org.CreditChecker);
			AssertNotNull("Pre-fetch Org_Diff.CompanyData to avoid its loading in background thread", Org_Diff.CompanyData);
			AssertNotNull("Pre-fetch Org_Diff.CreditChecker to use ObjectFactory synchronously in main thread as it is not thread safe", Org_Diff.CreditChecker);

			ThreadSafeAccessTestCase.RunTestOnMultipleThreads(delegate
			{
				Org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				Org.CreditChecker.AsyncFetchCreditLimitAndOutstandingBalance();
				Org.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit();
				Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsPayable, 10m);

				System.Threading.Thread.Sleep(10);

				Org_Diff.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
				Org_Diff.CreditChecker.AsyncFetchCreditLimitAndOutstandingBalance();
				Org_Diff.CreditChecker.IsCreditOnHold();
				Org_Diff.CreditChecker.IsCreditLimitExceededAfterThisAmount(LedgerTypes.AccountsReceivable, 100m);
			}, 10, ThreadSafeAccessTestCase.EndThreadTestAction.Join);
		}

		public void TestInAsyncCall()
		{
			DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
			dummyBizO.TestHeader = Org;
			dummyBizO.Ledger = LedgerTypes.AccountsReceivable;

			AssertEquals("InAsycCall default value", false, Org.CreditChecker.InAsyncCall);
			Assert("Can fetch Details", Org.CreditChecker.AsyncFetchCreditLimitAndOutstandingBalance());

			AccTransactionHeader aRInv = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 101m);
			Org.CompanyData.OB_ARCreditLimit = 100m;
			Factory.Save();

			Org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			Org.CreditChecker.InAsyncCall = true;

			// All public methods should not produce any errors while Async Call is in progress
			AssertEquals("Can't fetch Details", false, Org.CreditChecker.AsyncFetchCreditLimitAndOutstandingBalance());
			AssertNull("cachedBalance", Org.CreditChecker.cachedBalance);
			AssertNotNull("CachedCreditLimitAndOutstandingBalance can be invoked", Org.CreditChecker.CachedCreditLimitAndOutstandingBalance);

			AssertEquals(-1, Org.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit());

			AssertEquals(string.Empty, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, 1000000m));

			Org.CreditChecker.ValidateIsCreditLimitExceeded(dummyBizO.Z0_GuidInfo, LedgerTypes.AccountsReceivable);
			AssertNoWarnings(dummyBizO.Z0_GuidInfo);

			Org.CreditChecker.InAsyncCall = false;
			Org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

			AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(0, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));

			AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			// Back to normal after Async Call completed
			Assert("Can fetch Details", Org.CreditChecker.AsyncFetchCreditLimitAndOutstandingBalance());
			AssertNotNull("cachedBalance", Org.CreditChecker.cachedBalance);
			AssertNotNull("CachedCreditLimitAndOutstandingBalance can be invoked", Org.CreditChecker.CachedCreditLimitAndOutstandingBalance);

			AssertEquals("RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit", 3, Org.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit());
			AssertNotEquals("GetCreditLimitExceededValidation", string.Empty, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, 1000000m));

			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();

			AssertHasWarnings("Validation should produce a Credit Limit Warning", dummyBizO.Z0_GuidInfo);

			// Test On Hold Validation
			Org.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();

			Org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			Org.CreditChecker.InAsyncCall = true;

			AssertEquals("IsCreditOnHold", false, Org.CreditChecker.IsCreditOnHold());
			Org.CreditChecker.ValidateIsCreditOnHold(dummyBizO.Z0_GuidInfo, LedgerTypes.AccountsReceivable);
			AssertNoErrors("No errors as InAsyncCall", dummyBizO.Z0_GuidInfo);

			Org.CreditChecker.InAsyncCall = false;
			Org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

			AssertEquals("IsCreditOnHold", true, Org.CreditChecker.IsCreditOnHold());
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();

			AssertHasErrors("Credit On Hold error", dummyBizO.Z0_GuidInfo);
		}

		public void TestCachedError()
		{
			AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			var creditLimitAndBalance = Org.CreditChecker.CachedCreditLimitAndOutstandingBalance;
			Org.CreditChecker.HandleException(new InvalidOperationException("Some error"));

			AssertNotEquals("No caching for Details or error, should be new one", creditLimitAndBalance, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance);
			AssertNull("Cached Error", Org.CreditChecker.cachedError);

			AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			Org.CreditChecker.HandleException(new InvalidOperationException("Some error"));
			AssertEquals("Cached Error", "Some error", Org.CreditChecker.cachedError);

			try
			{
				AssertNull("Caching in use, should be null", Org.CreditChecker.CachedCreditLimitAndOutstandingBalance);
				Fail("Should throw Cached Error");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				AssertEquals("Cached Error thrown", "Some error", ex.Message);
			}
			AssertEquals("Cached Error", "Some error", Org.CreditChecker.cachedError);

			Org.CreditChecker.ResetCache(true);
			Org.CreditChecker.HandleException(new InvalidOperationException("WebService Error", new OrgCreditLimitAndBalanceExceptions.WebServiceException(Business.OrgCreditLimitAndBalanceExceptions.WebServiceException.ExceptionReason.NoDataInCache)));
			AssertNull("WebService Error should not be cached", Org.CreditChecker.cachedError);
		}

		public void TestGetCreditDetails()
		{
			var inv = CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			FillInvoice(inv);

			CreateUnPostedUnRecognisedRevenue(50m);
			CreateUnPostedRecognisedRevenue(25m);
			Factory.Save();

			CreateClaim(inv, "OPN", 20m);
			Factory.Save();

			Org.OH_Code = "TestOrg";
			Org.CompanyData.OB_AROnCreditHold = ZBool.True;
			Org.CompanyData.OB_ARCreditLimit = 200m;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 190m;
			Org.MiscServ.OM_ARGlobalOnCreditHold = true;
			Factory.Save();

			using (IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted))
			using (ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var results = Org.CreditChecker.GetCreditDetails(LedgerTypes.AccountsReceivable);
				AssertEquals(200m, results.CreditLimit);
				AssertEquals(80m, results.TotalOutstandingAmount);
				Assert(results.OnCreditHold);
				Assert(results.IsGlobalCreditApproved);
				AssertEquals("AUD", results.GlobalCreditCurrency);
				AssertEquals(190m, results.GlobalCreditLimit);
				AssertEquals(80m, results.GlobalTotalOutstandingAmount);
				Assert(results.IsOnGlobalCreditHold);
			}
		}

		public void TestCachedCreditLimitAndOutstandingBalance()
		{
			AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var creditLimitAndBalance = Org.CreditChecker.CachedCreditLimitAndOutstandingBalance;
			AssertNotEquals("No caching, should be new one", creditLimitAndBalance, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance);

			AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			Org = Factory.NewWithValidTestData<OrgHeader>();  // Need a new Org as CreditChecked reads CreditLimitCacheExpiryPeriod registry only on construction
			creditLimitAndBalance = Org.CreditChecker.CachedCreditLimitAndOutstandingBalance;
			AssertEquals("Caching in use, should be the same", creditLimitAndBalance, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance);
		}

		public void TestCachedCreditLimitAndOutstandingBalance_OutstandingBalanceAR()
		{
			AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			AccTransactionHeader aRInv = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 98m);
			AccTransactionHeader aRRec = GetNewReceipt(Org, LedgerTypes.AccountsReceivable, -11m);
			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Factory.Save();
			AssertEquals("AR Credit Balance should be 87", 87m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.OutstandingBalance(LedgerTypes.AccountsReceivable));
			AssertEquals("AR Global Credit Balance should be 87", 87m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.ARGlobalOutstandingBalance);

			AccTransactionHeader aRInv_Cancelled = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 70m, true);
			AccTransactionHeader aRInv_Reversed = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, -70m, true);
			Factory.Save();
			AssertEquals("AR Credit Balance should still be 87", 87m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.OutstandingBalance(LedgerTypes.AccountsReceivable));
			AssertEquals("AR Global Credit Balance should still be 87", 87m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.ARGlobalOutstandingBalance);

			AccTransactionHeader aRInv_DiffOrg = GetNewInvoice(Org_Diff, LedgerTypes.AccountsReceivable, 188m);
			Factory.Save();
			AssertEquals("AR Credit Balance should still be 87", 87m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.OutstandingBalance(LedgerTypes.AccountsReceivable));
			AssertEquals("AR Global Credit Balance should still be 87", 87m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.ARGlobalOutstandingBalance);

			AccTransactionHeader aPPay = GetNewPayment(Org, LedgerTypes.AccountsPayable, 654m);
			Factory.Save();
			AssertEquals("AR Credit Balance should still be 87", 87m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.OutstandingBalance(LedgerTypes.AccountsReceivable));
			AssertEquals("AR Global Credit Balance should still be 87", 87m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.ARGlobalOutstandingBalance);

			GlbCompany company_Diff = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch branch_Diff = Factory.NewWithValidTestData<GlbBranch>();
			branch_Diff.GB_GC = company_Diff.PK;
			Factory.Save();

			AccTransactionHeader aRInv_DiffComp = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 80m);
			aRInv_DiffComp.AH_GB = branch_Diff.PK;
			Factory.Save();
			AssertEquals("AR Credit Balance should still be 87", 87m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.OutstandingBalance(LedgerTypes.AccountsReceivable));
			AssertEquals("AR Global Credit Balance should still be 87", 87m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.ARGlobalOutstandingBalance);
		}

		public void TestCachedCreditLimitAndOutstandingBalance_OutstandingBalanceAP()
		{
			AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			Org.OH_IsCreditor = true;
			Factory.Save();
			Org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

			AccTransactionHeader aPInv = GetNewInvoice(Org, LedgerTypes.AccountsPayable, -166m);
			Factory.Save();
			AssertEquals("AP Credit Balance should be 166", 166m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.OutstandingBalance(LedgerTypes.AccountsPayable));

			AccTransactionHeader aPPay = GetNewPayment(Org, LedgerTypes.AccountsPayable, 33m);
			Factory.Save();
			AssertEquals("AP Credit Balance should be 133", 133m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.OutstandingBalance(LedgerTypes.AccountsPayable));
		}

		public void TestCachedOutstandingBalance()
		{
			AccTransactionHeader aRInv = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 70m);
			// Minimum set up for a GLobal Credit
			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Factory.Save();
			AssertEquals("Cached outstanding balance should be 70", 70m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.OutstandingBalance(LedgerTypes.AccountsReceivable));
			AssertEquals("Cached globaloutstanding balance should be 70", 70m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.ARGlobalOutstandingBalance);

			AccTransactionHeader aRInv2 = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 80m);
			Factory.Save();
			AssertEquals("Cached outstanding balance should still be 70", 70m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.OutstandingBalance(LedgerTypes.AccountsReceivable));
			AssertEquals("Cached global outstanding balance should still be 70", 70m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.ARGlobalOutstandingBalance);
		}

		public void TestIsCreditLimitExceededAfterThisAmount()
		{
			AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			Org.OH_IsDebtor = true;
			Org.CompanyData.OB_ARCreditLimit = 130m;
			Factory.Save();

			AccTransactionHeader aRInv = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 100m);
			Factory.Save();

			bool? isCreditLimitExceeded = !Org.CreditChecker.IsCreditLimitExceededAfterThisAmount(LedgerTypes.AccountsReceivable, 20m);
			AssertNotNull(isCreditLimitExceeded);
			Assert("Adding 20 should not exceed the credit limit", isCreditLimitExceeded.Value);

			isCreditLimitExceeded = !Org.CreditChecker.IsCreditLimitExceededAfterThisAmount(LedgerTypes.AccountsReceivable, 30m);
			AssertNotNull(isCreditLimitExceeded);
			Assert("Adding 30 should not exceed the credit limit", isCreditLimitExceeded.Value);

			isCreditLimitExceeded = Org.CreditChecker.IsCreditLimitExceededAfterThisAmount(LedgerTypes.AccountsReceivable, 40m);
			AssertNotNull(isCreditLimitExceeded);
			Assert("Adding 40 should exceed the credit limit", isCreditLimitExceeded.Value);

			AccTransactionHeader aRRec = GetNewReceipt(Org, LedgerTypes.AccountsReceivable, -150m);
			Factory.Save();

			isCreditLimitExceeded = !Org.CreditChecker.IsCreditLimitExceededAfterThisAmount(LedgerTypes.AccountsReceivable, 10m);
			AssertNotNull(isCreditLimitExceeded);
			Assert("Adding 10 should not exceed the credit limit", isCreditLimitExceeded.Value);

			isCreditLimitExceeded = Org.CreditChecker.IsCreditLimitExceededAfterThisAmount(LedgerTypes.AccountsReceivable, 200m);
			AssertNotNull(isCreditLimitExceeded);
			Assert("Adding 200 should exceed the credit limit", isCreditLimitExceeded.Value);

			AccTransactionHeader aRInv2 = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 300m);
			Factory.Save();

			Org.CompanyData.OB_ARCreditLimit = 0m;
			Factory.Save();

			isCreditLimitExceeded = !Org.CreditChecker.IsCreditLimitExceededAfterThisAmount(LedgerTypes.AccountsReceivable, 1m);
			AssertNotNull(isCreditLimitExceeded);
			Assert("Adding anything should not exceed the credit limit since there is no set limit", isCreditLimitExceeded.Value);
		}

		public void TestCachedIsCreditLimitExceededAfterThisAmount()
		{
			Org.OH_IsDebtor = true;
			Org.CompanyData.OB_ARCreditLimit = 130m;
			Factory.Save();

			AccTransactionHeader aRInv = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 100m);
			Factory.Save();

			bool? isCreditLimitExceeded = Org.CreditChecker.IsCreditLimitExceededAfterThisAmount(LedgerTypes.AccountsReceivable, 20m);
			AssertNotNull(isCreditLimitExceeded);
			AssertEquals("Adding 20 should not exceed the credit limit", false, isCreditLimitExceeded.Value);

			aRInv = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 50m);
			Factory.Save();

			isCreditLimitExceeded = Org.CreditChecker.IsCreditLimitExceededAfterThisAmount(LedgerTypes.AccountsReceivable, 20m);
			AssertNotNull(isCreditLimitExceeded);
			AssertEquals("Still should not exceed the credit limit because it is cached", false, isCreditLimitExceeded.Value);
		}

		public void TestIsCreditLimitExceeded()
		{
			Org.OH_IsCreditor = true;
			Org.CompanyData.OB_APCreditLimit = 400m;
			Factory.Save();

			AccTransactionHeader aPInv = GetNewInvoice(Org, LedgerTypes.AccountsPayable, -300m);
			Factory.Save();

			bool? isCreditLimitExceeded = Org.CreditChecker.IsCreditLimitExceededClearCacheForTest(LedgerTypes.AccountsPayable);
			AssertNotNull(isCreditLimitExceeded);
			AssertEquals("Credit limit should not be exceeded", false, isCreditLimitExceeded.Value);

			AccTransactionHeader aPInv2 = GetNewInvoice(Org, LedgerTypes.AccountsPayable, -101m);
			FillInvoice(aPInv2);
			Factory.Save();

			isCreditLimitExceeded = Org.CreditChecker.IsCreditLimitExceededClearCacheForTest(LedgerTypes.AccountsPayable);
			AssertNotNull(isCreditLimitExceeded);
			AssertEquals("Credit limit should be exceeded", true, isCreditLimitExceeded.Value);

			CreateClaim(aPInv2, "OPN", 2m);
			Factory.Save();
			using (ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				isCreditLimitExceeded = Org.CreditChecker.IsCreditLimitExceededClearCacheForTest(LedgerTypes.AccountsPayable);
				AssertNotNull(isCreditLimitExceeded);
				AssertEquals("Credit limit should be exceeded", false, isCreditLimitExceeded.Value);
			}
		}

		public void TestRequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit()
		{
			AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit(false);
		}

		public void TestRequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit_CreditOnHold()
		{
			AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit(true);
		}

		public void TestRequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingGlobalCreditLimit()
		{
			AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit(false, true);
		}

		public void TestRequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingGlobalCreditLimit_CreditOnHold()
		{
			AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit(true, true);
		}

		public void AssertRequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit(bool creditOnHold, bool isGlobal = false)
		{
			AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var emptyCollection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();

			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(100, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired));
			collection.Add(CreateNewSettings(500, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
			collection.Add(CreateNewSettings(1000, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly));
			collection.Add(CreateNewSettings(1000, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));

			if (isGlobal)
			{
				AccountingMasterFilesRegistry.Instance.GlobalCreditControllerOverrideThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
				AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emptyCollection);

				Org.MiscServ.OM_ARGlobalCreditApproved = true;
				Org.MiscServ.OM_ARGlobalOnCreditHold = creditOnHold;
				Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
				Org.MiscServ.OM_ARGlobalCreditLimit = 400m;
			}
			else
			{
				AccountingMasterFilesRegistry.Instance.GlobalCreditControllerOverrideThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyCollection);
				AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

				Org.CompanyData.OB_ARCreditLimit = 400m;
				Org.CompanyData.OB_AROnCreditHold = creditOnHold;
			}

			Org.OH_IsDebtor = true;
			Org.OH_IsCreditor = true;
			Factory.Save();

			var aPInv = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 300m);
			Factory.Save();

			AssertEquals("Not over credit limit so no authorization required", 0, Org.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit());

			var aPInv2 = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 200m);
			Factory.Save();

			AssertEquals("Credit limit exceeded, Level 0 authorization required", 0, Org.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit());

			var aPInv3 = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 200m);
			Factory.Save();

			AssertEquals("Credit limit exceeded, Level 1 authorization required", 1, Org.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit());

			var aPInv4 = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 1300m);
			Factory.Save();

			AssertEquals("Credit limit exceeded, Level 3 authorization required", 3, Org.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit());

			if (isGlobal)
			{
				Org.MiscServ.OM_ARGlobalCreditLimit = 2000m;
			}
			else
			{
				Org.CompanyData.OB_ARCreditLimit = 2000m;
			}
			Factory.Save();

			AssertEquals("At credit limit, no authorization required", 0, Org.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit());

			var registryItem = isGlobal ? IncludeUnpostedRevenueInGlobalCreditLimitCalculation : IncludeUnpostedRevenueInCreditLimitCalculation;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedAndRecognized);

			CreateUnPostedUnRecognisedRevenue(150m);
			if (isGlobal)
			{
				Org.MiscServ.OM_ARGlobalCreditLimit = 2001m;
			}
			else
			{
				Org.CompanyData.OB_ARCreditLimit = 2001m;
			}
			Factory.Save();

			AssertEquals("Below credit limit so no authorization required", 0, Org.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit());

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);
			Factory.Save();

			AssertEquals("Credit limit exceeded, Level 1 authorization required", 1, Org.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit());
		}

		AmountOrPercentageBasedThreeLevelAuthorisationRequirement CreateNewSettings(ZDecimal amount, ZDecimal percentage, ZString range, ZString auth)
		{
			var settings = new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();
			settings.Amount = amount;
			settings.Percentage = percentage;
			settings.Range = range;
			settings.AuthorisationRequirement = auth;

			return settings;
		}

		public void TestDoesExceedCreditLimit()
		{
			Org.IsSettlementGroup("AR");
			Factory.ClearCachedValue<bool>(Org.PK.ToString() + "AR" + GlbCompany.CurrentCompany.PK);
			AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			Org.OH_IsDebtor = true;
			Org.OH_IsCreditor = true;
			Org.CompanyData.OB_ARCreditLimit = 400m;
			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 500m;
			Factory.Save();

			var aRInv = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 300m);
			Factory.Save();

			Assert("Should not be at or over Credit Limit", !Org.CreditChecker.DoesExceedCreditLimit());

			var aRInv2 = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 100m);
			Factory.Save();

			Assert("Should be at Credit Limit", !Org.CreditChecker.DoesExceedCreditLimit());

			var aRInv3 = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 10m);
			Factory.Save();

			Assert("Should be over Credit Limit, not over Global Credit Limit", Org.CreditChecker.DoesExceedCreditLimit());

			Org.CompanyData.OB_ARCreditLimit = 420m;
			Org.MiscServ.OM_ARGlobalCreditLimit = 400m;
			Factory.Save();

			Assert("Should be over Global Credit Limit, not over Credit Limit", Org.CreditChecker.DoesExceedCreditLimit());

			Org.CompanyData.OB_ARCreditLimit = 450m;
			Org.MiscServ.OM_ARGlobalCreditLimit = 450m;
			CreateUnPostedRecognisedRevenue(50m);
			Factory.Save();

			Assert("Should not be over Credit Limit and Global Credit Limit", !Org.CreditChecker.DoesExceedCreditLimit());

			var registryItem = IncludeUnpostedRevenueInCreditLimitCalculation;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedAndRecognized);

			Assert("Should be over Credit Limit and Global Credit Limit", Org.CreditChecker.DoesExceedCreditLimit());

			Org.CompanyData.OB_ARCreditLimit = 460m;
			Org.MiscServ.OM_ARGlobalCreditLimit = 460m;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);
			Factory.Save();

			Assert("Should not be over Credit Limit and Global Credit Limit", !Org.CreditChecker.DoesExceedCreditLimit());

			CreateUnPostedUnRecognisedRevenue(50m);
			Factory.Save();

			Assert("Should be over Credit Limit and Global Credit Limit", Org.CreditChecker.DoesExceedCreditLimit());
		}

		public void TestIsAtOrOverARCreditLimit()
		{
			Org.IsSettlementGroup("AR");
			Factory.ClearCachedValue<bool>(Org.PK.ToString() + "AR" + GlbCompany.CurrentCompany.PK);
			AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(0, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));
			AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			Org.OH_IsDebtor = true;
			Org.OH_IsCreditor = true;
			Org.CompanyData.OB_ARCreditLimit = 400m;
			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 380m;
			Factory.Save();

			AccTransactionHeader aPInv = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 300m);
			Factory.Save();

			AssertIsAtOrOverARCreditLimit(0, 9, "Should not be at or over Credit Limit");

			AccTransactionHeader aPInv2 = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 100m);
			Factory.Save();

			AssertIsAtOrOverARCreditLimit(0, 2, "Should be at Credit Limit but over Global Credit Limit. But we do not have globsl registry settings");

			collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(0, 5, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired));
			collection.Add(CreateNewSettings(0, 5, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
			AccountingMasterFilesRegistry.Instance.GlobalCreditControllerOverrideThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertIsAtOrOverARCreditLimit(1, 1, "Should be at Credit Limit but over Global Credit Limit. And we need first level approval from the globsl registry settings");

			AccTransactionHeader aPInv3 = GetNewInvoice(Org, LedgerTypes.AccountsReceivable, 1m);
			Factory.Save();

			AssertIsAtOrOverARCreditLimit(3, 7, "Should be over Credit Limit");

			Org.CompanyData.OB_ARCreditLimit = 500m;
			Org.MiscServ.OM_ARGlobalCreditLimit = 450m;
			CreateUnPostedRecognisedRevenue(100m);
			Factory.Save();

			AssertIsAtOrOverARCreditLimit(0, 1, "Should not be at or over Credit Limit anymore");

			var registryItem = IncludeUnpostedRevenueInCreditLimitCalculation;
			string originalValue = registryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedAndRecognized);
				AssertIsAtOrOverARCreditLimit(3, 3, "Should be at Credit Limit with Posted and Recognised amounts");

				Org.CompanyData.OB_ARCreditLimit = 550m;
				Org.MiscServ.OM_ARGlobalCreditLimit = 550m;
				CreateUnPostedUnRecognisedRevenue(50m);
				Factory.Save();

				AssertIsAtOrOverARCreditLimit(0, 2);

				Org.MiscServ.OM_ARGlobalCreditLimit = 480m;
				Factory.Save();

				AssertIsAtOrOverARCreditLimit(0, 2, "Below Credit limit and under 5% threshold for Global Credit");

				Org.MiscServ.OM_ARGlobalCreditLimit = 380m;
				Factory.Save();

				AssertIsAtOrOverARCreditLimit(1, 2, "Should be below Credit Limit but over Global Credit Limit. And we need first level approval from the globsl registry settings");

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);
				AssertIsAtOrOverARCreditLimit(3, 3, "Should be at Credit Limit with Posted, Recognised and Unrecognised amounts");

				Org.MiscServ.OM_ARGlobalCreditLimit = 400m;
				Factory.Save();
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
				AssertIsAtOrOverARCreditLimit(0, 2);
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		void AssertIsAtOrOverARCreditLimit(int expected, int loadCount, string assertionMsg = "IsAtOrOverARCreditLimit")
		{
			using (ParameterSuffixer.Instance.TemporaryUseNewCache_ForTest())
			{
				var cmtExecCount = Db.Connection.ExecutedCommandCount;
				var isAtOrOverCreditLimit = Org.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit();
				AssertEquals("DatabaseLoadCount", loadCount, Db.Connection.ExecutedCommandCount - cmtExecCount);
				AssertEquals(assertionMsg, expected, isAtOrOverCreditLimit);
			}
		}

		public void TestCachedIsAtOrOverCreditLimit()
		{
			Org.OH_IsCreditor = true;
			Org.CompanyData.OB_APCreditLimit = 400m;
			Factory.Save();

			AccTransactionHeader aPInv = GetNewInvoice(Org, LedgerTypes.AccountsPayable, -300m);
			Factory.Save();

			var cmtExecCount = Db.Connection.ExecutedCommandCount;
			int? isAtOrOverCreditLimit = Org.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit();
			AssertEquals("Stored procedure should be called", 8, Db.Connection.ExecutedCommandCount - cmtExecCount);
			AssertNotNull(isAtOrOverCreditLimit);
			Assert("Should not be at or over Credit Limit", isAtOrOverCreditLimit.Value == 0);

			AccTransactionHeader aPInv2 = GetNewInvoice(Org, LedgerTypes.AccountsPayable, -100m);
			Factory.Save();

			cmtExecCount = Db.Connection.ExecutedCommandCount;
			isAtOrOverCreditLimit = Org.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit();
			AssertNotNull(isAtOrOverCreditLimit);
			AssertEquals("Stored procedure should not be called", cmtExecCount, Db.Connection.ExecutedCommandCount);
			Assert("Should still not be at Credit Limit because of caching", isAtOrOverCreditLimit.Value == 0);
		}

		public void TestCachedCreditLimitAndOutstandingBalance_CreditLimit()
		{
			AccountingMasterFilesRegistry.Instance.CreditLimitCacheExpiryPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			AssertEquals("AP Credit Limit should be 0", 0m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.CreditLimit(LedgerTypes.AccountsPayable));

			Org.CompanyData.OB_ARCreditLimit = 90m;
			Factory.Save();

			AssertEquals("AP Credit Limit should be 0", 0m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.CreditLimit(LedgerTypes.AccountsPayable));
			AssertEquals("AR Credit Limit should be 90", 90m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.CreditLimit(LedgerTypes.AccountsReceivable));

			Org.CompanyData.OB_APCreditLimit = 100m;
			Factory.Save();

			AssertEquals("AP Credit limit should be 100", 100m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.CreditLimit(LedgerTypes.AccountsPayable));
			AssertEquals("AR Credit Limit should be 90", 90m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.CreditLimit(LedgerTypes.AccountsReceivable));
		}

		public void TestCachedCreditLimit()
		{
			Org.CompanyData.OB_APCreditLimit = 100m;
			Org.CompanyData.OB_ARCreditLimit = 90m;
			Factory.Save();

			AssertEquals("AP Credit limit should be 100", 100m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.CreditLimit(LedgerTypes.AccountsPayable));
			AssertEquals("AR Credit Limit should be 90", 90m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.CreditLimit(LedgerTypes.AccountsReceivable));

			Org.CompanyData.OB_APCreditLimit = 120m;
			Org.CompanyData.OB_ARCreditLimit = 100m;
			Factory.Save();

			AssertEquals("AP Credit limit should still be 100 because it is cached", 100m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.CreditLimit(LedgerTypes.AccountsPayable));
			AssertEquals("AR Credit Limit should still be 90 because it is cached", 90m, Org.CreditChecker.CachedCreditLimitAndOutstandingBalance.CreditLimit(LedgerTypes.AccountsReceivable));
		}

		public void TestValidateIsGlobalCreditOnHold()
		{
			DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
			dummyBizO.TestHeader = Org;
			dummyBizO.Ledger = LedgerTypes.AccountsReceivable;
			Org.CompanyData.OB_ARCreditLimit = 99m;
			Factory.Save();

			string expectedMessage = "This account is on global credit hold and cannot be billed to";
			Org_Diff.MiscServ.OM_ARGlobalCreditApproved = true;
			Org_Diff.MiscServ.OM_ARGlobalOnCreditHold = true;
			Org.MiscServ.OM_OH_ARGlobalCreditGroup = Org_Diff.PK;
			Org.CompanyData.OB_IsDebtor = ZBool.True;
			Org.CompanyData.OB_AROnCreditHold = false;
			Factory.Save();
			dummyBizO.CurrentTransactionAmount = 100m;
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertHasError("[AR Transaction] should be errors", dummyBizO.Z0_GuidInfo, expectedMessage);
			AssertNoWarnings("[AR Transaction] should be no warnings even though it is over credit limit because there is an error", dummyBizO.Z0_GuidInfo);
		}

		public void TestValidateIsCreditOnHold()
		{
			DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
			dummyBizO.TestHeader = Org;
			dummyBizO.Ledger = LedgerTypes.AccountsReceivable;
			Org.CompanyData.OB_ARCreditLimit = 99m;
			Factory.Save();

			string expectedMessage = "This account is on credit hold and cannot be billed to";
			Org.CompanyData.OB_IsDebtor = ZBool.True;
			Org.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();
			dummyBizO.CurrentTransactionAmount = 100m;
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertHasError("[AR Transaction] should be errors", dummyBizO.Z0_GuidInfo, expectedMessage);
			AssertNoWarnings("[AR Transaction] should be no warnings even though it is over credit limit because there is an error", dummyBizO.Z0_GuidInfo);

			Org.CompanyData.OB_AROnCreditHold = false;
			Factory.Save();
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertNoErrors("[AR Transaction] should be no errors", dummyBizO.Z0_GuidInfo);
			AssertHasWarnings("[AR Transaction] should be warnings because it is over credit limit", dummyBizO.Z0_GuidInfo);

			Org.CompanyData.OB_AROnCreditHold = true;
			dummyBizO.Ledger = LedgerTypes.AccountsPayable;
			Org.CompanyData.OB_APCreditLimit = 99m;
			Factory.Save();
			dummyBizO.CurrentTransactionAmount = 100m;
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertNoErrors("[AP Transaction] should be no errors", dummyBizO.Z0_GuidInfo);
			AssertHasWarnings("[AP Transaction] should be warnings because it is over credit limit", dummyBizO.Z0_GuidInfo);

			Org.CompanyData.OB_AROnCreditHold = false;
			Factory.Save();
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertNoErrors("[AP Transaction] should be no errors", dummyBizO.Z0_GuidInfo);
			AssertHasWarnings("[AP Transaction] should be warnings because it is over credit limit", dummyBizO.Z0_GuidInfo);
		}

		public void TestValidateIsAPCreditLimitExceeded()
		{
			CreatePostedRevenue(-100m, LedgerTypes.AccountsPayable);
			Org.CompanyData.OB_IsDebtor = ZBool.True;
			Factory.Save();

			DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
			dummyBizO.TestHeader = Org;
			dummyBizO.Ledger = LedgerTypes.AccountsPayable;
			Org.CompanyData.OB_APCreditLimit = 99m;
			Factory.Save();

			string expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 99.00 AUD. Credit approved.
The Total Outstanding Balance is 100.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD";

			dummyBizO.CurrentTransactionAmount = 0m;
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
			AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

			Org.CompanyData.OB_APCreditLimit = 149m;
			Factory.Save();

			expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 149.00 AUD. Credit approved.
The Total Outstanding Balance is 150.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, 50.00 AUD";
			dummyBizO.CurrentTransactionAmount = 50m;
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
			AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

			Org.CompanyData.OB_APCreditLimit = 49m;
			Factory.Save();

			expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 49.00 AUD. Credit approved.
The Total Outstanding Balance is 50.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, -50.00 AUD";
			dummyBizO.CurrentTransactionAmount = -50m;
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
			AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);
		}

		public void TestValidateIsAPCreditLimitExceededWhenUnderCreditLimit()
		{
			CreatePostedRevenue(-100m, LedgerTypes.AccountsPayable);
			Factory.Save();

			Org.CompanyData.OB_IsDebtor = ZBool.True;
			Org.CompanyData.OB_IsCreditor = ZBool.True;
			Factory.Save();

			DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
			dummyBizO.TestHeader = Org;
			dummyBizO.Ledger = LedgerTypes.AccountsPayable;
			Org.CompanyData.OB_APCreditLimit = 49m;
			Factory.Save();

			dummyBizO.CurrentTransactionAmount = 52m;
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertHasWarnings("Should be warnings", dummyBizO.Z0_GuidInfo);
			AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

			Org.CompanyData.OB_APCreditLimit = 101m;
			Factory.Save();

			dummyBizO.CurrentTransactionAmount = 0m;
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertNoWarnings("Should be no warnings", dummyBizO.Z0_GuidInfo);
			AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);
		}

		public void TestValidateIsCreditLimitExceededWhenCreditOnHold()
		{
			CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			CreateUnPostedUnRecognisedRevenue(50m);
			CreateUnPostedRecognisedRevenue(25m);
			Factory.Save();

			var registryItem = IncludeUnpostedRevenueInCreditLimitCalculation;
			var originalValue = registryItem.Value;

			Org.OH_Code = "TestOrg";
			Org.CompanyData.OB_IsDebtor = ZBool.True;
			Org.CompanyData.OB_AROnCreditHold = ZBool.True;
			Org.CompanyData.OB_ARCreditLimit = 200m;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 200m;
			Factory.Save();

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted);

				const string expectedMessage = @"TestOrg is on Credit Hold.

The Credit Limit for TestOrg is set to 200.00 AUD. Credit approved.
The Total Outstanding Balance is 50.00 AUD, which has not exceeded the credit limit threshold.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, -50.00 AUD";
				AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, -50m));
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestGetCreditLimitExceededValidation_IsCreditOnHold_IsDebtorUsageConsistency()
		{
			Org.OH_Code = "TestOrg";
			Org.CompanyData.OB_IsDebtor = true;
			Org.CompanyData.OB_AROnCreditHold = true;
			Org.CompanyData.OB_ARCreditLimit = 200m;
			Factory.Save();

			Assert("Precondition: IsDebtor", Org.CompanyData.OB_IsDebtor);
			const string expectedMessage = @"TestOrg is on Credit Hold.

The Credit Limit for TestOrg is set to 200.00 AUD. Credit approved.
The Total Outstanding Balance is -50.00 AUD, which has not exceeded the credit limit threshold.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 0.00 AUD
  *  the Current Transaction Amount, -50.00 AUD";
			AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, -50m));
			Assert("IsCreditOnHold", Org.CreditChecker.IsCreditOnHold());

			Org.CompanyData.OB_IsDebtor = false;
			Factory.Save();
			Assert("Precondition: IsDebtor", !Org.CompanyData.OB_IsDebtor);
			AssertEquals("We check IsDebtor to keep that check consistent with IsCreditOnHold method.", "", Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, -50m));
			Assert("IsCreditOnHold", !Org.CreditChecker.IsCreditOnHold());
		}

		public void TestValidateIsCreditLimitExceededWhenUsingPostedOptionAndNegativeCurrentTransaction()
		{
			CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			CreateUnPostedUnRecognisedRevenue(50m);
			CreateUnPostedRecognisedRevenue(25m);
			Factory.Save();

			var registryItem = IncludeUnpostedRevenueInCreditLimitCalculation;
			string originalValue = registryItem.Value;

			Org.CompanyData.OB_IsDebtor = ZBool.True;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 100m;
			Factory.Save();

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted);
				DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
				dummyBizO.TestHeader = Org;
				Org.CompanyData.OB_ARCreditLimit = 49m;
				Factory.Save();

				string expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 49.00 AUD. Credit approved.
The Total Outstanding Balance is 50.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, -50.00 AUD";
				dummyBizO.CurrentTransactionAmount = -50m;
				AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, -50m));
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

				dummyBizO.CurrentTransactionAmount = -52m;
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertNoWarnings("Should be no warnings", dummyBizO.Z0_GuidInfo);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestValidateIsCreditLimitExceededWhenUsingPostedOption()
		{
			CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			CreateUnPostedUnRecognisedRevenue(50m);
			CreateUnPostedRecognisedRevenue(25m);
			Factory.Save();

			var registryItem = IncludeUnpostedRevenueInCreditLimitCalculation;
			string originalValue = registryItem.Value;

			Org.CompanyData.OB_IsDebtor = ZBool.True;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 100m;
			Factory.Save();

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted);

				DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
				dummyBizO.TestHeader = Org;

				string expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 99.00 AUD. Credit approved.
The Total Outstanding Balance is 100.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD";
				Org.CompanyData.OB_ARCreditLimit = 99m;
				Factory.Save();
				AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable));
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

				dummyBizO.CurrentTransactionAmount = 50m;
				expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 99.00 AUD. Credit approved.
The Total Outstanding Balance is 150.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, 50.00 AUD

XVBQP68SIYXQ is a standalone global credit organization or a global credit group.
The Global Credit Limit for XVBQP68SIYXQ is set to 100.00 AUD. Global credit approved.
The Total Global Outstanding Balance for this group or standalone organization is 150.00 AUD, which is over the global credit limit.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, 50.00 AUD";
				AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, 50m));
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

				dummyBizO.CurrentTransactionAmount = 0m;
				Org.CompanyData.OB_ARCreditLimit = 101m;
				Factory.Save();
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertNoWarnings("Should be no warnings", dummyBizO.Z0_GuidInfo);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestValidateIsCreditLimitExceededWhenUsingPostedOptionAndTemporaryCreditLimit()
		{
			CreatePostedRevenue(500m, LedgerTypes.AccountsReceivable);
			CreateUnPostedUnRecognisedRevenue(50m);
			CreateUnPostedRecognisedRevenue(25m);
			Factory.Save();

			var registryItem = IncludeUnpostedRevenueInCreditLimitCalculation;
			string originalValue = registryItem.Value;

			Org.CompanyData.OB_IsDebtor = ZBool.True;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 100m;
			Factory.Save();

			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(Org.CompanyData, 99m, 100m);

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted);

				DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
				dummyBizO.TestHeader = Org;

				string expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 199.00 AUD. Credit approved.
The Total Outstanding Balance is 500.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 500.00 AUD

The Credit Limit is calculated by summing the following:
  *  the Credit Limit, 99.00 AUD
  *  the Temporary Credit Limit Increase, 100.00 AUD

XVBQP68SIYXQ is a standalone global credit organization or a global credit group.
The Global Credit Limit for XVBQP68SIYXQ is set to 100.00 AUD. Global credit approved.
The Total Global Outstanding Balance for this group or standalone organization is 500.00 AUD, which is over the global credit limit.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 500.00 AUD";
				Factory.Save();
				AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable));
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestValidateIsCreditLimitExceededWhenUsingPostedAndRecognizedOption()
		{
			Org.CompanyData.OB_IsDebtor = ZBool.True;

			CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			CreateUnPostedRecognisedRevenue(50m);
			CreateUnPostedUnRecognisedRevenue(25m);
			Factory.Save();

			var registryItem = IncludeUnpostedRevenueInCreditLimitCalculation;
			string originalValue = registryItem.Value;

			Factory.Save();

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedAndRecognized);

				DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
				dummyBizO.TestHeader = Org;

				string expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 149.00 AUD. Credit approved.
The Total Outstanding Balance is 150.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD
  *  the Unposted Recognized Revenue, 50.00 AUD";
				Org.CompanyData.OB_ARCreditLimit = 149m;
				Factory.Save();
				AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable));
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

				expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 224.00 AUD. Credit approved.
The Total Outstanding Balance is 225.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, 75.00 AUD
  *  the Unposted Recognized Revenue, 50.00 AUD";
				Org.CompanyData.OB_ARCreditLimit = 224m;
				Factory.Save();
				dummyBizO.CurrentTransactionAmount = 75m;
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

				dummyBizO.CurrentTransactionAmount = 0m;
				Org.CompanyData.OB_ARCreditLimit = 151m;
				Factory.Save();
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertNoWarnings("Should be no warnings", dummyBizO.Z0_GuidInfo);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestValidateIsCreditLimitExceededWhenUsingPostedRecognizedAndUnrecognizedOption()
		{
			Org.CompanyData.OB_IsDebtor = ZBool.True;

			CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			CreateUnPostedRecognisedRevenue(50m);
			CreateUnPostedUnRecognisedRevenue(25m);
			Factory.Save();

			var registryItem = IncludeUnpostedRevenueInCreditLimitCalculation;
			string originalValue = registryItem.Value;

			Factory.Save();

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);

				DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
				dummyBizO.TestHeader = Org;

				string expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 174.00 AUD. Credit approved.
The Total Outstanding Balance is 175.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD
  *  the Unposted Recognized Revenue, 50.00 AUD
  *  the Unposted Unrecognized Revenue, 25.00 AUD";
				Org.CompanyData.OB_ARCreditLimit = 174m;
				Factory.Save();
				AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable));
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

				expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 249.00 AUD. Credit approved.
The Total Outstanding Balance is 250.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, 75.00 AUD
  *  the Unposted Recognized Revenue, 50.00 AUD
  *  the Unposted Unrecognized Revenue, 25.00 AUD";
				dummyBizO.CurrentTransactionAmount = 75m;
				Org.CompanyData.OB_ARCreditLimit = 249m;
				Factory.Save();
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

				dummyBizO.CurrentTransactionAmount = 0m;
				Org.CompanyData.OB_ARCreditLimit = 176m;
				Factory.Save();
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertNoWarnings("Should be no warnings", dummyBizO.Z0_GuidInfo);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestValidateIsCreditLimitExceededWhenUsingPostedOptionAndCreditApproved()
		{
			CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			CreateUnPostedUnRecognisedRevenue(50m);
			CreateUnPostedRecognisedRevenue(25m);
			Factory.Save();

			var registryItem = IncludeUnpostedRevenueInCreditLimitCalculation;
			string originalValue = registryItem.Value;

			Org.CompanyData.OB_IsDebtor = ZBool.True;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 100m;
			Factory.Save();

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted);

				DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
				dummyBizO.TestHeader = Org;

				string expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 99.00 AUD. Credit approved.
The Total Outstanding Balance is 100.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD";
				Org.CompanyData.OB_ARCreditApproved = true;
				Org.CompanyData.OB_ARCreditLimit = 99m;
				Factory.Save();
				AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable));
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

				expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 99.00 AUD. Credit pending approval.
The Total Outstanding Balance is 100.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD";
				Org.CompanyData.OB_ARCreditApproved = false;
				AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable));
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

				Org.CompanyData.OB_ARCreditLimit = 101m;
				Factory.Save();
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertNoWarnings("Should be no warnings", dummyBizO.Z0_GuidInfo);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestValidateIsCreditLimitExceededWhenUsingPostedOptionAndOrgIsSettlementGroup()
		{
			OrgHeader orgHeaderParent = Factory.NewWithValidTestData<OrgHeader>();
			OrgRelatedParty orgRelatedParty = Factory.New<OrgRelatedParty>();
			orgRelatedParty.PR_OH_Parent = orgHeaderParent.PK;
			orgRelatedParty.PR_OH_RelatedParty = Org.PK;
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			orgRelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			CreateUnPostedUnRecognisedRevenue(50m);
			CreateUnPostedRecognisedRevenue(25m);
			Factory.Save();

			var registryItem = IncludeUnpostedRevenueInCreditLimitCalculation;
			string originalValue = registryItem.Value;

			Org.CompanyData.OB_IsDebtor = ZBool.True;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 100m;
			Factory.Save();

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted);

				DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
				dummyBizO.TestHeader = Org;

				string expectedMessage = @"XVBQP68SIYXQ is a settlement group.
The Credit Limit for XVBQP68SIYXQ is set to 99.00 AUD. Credit approved.
The Total Outstanding Balance for this settlement group is 100.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD";
				Org.CompanyData.OB_ARCreditLimit = 99m;
				Factory.Save();
				AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable));
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();

				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

				Org.CompanyData.OB_ARCreditLimit = 101m;
				Factory.Save();
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertNoWarnings("Should be no warnings", dummyBizO.Z0_GuidInfo);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestValidateIsCreditLimitExceededWhenUsingPostedOptionAndOrgIsConfiguredToUseCreditLimitOfSettlementGroup()
		{
			CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			CreateUnPostedUnRecognisedRevenue(50m);
			CreateUnPostedRecognisedRevenue(25m);
			Factory.Save();

			var registryItem = IncludeUnpostedRevenueInCreditLimitCalculation;
			string originalValue = registryItem.Value;

			Org.CompanyData.OB_IsDebtor = ZBool.True;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 100m;
			Factory.Save();

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted);

				DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
				dummyBizO.TestHeader = Org;

				string expectedMessage = @"XVBQP68SIYXQ is configured to use the credit limit of settlement group XYZXYZ.
The Credit Limit for XYZXYZ is set to 99.00 AUD. Credit approved.
The Total Outstanding Balance for this settlement group is 100.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD";
				OrgHeader settlementGroup = Factory.NewWithValidTestData<OrgHeader>();
				settlementGroup.OH_Code = "XYZXYZ";
				Org.ARSettlementGroupPK = settlementGroup.PK;
				Org.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
				settlementGroup.CompanyData.OB_ARCreditLimit = 99m;
				Factory.Save();
				AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable));
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();

				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);

				settlementGroup.CompanyData.OB_ARCreditLimit = 101m;
				Factory.Save();
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertNoWarnings("Should be no warnings", dummyBizO.Z0_GuidInfo);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestErrorsWithWebServiceForAR()
		{
			AssertErrorsWithWebService(LedgerTypes.AccountsReceivable);
		}

		public void TestErrorsWithWebServiceForAP()
		{
			AssertErrorsWithWebService(LedgerTypes.AccountsPayable);
		}

		void AssertErrorsWithWebService(string ledger)
		{
			DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
			dummyBizO.TestHeader = Org;
			dummyBizO.Ledger = ledger;
			Org.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			string expectedWarning = @$"Unable to retrieve a value for Outstanding Balance. An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {Core.Constants.ProductName} registry or a problem with an external system.
Error message: 'Registry Item 'Accounting -> Credit Controlled Documents Configuration -> Credit Limit Check Web Service URL' is not set.'.";

			if (ledger == LedgerTypes.AccountsPayable)
			{
				AssertNoErrors(dummyBizO.Z0_GuidInfo);
				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedWarning);
			}
			else
			{
				AssertHasError(dummyBizO.Z0_GuidInfo,
@$"Unable to retrieve a value for Credit On Hold. An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {Core.Constants.ProductName} registry or a problem with an external system.
Error message: 'Registry Item 'Accounting -> Credit Controlled Documents Configuration -> Credit Limit Check Web Service URL' is not set.'.");
				AssertNoWarnings(dummyBizO.Z0_GuidInfo);
			}

			dummyBizO.ValidateIsCreditOnHold = false;
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertNoErrors(dummyBizO.Z0_GuidInfo);
			AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedWarning);

			UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertNoErrors(dummyBizO.Z0_GuidInfo);
			AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedWarning);

			IncludeUnpostedRevenueInCreditLimitCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);
			UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertNoErrors(dummyBizO.Z0_GuidInfo);
			AssertHasWarning(dummyBizO.Z0_GuidInfo,
@$"Unable to retrieve a value for Unposted Recognized Revenue. An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {Core.Constants.ProductName} registry or a problem with an external system.
Error message: 'Registry Item 'Accounting -> Credit Controlled Documents Configuration -> Credit Limit Check Web Service URL' is not set.'.
Unable to retrieve a value for Unposted Unrecognized Revenue. An error occurred when retrieving data from an external system.
Please contact your internal IT department as this error indicates a problem with configuration of the {Core.Constants.ProductName} registry or a problem with an external system.
Error message: 'Registry Item 'Accounting -> Credit Controlled Documents Configuration -> Credit Limit Check Web Service URL' is not set.'.");

			UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			UseWebServiceForOutstandingBalance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			UseWebServiceForUnpostedRevenue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertNoErrors(dummyBizO.Z0_GuidInfo);
			AssertNoWarnings(dummyBizO.Z0_GuidInfo);
		}

		public void TestCreditOnHoldForAPDontAskAnyData()
		{
			DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
			dummyBizO.TestHeader = Org;
			dummyBizO.CurrentTransactionAmount = 100m;
			dummyBizO.Ledger = LedgerTypes.AccountsPayable;
			dummyBizO.ValidateIsCreditLimitExceeded = false;
			Org.CompanyData.OB_IsDebtor = false;
			Org.CompanyData.OB_IsCreditor = true;
			Org.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();

			Org.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
			Org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			AssertEquals("IsCreditOnHold for AP organisation", false, Org.CreditChecker.IsCreditOnHold());
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			AssertNoErrors("[AP Transaction] should have any errors for Credit On Hold", dummyBizO.Z0_GuidInfo);
			AssertEquals("Not data should be asked from OrgCreditLimitAndBalanceDetailsProvider", 0, Org.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);

			Org.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
			Org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			Org.CompanyData.OB_IsDebtor = true;
			AssertEquals("IsCreditOnHold for AR organisation", true, Org.CreditChecker.IsCreditOnHold());
			AssertEquals("Credit On Hold data should be asked from OrgCreditLimitAndBalanceDetailsProvider", 1, Org.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);

			Org.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest = 0;
			Org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			dummyBizO.Ledger = LedgerTypes.AccountsReceivable;
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			string expectedMessage = "This account is on credit hold and cannot be billed to";
			AssertHasError("[AR Transaction] should be errors", dummyBizO.Z0_GuidInfo, expectedMessage);
			AssertEquals("Credit On Hold data should be asked from OrgCreditLimitAndBalanceDetailsProvider", 1, Org.CreditChecker.CreditLimitAndOutstandingBalanceCacheInitializationAmountForTest);
		}

		public void TestValidateIsGlobalCreditLimitExceededWhenGlobalCreditOnHold()
		{
			CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			CreateUnPostedUnRecognisedRevenue(50m);
			CreateUnPostedRecognisedRevenue(25m);
			Factory.Save();

			var registryItem = IncludeUnpostedRevenueInGlobalCreditLimitCalculation;
			var originalValue = registryItem.Value;

			Org.OH_Code = "TestOrg";
			Org.CompanyData.OB_IsDebtor = true;
			Org.CompanyData.OB_ARCreditLimit = 250m;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 200m;
			Org.MiscServ.OM_ARGlobalOnCreditHold = true;
			Factory.Save();

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted);
				const string expectedMessageForPosted = @"TestOrg is on Global Credit Hold.

TestOrg is a standalone global credit organization or a global credit group.
The Global Credit Limit for TestOrg is set to 200.00 AUD. Global credit approved.
The Total Global Outstanding Balance for this group or standalone organization is 50.00 AUD, which has not exceeded the global credit limit threshold.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, -50.00 AUD";
				AssertEquals(expectedMessageForPosted, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, -50m));

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedAndRecognized);
				const string expectedMessageForPostedAndRecognised = @"TestOrg is on Global Credit Hold.

TestOrg is a standalone global credit organization or a global credit group.
The Global Credit Limit for TestOrg is set to 200.00 AUD. Global credit approved.
The Total Global Outstanding Balance for this group or standalone organization is 75.00 AUD, which has not exceeded the global credit limit threshold.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, -50.00 AUD
  *  the Unposted Global Recognized Revenue, 25.00 AUD";
				AssertEquals(expectedMessageForPostedAndRecognised, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, -50m));

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);
				const string expectedMessageForAll = @"TestOrg is on Global Credit Hold.

TestOrg is a standalone global credit organization or a global credit group.
The Global Credit Limit for TestOrg is set to 200.00 AUD. Global credit approved.
The Total Global Outstanding Balance for this group or standalone organization is 125.00 AUD, which has not exceeded the global credit limit threshold.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, -50.00 AUD
  *  the Unposted Global Recognized Revenue, 25.00 AUD
  *  the Unposted Global Unrecognized Revenue, 50.00 AUD";
				AssertEquals(expectedMessageForAll, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, -50m));
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestValidateIsGlobalCreditLimitExceededWhenGlobalCreditOnHold_GlobalGroupMember()
		{
			CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			CreateUnPostedUnRecognisedRevenue(50m);
			CreateUnPostedRecognisedRevenue(25m);
			Factory.Save();

			var registryItem = IncludeUnpostedRevenueInGlobalCreditLimitCalculation;
			var originalValue = registryItem.Value;

			Org.OH_Code = "TestOrg";
			Org.CompanyData.OB_IsDebtor = true;
			Org.CompanyData.OB_ARCreditLimit = 250m;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_OH_ARGlobalCreditGroup = Org_Diff.PK;

			Org_Diff.OH_Code = "TestGroup";
			Org_Diff.MiscServ.OM_ARGlobalCreditApproved = true;
			Org_Diff.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org_Diff.MiscServ.OM_ARGlobalCreditLimit = 200m;
			Org_Diff.MiscServ.OM_ARGlobalOnCreditHold = true;
			Factory.Save();

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted);
				const string expectedMessageForPosted = @"TestOrg is on Global Credit Hold.

TestOrg is a member of global credit group TestGroup.
The Global Credit Limit for TestGroup is set to 200.00 AUD. Global credit approved.
The Total Global Outstanding Balance is 50.00 AUD, which has not exceeded the global credit limit threshold.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, -50.00 AUD";
				AssertEquals(expectedMessageForPosted, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, -50m));

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedAndRecognized);
				const string expectedMessageForPostedAndRecognised = @"TestOrg is on Global Credit Hold.

TestOrg is a member of global credit group TestGroup.
The Global Credit Limit for TestGroup is set to 200.00 AUD. Global credit approved.
The Total Global Outstanding Balance is 75.00 AUD, which has not exceeded the global credit limit threshold.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, -50.00 AUD
  *  the Unposted Global Recognized Revenue, 25.00 AUD";
				AssertEquals(expectedMessageForPostedAndRecognised, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, -50m));

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);
				const string expectedMessageForAll = @"TestOrg is on Global Credit Hold.

TestOrg is a member of global credit group TestGroup.
The Global Credit Limit for TestGroup is set to 200.00 AUD. Global credit approved.
The Total Global Outstanding Balance is 125.00 AUD, which has not exceeded the global credit limit threshold.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, -50.00 AUD
  *  the Unposted Global Recognized Revenue, 25.00 AUD
  *  the Unposted Global Unrecognized Revenue, 50.00 AUD";
				AssertEquals(expectedMessageForAll, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, -50m));
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestValidateIsGlobalCreditLimitExceededWhenAboveGlobalCreditLimit()
		{
			CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			CreateUnPostedUnRecognisedRevenue(50m);
			CreateUnPostedRecognisedRevenue(25m);
			Factory.Save();

			var registryItem = IncludeUnpostedRevenueInGlobalCreditLimitCalculation;
			var originalValue = registryItem.Value;

			Org.OH_Code = "TestOrg";
			Org.CompanyData.OB_IsDebtor = true;
			Org.CompanyData.OB_ARCreditLimit = 276m;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 200m;
			Factory.Save();

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted);
				const string expectedMessageForPosted = @"TestOrg is a standalone global credit organization or a global credit group.
The Global Credit Limit for TestOrg is set to 200.00 AUD. Global credit approved.
The Total Global Outstanding Balance for this group or standalone organization is 201.00 AUD, which is over the global credit limit.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, 101.00 AUD";
				AssertEquals(expectedMessageForPosted, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, 101m));

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedAndRecognized);
				const string expectedMessageForPostedAndRecognised = @"TestOrg is a standalone global credit organization or a global credit group.
The Global Credit Limit for TestOrg is set to 200.00 AUD. Global credit approved.
The Total Global Outstanding Balance for this group or standalone organization is 226.00 AUD, which is over the global credit limit.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, 101.00 AUD
  *  the Unposted Global Recognized Revenue, 25.00 AUD";
				AssertEquals(expectedMessageForPostedAndRecognised, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, 101m));

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);
				const string expectedMessageForAll = @"TestOrg is a standalone global credit organization or a global credit group.
The Global Credit Limit for TestOrg is set to 200.00 AUD. Global credit approved.
The Total Global Outstanding Balance for this group or standalone organization is 276.00 AUD, which is over the global credit limit.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, 101.00 AUD
  *  the Unposted Global Recognized Revenue, 25.00 AUD
  *  the Unposted Global Unrecognized Revenue, 50.00 AUD";
				AssertEquals(expectedMessageForAll, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, 101m));
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestValidateIsGlobalCreditLimitExceededWhenAboveGlobalCreditLimit_GlobalGroupMember()
		{
			CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			CreateUnPostedUnRecognisedRevenue(50m);
			CreateUnPostedRecognisedRevenue(25m);
			Factory.Save();

			var registryItem = IncludeUnpostedRevenueInGlobalCreditLimitCalculation;
			var originalValue = registryItem.Value;

			Org.OH_Code = "TestOrg";
			Org.CompanyData.OB_IsDebtor = true;
			Org.CompanyData.OB_ARCreditLimit = 276m;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_OH_ARGlobalCreditGroup = Org_Diff.PK;

			Org_Diff.MiscServ.OM_ARGlobalCreditApproved = true;
			Org_Diff.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org_Diff.MiscServ.OM_ARGlobalCreditLimit = 200m;
			Factory.Save();

			try
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted);
				const string expectedMessageForPosted = @"TestOrg is a member of global credit group H5ZX52PAMCOI.
The Global Credit Limit for H5ZX52PAMCOI is set to 200.00 AUD. Global credit approved.
The Total Global Outstanding Balance is 201.00 AUD, which is over the global credit limit.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, 101.00 AUD";
				AssertEquals(expectedMessageForPosted, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, 101m));

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedAndRecognized);
				const string expectedMessageForPostedAndRecognised = @"TestOrg is a member of global credit group H5ZX52PAMCOI.
The Global Credit Limit for H5ZX52PAMCOI is set to 200.00 AUD. Global credit approved.
The Total Global Outstanding Balance is 226.00 AUD, which is over the global credit limit.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, 101.00 AUD
  *  the Unposted Global Recognized Revenue, 25.00 AUD";
				AssertEquals(expectedMessageForPostedAndRecognised, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, 101m));

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);
				const string expectedMessageForAll = @"TestOrg is a member of global credit group H5ZX52PAMCOI.
The Global Credit Limit for H5ZX52PAMCOI is set to 200.00 AUD. Global credit approved.
The Total Global Outstanding Balance is 276.00 AUD, which is over the global credit limit.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Current Transaction Amount, 101.00 AUD
  *  the Unposted Global Recognized Revenue, 25.00 AUD
  *  the Unposted Global Unrecognized Revenue, 50.00 AUD";
				AssertEquals(expectedMessageForAll, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, 101m));
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestInvalidGlobalCreditCurrencyOrMissingExRate()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.OB_ARCreditLimit = 250m;
			org.MiscServ.OM_ARGlobalCreditApproved = true;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = "USD";
			org.MiscServ.OM_ARGlobalCreditLimit = 500m;
			Factory.Save();

			const string expectedMessage = @"Global outstanding transactions balance cannot be calculated for the global credit group.
It requires valid exchange rates for today to be entered (using ‘GCB’ or ‘PER’ exchange rate type) in all system companies where the Global Credit Group has transactions and/or local credit limits.
For a list of system companies and currency codes, please refer to Organization (TestOrg) > A/R > Credit Control and Settlement > Global > Companies Local Credit Control and Settlement Details.";

			AssertEquals(expectedMessage, org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable, 101m));
		}

		public void TestContentOfCreditCheckerTracerInfo()
		{
			var dummyTracer = new DummyTracer();
			ObjectFactory.Substitute<ITracer>(dummyTracer);

			var dummyBizO = Factory.New<DummyBizoForCreditLimits>();
			dummyBizO.TestHeader = Org;
			dummyBizO.Ledger = LedgerTypes.AccountsReceivable;
			Org.CompanyData.OB_ARCreditLimit = 99m;
			Org.CompanyData.OB_IsDebtor = ZBool.True;
			Org.CompanyData.OB_AROnCreditHold = true;
			Factory.Save();

			dummyBizO.CurrentTransactionAmount = 100m;
			dummyBizO.MarkAsNeedingValidation();
			dummyBizO.RunPreSaveValidation();
			var actualTraceMessage = string.Join("", dummyTracer.Traces);

			AssertContains("CreditCheckerCache", actualTraceMessage);
			AssertContains($"cachedBalance for {Org.OH_Code}:", actualTraceMessage);
			AssertContains("UnableToCalculateARGlobalUnpostedRevenue", actualTraceMessage);
			AssertContains("ARGlobalUnpostedRevenueRecognised", actualTraceMessage);
			AssertContains("ARGlobalCreditLimit", actualTraceMessage);
			AssertContains("ARGlobalCreditCurrencyCode", actualTraceMessage);
			AssertContains("IsOverARGlobalCreditLimit", actualTraceMessage);
			AssertContains("IsARGlobalCreditApproved", actualTraceMessage);
			AssertContains("CompanyCode", actualTraceMessage);
			AssertContains("ARGlobalUnpostedRevenueUnrecognised", actualTraceMessage);
			AssertContains("OrgCode", actualTraceMessage);
			AssertContains("CreditLimit", actualTraceMessage);
			AssertContains("IsOverCreditLimit", actualTraceMessage);
			AssertContains("IsOverCreditTerms", actualTraceMessage);
			AssertContains("OnCreditHold", actualTraceMessage);
			AssertContains("OutstandingBalance", actualTraceMessage);
			AssertContains("OutstandingBalanceNotOverdue", actualTraceMessage);
			AssertContains("OutstandingBalanceOverdue", actualTraceMessage);
			AssertContains("UnpostedRevenue", actualTraceMessage);
			AssertContains("UnpostedRevenueRecognised", actualTraceMessage);
			AssertContains("UnpostedRevenueUnrecognised", actualTraceMessage);
			AssertContains("stackTrace", actualTraceMessage);

			AssertContains("CreditLimitCacheReset", actualTraceMessage);
			AssertContains($"Resetting cache for {Org.OH_Code}:", actualTraceMessage);
			AssertContains("ARGlobalClaim", actualTraceMessage);
			AssertContains("Claim", actualTraceMessage);

			dummyTracer.Traces.Clear();
		}

		public void TestTracerInfoIsPrintedWhenCacheIsCleared()
		{
			var dummyTracer = new DummyTracer();
			ObjectFactory.Substitute<ITracer>(dummyTracer);

			Org.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

			var actualTraceMessage = string.Join("", dummyTracer.Traces);

			AssertContains("CreditLimitCacheCleared", actualTraceMessage);
			AssertContains("Clearing cache for", actualTraceMessage);

			dummyTracer.Traces.Clear();
		}

		public void TestGetCreditLimitExceededValidation_ExcludeOpenClaimAmount()
		{
			var inv = CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			FillInvoice(inv);
			Factory.Save();

			Org.CompanyData.OB_IsDebtor = ZBool.True;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 79m;
			Factory.Save();

			CreateClaim(inv, "OPN", 20m);
			Factory.Save();

			using (IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted))
			using (ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
				dummyBizO.TestHeader = Org;

				string expectedMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 79.00 AUD. Credit approved.
The Total Outstanding Balance (excluding Open Claims Amount) is 80.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD
  *  the Open Claims Amounts Excluded, -20.00 AUD

XVBQP68SIYXQ is a standalone global credit organization or a global credit group.
The Global Credit Limit for XVBQP68SIYXQ is set to 79.00 AUD. Global credit approved.
The Total Global Outstanding Balance for this group or standalone organization (excluding Open Claims Amount) is 80.00 AUD, which is over the global credit limit.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Open Global Claims Amounts Excluded, -20.00 AUD";
				Org.CompanyData.OB_ARCreditApproved = true;
				Org.CompanyData.OB_ARCreditLimit = 79m;
				Factory.Save();
				AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable));
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);
			}
		}

		public void TestGetCreditLimitExceededValidation_ExcludeOpenClaimAmountAndOrgIsSettlementGroup()
		{
			OrgHeader orgHeaderParent = Factory.NewWithValidTestData<OrgHeader>();
			OrgRelatedParty orgRelatedParty = Factory.New<OrgRelatedParty>();
			orgRelatedParty.PR_OH_Parent = orgHeaderParent.PK;
			orgRelatedParty.PR_OH_RelatedParty = Org.PK;
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			orgRelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var inv = CreatePostedRevenue(100m, LedgerTypes.AccountsReceivable);
			FillInvoice(inv);
			Factory.Save();

			Org.CompanyData.OB_IsDebtor = ZBool.True;

			Org.MiscServ.OM_ARGlobalCreditApproved = true;
			Org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			Org.MiscServ.OM_ARGlobalCreditLimit = 79m;
			Factory.Save();

			CreateClaim(inv, "OPN", 20m);
			Factory.Save();

			using (IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted))
			using (ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				DummyBizoForCreditLimits dummyBizO = Factory.New<DummyBizoForCreditLimits>();
				dummyBizO.TestHeader = Org;

				string expectedMessage = @"XVBQP68SIYXQ is a settlement group.
The Credit Limit for XVBQP68SIYXQ is set to 79.00 AUD. Credit approved.
The Total Outstanding Balance for this settlement group (excluding Open Claims Amount) is 80.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 100.00 AUD
  *  the Open Claims Amounts Excluded, -20.00 AUD

XVBQP68SIYXQ is a standalone global credit organization or a global credit group.
The Global Credit Limit for XVBQP68SIYXQ is set to 79.00 AUD. Global credit approved.
The Total Global Outstanding Balance for this group or standalone organization (excluding Open Claims Amount) is 80.00 AUD, which is over the global credit limit.

The Total Global Outstanding Balance is calculated by summing the following:
  *  the Posted Global Outstanding Balance, 100.00 AUD
  *  the Open Global Claims Amounts Excluded, -20.00 AUD";
				Org.CompanyData.OB_ARCreditApproved = true;
				Org.CompanyData.OB_ARCreditLimit = 79m;
				Factory.Save();
				AssertEquals(expectedMessage, Org.CreditChecker.GetCreditLimitExceededValidation(LedgerTypes.AccountsReceivable));
				dummyBizO.MarkAsNeedingValidation();
				dummyBizO.RunPreSaveValidation();
				AssertHasWarning(dummyBizO.Z0_GuidInfo, expectedMessage);
				AssertNoErrors("Should be no errors", dummyBizO.Z0_GuidInfo);
			}
		}

		#region Implementation

		AccTransactionHeader CreatePostedRevenue(decimal amount, string ledger)
		{
			AccTransactionHeader aRInv = GetNewInvoice(Org, ledger, amount);
			return aRInv;
		}

		void CreateUnPostedRecognisedRevenue(decimal amount)
		{
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_JobNum = "S00001000";
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			AccTransactionLines wip = Factory.New<AccTransactionLines>();
			wip.AL_AC = chargeCode.PK;
			wip.AL_JH = jobHeader.PK;
			wip.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			wip.AL_GB = GlbBranch.CurrentBranch.PK;
			wip.AL_GE = GlbDepartment.CurrentDepartment.PK;
			wip.AL_PostDate = ZDateTime.Now;
			wip.AL_OSAmount = -amount;
			wip.AL_LineAmount = -amount;
			wip.AL_ReverseDate = ZDateTime.Empty;
			wip.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			wip.AL_ExchangeRate = 1.0m;
			wip.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			wip.AL_OH = Org.PK;

			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_AL_ARLine = wip.PK;
			jobCharge.JR_AC = wip.AL_AC;
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_GB = jobHeader.JH_GB;
			jobCharge.JR_GE = jobHeader.JH_GE;
			jobCharge.JR_OSSellAmt = amount;
			jobCharge.JR_LocalSellAmt = amount;
			using (new CreditChecker.CreditLimitValidationSuspender(Factory))
			{
				jobCharge.JR_OH_SellAccount = Org.PK;
			}
			jobCharge.SetChargeValuesFromLinkedARLineForTests();
		}

		void CreateUnPostedUnRecognisedRevenue(decimal amount)
		{
			ObjectFactory.Get<IAccounting>().Registry.CreateWIPOrAccrualWhenNoInvoicesPosted_ForTestOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_JobNum = "S00001001";
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_GB = jobHeader.JH_GB;
			jobCharge.JR_GE = jobHeader.JH_GE;
			jobCharge.JR_OSSellAmt = amount;
			jobCharge.JR_LocalSellAmt = amount;
			using (new CreditChecker.CreditLimitValidationSuspender(Factory))
			{
				jobCharge.JR_OH_SellAccount = Org.PK;
			}
		}

		class DummyBizoForCreditLimits : AutoDummyBizo
		{
			public DummyBizoForCreditLimits(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			protected override DummyBizoValidation GetNewValidation()
			{
				return new DummyBizoValidationForCreditLimits(this)
				{
					ValidateIsCreditOnHold = this.ValidateIsCreditOnHold,
					ValidateIsCreditLimitExceeded = this.ValidateIsCreditLimitExceeded
				};
			}

			public OrgHeader TestHeader;

			public decimal CurrentTransactionAmount { get; set; }

			public string Ledger
			{
				get { return ledger; }
				set { ledger = value; }
			}
			string ledger = LedgerTypes.AccountsReceivable;

			public bool ValidateIsCreditOnHold = true;
			public bool ValidateIsCreditLimitExceeded = true;
		}

		class DummyBizoValidationForCreditLimits : DummyBizoValidation
		{
			public DummyBizoValidationForCreditLimits(DummyBizoForCreditLimits parent)
				: base(parent)
			{
			}

			public new DummyBizoForCreditLimits Parent
			{
				get { return (DummyBizoForCreditLimits)base.Parent; }
			}

			protected override void CheckZ0_Guid()
			{
				if (ValidateIsCreditOnHold)
				{
					Parent.TestHeader.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
					Parent.TestHeader.CreditChecker.ValidateIsCreditOnHold(Parent.Z0_GuidInfo, Parent.Ledger);
				}

				if (ValidateIsCreditLimitExceeded)
				{
					Parent.TestHeader.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
					Parent.TestHeader.CreditChecker.ValidateIsCreditLimitExceeded(Parent.Z0_GuidInfo, Parent.Ledger, Parent.CurrentTransactionAmount);
				}
			}

			public bool ValidateIsCreditOnHold = true;
			public bool ValidateIsCreditLimitExceeded = true;
		}

		AccTransactionHeader GetNewInvoice(OrgHeader org, string ledger, decimal outstandingAmount, bool isCancelled = false)
		{
			return GetNewTransactionHeader(org, ZArchitecture.Core.TransactionTypes.Invoice, ledger, outstandingAmount);
		}

		AccTransactionHeader GetNewReceipt(OrgHeader org, string ledger, decimal outstandingAmount, bool isCancelled = false)
		{
			return GetNewTransactionHeader(org, ZArchitecture.Core.TransactionTypes.Receipt, ledger, outstandingAmount);
		}

		AccTransactionHeader GetNewPayment(OrgHeader org, string ledger, decimal outstandingAmount, bool isCancelled = false)
		{
			return GetNewTransactionHeader(org, ZArchitecture.Core.TransactionTypes.Payment, ledger, outstandingAmount);
		}

		AccTransactionHeader GetNewTransactionHeader(OrgHeader org, string transactionType, string ledger, decimal outstandingAmount, bool isCancelled = false)
		{
			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_TransactionType = transactionType;
			invoice.AH_OH = org.PK;
			invoice.AH_Ledger = ledger;
			invoice.AH_OutstandingAmount = outstandingAmount;
			invoice.AH_InvoiceAmount = outstandingAmount;
			invoice.AH_OSTotal = outstandingAmount;
			invoice.AH_IsCancelled = false;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			invoice.AH_IsCancelled = isCancelled;
			return invoice;
		}

		AccQueryClaim CreateClaim(AccTransactionHeader invoice, string claimStatus, decimal claimAmount)
		{
			var claim = (AccQueryClaim)Factory.New<IARAccQueryClaim>();
			claim.FillWithValidTestData();
			claim.AY_AH = invoice.PK;
			claim.AY_GB = invoice.AH_GB;
			claim.AY_OH_Debtor = invoice.AH_OH;
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_OH = invoice.AH_OH;
			claim.AY_OC = orgContact.PK;
			claim.AY_QueryClaimAmount = claimAmount;
			claim.AY_QueryClaimStatus = claimStatus;
			return claim;
		}

		void FillInvoice(AccTransactionHeader invoice)
		{
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_ExchangeRate = 1;
			invoice.AH_TransactionNum = "1111";
			invoice.AH_InvoiceDate = ZDateTime.Now;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_DueDate = ZDateTime.Now;

			var line = Factory.New<AccTransactionLines>();
			line.AL_AH = invoice.PK;
			line.AL_LineType = invoice.AH_Ledger == "AR" ? TransactionLineTypes.Revenue : TransactionLineTypes.Cost;
			line.AL_LineAmount = invoice.AH_InvoiceAmount;
			line.AL_OSAmount = invoice.AH_InvoiceAmount;
			line.AL_RX_NKTransactionCurrency = "AUD";
			line.AL_ExchangeRate = 1;
			line.AL_OSAmount = invoice.AH_InvoiceAmount;
			line.AL_Desc = "tee he he";
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Org.CompanyData.OB_IsDebtor = true;
			Org.CompanyData.OB_IsCreditor = true;

			Org_Diff = Factory.NewWithValidTestData<OrgHeader>();
			Org_Diff.CompanyData.OB_IsDebtor = true;
			Org_Diff.CompanyData.OB_IsCreditor = true;

			Factory.Save();
		}

		OrgHeader Org;
		OrgHeader Org_Diff;

		BooleanRegistryItem UseWebServiceForCreditLimit
		{
			get { return ObjectFactory.Get<IAccounting>().Registry.UseWebServiceForCreditLimit as BooleanRegistryItem; }
		}

		BooleanRegistryItem UseWebServiceForOutstandingBalance
		{
			get { return ObjectFactory.Get<IAccounting>().Registry.UseWebServiceForOutstandingBalance as BooleanRegistryItem; }
		}

		BooleanRegistryItem UseWebServiceForUnpostedRevenue
		{
			get { return ObjectFactory.Get<IAccounting>().Registry.UseWebServiceForUnpostedRevenue as BooleanRegistryItem; }
		}

		CodePairRegistryItem IncludeUnpostedRevenueInCreditLimitCalculation
		{
			get { return ObjectFactory.Get<IAccounting>().IncludeUnpostedRevenueInCreditLimitCalculation as CodePairRegistryItem; }
		}

		CodePairRegistryItem IncludeUnpostedRevenueInGlobalCreditLimitCalculation
		{
			get { return ObjectFactory.Get<IAccounting>().IncludeUnpostedRevenueInGlobalCreditLimitCalculation as CodePairRegistryItem; }
		}

		BooleanRegistryItem GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation
		{
			get
			{
				return ObjectFactory.Get<IAccounting>().Registry?.GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation as BooleanRegistryItem;
			}
		}

		BooleanRegistryItem ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation
		{
			get
			{
				return ObjectFactory.Get<IAccounting>().Registry?.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation as BooleanRegistryItem;
			}
		}

		#endregion

	}
}
