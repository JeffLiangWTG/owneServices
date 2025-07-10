using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTransactionMatchLinkLoaderTest : TestCaseWithFactory
	{
		public void TestLoadByAccTransactionHeader_TransactionHeaderIsInDatabase()
		{
			var creator = ObjectFactory.Get<ITransactionCreator>();
			var arINV = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);

			var matchLink1 = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLink1.AP_MatchGroupNum = "M001";
			matchLink1.AP_AH = arINV.PK;
			matchLink1.AP_Amount = 100m;

			var matchLink2 = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLink2.AP_MatchGroupNum = "M001";
			matchLink2.AP_AH = arINV.PK;
			matchLink2.AP_Amount = -100m;

			var group = new AccTransactionMatchLinkCriticalValidationTest.MatchLinkGroupForTest(Factory);
			group.Add(matchLink1);
			group.Add(matchLink2);

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals("Precondition", true, arINV.IsInDatabase);
			AssertEquals(2, AccTransactionMatchLinkLoader.LoadByAccTransactionHeader(arINV).Length);
		}

		public void TestLoadByAccTransactionHeader_TransactionHeaderIsNotInDatabase()
		{
			var creator = ObjectFactory.Get<ITransactionCreator>();
			var arINV = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);

			var matchLink = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLink.AP_MatchGroupNum = "M001";
			matchLink.AP_AH = arINV.PK;
			matchLink.AP_Amount = 100m;

			AssertEquals("Precondition", false, arINV.IsInDatabase);
			AssertEquals(1, AccTransactionMatchLinkLoader.LoadByAccTransactionHeader(arINV).Length);
		}
	}
}
