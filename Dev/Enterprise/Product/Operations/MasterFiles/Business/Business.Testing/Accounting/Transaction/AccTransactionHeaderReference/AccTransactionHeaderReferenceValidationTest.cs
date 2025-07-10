using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTransactionHeaderReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestShouldNotValidateFKToCancelledRecord()
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_TransactionType = TransactionTypes.CreditNote;
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_IsCancelled = true;

			var reference = Factory.NewWithValidTestData<AccTransactionHeaderReference>();
			reference.AH1_Type = AccTransactionHeaderReferenceTypes.MXR;
			reference.AH1_AH = header.PK;

			AssertNoErrors(reference);
		}
	}
}
