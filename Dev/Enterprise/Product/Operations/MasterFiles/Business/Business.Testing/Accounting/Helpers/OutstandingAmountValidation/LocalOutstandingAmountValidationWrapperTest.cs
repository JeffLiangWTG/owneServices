using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.Testing.AccTransactionMatchLinkCriticalValidationTest;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Helpers
{
	sealed class LocalOutstandingAmountValidationWrapperTest : TestCaseWithFactory
	{
		public void TestOutstandingAmount()
		{
			TestTransaction.AH_OutstandingAmount = 0m;
			AssertEquals(0m, Wrapper.OutstandingAmount);

			TestTransaction.AH_OutstandingAmount = 100m;
			AssertEquals(100m, Wrapper.OutstandingAmount);
		}

		public void TestTotalAmount()
		{
			TestTransaction.AH_InvoiceAmount = 0m;
			AssertEquals(0m, Wrapper.TotalAmount);

			TestTransaction.AH_InvoiceAmount = 100m;
			AssertEquals(100m, Wrapper.TotalAmount);
		}

		public void TestMatchLinkAmountSum()
		{
			var wrapper1 = new LocalOutstandingAmountValidationWrapper(TestTransaction);
			AssertEquals(0m, wrapper1.MatchLinkAmountSum);

			var matchLink = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLink.AP_MatchGroupNum = "M001";
			matchLink.AP_AH = TestTransaction.PK;
			matchLink.AP_Amount = 100m;

			var wrapper2 = new LocalOutstandingAmountValidationWrapper(TestTransaction);
			AssertEquals(100m, wrapper2.MatchLinkAmountSum);
		}

		public void TestShouldCheckTransactionHeaderOutstandingAmount()
		{
			AssertEquals("Precondition", false, TestTransaction.IsInDatabase);
			AssertEquals(true, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);

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
		}

		#region ShouldCheckMatchLinkOutstandingAmount

		public void TestShouldCheckMatchLinkOutstandingAmount_ShouldCheckTransactionHeaderOutstandingAmount()
		{
			Factory.Save();
			AssertEquals("Precondition", true, TestTransaction.IsInDatabase);

			AssertEquals("Precondition", false, Wrapper.ShouldCheckTransactionHeaderOutstandingAmount);
			AssertEquals(false, Wrapper.ShouldCheckMatchLinkOutstandingAmount);

			TestTransaction.AH_OutstandingAmount = 110m;
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
			matchLinkForARINV.AP_Amount = 100m;
			arINV.AH_OutstandingAmount = 0m;

			var arCRD = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote);
			var matchLinkForARCRD = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLinkForARCRD.AP_MatchGroupNum = "M001";
			matchLinkForARCRD.AP_AH = arCRD.PK;
			matchLinkForARCRD.AP_Amount = -100m;
			arCRD.AH_OutstandingAmount = 0m;

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

		#endregion

		public void TestGetTransactionHeaderErrroMessage()
		{
			AssertEquals("Outstanding Amount is incorrect: outstanding amount = 100, local total amount = 100", Wrapper.GetTransactionHeaderErrorMessage());
		}

		public void TestGetMatchLinkErrroMessage()
		{
			var wrapper1 = new LocalOutstandingAmountValidationWrapper(TestTransaction);
			AssertEquals("Precondition", false, AccTransactionMatchLinkLoader.LoadByAccTransactionHeader(TestTransaction).Any());
			AssertEquals("Outstanding Amount is incorrect: outstanding amount = 100, local total amount = 100, sum ap amount = 0", wrapper1.GetMatchLinkErrorMessage());

			var matchLink = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLink.AP_MatchGroupNum = "M001";
			matchLink.AP_AH = TestTransaction.PK;
			matchLink.AP_Amount = 100m;

			var wrapper2 = new LocalOutstandingAmountValidationWrapper(TestTransaction);
			AssertEquals("Precondition", true, AccTransactionMatchLinkLoader.LoadByAccTransactionHeader(TestTransaction).Any());
			AssertContainsInOrder("Error Message", wrapper2.GetMatchLinkErrorMessage(),
				"Outstanding Amount is incorrect: outstanding amount = 100, local total amount = 100, sum ap amount = 100",
				"Related Match Links:",
				"Match Link Factory Instance Number =",
				"Match Link: Group Number = M001,");
		}

		protected override void SetUp()
		{
			TransactionCreator = ObjectFactory.Get<ITransactionCreator>();
			TestTransaction = TransactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			Wrapper = new LocalOutstandingAmountValidationWrapper(TestTransaction);
		}

		ITransactionCreator TransactionCreator;
		AccTransactionHeader TestTransaction;
		IOutstandingAmountValidationWrapper Wrapper;
	}
}


