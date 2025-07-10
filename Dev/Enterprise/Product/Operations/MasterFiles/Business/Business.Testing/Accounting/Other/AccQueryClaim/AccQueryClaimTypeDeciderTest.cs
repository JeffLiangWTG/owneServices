using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccQueryClaimTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var arInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice.AH_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var apInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			apInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			apInvoice.AH_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var arQueryClaim = (AccQueryClaim)Factory.New<IARAccQueryClaim>();
			arQueryClaim.FillWithValidTestData();
			arQueryClaim.AY_AH = arInvoice.PK;

			var apQueryClaim = (AccQueryClaim)Factory.New<IAPAccQueryClaim>();
			apQueryClaim.FillWithValidTestData();
			apQueryClaim.AY_AH = apInvoice.PK;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			AssertEquals("Enterprise.Accounting.Business.AccQueryClaims.ARAccQueryClaim", otherFactory.Load<AccQueryClaim>(arQueryClaim.PK).GetType().FullName);
			AssertEquals("Enterprise.Accounting.Business.AccQueryClaims.APAccQueryClaim", otherFactory.Load<AccQueryClaim>(apQueryClaim.PK).GetType().FullName);
		}
	}
}
