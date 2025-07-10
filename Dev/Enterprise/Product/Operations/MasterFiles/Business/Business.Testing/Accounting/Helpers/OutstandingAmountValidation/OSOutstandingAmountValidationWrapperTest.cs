using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.Testing.AccTransactionMatchLinkCriticalValidationTest;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Helpers
{
	sealed class OSOutstandingAmountValidationWrapperTest : TestCaseWithFactory
	{
		public void TestOutstandingAmount()
		{
			TestTransaction.AH_OSOutstandingAmount = 0m;
			AssertEquals(0m, Wrapper.OutstandingAmount);

			TestTransaction.AH_OSOutstandingAmount = 100m;
			AssertEquals(100m, Wrapper.OutstandingAmount);
		}

		public void TestTotalAmount()
		{
			TestTransaction.AH_OSTotal = 0m;
			AssertEquals(0m, Wrapper.TotalAmount);

			TestTransaction.AH_OSTotal = 100m;
			AssertEquals(100m, Wrapper.TotalAmount);
		}

		public void TestMatchLinkAmountSum()
		{
			var wrapper1 = new OSOutstandingAmountValidationWrapper(TestTransaction);
			AssertEquals(0m, wrapper1.MatchLinkAmountSum);

			var matchLink = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLink.AP_MatchGroupNum = "M001";
			matchLink.AP_AH = TestTransaction.PK;
			matchLink.AP_OSAmount = 100m;

			var wrapper2 = new OSOutstandingAmountValidationWrapper(TestTransaction);
			AssertEquals(100m, wrapper2.MatchLinkAmountSum);
		}

		public void TestShouldCheckTransactionHeaderOutstandingAmount()
		{
			TestTransaction.AH_IsOSOutstandingAmountApplicable = false;
			AssertEquals("Precondition", false, TestTransaction.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Precondition", false, TestTransaction.IsInDatabase);
			AssertEquals(false, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);

			TestTransaction.AH_IsOSOutstandingAmountApplicable = true;
			AssertEquals("Precondition", true, TestTransaction.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Precondition", false, TestTransaction.IsInDatabase);
			AssertEquals(true, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);

			TestTransaction.AH_OSOutstandingAmount = 100m;
			Factory.Save();
			AssertEquals("Precondition", true, TestTransaction.IsInDatabase);
			AssertEquals(false, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);

			TestTransaction.AH_OutstandingAmount = 110m;
			AssertEquals("Precondition", true, TestTransaction.AH_OutstandingAmountInfo.HasChanges);
			AssertEquals(true, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);

			TestTransaction.Reload();
			AssertEquals("Precondition", false, TestTransaction.AH_OutstandingAmountInfo.HasChanges);
			AssertEquals(false, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);

			TestTransaction.AH_InvoiceAmount = 110m;
			AssertEquals("Precondition", true, TestTransaction.AH_InvoiceAmountInfo.HasChanges);
			AssertEquals(true, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);

			TestTransaction.Reload();
			AssertEquals("Precondition", false, TestTransaction.AH_InvoiceAmountInfo.HasChanges);
			AssertEquals(false, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);

			TestTransaction.AH_GSTAmount = 10m;
			AssertEquals("Precondition", true, TestTransaction.AH_GSTAmountInfo.HasChanges);
			AssertEquals(true, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);

			TestTransaction.Reload();
			AssertEquals("Precondition", false, TestTransaction.AH_GSTAmountInfo.HasChanges);
			AssertEquals(false, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);

			TestTransaction.AH_OSOutstandingAmount = 10m;
			AssertEquals("Precondition", true, TestTransaction.AH_OSOutstandingAmountInfo.HasChanges);
			AssertEquals(true, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);

			TestTransaction.Reload();
			AssertEquals("Precondition", false, TestTransaction.AH_OSOutstandingAmountInfo.HasChanges);
			AssertEquals(false, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);

			TestTransaction.AH_OSTotal = 10m;
			AssertEquals("Precondition", true, TestTransaction.AH_OSTotalInfo.HasChanges);
			AssertEquals(true, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);

			TestTransaction.Reload();
			AssertEquals("Precondition", false, TestTransaction.AH_OSTotalInfo.HasChanges);
			AssertEquals(false, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);
		}

		#region ShouldCheckMatchLinkOutstandingAmount

		public void TestShouldCheckMatchLinkOutstandingAmount_ShouldCheckTransactionHeaderOutstandingAmount()
		{
			Factory.Save();
			AssertEquals("Precondition", true, TestTransaction.IsInDatabase);

			AssertEquals("Precondition", false, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);
			AssertEquals(false, Wrapper.ShouldCheckMatchLinkOutstandingAmount);

			TestTransaction.AH_OSOutstandingAmount = 110m;
			AssertEquals("Precondition", true, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);
			AssertEquals(true, Wrapper.ShouldCheckMatchLinkOutstandingAmount);
		}

		public void TestShouldCheckMatchLinkOutstandingAmount_MatchLinksHasChanges()
		{
			Factory.Save();

			var arINV = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			var matchLinkForARINV = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLinkForARINV.AP_MatchGroupNum = "M001";
			matchLinkForARINV.AP_AH = arINV.PK;
			matchLinkForARINV.AP_Amount = matchLinkForARINV.AP_OSAmount = 100m;
			arINV.AH_OutstandingAmount = 0m;
			arINV.AH_IsOSOutstandingAmountApplicable = true;

			var arCRD = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
			var matchLinkForARCRD = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLinkForARCRD.AP_MatchGroupNum = "M001";
			matchLinkForARCRD.AP_AH = arCRD.PK;
			matchLinkForARCRD.AP_Amount = matchLinkForARCRD.AP_OSAmount = -100m;
			arCRD.AH_OutstandingAmount = 0m;
			arCRD.AH_IsOSOutstandingAmountApplicable = true;

			var group = new MatchLinkGroupForTest(Factory);
			group.Add(matchLinkForARINV);
			group.Add(matchLinkForARCRD);
			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Precondition", true, arINV.IsInDatabase);
			AssertEquals("Precondition", true, arCRD.IsInDatabase);

			var wrapper1 = new LocalOutstandingAmountValidationWrapper(arINV);
			var matchLinks1 = AccTransactionMatchLinkLoader.LoadByAccTransactionHeader(arINV);
			AssertEquals("Precondition", false, matchLinks1.Any(x => x.HasChanges));
			AssertEquals(false, wrapper1.ShouldCheckMatchLinkOutstandingAmount);

			matchLinkForARCRD.AP_MatchDate = ZDateTime.Now.AddDays(-1);
			var wrapper2 = new LocalOutstandingAmountValidationWrapper(arCRD);
			var matchLinks2 = AccTransactionMatchLinkLoader.LoadByAccTransactionHeader(arCRD);
			AssertEquals("Precondition", true, matchLinks2.Any(x => x.HasChanges));
			AssertEquals(true, wrapper2.ShouldCheckMatchLinkOutstandingAmount);
		}

		public void TestShouldCheckMatchLinkOutstandingAmount_AH_IsOSOutstandingAmountApplicable()
		{
			TestTransaction.AH_OSOutstandingAmount = 110m;
			AssertEquals("Precondition", true, TestTransaction.AH_IsOSOutstandingAmountApplicable);

			AssertEquals(true, Wrapper.ShouldCheckMatchLinkOutstandingAmount);
			TestTransaction.AH_IsOSOutstandingAmountApplicable = false;
			AssertEquals(false, Wrapper.ShouldCheckMatchLinkOutstandingAmount);
		}

		public void TestShouldCheckMatchLinkOutstandingAmount_MatchLinksAllAP_IsOSAmountApplicable()
		{
			var matchLink1 = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLink1.AP_MatchGroupNum = "M001";
			matchLink1.AP_AH = TestTransaction.PK;
			matchLink1.AP_Amount = matchLink1.AP_OSAmount = 100m;
			matchLink1.TransactionHeader.AH_IsOSOutstandingAmountApplicable = true;

			var matchLink2 = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLink2.AP_MatchGroupNum = "M001";
			matchLink2.AP_AH = TestTransaction.PK;
			matchLink2.AP_Amount = matchLink2.AP_OSAmount = 100m;
			matchLink2.TransactionHeader.AH_IsOSOutstandingAmountApplicable = true;

			var group = new MatchLinkGroupForTest(Factory);
			group.Add(matchLink1);
			group.Add(matchLink2);

			AssertEquals(true, Wrapper.ShouldCheckMatchLinkOutstandingAmount);
			matchLink2.TransactionHeader.AH_IsOSOutstandingAmountApplicable = false;
			AssertEquals(false, Wrapper.ShouldCheckMatchLinkOutstandingAmount);
		}

		#endregion

		public void TestGetTransactionHeaderErrroMessage()
		{
			AssertEquals("OS Outstanding Amount is incorrect: os outstanding amount = 100, os total amount = 100", Wrapper.GetTransactionHeaderErrorMessage());
		}

		public void TestGetMatchLinkErrroMessage()
		{
			var wrapper1 = new OSOutstandingAmountValidationWrapper(TestTransaction);
			AssertEquals("Precondition", false, AccTransactionMatchLinkLoader.LoadByAccTransactionHeader(TestTransaction).Any());
			AssertEquals("OS Outstanding Amount is incorrect: os outstanding amount = 100, os total amount = 100, sum ap os amount = 0", wrapper1.GetMatchLinkErrorMessage());

			var matchLink = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLink.AP_MatchGroupNum = "M001";
			matchLink.AP_AH = TestTransaction.PK;
			matchLink.AP_OSAmount = 100m;

			var wrapper2 = new OSOutstandingAmountValidationWrapper(TestTransaction);
			AssertEquals("Precondition", true, AccTransactionMatchLinkLoader.LoadByAccTransactionHeader(TestTransaction).Any());
			AssertContainsInOrder("Error Message", wrapper2.GetMatchLinkErrorMessage(),
				"OS Outstanding Amount is incorrect: os outstanding amount = 100, os total amount = 100, sum ap os amount = 100",
				"Related Match Links:",
				"Match Link Factory Instance Number =",
				"Match Link: Group Number = M001,");
		}

		protected override void SetUp()
		{
			TransactionCreator = ObjectFactory.Get<ITransactionCreator>();
			TestTransaction = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			TestTransaction.MakeOSOutstandingAmountApplicable(100m);
			Wrapper = new OSOutstandingAmountValidationWrapper(TestTransaction);
		}

		ITransactionCreator TransactionCreator;
		AccTransactionHeader TestTransaction;
		IOutstandingAmountValidationWrapper Wrapper;
	}
}

